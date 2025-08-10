using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using play_app_api.Data;

namespace play_app_api.Services;

public class ExtractionJobService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExtractionJobService> _logger;

    public ExtractionJobService(IServiceProvider serviceProvider, ILogger<ExtractionJobService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExtractionJobService started");
        try
        {
            _serviceProvider.GetRequiredService<JobRuntimeMonitor>().WorkerStarted();
            // Sync with database state and clean up old stuck jobs
            await SyncMonitorWithDatabaseAndCleanup(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync monitor with database on startup");
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobs(stoppingToken);
                var delayMs = Math.Max(1000, (Environment.GetEnvironmentVariable("EXTRACTION_POLL_MS") is string s && int.TryParse(s, out var ms)) ? ms : 5000);
                await Task.Delay(delayMs, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExtractionJobService");
                try { _serviceProvider.GetRequiredService<JobRuntimeMonitor>().RecordError(ex.Message); } catch { }
                await Task.Delay(10000, stoppingToken); // Wait longer on error
            }
        }

        _logger.LogInformation("ExtractionJobService stopped");
    }

    private async Task ProcessPendingJobs(CancellationToken cancellationToken)
    {
        // Use a short-lived context to read the next batch of pending job IDs
        using var readScope = _serviceProvider.CreateScope();
        var readContext = readScope.ServiceProvider.GetRequiredService<AppDb>();
        var configForConcurrency = readScope.ServiceProvider.GetRequiredService<IConfiguration>();
        var concurrencyLimit = Math.Max(1, configForConcurrency.GetValue<int?>("EXTRACTION_CONCURRENCY") ?? 2);
        int pendingCount = 0, inProgressCount = 0;
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            pendingCount = await readContext.ExtractionJobs.CountAsync(j => j.Status == JobStatus.Pending, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Pending count query failed (transient)");
        }
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            inProgressCount = await readContext.ExtractionJobs.CountAsync(j => j.Status == JobStatus.InProgress, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "InProgress count query failed (transient)");
        }
        _logger.LogInformation("Queue snapshot: pending={Pending}, inProgress={InProgress}", pendingCount, inProgressCount);
        var runtime = readScope.ServiceProvider.GetRequiredService<JobRuntimeMonitor>();
        runtime.UpdateHeartbeat(pendingCount, inProgressCount, concurrencyLimit);

        List<int> pendingJobIds = new();
        try
        {
            _logger.LogDebug("Fetching pending jobs (limit={Limit})", concurrencyLimit);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30)); // Increase timeout
            pendingJobIds = await readContext.ExtractionJobs
                .Where(j => j.Status == JobStatus.Pending)
                .OrderBy(j => j.CreatedAt)
                .Take(concurrencyLimit)
                .Select(j => j.Id)
                .ToListAsync(cts.Token);
            _logger.LogDebug("Fetched {Count} pending job IDs", pendingJobIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Pending job fetch failed (transient) - continuing with empty list");
            _serviceProvider.GetRequiredService<JobRuntimeMonitor>().RecordError($"pending_fetch: {ex.Message}");
            // Don't return - continue with empty list so worker keeps heartbeat
        }

        if (pendingJobIds.Count == 0)
        {
            return;
        }

        // Process each job in its own scope/context to avoid sharing DbContext across threads
        var jobTasks = pendingJobIds.Select(async jobId =>
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDb>();
            var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var runtimeMon = scope.ServiceProvider.GetRequiredService<JobRuntimeMonitor>();

            var job = await dbContext.ExtractionJobs.FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);
            if (job != null)
            {
                _logger.LogInformation("Picked job {JobToken} (contentType={ContentType}) for processing", job.JobToken, job.ContentType);
                runtimeMon.MarkPicked(job.JobToken, job.ContentType);
                await ProcessJob(job, dbContext, httpClientFactory, configuration, cancellationToken);
            }
            else
            {
                _logger.LogDebug("Job {JobId} not found (may have been processed)", jobId);
            }
        });

        await Task.WhenAll(jobTasks);
    }

    private async Task ProcessJob(ExtractionJob job, AppDb dbContext, IHttpClientFactory httpClientFactory, IConfiguration configuration, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing job {JobToken}", job.JobToken);

        // Update job status to in progress
        job.Status = JobStatus.InProgress;
        job.StartedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            var modelName = configuration["OPENROUTER_MODEL"] ?? "google/gemini-2.5-flash";
            _logger.LogInformation("Job {JobToken} state=InProgress at {StartedAt}", job.JobToken, job.StartedAt);
            var runtimeMon2 = _serviceProvider.GetRequiredService<JobRuntimeMonitor>();
            runtimeMon2.StartSubtask(job.JobToken, "ai_call");
            var character = await ExtractCharacterFromFile(job, httpClientFactory, modelName, dbContext, cancellationToken);
            runtimeMon2.CompleteSubtask(job.JobToken, "ai_call", character != null);

            if (character != null)
            {
                job.ResultCharacterId = character.Id;
                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Job {JobToken} completed successfully", job.JobToken);
                _serviceProvider.GetRequiredService<JobRuntimeMonitor>().MarkCompleted(job.JobToken, true);
            }
            else
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Failed to extract character data";
                job.CompletedAt = DateTime.UtcNow;

                _logger.LogWarning("Job {JobToken} failed - no character extracted", job.JobToken);
                _serviceProvider.GetRequiredService<JobRuntimeMonitor>().MarkCompleted(job.JobToken, false);
            }
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogError(ex, "Job {JobToken} failed with exception: {ErrorMessage}", job.JobToken, ex.Message);
            var monitor = _serviceProvider.GetRequiredService<JobRuntimeMonitor>();
            monitor.MarkCompleted(job.JobToken, false);
            // Mark any running subtasks as failed
            monitor.MarkSubtaskError(job.JobToken, "overall_job", ex.Message);
        }

        // Always save the job status, even if previous saves failed
        await SaveJobStatusWithRetry(dbContext, job, cancellationToken);
        _logger.LogInformation("Job {JobToken} state={State} completedAt={CompletedAt}", job.JobToken, job.Status, job.CompletedAt);
    }

    private async Task<Character?> ExtractCharacterFromFile(ExtractionJob job, IHttpClientFactory httpClientFactory, string modelName, AppDb dbContext, CancellationToken cancellationToken)
    {
        try
        {
            // Build messages with the CORRECT shapes per OpenRouter docs
            object userContent =
                job.ContentType == "application/pdf"
                ? new object[] {
                    new { type = "text", text = "Extract a D&D 5e character sheet as strict JSON." },
                    new { type = "file", file = new { filename = job.FileName, file_data = job.FileDataUrl } }
                  }
                : new object[] {
                    new { type = "text", text = "Extract a D&D 5e character sheet as strict JSON." },
                    new { type = "image_url", image_url = new { url = job.FileDataUrl } }
                  };

            var messages = new object[] {
                new { role = "system", content = GetSystemPrompt() },
                new { role = "user", content = userContent }
            };

            var responseFormat = new { type = "json_object" };

            object? plugins = job.ContentType == "application/pdf"
                ? new object[] { new { id = "file-parser", pdf = new { engine = "mistral-ocr" } } }
                : null;

            var body = new Dictionary<string, object?>
            {
                ["model"] = modelName,
                ["messages"] = messages,
                ["response_format"] = responseFormat
            };
            if (plugins != null) body["plugins"] = plugins;

            // Call OpenRouter
            var http = httpClientFactory.CreateClient("openrouter");
            var requestBody = JsonSerializer.Serialize(body);
            _logger.LogDebug("Request to OpenRouter: {RequestBody}", requestBody);

            using var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var start = DateTime.UtcNow;
            var runtimeMon = _serviceProvider.GetRequiredService<JobRuntimeMonitor>();
            _logger.LogInformation("OpenRouter request started for job {JobToken} using model {Model}", job.JobToken, modelName);
            runtimeMon.MarkOpenRouterStart(job.JobToken);
            var resp = await http.PostAsync("chat/completions", content, cancellationToken);
            var elapsed = DateTime.UtcNow - start;
            _logger.LogInformation("OpenRouter request finished for job {JobToken} in {ElapsedMs} ms (status {StatusCode})", job.JobToken, (int)elapsed.TotalMilliseconds, (int)resp.StatusCode);
            runtimeMon.MarkOpenRouterFinish(job.JobToken, (long)elapsed.TotalMilliseconds, (int)resp.StatusCode);

            var text = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("Raw OpenRouter Response: {ResponseText}", text);

            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"OpenRouter error {(int)resp.StatusCode}: {text}");
            }

            // Extract assistant message text (the JSON string)
            using var doc = JsonDocument.Parse(text);
            var contentStr = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            _logger.LogDebug("AI Response: {AIResponse}", contentStr);

            if (string.IsNullOrEmpty(contentStr))
            {
                _logger.LogError("Failed to get character sheet content for job {JobId}", job.Id);
                return null;
            }

            var monitor = _serviceProvider.GetRequiredService<JobRuntimeMonitor>();
            monitor.StartSubtask(job.JobToken, "parse_sections");
            var sheet = await ProcessCharacterSheetWithTracking(contentStr, job, dbContext, cancellationToken);
            monitor.CompleteSubtask(job.JobToken, "parse_sections", sheet != null);
            if (sheet == null)
            {
                _logger.LogError("Failed to process character sheet for job {JobId}", job.Id);
                return null;
            }

            var character = new Character
            {
                Name = sheet.CharacterInfo.Name,
                Class = sheet.CharacterInfo.Class,
                Species = sheet.CharacterInfo.Species
            };

            // Save the Character first (without the Sheet relationship)
            monitor.StartSubtask(job.JobToken, "persist_character");
            
            try
            {
                // Use a transaction to ensure atomicity
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                
                try
                {
                    dbContext.Characters.Add(character);
                    using var cts1 = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts1.CancelAfter(TimeSpan.FromSeconds(45));
                    await dbContext.SaveChangesAsync(cts1.Token);

                    // Now set the CharacterId on the CharacterSheet and save it
                    sheet.CharacterId = character.Id;
                    sheet.Character = character;
                    character.Sheet = sheet;
                    dbContext.CharacterSheets.Add(sheet);
                    
                    using var cts2 = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    cts2.CancelAfter(TimeSpan.FromSeconds(45));
                    await dbContext.SaveChangesAsync(cts2.Token);
                    
                    await transaction.CommitAsync(cancellationToken);
                    monitor.CompleteSubtask(job.JobToken, "persist_character", true);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }
            catch (Exception saveEx)
            {
                _logger.LogError(saveEx, "Database save failed for job {JobToken}: {Error}", job.JobToken, saveEx.Message);
                monitor.MarkSubtaskError(job.JobToken, "persist_character", saveEx.Message);
                
                // Don't re-throw - let the job complete but mark as failed
                // This prevents the job from being stuck in InProgress
                _logger.LogWarning("Continuing job {JobToken} as failed due to persistence error", job.JobToken);
                return null;
            }

            _logger.LogInformation("Character {CharacterName} extracted and saved with ID {CharacterId}", character.Name, character.Id);
            return character;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract character for job {JobId}", job.Id);
            return null;
        }
    }

    private async Task<CharacterSheet?> ProcessCharacterSheetWithTracking(string contentStr, ExtractionJob job, AppDb dbContext, CancellationToken cancellationToken)
    {
        var sheet = new CharacterSheet();
        var sectionResults = new List<SectionResult>();

        try
        {
            using var parseDoc = JsonDocument.Parse(contentStr);
            var root = parseDoc.RootElement;

            // Define all sections to process
            var sections = new Dictionary<string, Func<JsonElement, Task<object?>>>
            {
                ["CharacterInfo"] = element => {
                    var result = JsonSerializer.Deserialize<CharacterInfo>(element.GetRawText()) ?? new();
                    sheet.CharacterInfo = result;
                    return Task.FromResult<object?>(result);
                },
                ["Appearance"] = element => {
                    var result = JsonSerializer.Deserialize<Appearance>(element.GetRawText()) ?? new();
                    sheet.Appearance = result;
                    return Task.FromResult<object?>(result);
                },
                ["AbilityScores"] = element => {
                    var result = JsonSerializer.Deserialize<AbilityScores>(element.GetRawText()) ?? new();
                    sheet.AbilityScores = result;
                    return Task.FromResult<object?>(result);
                },
                ["SavingThrows"] = element => {
                    var result = JsonSerializer.Deserialize<SavingThrows>(element.GetRawText()) ?? new();
                    sheet.SavingThrows = result;
                    return Task.FromResult<object?>(result);
                },
                ["Skills"] = element => {
                    var result = JsonSerializer.Deserialize<List<Skill>>(element.GetRawText()) ?? new();
                    sheet.Skills = result;
                    return Task.FromResult<object?>(result);
                },
                ["Combat"] = element => {
                    var combatOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var combat = JsonSerializer.Deserialize<Combat>(element.GetRawText(), combatOptions) ?? new();
                    // Handle fields that might be strings instead of integers
                    if (element.TryGetProperty("hitPoints", out var hitPointsElement))
                    {
                        var hp = new HitPoints();
                        if (hitPointsElement.TryGetProperty("max", out var maxHpElement) && maxHpElement.ValueKind == JsonValueKind.Number && maxHpElement.TryGetInt32(out int maxHp))
                        {
                            hp.Max = maxHp;
                        }
                        if (hitPointsElement.TryGetProperty("current", out var currentHpElement))
                        {
                            if (currentHpElement.ValueKind == JsonValueKind.Number && currentHpElement.TryGetInt32(out int currentHp))
                            {
                                hp.Current = currentHp;
                            }
                            else if (currentHpElement.ValueKind == JsonValueKind.String && int.TryParse(currentHpElement.GetString(), out currentHp))
                            {
                                hp.Current = currentHp;
                            }
                        }
                        if (hitPointsElement.TryGetProperty("temporary", out var tempHpElement))
                        {
                            if (tempHpElement.ValueKind == JsonValueKind.Number && tempHpElement.TryGetInt32(out int tempHp))
                            {
                                hp.Temporary = tempHp;
                            }
                            else if (tempHpElement.ValueKind == JsonValueKind.String && int.TryParse(tempHpElement.GetString(), out tempHp))
                            {
                                hp.Temporary = tempHp;
                            }
                        }
                        combat.HitPoints = hp;
                    }
                    if (element.TryGetProperty("armorClass", out var acElement))
                    {
                        if (acElement.ValueKind == JsonValueKind.Number && acElement.TryGetInt32(out int ac))
                        {
                            combat.ArmorClass = ac;
                        }
                        else if (acElement.ValueKind == JsonValueKind.String && int.TryParse(acElement.GetString(), out ac))
                        {
                            combat.ArmorClass = ac;
                        }
                    }
                    if (element.TryGetProperty("initiative", out var initElement))
                    {
                        if (initElement.ValueKind == JsonValueKind.Number && initElement.TryGetInt32(out int init))
                        {
                            combat.Initiative = init;
                        }
                        else if (initElement.ValueKind == JsonValueKind.String && int.TryParse(initElement.GetString(), out init))
                        {
                            combat.Initiative = init;
                        }
                    }
                    if (element.TryGetProperty("proficiencyBonus", out var profElement))
                    {
                        if (profElement.ValueKind == JsonValueKind.Number && profElement.TryGetInt32(out int prof))
                        {
                            combat.ProficiencyBonus = prof;
                        }
                        else if (profElement.ValueKind == JsonValueKind.String && int.TryParse(profElement.GetString(), out prof))
                        {
                            combat.ProficiencyBonus = prof;
                        }
                    }
                    if (element.TryGetProperty("passiveScores", out var passiveScoresElement))
                    {
                        var ps = new PassiveScores();
                        if (passiveScoresElement.TryGetProperty("perception", out var percElement))
                        {
                            if (percElement.ValueKind == JsonValueKind.Number && percElement.TryGetInt32(out int perc))
                            {
                                ps.Perception = perc;
                            }
                            else if (percElement.ValueKind == JsonValueKind.String && int.TryParse(percElement.GetString(), out perc))
                            {
                                ps.Perception = perc;
                            }
                        }
                        if (passiveScoresElement.TryGetProperty("insight", out var insightElement))
                        {
                            if (insightElement.ValueKind == JsonValueKind.Number && insightElement.TryGetInt32(out int insight))
                            {
                                ps.Insight = insight;
                            }
                            else if (insightElement.ValueKind == JsonValueKind.String && int.TryParse(insightElement.GetString(), out insight))
                            {
                                ps.Insight = insight;
                            }
                        }
                        if (passiveScoresElement.TryGetProperty("investigation", out var investElement))
                        {
                            if (investElement.ValueKind == JsonValueKind.Number && investElement.TryGetInt32(out int invest))
                            {
                                ps.Investigation = invest;
                            }
                            else if (investElement.ValueKind == JsonValueKind.String && int.TryParse(investElement.GetString(), out invest))
                            {
                                ps.Investigation = invest;
                            }
                        }
                        combat.PassiveScores = ps;
                    }
                    sheet.Combat = combat;
                    return Task.FromResult<object?>(combat);
                },
                ["Proficiencies"] = element => {
                    var result = JsonSerializer.Deserialize<Proficiencies>(element.GetRawText()) ?? new();
                    sheet.Proficiencies = result;
                    return Task.FromResult<object?>(result);
                },
                ["FeaturesAndTraits"] = element => {
                    var result = JsonSerializer.Deserialize<List<FeatureTrait>>(element.GetRawText()) ?? new();
                    sheet.FeaturesAndTraits = result;
                    return Task.FromResult<object?>(result);
                },
                ["Equipment"] = element => {
                    var equipmentOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    equipmentOptions.Converters.Add(new ItemsConverter());
                    var result = JsonSerializer.Deserialize<Equipment>(element.GetRawText(), equipmentOptions) ?? new();
                    sheet.Equipment = result;
                    return Task.FromResult<object?>(result);
                },
                            ["Spellcasting"] = element => {
                var spellcasting = new Spellcasting();
                
                // Manually deserialize basic properties
                if (element.TryGetProperty("class", out var classElement) && classElement.ValueKind == JsonValueKind.String)
                {
                    spellcasting.Class = classElement.GetString() ?? "";
                }
                if (element.TryGetProperty("ability", out var abilityElement) && abilityElement.ValueKind == JsonValueKind.String)
                {
                    spellcasting.Ability = abilityElement.GetString() ?? "";
                }
                if (element.TryGetProperty("saveDC", out var saveDCElement) && saveDCElement.ValueKind == JsonValueKind.Number && saveDCElement.TryGetInt32(out int saveDC))
                {
                    spellcasting.SaveDC = saveDC;
                }
                if (element.TryGetProperty("attackBonus", out var attackBonusElement) && attackBonusElement.ValueKind == JsonValueKind.Number && attackBonusElement.TryGetInt32(out int attackBonus))
                {
                    spellcasting.AttackBonus = attackBonus;
                }
                
                // Handle spellSlots
                if (element.TryGetProperty("spellSlots", out var spellSlotsElement))
                {
                    try
                    {
                        var spellSlots = JsonSerializer.Deserialize<SpellSlots>(spellSlotsElement.GetRawText()) ?? new();
                        spellcasting.SpellSlots = spellSlots;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize spell slots");
                        spellcasting.SpellSlots = new SpellSlots();
                    }
                }
                
                // Handle cantrips as lists of strings or objects
                if (element.TryGetProperty("cantrips", out var cantripsElement) && cantripsElement.ValueKind == JsonValueKind.Array)
                {
                    var cantrips = new List<Spell>();
                    foreach (var cantrip in cantripsElement.EnumerateArray())
                    {
                        if (cantrip.ValueKind == JsonValueKind.String)
                        {
                            cantrips.Add(new Spell { Name = cantrip.GetString() ?? "", SpellType = "cantrip" });
                        }
                        else if (cantrip.ValueKind == JsonValueKind.Object)
                        {
                            try
                            {
                                var spell = JsonSerializer.Deserialize<Spell>(cantrip.GetRawText());
                                if (spell != null)
                                {
                                    spell.SpellType = "cantrip";
                                    cantrips.Add(spell);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to deserialize cantrip object, using name only");
                                // Fallback to just the name if object deserialization fails
                                if (cantrip.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                                {
                                    cantrips.Add(new Spell { Name = nameElement.GetString() ?? "", SpellType = "cantrip" });
                                }
                            }
                        }
                    }
                    spellcasting.Cantrips = cantrips;
                }
                
                // Handle spellsKnown as lists of strings or objects
                if (element.TryGetProperty("spellsKnown", out var spellsKnownElement) && spellsKnownElement.ValueKind == JsonValueKind.Array)
                {
                    var spellsKnown = new List<Spell>();
                    foreach (var spell in spellsKnownElement.EnumerateArray())
                    {
                        if (spell.ValueKind == JsonValueKind.String)
                        {
                            spellsKnown.Add(new Spell { Name = spell.GetString() ?? "", SpellType = "spell" });
                        }
                        else if (spell.ValueKind == JsonValueKind.Object)
                        {
                            try
                            {
                                var spellObj = JsonSerializer.Deserialize<Spell>(spell.GetRawText());
                                if (spellObj != null)
                                {
                                    spellObj.SpellType = "spell";
                                    spellsKnown.Add(spellObj);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to deserialize spell object, using name only");
                                // Fallback to just the name if object deserialization fails
                                if (spell.TryGetProperty("name", out var nameElement) && nameElement.ValueKind == JsonValueKind.String)
                                {
                                    spellsKnown.Add(new Spell { Name = nameElement.GetString() ?? "", SpellType = "spell" });
                                }
                            }
                        }
                    }
                    spellcasting.SpellsKnown = spellsKnown;
                }
                
                sheet.Spellcasting = spellcasting;
                return Task.FromResult<object?>(spellcasting);
            },
                ["Persona"] = element => {
                    var result = JsonSerializer.Deserialize<Persona>(element.GetRawText()) ?? new();
                    sheet.Persona = result;
                    return Task.FromResult<object?>(result);
                },
                ["Backstory"] = element => {
                    var result = JsonSerializer.Deserialize<Backstory>(element.GetRawText()) ?? new();
                    sheet.Backstory = result;
                    return Task.FromResult<object?>(result);
                }
            };

            // Process each section and track results
            foreach (var (sectionName, processor) in sections)
            {
                var sectionResult = new SectionResult
                {
                    SectionName = sectionName,
                    ExtractionJobId = job.Id,
                    ProcessedAt = DateTime.UtcNow
                };

                try
                {
                    var propertyName = char.ToLowerInvariant(sectionName[0]) + sectionName[1..];
                    if (root.TryGetProperty(propertyName, out var element))
                    {
                        await processor(element);
                        sectionResult.IsSuccessful = true;
                        _logger.LogDebug("✅ {SectionName} processed successfully", sectionName);
                    }
                    else
                    {
                        sectionResult.IsSuccessful = false;
                        sectionResult.ErrorMessage = "Section not found in response";
                        _logger.LogWarning("⚠️ {SectionName} not found in response", sectionName);
                    }
                }
                catch (Exception ex)
                {
                    sectionResult.IsSuccessful = false;
                    sectionResult.ErrorMessage = ex.Message;
                    _logger.LogError(ex, "❌ {SectionName} processing failed", sectionName);
                }

                sectionResults.Add(sectionResult);
            }

            // Save section results to database
            dbContext.SectionResults.AddRange(sectionResults);
            await dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Processed {TotalSections} sections, {SuccessfulSections} successful", 
                sectionResults.Count, sectionResults.Count(s => s.IsSuccessful));

            return sheet;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process character sheet");
            return null;
        }
    }

    private static string GetSystemPrompt()
    {
        return @"You are a D&D 5e character sheet parser. Your task is to extract ALL the actual character information from the provided document and return it as a valid JSON object.

IMPORTANT: 
- Extract ONLY the REAL data that appears in the document
- Do NOT return placeholder text like 'CHARACTER NAME', 'PLAYER NAME', etc.
- If a field is empty or not present in the document, use an empty string """" or 0 for numbers
- Look carefully at the document and extract the actual values

Return the data in this exact JSON structure:

{
  ""characterInfo"": {
    ""characterName"": ""actual character name from document"",
    ""playerName"": ""actual player name from document"",
    ""classAndLevel"": ""actual class and level from document"",
    ""species"": ""actual species/race from document"",
    ""background"": ""actual background from document"",
    ""experiencePoints"": ""actual XP from document"",
    ""alignment"": ""actual alignment from document""
  },
  ""appearance"": {
    ""size"": ""actual size from document"",
    ""gender"": ""actual gender from document"",
    ""age"": ""actual age from document"",
    ""height"": ""actual height from document"",
    ""weight"": ""actual weight from document"",
    ""skin"": ""actual skin color from document"",
    ""eyes"": ""actual eye color from document"",
    ""hair"": ""actual hair color from document""
  },
  ""abilityScores"": {
    ""strength"": { ""score"": actual number, ""modifier"": actual number },
    ""dexterity"": { ""score"": actual number, ""modifier"": actual number },
    ""constitution"": { ""score"": actual number, ""modifier"": actual number },
    ""intelligence"": { ""score"": actual number, ""modifier"": actual number },
    ""wisdom"": { ""score"": actual number, ""modifier"": actual number },
    ""charisma"": { ""score"": actual number, ""modifier"": actual number }
  },
  ""savingThrows"": {
    ""strength"": { ""proficient"": true/false based on document },
    ""dexterity"": { ""proficient"": true/false based on document },
    ""constitution"": { ""proficient"": true/false based on document },
    ""intelligence"": { ""proficient"": true/false based on document },
    ""wisdom"": { ""proficient"": true/false based on document },
    ""charisma"": { ""proficient"": true/false based on document }
  },
  ""skills"": [
    { ""name"": ""skill name"", ""proficient"": true/false, ""modifier"": actual number }
  ],
  ""combat"": {
    ""armorClass"": actual number,
    ""initiative"": actual number,
    ""speed"": ""actual speed from document"",
    ""proficiencyBonus"": actual number,
    ""inspiration"": true/false,
    ""hitPoints"": { ""max"": actual number, ""current"": actual number, ""temporary"": actual number },
    ""hitDice"": { ""total"": ""actual dice from document"", ""current"": ""actual current dice from document"" },
    ""deathSaves"": { ""successes"": actual number, ""failures"": actual number },
    ""passiveScores"": { ""perception"": actual number, ""insight"": actual number, ""investigation"": actual number }
  },
  ""proficiencies"": {
    ""armor"": [""actual armor proficiencies from document""],
    ""weapons"": [""actual weapon proficiencies from document""],
    ""tools"": [""actual tool proficiencies from document""],
    ""languages"": [""actual languages from document""]
  },
  ""featuresAndTraits"": [
    { ""name"": ""feature name"", ""description"": ""feature description"" }
  ],
  ""equipment"": {
    ""items"": [""actual equipment items from document""],
    ""currency"": { ""cp"": actual number, ""sp"": actual number, ""ep"": actual number, ""gp"": actual number, ""pp"": actual number },
    ""carryingCapacity"": { ""weightCarried"": actual number, ""encumbered"": actual number, ""pushDragLift"": actual number }
  },
  ""spellcasting"": {
    ""class"": ""actual spellcasting class from document"",
    ""ability"": ""actual spellcasting ability from document"",
    ""saveDC"": actual number,
    ""attackBonus"": actual number,
    ""spellSlots"": {
      ""level1"": { ""total"": actual number, ""used"": actual number },
      ""level2"": { ""total"": actual number, ""used"": actual number },
      ""level3"": { ""total"": actual number, ""used"": actual number },
      ""level4"": { ""total"": actual number, ""used"": actual number },
      ""level5"": { ""total"": actual number, ""used"": actual number },
      ""level6"": { ""total"": actual number, ""used"": actual number },
      ""level7"": { ""total"": actual number, ""used"": actual number },
      ""level8"": { ""total"": actual number, ""used"": actual number },
      ""level9"": { ""total"": actual number, ""used"": actual number }
    },
    ""cantrips"": [""actual cantrips from document""],
    ""spellsKnown"": [""actual spells from document""]
  },
  ""persona"": {
    ""personalityTraits"": ""actual personality traits from document"",
    ""ideals"": ""actual ideals from document"",
    ""bonds"": ""actual bonds from document"",
    ""flaws"": ""actual flaws from document""
  },
  ""backstory"": {
    ""alliesAndOrganizations"": ""actual allies from document"",
    ""characterBackstory"": ""actual backstory from document"",
    ""additionalNotes"": ""actual notes from document""
  }
}

CRITICAL: Only extract the actual data that appears in the document. Do not make up or guess values. If something is not present, use empty strings or 0.";
    }

    private async Task SyncMonitorWithDatabaseAndCleanup(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDb>();
        var monitor = _serviceProvider.GetRequiredService<JobRuntimeMonitor>();

        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);
            
            // Find jobs stuck in InProgress for more than 1 hour
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            
            var stuckJobs = await dbContext.ExtractionJobs
                .Where(j => j.Status == JobStatus.InProgress && 
                           j.StartedAt != null && 
                           j.StartedAt < oneHourAgo)
                .ToListAsync(cts.Token);

            _logger.LogInformation("Found {Count} stuck jobs older than 1 hour", stuckJobs.Count);

            // Mark stuck jobs as failed
            foreach (var job in stuckJobs)
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Job timed out after 1 hour";
                job.CompletedAt = DateTime.UtcNow;
                _logger.LogWarning("Marking stuck job {JobToken} as failed (started at {StartedAt})", 
                    job.JobToken, job.StartedAt);
            }

            if (stuckJobs.Count > 0)
            {
                await dbContext.SaveChangesAsync(cts.Token);
            }

            // Get current database state for recent jobs (last 10 completed/failed)
            var recentJobs = await dbContext.ExtractionJobs
                .Where(j => j.Status == JobStatus.Completed || j.Status == JobStatus.Failed)
                .OrderByDescending(j => j.CompletedAt)
                .Take(10)
                .ToListAsync(cts.Token);

            var pendingJobs = await dbContext.ExtractionJobs
                .Where(j => j.Status == JobStatus.Pending)
                .OrderBy(j => j.CreatedAt)
                .ToListAsync(cts.Token);

            var inProgressJobs = await dbContext.ExtractionJobs
                .Where(j => j.Status == JobStatus.InProgress)
                .ToListAsync(cts.Token);

            // Convert to RuntimeJobInfo
            var pendingRuntime = pendingJobs.Select(ConvertToRuntimeJobInfo).ToList();
            var inProgressRuntime = inProgressJobs.Select(ConvertToRuntimeJobInfo).ToList();
            var recentRuntime = recentJobs.Select(ConvertToRuntimeJobInfo).ToList();

            // Sync the monitor
            monitor.SyncWithDatabase(pendingRuntime, inProgressRuntime, recentRuntime);

            _logger.LogInformation("Synced monitor: {Pending} pending, {InProgress} in-progress, {Recent} recent jobs", 
                pendingRuntime.Count, inProgressRuntime.Count, recentRuntime.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync monitor with database");
            monitor.RecordError($"database_sync: {ex.Message}");
        }
    }

    private static RuntimeJobInfo ConvertToRuntimeJobInfo(ExtractionJob job)
    {
        var state = job.Status switch
        {
            JobStatus.Pending => "queued",
            JobStatus.InProgress => "in_progress", 
            JobStatus.Completed => "completed",
            JobStatus.Failed => "failed",
            _ => "unknown"
        };

        return new RuntimeJobInfo
        {
            JobToken = job.JobToken,
            ContentType = job.ContentType ?? "",
            State = state,
            StartedAt = job.StartedAt,
            LastUpdatedAt = job.CompletedAt ?? job.StartedAt ?? job.CreatedAt,
            LastEvent = state,
            Subtasks = new List<SubtaskInfo>() // Database doesn't track subtasks, only runtime does
        };
    }

    private async Task SaveJobStatusWithRetry(AppDb dbContext, ExtractionJob job, CancellationToken cancellationToken)
    {
        const int maxRetries = 3;
        var delay = TimeSpan.FromSeconds(1);

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(15));

                // Detach and re-attach the entity to avoid tracking conflicts
                dbContext.Entry(job).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                
                // Reload the job from database to get the latest version
                var dbJob = await dbContext.ExtractionJobs
                    .FirstOrDefaultAsync(j => j.Id == job.Id, cts.Token);
                
                if (dbJob != null)
                {
                    // Update only the status fields
                    dbJob.Status = job.Status;
                    dbJob.ErrorMessage = job.ErrorMessage;
                    dbJob.CompletedAt = job.CompletedAt;
                    dbJob.ResultCharacterId = job.ResultCharacterId;
                    
                    await dbContext.SaveChangesAsync(cts.Token);
                    _logger.LogInformation("Successfully saved job status for {JobToken} on attempt {Attempt}", job.JobToken, attempt);
                    return;
                }
                else
                {
                    _logger.LogError("Could not find job {JobId} in database for status update", job.Id);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save job status for {JobToken} on attempt {Attempt}/{MaxRetries}: {Error}", 
                    job.JobToken, attempt, maxRetries, ex.Message);
                
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "CRITICAL: Failed to save job status for {JobToken} after {MaxRetries} attempts. Job may appear stuck.", 
                        job.JobToken, maxRetries);
                    // Record error in runtime monitor
                    try
                    {
                        _serviceProvider.GetRequiredService<JobRuntimeMonitor>().RecordError($"status_save_failed: {ex.Message}");
                    }
                    catch { }
                    return;
                }
                
                await Task.Delay(delay, cancellationToken);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff
            }
        }
    }
}