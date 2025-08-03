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

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(SwaCors, policy =>
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .AllowCredentials()); // only if you truly need cookies/auth
        });

        builder.Services.AddEndpointsApiExplorer();

        var app = builder.Build();
        app.UseCors(SwaCors);

        // Example health/ping endpoints
        app.MapGet("/ping", () => Results.Ok(new { ok = true, at = DateTimeOffset.UtcNow }));
        app.MapGet("/health", () => Results.Ok("OK"));

        app.Run();
    }
}
