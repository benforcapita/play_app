using Microsoft.Extensions.Configuration;

namespace play_app_api.Configuration;

public static class ConfigurationBuilder
{
    public static AppConfiguration BuildAppConfiguration(IConfiguration configuration)
    {
        return new AppConfiguration
        {
            OpenRouterApiKey = configuration["OPENROUTER_API_KEY"] ?? "",
            OpenRouterModel = configuration["OPENROUTER_MODEL"] ?? "google/gemini‑2.5‑pro",
            AppReferer = configuration["APP_REFERER"],
            AppTitle = configuration["APP_TITLE"],
            SupabaseAnonKey = configuration["supabase-anon-key"] ?? "",
            SupabaseProjectId = configuration["supabase-project-id"] ?? 
                throw new InvalidOperationException("SUPABASE_PROJECT_ID is not set"),
            LogLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? "Information"
        };
    }
} 