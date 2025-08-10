using play_app_api.Configuration;

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
        logger.LogInformation("OpenRouter API Key configured: {HasKey}", !string.IsNullOrEmpty(appConfig.OpenRouterApiKey));
        logger.LogInformation("OpenRouter Model: {Model}", appConfig.OpenRouterModel);
        logger.LogInformation("Supabase Project ID: {ProjectId}", appConfig.SupabaseProjectId);
        logger.LogInformation("Allowed origins: {Origins}", string.Join(", ", appConfig.AllowedOrigins));
        
        // Log environment variables for debugging
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        logger.LogInformation("DATABASE_URL environment variable: {HasValue}", !string.IsNullOrEmpty(databaseUrl));
        if (!string.IsNullOrEmpty(databaseUrl))
        {
            logger.LogInformation("DATABASE_URL length: {Length}", databaseUrl.Length);
        }
        
        // Test database connection
        await DatabaseConfiguration.TestDatabaseConnectionAsync(app.Services, logger);
        
        // Configure middleware and endpoints
        app.ConfigureMiddleware(appConfig, logger);

        app.Run();
    }
}
