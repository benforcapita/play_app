using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace play_app_api.Services;

public class SupabaseJwtHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly string _supabaseProjectId;
    private readonly string _supabaseJwtSecret;

    public SupabaseJwtHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _supabaseProjectId = configuration["supabase-project-id"] ?? "";
        _supabaseJwtSecret = configuration["supabase-jwt-secret"] ?? configuration["SUPABASE_JWT_SECRET"] ?? "";
        
        Logger.LogInformation("SupabaseJwtHandler initialized");
        Logger.LogInformation("Project ID: {ProjectId}", _supabaseProjectId);
        Logger.LogInformation("JWT Secret configured: {HasSecret}", !string.IsNullOrEmpty(_supabaseJwtSecret));
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Logger.LogInformation("JWT Handler: HandleAuthenticateAsync called");
        
        if (!Request.Headers.ContainsKey("Authorization"))
        {
            Logger.LogWarning("JWT Handler: No Authorization header found for {Path}", Request.Path);
            return Task.FromResult(AuthenticateResult.Fail("Authorization header not found."));
        }

        var authHeader = Request.Headers["Authorization"].ToString();
        Logger.LogInformation("JWT Handler: Authorization header: {Header}", authHeader);
        
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogWarning("JWT Handler: No Bearer token found in header");
            return Task.FromResult(AuthenticateResult.Fail("Bearer token not found."));
        }

        var token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            var issuer = $"https://{_supabaseProjectId}.supabase.co/auth/v1";

            if (string.IsNullOrWhiteSpace(_supabaseJwtSecret))
            {
                Logger.LogError("JWT secret not configured");
                return Task.FromResult(AuthenticateResult.Fail("Server configuration error."));
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_supabaseJwtSecret));

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                NameClaimType = "sub",
                RoleClaimType = "role"
            };

            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
            var principal = handler.ValidateToken(token, parameters, out var validatedToken);

            string? sub = principal.FindFirst("sub")?.Value
                           ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(sub) && validatedToken is JwtSecurityToken jwt)
            {
                sub = jwt.Subject ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            }

            if (string.IsNullOrEmpty(sub))
                return Task.FromResult(AuthenticateResult.Fail("Invalid token: no user id."));

            var identity = (ClaimsIdentity)principal.Identity!;
            if (!principal.HasClaim(c => c.Type == "uid"))
                identity.AddClaim(new Claim("uid", sub));

            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "JWT validation failed for token: {TokenStart}... Path: {Path}", 
                token.Length > 10 ? token.Substring(0, 10) : token, Request.Path);
            return Task.FromResult(AuthenticateResult.Fail($"Invalid token: {ex.Message}"));
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = "Bearer";
        Response.StatusCode = 401;
        
        // Return a proper JSON error response
        var errorResponse = new
        {
            error = "Unauthorized",
            message = "Authentication required. Please provide a valid Bearer token.",
            statusCode = 401
        };
        
        Response.ContentType = "application/json";
        var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        return Response.WriteAsync(json);
    }

    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 403;
        
        // Return a proper JSON error response for forbidden access
        var errorResponse = new
        {
            error = "Forbidden", 
            message = "Access denied. You do not have permission to access this resource.",
            statusCode = 403
        };
        
        Response.ContentType = "application/json";
        var json = System.Text.Json.JsonSerializer.Serialize(errorResponse);
        return Response.WriteAsync(json);
    }
} 