using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using play_app_api.Data;

namespace play_app_api;

public static class ExtractionEndpoints
{
    public static void MapExtractionEndpoints(this IEndpointRouteBuilder app, string modelName)
    {
        // Start an extraction job and return a token
        app.MapPost("/api/extract/characters", async (HttpRequest req, AppDb db) =>
        {
            if (!req.HasFormContentType) return Results.BadRequest("multipart/form-data expected");
            var form = await req.ReadFormAsync();
            var file = form.Files.SingleOrDefault();
            if (file is null || file.Length == 0) return Results.BadRequest("file required");

            // Validate file type
            var supportedTypes = new[] { "image/png", "image/jpeg", "image/webp", "image/gif", "application/pdf" };
            if (!supportedTypes.Contains(file.ContentType))
                return Results.BadRequest("Unsupported content type. Supported types: PNG, JPEG, WebP, GIF, PDF");

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

            // Create extraction job
            var jobToken = Guid.NewGuid().ToString("N")[..16]; // 16-character token
            var job = new ExtractionJob
            {
                JobToken = jobToken,
                FileName = file.FileName ?? "unknown",
                ContentType = file.ContentType ?? "unknown",
                FileDataUrl = dataUrl,
                Status = JobStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            db.ExtractionJobs.Add(job);
            await db.SaveChangesAsync();

            return Results.Ok(new { jobToken, message = "Extraction job started. Use the job token to check status." });
        });

        // Check job status
        app.MapGet("/api/extract/jobs/{jobToken}/status", async (string jobToken, AppDb db) =>
        {
            var job = await db.ExtractionJobs
                .Include(j => j.SectionResults)
                .Include(j => j.ResultCharacter)
                .FirstOrDefaultAsync(j => j.JobToken == jobToken);

            if (job == null)
                return Results.NotFound(new { message = "Job not found" });

            var response = new
            {
                jobToken = job.JobToken,
                status = job.Status.ToString().ToLower(),
                createdAt = job.CreatedAt,
                startedAt = job.StartedAt,
                completedAt = job.CompletedAt,
                isSuccessful = job.IsSuccessful,
                errorMessage = job.ErrorMessage,
                sectionResults = job.SectionResults.Select(s => new
                {
                    sectionName = s.SectionName,
                    isSuccessful = s.IsSuccessful,
                    errorMessage = s.ErrorMessage,
                    processedAt = s.ProcessedAt
                }).ToList(),
                character = job.ResultCharacter
            };

            return Results.Ok(response);
        });

        // Get completed job result
        app.MapGet("/api/extract/jobs/{jobToken}/result", async (string jobToken, AppDb db) =>
        {
            var job = await db.ExtractionJobs
                .Include(j => j.ResultCharacter)
                .Include(j => j.SectionResults)
                .FirstOrDefaultAsync(j => j.JobToken == jobToken);

            if (job == null)
                return Results.NotFound(new { message = "Job not found" });

            if (job.Status != JobStatus.Completed)
                return Results.BadRequest(new { message = $"Job not completed. Current status: {job.Status}" });

            if (job.ResultCharacter == null)
                return Results.Problem("Job completed but no character result available");

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

            return Results.Ok(response);
        });
    }
}