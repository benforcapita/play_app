using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobs(stoppingToken);
                await Task.Delay(5000, stoppingToken); // Check every 5 seconds
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExtractionJobService");
                await Task.Delay(10000, stoppingToken); // Wait longer on error
            }
        }

        _logger.LogInformation("ExtractionJobService stopped");
    }

    private async Task ProcessPendingJobs(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDb>();
        var httpClientFactory = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var pendingJobs = await dbContext.ExtractionJobs
            .Where(j => j.Status == JobStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .Take(1) // Process one job at a time
            .ToListAsync(cancellationToken);

        foreach (var job in pendingJobs)
        {
            await ProcessJob(job, dbContext, httpClientFactory, configuration, cancellationToken);
        }
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
            var character = await ExtractCharacterFromFile(job, httpClientFactory, modelName, dbContext, cancellationToken);

            if (character != null)
            {
                // Save the character
                dbContext.Characters.Add(character);
                await dbContext.SaveChangesAsync(cancellationToken);

                job.ResultCharacterId = character.Id;
                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;

                _logger.LogInformation("Job {JobToken} completed successfully", job.JobToken);
            }
            else
            {
                job.Status = JobStatus.Failed;
                job.ErrorMessage = "Failed to extract character data";
                job.CompletedAt = DateTime.UtcNow;

                _logger.LogWarning("Job {JobToken} failed - no character extracted", job.JobToken);
            }
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            _logger.LogError(ex, "Job {JobToken} failed with exception", job.JobToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Character?> ExtractCharacterFromFile(ExtractionJob job, IHttpClientFactory httpClientFactory, string modelName, AppDb dbContext, CancellationToken cancellationToken)
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
        var resp = await http.PostAsync("chat/completions", content, cancellationToken);

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

        // Process the response and track section results
        var sheet = await ProcessCharacterSheetWithTracking(contentStr!, job, dbContext, cancellationToken);

        if (sheet == null) return null;

        var name = sheet.CharacterInfo.CharacterName ?? "Unnamed";
        var cls = sheet.CharacterInfo.ClassAndLevel ?? "";
        var sp = sheet.CharacterInfo.Species ?? "";

        return new Character { Name = name, Class = cls, Species = sp, Sheet = sheet };
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
                ["CharacterInfo"] = async element => {
                    var result = JsonSerializer.Deserialize<CharacterInfo>(element.GetRawText()) ?? new();
                    sheet.CharacterInfo = result;
                    return result;
                },
                ["Appearance"] = async element => {
                    var result = JsonSerializer.Deserialize<Appearance>(element.GetRawText()) ?? new();
                    sheet.Appearance = result;
                    return result;
                },
                ["AbilityScores"] = async element => {
                    var result = JsonSerializer.Deserialize<AbilityScores>(element.GetRawText()) ?? new();
                    sheet.AbilityScores = result;
                    return result;
                },
                ["SavingThrows"] = async element => {
                    var result = JsonSerializer.Deserialize<SavingThrows>(element.GetRawText()) ?? new();
                    sheet.SavingThrows = result;
                    return result;
                },
                ["Skills"] = async element => {
                    var result = JsonSerializer.Deserialize<List<Skill>>(element.GetRawText()) ?? new();
                    sheet.Skills = result;
                    return result;
                },
                ["Combat"] = async element => {
                    var result = JsonSerializer.Deserialize<Combat>(element.GetRawText()) ?? new();
                    sheet.Combat = result;
                    return result;
                },
                ["Proficiencies"] = async element => {
                    var result = JsonSerializer.Deserialize<Proficiencies>(element.GetRawText()) ?? new();
                    sheet.Proficiencies = result;
                    return result;
                },
                ["FeaturesAndTraits"] = async element => {
                    var result = JsonSerializer.Deserialize<List<FeatureTrait>>(element.GetRawText()) ?? new();
                    sheet.FeaturesAndTraits = result;
                    return result;
                },
                ["Equipment"] = async element => {
                    var equipmentOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    equipmentOptions.Converters.Add(new ItemsConverter());
                    var result = JsonSerializer.Deserialize<Equipment>(element.GetRawText(), equipmentOptions) ?? new();
                    sheet.Equipment = result;
                    return result;
                },
                ["Spellcasting"] = async element => {
                    var result = JsonSerializer.Deserialize<Spellcasting>(element.GetRawText()) ?? new();
                    sheet.Spellcasting = result;
                    return result;
                },
                ["Persona"] = async element => {
                    var result = JsonSerializer.Deserialize<Persona>(element.GetRawText()) ?? new();
                    sheet.Persona = result;
                    return result;
                },
                ["Backstory"] = async element => {
                    var result = JsonSerializer.Deserialize<Backstory>(element.GetRawText()) ?? new();
                    sheet.Backstory = result;
                    return result;
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
}