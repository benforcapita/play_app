using Microsoft.EntityFrameworkCore;
using play_app_api.ApiEndpoints;
using play_app_api.Data;
using play_app_api.Services;

namespace play_app_api;

public class Program
{
    public static void Main(string[] args)
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
        // builder.Services.AddDbContext<AppDb>(opt =>
        //     opt.UseInMemoryDatabase("app"));
        
        builder.Services.AddDbContext<AppDb>(opt =>
            opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured")));

        // Register the DbContext
        // Register background service for processing extraction jobs
        builder.Services.AddHostedService<ExtractionJobService>();

        var app = builder.Build();
        app.UseCors(SwaCors);

        // Example health/ping endpoints
        app.MapGet("/ping", () => Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow }));
        app.MapGet("/health", () => Results.Ok("OK"));
        app.MapCharacterEndpoints();
        app.MapExtractionEndpoints(openRouterModel);

        app.Run();
    }
}
