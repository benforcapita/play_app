using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace play_app_api.ApiEndpoints;

public static class AuthEndpoints
{
    // DTOs for requests
    public record LoginRequest(string Email, string Password);
    public record SignupRequest(string Email, string Password);
    public record RefreshRequest(string RefreshToken);
    public record ResetPasswordRequest(string Email);

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // No auth required for auth endpoints themselves
        var grp = app.MapGroup("/api/auth");

        // POST /api/auth/signup -> returns { access_token, refresh_token, expires_in, token_type, user } (TEST)
        grp.MapPost("/signup", async (SignupRequest req, IHttpClientFactory httpFactory, ILogger<Program> logger) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest(new { message = "email and password are required" });

            var http = httpFactory.CreateClient("supabase");
            
            // Debug logging
            logger.LogInformation("Signup attempt for email: {Email}", req.Email);
            logger.LogInformation("Supabase client base address: {BaseAddress}", http.BaseAddress);
            logger.LogInformation("Supabase client headers: {Headers}", string.Join(", ", http.DefaultRequestHeaders.Select(h => $"{h.Key}={h.Value.FirstOrDefault()}")));

            var url = "auth/v1/signup";
            var payload = new
            {
                email = req.Email,
                password = req.Password
            };

            logger.LogInformation("Making signup request to: {Url}", url);
            logger.LogInformation("Signup request payload: {Payload}", System.Text.Json.JsonSerializer.Serialize(payload));

            var resp = await http.PostAsJsonAsync(url, payload);
            var text = await resp.Content.ReadAsStringAsync();

            logger.LogInformation("Signup response status: {Status}", (int)resp.StatusCode);
            logger.LogInformation("Signup response body: {Body}", text);

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("Supabase signup failed ({Status}): {Body}", (int)resp.StatusCode, text);
                // Forward a minimal error message
                return Results.StatusCode((int)resp.StatusCode);
            }

            // Pass through the JSON from Supabase
            var json = JsonDocument.Parse(text).RootElement.Clone();
            // Typically contains: access_token, refresh_token, expires_in, token_type, user
            return Results.Ok(json);
        });

        // POST /api/auth/reset-password -> sends password reset email
        grp.MapPost("/reset-password", async (ResetPasswordRequest req, IHttpClientFactory httpFactory, ILogger<Program> logger) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email))
                return Results.BadRequest(new { message = "email is required" });

            var http = httpFactory.CreateClient("supabase");
            
            logger.LogInformation("Password reset request for email: {Email}", req.Email);

            var url = "auth/v1/recover";
            var payload = new
            {
                email = req.Email
            };

            logger.LogInformation("Making password reset request to: {Url}", url);

            var resp = await http.PostAsJsonAsync(url, payload);
            var text = await resp.Content.ReadAsStringAsync();

            logger.LogInformation("Password reset response status: {Status}", (int)resp.StatusCode);
            logger.LogInformation("Password reset response body: {Body}", text);

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("Supabase password reset failed ({Status}): {Body}", (int)resp.StatusCode, text);
                return Results.StatusCode((int)resp.StatusCode);
            }

            return Results.Ok(new { message = "Password reset email sent" });
        });

        // POST /api/auth/login  -> returns { access_token, refresh_token, expires_in, token_type, user }
        grp.MapPost("/login", async (LoginRequest req, IHttpClientFactory httpFactory, ILogger<Program> logger) =>
        {
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest(new { message = "email and password are required" });

            var http = httpFactory.CreateClient("supabase");
            
            // Debug logging
            logger.LogInformation("Login attempt for email: {Email}", req.Email);
            logger.LogInformation("Supabase client base address: {BaseAddress}", http.BaseAddress);
            logger.LogInformation("Supabase client headers: {Headers}", string.Join(", ", http.DefaultRequestHeaders.Select(h => $"{h.Key}={h.Value.FirstOrDefault()}")));

            var url = "auth/v1/token?grant_type=password";
            var payload = new
            {
                email = req.Email,
                password = req.Password
            };

            logger.LogInformation("Making request to: {Url}", url);
            logger.LogInformation("Request payload: {Payload}", System.Text.Json.JsonSerializer.Serialize(payload));

            var resp = await http.PostAsJsonAsync(url, payload);
            var text = await resp.Content.ReadAsStringAsync();

            logger.LogInformation("Response status: {Status}", (int)resp.StatusCode);
            logger.LogInformation("Response body: {Body}", text);

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("Supabase login failed ({Status}): {Body}", (int)resp.StatusCode, text);
                // Forward a minimal error message
                return Results.StatusCode((int)resp.StatusCode);
            }

            // Pass through the JSON from Supabase
            var json = JsonDocument.Parse(text).RootElement.Clone();
            // Typically contains: access_token, refresh_token, expires_in, token_type, user
            return Results.Ok(json);
        });

        // POST /api/auth/refresh -> returns new { access_token, refresh_token, ... }
        grp.MapPost("/refresh", async (RefreshRequest req, IHttpClientFactory httpFactory, ILogger<Program> logger) =>
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
                return Results.BadRequest(new { message = "refresh_token is required" });

            var http = httpFactory.CreateClient("supabase");

            var url = "auth/v1/token?grant_type=refresh_token";
            var payload = new
            {
                refresh_token = req.RefreshToken
            };

            var resp = await http.PostAsJsonAsync(url, payload);
            var text = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogWarning("Supabase refresh failed ({Status}): {Body}", (int)resp.StatusCode, text);
                return Results.StatusCode((int)resp.StatusCode);
            }

            var json = JsonDocument.Parse(text).RootElement.Clone();
            return Results.Ok(json);
        });
    }
}
