using Microsoft.Extensions.Configuration;

namespace play_app_api.Configuration;

public static class ConfigurationBuilder
{
    public static AppConfiguration BuildAppConfiguration(IConfiguration configuration)
    {
        var jwtSecret = configuration["supabase-jwt-secret"] ?? configuration["SUPABASE_JWT_SECRET"] ?? "";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        Console.WriteLine($"ConfigurationBuilder: Environment: {environment}");
        Console.WriteLine($"ConfigurationBuilder: JWT Secret configured: {!string.IsNullOrEmpty(jwtSecret)}");
        Console.WriteLine($"ConfigurationBuilder: SUPABASE_JWT_SECRET env var: {Environment.GetEnvironmentVariable("SUPABASE_JWT_SECRET") != null}");
        
        // For testing environments, provide default values to avoid throwing exceptions
        var supabaseProjectId = configuration["supabase-project-id"];
        if (string.IsNullOrEmpty(supabaseProjectId))
        {
            if (environment.Equals("Testing", StringComparison.OrdinalIgnoreCase))
            {
                supabaseProjectId = "test-project";
                Console.WriteLine("ConfigurationBuilder: Using test project ID for Testing environment");
            }
            else
            {
                throw new InvalidOperationException("SUPABASE_PROJECT_ID is not set");
            }
        }
        
        return new AppConfiguration
        {
            OpenRouterApiKey = configuration["OPENROUTER_API_KEY"] ?? "",
            OpenRouterModel = configuration["OPENROUTER_MODEL"] ?? "google/gemini‑2.5‑pro",
            AppReferer = configuration["APP_REFERER"],
            AppTitle = configuration["APP_TITLE"],
            SupabaseAnonKey = configuration["supabase-anon-key"] ?? "",
            SupabaseJwtSecret = jwtSecret,
            SupabaseProjectId = supabaseProjectId,
            LogLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information"
        };
    }
} 