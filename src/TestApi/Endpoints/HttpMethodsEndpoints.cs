using Fuzn.FluentHttp.TestApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class HttpMethodsEndpoints
{
    public static void MapHttpMethodsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/methods/get", () => 
            Results.Ok(new MethodResponse { Method = "GET", Success = true }));

        app.MapPost("/api/methods/post", ([FromBody] object? body) => 
            Results.Ok(new MethodResponseWithBody { Method = "POST", Success = true, ReceivedBody = body }));

        app.MapPut("/api/methods/put", ([FromBody] object? body) => 
            Results.Ok(new MethodResponseWithBody { Method = "PUT", Success = true, ReceivedBody = body }));

        app.MapDelete("/api/methods/delete", () => 
            Results.Ok(new MethodResponse { Method = "DELETE", Success = true }));

        app.MapPatch("/api/methods/patch", ([FromBody] object? body) => 
            Results.Ok(new MethodResponseWithBody { Method = "PATCH", Success = true, ReceivedBody = body }));

        app.MapMethods("/api/methods/head", ["HEAD"], () => 
            Results.Ok());

        app.MapMethods("/api/methods/options", ["OPTIONS"], () => 
            Results.Ok(new { method = "OPTIONS", allowedMethods = "GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS" }));
    }
}
