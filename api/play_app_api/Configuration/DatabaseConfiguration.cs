using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using play_app_api.Data;
using System.Web;

namespace play_app_api.Configuration;

public static class DatabaseConfiguration
{
    public static IServiceCollection AddDatabaseServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Only register the AppDb provider if tests or host haven't already registered options
        var appDbAlreadyRegistered = services.Any(sd => sd.ServiceType == typeof(DbContextOptions<AppDb>));
        if (appDbAlreadyRegistered)
        {
            return services;
        }

        // Get connection string - check environment variable first, then appsettings
        var rawConnectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                                  Environment.GetEnvironmentVariable("database-url") ??
                                  configuration.GetConnectionString("DefaultConnection") ?? 
                                  throw new InvalidOperationException("No database connection string configured");
        
        // Parse and fix malformed connection string if it's a URL
        var connectionString = ParseAndFixConnectionString(rawConnectionString);
        
        // Log the original and parsed connection strings for debugging
        Console.WriteLine("Original connection string type: {0}", 
            rawConnectionString.StartsWith("postgresql://") ? "URL" : "Connection String");
        if (rawConnectionString.StartsWith("postgresql://"))
        {
            Console.WriteLine("Converting URL format to connection string format");
            Console.WriteLine("Original URL: {0}", rawConnectionString);
        }
        
        // Log the connection string (without password for security)
        var connectionStringForLogging = connectionString.Contains("Password=") 
            ? connectionString.Substring(0, connectionString.IndexOf("Password=")) + "Password=***"
            : connectionString;
        Console.WriteLine("Using connection string: {0}", connectionStringForLogging);
        
        // Log which connection string was used
        if (Environment.GetEnvironmentVariable("DATABASE_URL") != null)
        {
            Console.WriteLine("Source: DATABASE_URL environment variable");
        }
        else if (Environment.GetEnvironmentVariable("database-url") != null)
        {
            Console.WriteLine("Source: database-url environment variable");
        }
        else
        {
            Console.WriteLine("Source: appsettings.json");
        }
        
        // Configure database based on connection string type
        if (connectionString.StartsWith("Data Source=") || connectionString.StartsWith("DataSource="))
        {
            // SQLite connection string
            services.AddDbContext<AppDb>(opt =>
                opt.UseSqlite(connectionString));
        }
        else
        {
            // PostgreSQL connection string with transient retry and sane timeouts
            services.AddDbContext<AppDb>(opt =>
                opt.UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(2), errorCodesToAdd: null);
                    npgsql.CommandTimeout(30);
                }));
        }

        return services;
    }

    public static async Task TestDatabaseConnectionAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
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
                var query = HttpUtility.ParseQueryString(uri.Query);
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