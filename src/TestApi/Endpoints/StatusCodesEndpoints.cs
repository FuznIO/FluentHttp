namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class StatusCodesEndpoints
{
    public static void MapStatusCodesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/status/{code}", (int code) =>
        {
            return Results.StatusCode(code);
        });

        app.MapGet("/api/status/ok", () => 
            Results.Ok(new { status = "OK" }));
        
        app.MapGet("/api/status/created", () => 
            Results.Created("/api/resource/1", new { status = "Created", id = 1 }));
        
        app.MapGet("/api/status/nocontent", () => 
            Results.NoContent());
        
        app.MapGet("/api/status/badrequest", () => 
            Results.BadRequest(new { error = "Bad request" }));
        
        app.MapGet("/api/status/unauthorized", () => 
            Results.Unauthorized());
        
        app.MapGet("/api/status/notfound", () => 
            Results.NotFound(new { error = "Not found" }));
        
        app.MapGet("/api/status/error", () => 
            Results.StatusCode(500));
    }
}
