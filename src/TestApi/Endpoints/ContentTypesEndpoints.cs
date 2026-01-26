using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class ContentTypesEndpoints
{
    public static void MapContentTypesEndpoints(this WebApplication app)
    {
        app.MapPost("/api/content/json", ([FromBody] JsonElement body) =>
            Results.Ok(new { contentType = "application/json", received = body }));

        app.MapPost("/api/content/text", async (HttpContext context) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var text = await reader.ReadToEndAsync();
            return Results.Ok(new { contentType = "text/plain", received = text });
        });

        app.MapPost("/api/content/form", async (HttpContext context) =>
        {
            var form = await context.Request.ReadFormAsync();
            var fields = form.ToDictionary(f => f.Key, f => f.Value.ToString());
            return Results.Ok(new { contentType = "application/x-www-form-urlencoded", fields });
        });
    }
}
