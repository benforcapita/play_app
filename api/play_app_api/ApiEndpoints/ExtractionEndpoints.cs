
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using play_app_api.Data;
using play_app_api.Services;
using System.Security.Claims;

namespace play_app_api;

public static class ExtractionEndpoints
{
    public static void MapExtractionEndpoints(this IEndpointRouteBuilder app, string modelName)
    {
        var grp = app.MapGroup("/api/extract").RequireAuthorization("UserOnly");

        // Start an extraction job and return a token
        grp.MapPost("/characters", async (HttpRequest req, AppDb db, JobRuntimeMonitor monitor, ILogger<Program> logger, ClaimsPrincipal user) =>
        {
            logger.LogInformation("Extraction request received at {Timestamp}", DateTime.UtcNow);
            logger.LogInformation("Request method: {Method}, Content-Type: {ContentType}", req.Method, req.ContentType);
            logger.LogInformation("Request headers: {Headers}", string.Join(", ", req.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value.ToArray())}")));

            try
            {
                if (!req.HasFormContentType)
                {
                    logger.LogWarning("Request does not have form content type. Expected multipart/form-data, got: {ContentType}", req.ContentType);
                    return Results.BadRequest("multipart/form-data expected");
                }

                var form = await req.ReadFormAsync();
                logger.LogInformation("Form data received with {FieldCount} fields", form.Count);

                var file = form.Files.SingleOrDefault();
                if (file is null || file.Length == 0)
                {
                    logger.LogWarning("No file found in form data or file is empty. File count: {FileCount}", form.Files.Count);
                    return Results.BadRequest("file required");
                }

                logger.LogInformation("File received: {FileName}, Size: {FileSize}, ContentType: {ContentType}",
                   file.FileName, file.Length, file.ContentType);

                // Validate file type
                var supportedTypes = new[] { "image/png", "image/jpeg", "image/webp", "image/gif", "application/pdf" };
                if (!supportedTypes.Contains(file.ContentType))
                {
                    logger.LogWarning("Unsupported file type: {ContentType}. Supported types: {SupportedTypes}",
                       file.ContentType, string.Join(", ", supportedTypes));
                    return Results.BadRequest("Unsupported content type. Supported types: PNG, JPEG, WebP, GIF, PDF");
                }

                // Convert file to data URL
                await using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());
                var dataUrl = file.ContentType switch
                {
                    "image/png" or "image/jpeg" or "image/webp" or "image/gif"
                       => $"data:{file.ContentType};base64,{base64}",
                    "application/pdf" => $"data:application/pdf;base64,{base64}",
                    _ => throw new InvalidOperationException("Unsupported content type")
                };

                logger.LogInformation("File converted to data URL. Base64 length: {Base64Length}", base64.Length);

                var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User ID not found");

                // Create extraction job
                var jobToken = Guid.NewGuid().ToString("N")[..16]; // 16-character token
                var job = new ExtractionJob
                {
                    JobToken = jobToken,
                    FileName = file.FileName ?? "unknown",
                    ContentType = file.ContentType ?? "unknown",
                    FileDataUrl = dataUrl,
                    Status = JobStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    OwnerId = uid
                };

                logger.LogInformation("Creating extraction job with token: {JobToken}", jobToken);

                db.ExtractionJobs.Add(job);
                await db.SaveChangesAsync();

                logger.LogInformation("Extraction job created successfully. JobToken: {JobToken}, JobId: {JobId}", jobToken, job.Id);

                // Track queued state in runtime monitor (with content type)
                monitor.MarkQueued(jobToken, job.ContentType);

                var response = new { jobToken, message = "Extraction job started. Use the job token to check status." };
                logger.LogInformation("Returning successful response: {Response}", JsonSerializer.Serialize(response));

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing extraction request");

                // Provide more specific error messages based on exception type
                if (ex is InvalidOperationException)
                {
                    return Results.BadRequest($"Invalid operation: {ex.Message}");
                }
                else if (ex.Message.Contains("connection") || ex.Message.Contains("database"))
                {
                    return Results.Problem("Database connection error. Please try again later.");
                }
                else
                {
                    return Results.Problem("Internal server error during extraction request processing");
                }
            }
        });

        grp.MapGet("/jobs/{jobToken}/status", async (string jobToken, AppDb db, ILogger<Program> logger, ClaimsPrincipal user) =>
        {
            logger.LogInformation("Status check requested for job token: {JobToken}", jobToken);

            try
            {
                var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? throw new InvalidOperationException("User ID not found");

                var job = await db.ExtractionJobs
                    .Include(j => j.SectionResults)
                    .Include(j => j.ResultCharacter)
                    .FirstOrDefaultAsync(j => j.JobToken == jobToken && j.OwnerId == uid);

                if (job == null)
                {
                    logger.LogWarning("Job not found for token: {JobToken}", jobToken);
                    return Results.NotFound(new { message = "Job not found" });
                }

                logger.LogInformation("Job found. Status: {Status}, CreatedAt: {CreatedAt}", job.Status, job.CreatedAt);

                // Compute queue position if pending or in-progress
                int? queuePosition = null;
                if (job.Status == JobStatus.Pending)
                {
                    queuePosition = await db.ExtractionJobs
                        .Where(j => j.OwnerId == uid && j.Status == JobStatus.Pending && j.CreatedAt < job.CreatedAt)
                        .CountAsync();
                }
                else if (job.Status == JobStatus.InProgress)
                {
                    // 0 means currently being processed
                    queuePosition = 0;
                }

                var response = new
                {
                    jobToken = job.JobToken,
                    status = job.Status.ToString().ToLower(),
                    createdAt = job.CreatedAt,
                    startedAt = job.StartedAt,
                    completedAt = job.CompletedAt,
                    isSuccessful = job.IsSuccessful,
                    errorMessage = job.ErrorMessage,
                    queuePosition,
                    sectionResults = job.SectionResults.Select(s => new
                    {
                        sectionName = s.SectionName,
                        isSuccessful = s.IsSuccessful,
                        errorMessage = s.ErrorMessage,
                        processedAt = s.ProcessedAt
                    }).ToList(),
                    character = job.ResultCharacter
                };

                logger.LogInformation("Returning job status response for token: {JobToken}", jobToken);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking job status for token: {JobToken}", jobToken);

                if (ex.Message.Contains("connection") || ex.Message.Contains("database"))
                {
                    return Results.Problem("Database connection error. Please try again later.");
                }
                else
                {
                    return Results.Problem("Internal server error during status check");
                }
            }
        });

        // Get completed job result
        grp.MapGet("/jobs/{jobToken}/result", async (string jobToken, AppDb db, ILogger<Program> logger, ClaimsPrincipal user) =>
        {
            logger.LogInformation("Result requested for job token: {JobToken}", jobToken);

            try
            {
                var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? throw new InvalidOperationException("User ID not found");

                var job = await db.ExtractionJobs
                    .Include(j => j.ResultCharacter)
                    .Include(j => j.SectionResults)
                    .FirstOrDefaultAsync(j => j.JobToken == jobToken && j.OwnerId == uid);

                if (job == null)
                {
                    logger.LogWarning("Job not found for token: {JobToken}", jobToken);
                    return Results.NotFound(new { message = "Job not found" });
                }

                logger.LogInformation("Job found. Status: {Status}, IsSuccessful: {IsSuccessful}", job.Status, job.IsSuccessful);

                if (job.Status != JobStatus.Completed)
                {
                    logger.LogWarning("Job not completed. Current status: {Status}", job.Status);
                    return Results.BadRequest(new { message = $"Job not completed. Current status: {job.Status}" });
                }

                if (job.ResultCharacter == null)
                {
                    logger.LogWarning("Job completed but no character result available for token: {JobToken}", jobToken);
                    return Results.Problem("Job completed but no character result available");
                }

                var successfulSections = job.SectionResults.Count(s => s.IsSuccessful);
                var totalSections = job.SectionResults.Count;
                var successRate = totalSections > 0 ? (double)successfulSections / totalSections : 0;

                var response = new
                {
                    character = job.ResultCharacter,
                    jobSummary = new
                    {
                        isSuccessful = job.IsSuccessful,
                        successRate = successRate,
                        successfulSections = successfulSections,
                        totalSections = totalSections,
                        completedAt = job.CompletedAt
                    }
                };

                logger.LogInformation("Returning job result for token: {JobToken}. Success rate: {SuccessRate}", jobToken, successRate);
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting job result for token: {JobToken}", jobToken);

                if (ex.Message.Contains("connection") || ex.Message.Contains("database"))
                {
                    return Results.Problem("Database connection error. Please try again later.");
                }
                else
                {
                    return Results.Problem("Internal server error during result retrieval");
                }
            }
        });

        // Queue overview (debug)
        grp.MapGet("/queue", (JobRuntimeMonitor monitor, ILogger<Program> logger) =>
        {
            var snapshot = monitor.Snapshot();
            return Results.Ok(snapshot);
        });

        // Clear queue (dangerous). By default clears only pending and in-progress.
        // Pass ?all=true to delete all jobs regardless of status.
        grp.MapDelete("/queue", async (bool? all, AppDb db, JobRuntimeMonitor monitor, ILogger<Program> logger, ClaimsPrincipal user) =>
        {
            try
            {
                var uid = user.FindFirstValue("uid") ?? user.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? throw new InvalidOperationException("User ID not found");

                var deleteAll = all == true;
                var query = db.ExtractionJobs.Where(j => j.OwnerId == uid);
                if (!deleteAll)
                {
                    query = query.Where(j => j.Status == JobStatus.Pending || j.Status == JobStatus.InProgress);
                }

                var jobs = await query.ToListAsync();
                var count = jobs.Count;
                if (count == 0)
                {
                    return Results.Ok(new { deleted = 0 });
                }

                db.ExtractionJobs.RemoveRange(jobs);
                await db.SaveChangesAsync();

                logger.LogWarning("Deleted {Count} job(s) from queue (all={All})", count, deleteAll);
                // Also clear runtime mirrors
                monitor.ClearRuntime();
                return Results.Ok(new { deleted = count, all = deleteAll });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error clearing queue");
                return Results.Problem("Failed to clear queue");
            }
        });
    }
}