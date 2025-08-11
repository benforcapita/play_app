namespace play_app_api.Configuration;

public class AppConfiguration
{
    public string[] AllowedOrigins { get; set; } = new[]
    {
        "https://yellow-coast-0848b7a03.2.azurestaticapps.net",
        "http://localhost:4200",
        "http://localhost:4300"
    };

    public string OpenRouterApiKey { get; set; } = string.Empty;
    public string OpenRouterModel { get; set; } = "google/gemini‑2.5‑pro";
    public string? AppReferer { get; set; }
    public string? AppTitle { get; set; }
    public string SupabaseProjectId { get; set; } = string.Empty;
    public string SupabaseAnonKey { get; set; } = string.Empty;
    public string SupabaseUrl => $"https://{SupabaseProjectId}.supabase.co";
    public string Authority => $"{SupabaseUrl}/auth/v1";
    public string CorsPolicyName { get; set; } = "SwaCors";
    public string LogLevel { get; set; } = "Information";
    public int HttpClientTimeoutSeconds { get; set; } = 180;
    public int DatabaseMaxRetryCount { get; set; } = 5;
    public int DatabaseMaxRetryDelaySeconds { get; set; } = 2;
    public int DatabaseCommandTimeoutSeconds { get; set; } = 30;
} 