using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using play_app_api.Configuration;
using play_app_api.Services;
using System.Security.Claims;

namespace play_app_api.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        AppConfiguration appConfig)
    {
        // Add HTTP client for OpenRouter
        services.AddHttpClient("openrouter", client =>
        {
            client.BaseAddress = new Uri("https://openrouter.ai/api/v1/");
            client.Timeout = TimeSpan.FromSeconds(appConfig.HttpClientTimeoutSeconds);
            if (!string.IsNullOrEmpty(appConfig.OpenRouterApiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {appConfig.OpenRouterApiKey}");
            }
            if (!string.IsNullOrEmpty(appConfig.AppReferer))
            {
                client.DefaultRequestHeaders.Add("Referer", appConfig.AppReferer);
            }
            if (!string.IsNullOrEmpty(appConfig.AppTitle))
            {
                client.DefaultRequestHeaders.Add("X-Title", appConfig.AppTitle);
            }
        });

        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy(appConfig.CorsPolicyName, policy =>
                policy.WithOrigins(appConfig.AllowedOrigins)
                      .AllowAnyHeader()
                      .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                      .AllowCredentials());
        });

        // Add API Explorer
        services.AddEndpointsApiExplorer();

        // Add HTTP Context Accessor
        services.AddHttpContextAccessor();

        // Add Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = appConfig.Authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = appConfig.Authority,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = ctx =>
                    {
                        var sub = ctx.Principal?.FindFirst("sub")?.Value;
                        if (!string.IsNullOrEmpty(sub))
                        {
                            var id = (ClaimsIdentity)ctx.Principal!.Identity!;
                            id.AddClaim(new Claim("uid", sub));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        // Add Authorization
        services.AddAuthorization(options =>
        {
            options.AddPolicy("UserOnly", policy => policy.RequireClaim("uid"));
        });

        // Add Background Services
        services.AddHostedService<ExtractionJobService>();
        services.AddSingleton<JobRuntimeMonitor>();

        return services;
    }

    public static IServiceCollection ConfigureLogging(
        this IServiceCollection services,
        ILoggingBuilder loggingBuilder,
        AppConfiguration appConfig)
    {
        // Elevate logging to Information by default; make Debug optional via env
        var minLevel = Environment.GetEnvironmentVariable("LOG_LEVEL") ?? appConfig.LogLevel;
        loggingBuilder.ClearProviders();
        loggingBuilder.AddConsole();
        loggingBuilder.SetMinimumLevel(Enum.TryParse<LogLevel>(minLevel, true, out var lv) ? lv : LogLevel.Information);

        return services;
    }
} 