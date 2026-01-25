namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class HeadersEndpoints
{
    public static void MapHeadersEndpoints(this WebApplication app)
    {
        app.MapGet("/api/headers/echo", (HttpContext context) =>
        {
            var headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            return Results.Ok(new { headers });
        });

        app.MapGet("/api/headers/custom", (HttpContext context) =>
        {
            var customHeader = context.Request.Headers["X-Custom-Header"].ToString();
            var userAgent = context.Request.Headers["User-Agent"].ToString();
            return Results.Ok(new { customHeader, userAgent });
        });

        app.MapGet("/api/headers/accept", (HttpContext context) =>
        {
            var accept = context.Request.Headers["Accept"].ToString();
            return Results.Ok(new { accept });
        });
    }
}
