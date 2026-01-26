using System.Text;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class ResponseTypesEndpoints
{
    public static void MapResponseTypesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/response/json", () => 
            Results.Ok(new { type = "json", data = new { id = 1, name = "Test" } }));

        app.MapGet("/api/response/text", () => 
            Results.Text("This is plain text response", "text/plain"));

        app.MapGet("/api/response/xml", () => 
            Results.Text("<response><type>xml</type><id>1</id></response>", "application/xml"));

        app.MapGet("/api/response/bytes", () =>
        {
            var bytes = Encoding.UTF8.GetBytes("Binary content test");
            return Results.Bytes(bytes, "application/octet-stream");
        });
    }
}
