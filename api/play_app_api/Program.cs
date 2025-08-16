using play_app_api.Configuration;
using Microsoft.EntityFrameworkCore;

namespace play_app_api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Build application configuration
        var appConfig = play_app_api.Configuration.ConfigurationBuilder.BuildAppConfiguration(builder.Configuration);

      

        // Configure services
        builder.Services
            .AddApplicationServices(builder.Configuration, appConfig)
            .ConfigureLogging(builder.Logging, appConfig)
            .AddDatabaseServices(builder.Configuration);

        var app = builder.Build();
        
        // Configure logging and startup information
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting up");
        // Print effective log directory
        var logDir = Environment.GetEnvironmentVariable("LOG_DIR") ?? Path.Combine(AppContext.BaseDirectory, "logs");
        try { Directory.CreateDirectory(logDir); } catch { }
        logger.LogInformation("Log directory: {LogDir}", logDir);
        logger.LogInformation("OpenRouter API Key configured: {HasKey}", !string.IsNullOrEmpty(appConfig.OpenRouterApiKey));
        logger.LogInformation("OpenRouter Model: {Model}", appConfig.OpenRouterModel);
        logger.LogInformation("Supabase Project ID: {ProjectId}", appConfig.SupabaseProjectId);
        logger.LogInformation("Allowed origins: {Origins}", string.Join(", ", appConfig.AllowedOrigins));
        logger.LogInformation("Supabase Anon Key: {SupabaseAnonKey}", appConfig.SupabaseAnonKey);
        logger.LogInformation("Supabase Anon Key Length: {Length}", appConfig.SupabaseAnonKey?.Length ?? 0);
        logger.LogInformation("Supabase Anon Key Is Empty: {IsEmpty}", string.IsNullOrEmpty(appConfig.SupabaseAnonKey));

        // Log environment variables for debugging
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        logger.LogInformation("DATABASE_URL environment variable: {HasValue}", !string.IsNullOrEmpty(databaseUrl));
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            logger.LogInformation("DATABASE_URL length: {Length}", databaseUrl.Length);
        }
        
        // Test database connection
        await DatabaseConfiguration.TestDatabaseConnectionAsync(app.Services, logger);
        
        // Force database schema creation (critical for SQLite)
        try
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<play_app_api.Data.AppDb>();
            logger.LogInformation("Database provider: {Provider}", db.Database.ProviderName);
            
            if ((db.Database.ProviderName ?? "").Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                var dbPath = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "(not set)";
                logger.LogInformation("SQLite connection (DATABASE_URL): {DbUrl}", dbPath);
                
                // For SQLite, use EnsureCreated which creates all tables from scratch
                logger.LogInformation("Creating SQLite database schema...");
                await db.Database.EnsureCreatedAsync();
                logger.LogInformation("SQLite database schema created successfully");
                
                // Verify critical tables exist
                await db.Database.OpenConnectionAsync();
                using var cmd = db.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name IN ('ExtractionJobs', 'Characters', 'SectionResults')";
                var tableCount = await cmd.ExecuteScalarAsync();
                logger.LogInformation("Critical tables found: {TableCount}/3", tableCount);
                await db.Database.CloseConnectionAsync();
            }
            else
            {
                // For other databases, use migrations
                await db.Database.MigrateAsync();
                logger.LogInformation("Applied pending EF Core migrations");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to apply EF Core migrations");
            // If using SQLite locally, attempt to reset the DB and re-apply migrations to match the current model
            try
            {
                using var scope2 = app.Services.CreateScope();
                var db2 = scope2.ServiceProvider.GetRequiredService<play_app_api.Data.AppDb>();
                if ((db2.Database.ProviderName ?? "").Contains("Sqlite", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogWarning("SQLite provider detected. Attempting to reset local database and re-apply migrations.");
                    await db2.Database.EnsureDeletedAsync();
                    await db2.Database.MigrateAsync();
                    logger.LogInformation("Local SQLite database reset and migrations applied successfully");
                }
            }
            catch (Exception ex2)
            {
                logger.LogError(ex2, "SQLite reset/migrate attempt failed");
            }
        }
        
        // Configure middleware and endpoints
        app.ConfigureMiddleware(appConfig, logger);

        app.Run();
    }
}
