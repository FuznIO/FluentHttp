using Fuzn.FluentHttp.TestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class UtilityEndpoints
{
    public static void MapUtilityEndpoints(this WebApplication app)
    {
        // Timeout Testing
        app.MapGet("/api/delay/{milliseconds}", async (int milliseconds) =>
        {
            await Task.Delay(milliseconds);
            return Results.Ok(new { delayed = milliseconds });
        });

        // Echo Endpoint - Returns everything about the request
        app.MapMethods("/api/echo", ["GET", "POST", "PUT", "DELETE", "PATCH"], async (HttpContext context) =>
        {
            string? body = null;
            if (context.Request.ContentLength > 0)
            {
                using var reader = new StreamReader(context.Request.Body);
                body = await reader.ReadToEndAsync();
            }
            
            return Results.Ok(new
            {
                method = context.Request.Method,
                path = context.Request.Path.Value,
                queryString = context.Request.QueryString.Value,
                query = context.Request.Query.ToDictionary(q => q.Key, q => q.Value.ToString()),
                headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                cookies = context.Request.Cookies.ToDictionary(c => c.Key, c => c.Value),
                contentType = context.Request.ContentType,
                body
            });
        });

        // Deserialization Testing
        app.MapPost("/api/deserialize/person", ([FromBody] PersonDto person) =>
            Results.Ok(new DeserializeResponse { Received = person, Type = "PersonDto" }));

        app.MapGet("/api/deserialize/person", () =>
            Results.Ok(new PersonDto { Id = 1, Name = "John Doe", Email = "john@example.com", Age = 30 }));

        app.MapGet("/api/deserialize/list", () =>
            Results.Ok(new[]
            {
                new PersonDto { Id = 1, Name = "John Doe", Email = "john@example.com", Age = 30 },
                new PersonDto { Id = 2, Name = "Jane Doe", Email = "jane@example.com", Age = 25 }
            }));
    }
}
