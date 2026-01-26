using System.Text;
using Fuzn.FluentHttp.TestApi.Models;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class AuthenticationEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/auth/bearer", (HttpContext context) =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return Results.Unauthorized();
            
            var token = authHeader["Bearer ".Length..];
            return Results.Ok(new BearerAuthResponse { Authenticated = true, TokenReceived = token });
        });

        app.MapGet("/api/auth/basic", (HttpContext context) =>
        {
            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Basic "))
                return Results.Unauthorized();
            
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(authHeader["Basic ".Length..]));
            var parts = credentials.Split(':');
            return Results.Ok(new BasicAuthResponse { Authenticated = true, Username = parts[0], Password = parts.Length > 1 ? parts[1] : "" });
        });

        app.MapGet("/api/auth/apikey", (HttpContext context) =>
        {
            var apiKey = context.Request.Headers["X-API-Key"].ToString();
            if (string.IsNullOrEmpty(apiKey))
                return Results.Unauthorized();
            
            return Results.Ok(new ApiKeyAuthResponse { Authenticated = true, ApiKey = apiKey });
        });

        app.MapGet("/api/auth/apikey-custom", (HttpContext context) =>
        {
            var apiKey = context.Request.Headers["X-My-Api-Key"].ToString();
            if (string.IsNullOrEmpty(apiKey))
                return Results.Unauthorized();
            
            return Results.Ok(new ApiKeyAuthResponse { Authenticated = true, ApiKey = apiKey });
        });
    }
}
