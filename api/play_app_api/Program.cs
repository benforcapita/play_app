namespace play_app_api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Configure the application to listen on port 8080 for container deployments
        // This is the port that Azure Container Apps expects
        builder.WebHost.UseUrls("http://0.0.0.0:8080");
        
        var app = builder.Build();

        app.MapGet("/ping", () => "pong");
        
        // Health check endpoint for Azure Container Apps startup probe
        app.MapGet("/health", () => "healthy");

        app.Run();
    }
}
