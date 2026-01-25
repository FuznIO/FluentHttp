using Microsoft.AspNetCore.Mvc;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class HttpMethodsEndpoints
{
    public static void MapHttpMethodsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/methods/get", () => 
            Results.Ok(new { method = "GET", success = true }));

        app.MapPost("/api/methods/post", ([FromBody] object? body) => 
            Results.Ok(new { method = "POST", success = true, receivedBody = body }));

        app.MapPut("/api/methods/put", ([FromBody] object? body) => 
            Results.Ok(new { method = "PUT", success = true, receivedBody = body }));

        app.MapDelete("/api/methods/delete", () => 
            Results.Ok(new { method = "DELETE", success = true }));

        app.MapPatch("/api/methods/patch", ([FromBody] object? body) => 
            Results.Ok(new { method = "PATCH", success = true, receivedBody = body }));

        app.MapMethods("/api/methods/head", ["HEAD"], () => 
            Results.Ok());

        app.MapMethods("/api/methods/options", ["OPTIONS"], () => 
            Results.Ok(new { method = "OPTIONS", allowedMethods = "GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS" }));
    }
}
