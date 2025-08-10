using Microsoft.Extensions.Logging;
using play_app_api.ApiEndpoints;
using play_app_api.Configuration;

namespace play_app_api.Configuration;

public static class MiddlewareConfiguration
{
    public static WebApplication ConfigureMiddleware(
        this WebApplication app,
        AppConfiguration appConfig,
        ILogger logger)
    {
        // Configure CORS
        app.UseCors(appConfig.CorsPolicyName);

        // Map health check endpoints
        app.MapGet("/ping", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Ping endpoint called at {Timestamp}", DateTimeOffset.UtcNow);
            return Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow });
        });

        app.MapGet("/health", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Health endpoint called at {Timestamp}", DateTimeOffset.UtcNow);
            return Results.Ok("OK");
        });

        // Map debug endpoint (remove in production)
        app.MapGet("/debug/env", (ILogger<Program> logger) =>
        {
            logger.LogInformation("Debug endpoint called at {Timestamp}", DateTimeOffset.UtcNow);
            
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var hasDatabaseUrl = !string.IsNullOrEmpty(databaseUrl);
            var databaseUrlLength = databaseUrl?.Length ?? 0;
            var isUrlFormat = databaseUrl?.StartsWith("postgresql://") ?? false;
            
            var databaseUrlLowercase = Environment.GetEnvironmentVariable("database-url");
            var hasDatabaseUrlLowercase = !string.IsNullOrEmpty(databaseUrlLowercase);
            
            // Test the connection string parsing
            string? parsedConnectionString = null;
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                try
                {
                    parsedConnectionString = ParseConnectionString(databaseUrl);
                }
                catch (Exception ex)
                {
                    parsedConnectionString = $"Error: {ex.Message}";
                }
            }
            else if (!string.IsNullOrEmpty(databaseUrlLowercase))
            {
                try
                {
                    parsedConnectionString = ParseConnectionString(databaseUrlLowercase);
                }
                catch (Exception ex)
                {
                    parsedConnectionString = $"Error: {ex.Message}";
                }
            }
            
            // Also test with the appsettings connection string
            string? appsettingsConnectionString = null;
            try
            {
                var appsettingsRaw = app.Configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(appsettingsRaw))
                {
                    appsettingsConnectionString = ParseConnectionString(appsettingsRaw);
                }
            }
            catch (Exception ex)
            {
                appsettingsConnectionString = $"Error: {ex.Message}";
            }
            
            return Results.Ok(new { 
                hasDatabaseUrl,
                databaseUrlLength,
                isUrlFormat,
                hasDatabaseUrlLowercase,
                parsedConnectionString,
                appsettingsConnectionString,
                timestamp = DateTimeOffset.UtcNow
            });
        });

        // Map application endpoints
        app.MapCharacterEndpoints();
        app.MapExtractionEndpoints(appConfig.OpenRouterModel);

        return app;
    }

    private static string ParseConnectionString(string rawConnectionString)
    {
        // If it's already a proper connection string (contains semicolons), return as-is
        if (rawConnectionString.Contains(";"))
        {
            return rawConnectionString;
        }
        
        // If it's a URL format, parse and convert it
        if (rawConnectionString.StartsWith("postgresql://"))
        {
            try
            {
                var uri = new Uri(rawConnectionString);
                var builder = new System.Data.Common.DbConnectionStringBuilder();
                
                // Extract components from the URL
                builder["Host"] = uri.Host;
                
                // Only set port if it's valid (not -1)
                if (uri.Port > 0)
                {
                    builder["Port"] = uri.Port;
                }
                else
                {
                    // Use default PostgreSQL port if not specified
                    builder["Port"] = 5432;
                }
                
                builder["Database"] = uri.AbsolutePath.TrimStart('/');
                builder["Username"] = uri.UserInfo.Split(':')[0];
                builder["Password"] = uri.UserInfo.Split(':')[1];
                
                // Handle query parameters
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                if (query["sslmode"] != null && !string.IsNullOrEmpty(query["sslmode"]))
                {
                    builder["SSL Mode"] = query["sslmode"];
                }
                else
                {
                    // Default SSL mode for Neon
                    builder["SSL Mode"] = "Require";
                }
                
                // Note: channel_binding is not supported by Npgsql, so we ignore it
                if (query["channel_binding"] != null)
                {
                    Console.WriteLine($"Ignoring channel_binding parameter: {query["channel_binding"]}");
                }
                
                var result = builder.ConnectionString;
                Console.WriteLine($"Parsed connection string: {result}");
                return result;
            }
            catch (Exception ex)
            {
                // If parsing fails, log and return the original
                Console.WriteLine($"Failed to parse connection string URL: {ex.Message}");
                return rawConnectionString;
            }
        }
        
        return rawConnectionString;
    }
} 