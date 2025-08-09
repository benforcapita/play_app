using Microsoft.EntityFrameworkCore;
using play_app_api.ApiEndpoints;
using play_app_api.Data;
using play_app_api.Services;
using System.Web;

namespace play_app_api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        const string SwaCors = "SwaCors";
        var allowedOrigins = new[]
    {
    "https://yellow-coast-0848b7a03.2.azurestaticapps.net",
    "http://localhost:4200",
    "http://localhost:4300"
};
var openRouterKey    = builder.Configuration["OPENROUTER_API_KEY"] ?? "";
var openRouterModel  = builder.Configuration["OPENROUTER_MODEL"]  ?? "google/gemini‑2.5‑pro"; // choose per models page
var appReferer       = builder.Configuration["APP_REFERER"]; // optional
var appTitle         = builder.Configuration["APP_TITLE"];   // optional
        builder.Services.AddHttpClient("openrouter", client =>
        {
            client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
            // Increase timeout to accommodate OCR-heavy PDFs
            client.Timeout = TimeSpan.FromSeconds(180);
            if (!string.IsNullOrEmpty(openRouterKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openRouterKey}");
            }
            if (!string.IsNullOrEmpty(appReferer))
            {
                client.DefaultRequestHeaders.Add("Referer", appReferer);
            }
            if (!string.IsNullOrEmpty(appTitle))
            {
                client.DefaultRequestHeaders.Add("X-Title", appTitle);
            }
        });
        builder.Services.AddCors(options =>
        {
            options.AddPolicy(SwaCors, policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .AllowCredentials()); // only if you truly need cookies/auth
        });

        builder.Services.AddEndpointsApiExplorer();

        // Only register the AppDb provider if tests or host haven't already registered options
        var appDbAlreadyRegistered = builder.Services.Any(sd => sd.ServiceType == typeof(DbContextOptions<AppDb>));
        if (!appDbAlreadyRegistered)
        {
            // Get connection string - check environment variable first, then appsettings
            var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                                      Environment.GetEnvironmentVariable("database-url") ??
                                      builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                                      throw new InvalidOperationException("No database connection string configured");
            
            // Parse and fix malformed connection string if it's a URL
            var connectionString = ParseAndFixConnectionString(rawConnectionString);
            
            // Log the original and parsed connection strings for debugging
            var tempLogger2 = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            tempLogger2.LogInformation("Original connection string type: {Type}", 
                rawConnectionString.StartsWith("postgresql://") ? "URL" : "Connection String");
            if (rawConnectionString.StartsWith("postgresql://"))
            {
                tempLogger2.LogInformation("Converting URL format to connection string format");
                tempLogger2.LogInformation("Original URL: {Url}", rawConnectionString);
            }
            
            // Log the connection string (without password for security)
            var tempLogger = builder.Services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
            var connectionStringForLogging = connectionString.Contains("Password=") 
                ? connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=***"
                : connectionString;
            tempLogger.LogInformation("Using connection string: {ConnectionString}", connectionStringForLogging);
            
            // Log which connection string was used
            if (Environment.GetEnvironmentVariable("DATABASE_URL") != null)
            {
                tempLogger.LogInformation("Source: DATABASE_URL environment variable");
            }
            else if (Environment.GetEnvironmentVariable("database-url") != null)
            {
                tempLogger.LogInformation("Source: database-url environment variable");
            }
            else
            {
                tempLogger.LogInformation("Source: appsettings.json");
            }
            
            // Configure database based on connection string type
            if (connectionString.StartsWith("Data Source=") || connectionString.StartsWith("DataSource="))
            {
                // SQLite connection string
                builder.Services.AddDbContext<AppDb>(opt =>
                    opt.UseSqlite(connectionString));
            }
            else
            {
            // PostgreSQL connection string with transient retry and sane timeouts
            builder.Services.AddDbContext<AppDb>(opt =>
                opt.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(2), errorCodesToAdd: null);
                    npgsql.CommandTimeout(30);
                }));
            }
        }

        // Register the DbContext
        // Register background service for processing extraction jobs
        builder.Services.AddHostedService<ExtractionJobService>();
        builder.Services.AddSingleton<JobRuntimeMonitor>();

        // Elevate logging to Information by default; make Debug optional via env
        var minLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information";
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(Enum.TryParse<LogLevel>(minLevel, true, out var lv) ? lv : LogLevel.Information);

        var app = builder.Build();
        
        // Configure logging
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting up");
        logger.LogInformation("OpenRouter API Key configured: {HasKey}", !string.IsNullOrEmpty(openRouterKey));
        logger.LogInformation("OpenRouter Model: {Model}", openRouterModel);
        logger.LogInformation("Allowed origins: {Origins}", string.Join(", ", allowedOrigins));
        
        // Log environment variables for debugging
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        logger.LogInformation("DATABASE_URL environment variable: {HasValue}", !string.IsNullOrEmpty(databaseUrl));
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            logger.LogInformation("DATABASE_URL length: {Length}", databaseUrl.Length);
        }
        
        // Test database connection only if AppDb has been registered by this host or tests
        try
        {
            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetService<AppDb>();
            if (dbContext != null)
            {
                var canConnect = await dbContext.Database.CanConnectAsync();
                logger.LogInformation("Database connection test successful: {CanConnect}", canConnect);
            }
            else
            {
                logger.LogInformation("Skipping database connection test (AppDb not registered in this host)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database connection test failed");
            // Don't throw here - let the app start but log the issue
        }
        
        app.UseCors(SwaCors);

        // Example health/ping endpoints
        app.MapGet("/ping", (ILogger<Program> logger) => {
            logger.LogInformation("Ping endpoint called at {Timestamp}", DateTimeOffset.UtcNow);
            return Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow });
        });
        app.MapGet("/health", (ILogger<Program> logger) => {
            logger.LogInformation("Health endpoint called at {Timestamp}", DateTimeOffset.UtcNow);
            return Results.Ok("OK");
        });
        
        // Debug endpoint to check environment variables (remove in production)
        app.MapGet("/debug/env", (ILogger<Program> logger) => {
            logger.LogInformation("Debug endpoint called at {Timestamp}", DateTimeOffset.UtcNow);
            
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var hasDatabaseUrl = !string.IsNullOrEmpty(databaseUrl);
            var databaseUrlLength = databaseUrl?.Length ?? 0;
            var isUrlFormat = databaseUrl?.StartsWith("postgresql://") ?? false;
            
            var databaseUrlLowercase = Environment.GetEnvironmentVariable("database-url");
            var hasDatabaseUrlLowercase = !string.IsNullOrEmpty(databaseUrlLowercase);
            
            // Test the connection string parsing
            string parsedConnectionString = null;
            if (!string.IsNullOrEmpty(databaseUrl))
            {
                try
                {
                    parsedConnectionString = ParseAndFixConnectionString(databaseUrl);
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
                    parsedConnectionString = ParseAndFixConnectionString(databaseUrlLowercase);
                }
                catch (Exception ex)
                {
                    parsedConnectionString = $"Error: {ex.Message}";
                }
            }
            
            // Also test with the appsettings connection string
            string appsettingsConnectionString = null;
            try
            {
                var appsettingsRaw = builder.Configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(appsettingsRaw))
                {
                    appsettingsConnectionString = ParseAndFixConnectionString(appsettingsRaw);
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
        app.MapCharacterEndpoints();
        app.MapExtractionEndpoints(openRouterModel);

        app.Run();
    }
    
    private static string ParseAndFixConnectionString(string rawConnectionString)
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
