namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class StreamingEndpoints
{
    public static void MapStreamingEndpoints(this WebApplication app)
    {
        app.MapGet("/api/stream/download", async (HttpContext context) =>
        {
            var content = "This is streamed content.\n"u8.ToArray();
            var repeatedContent = Enumerable.Repeat(content, 100).SelectMany(x => x).ToArray();
            
            context.Response.ContentType = "application/octet-stream";
            context.Response.Headers.ContentDisposition = "attachment; filename=\"test-file.txt\"";
            context.Response.ContentLength = repeatedContent.Length;
            
            await context.Response.Body.WriteAsync(repeatedContent);
        });

        app.MapGet("/api/stream/large", async (HttpContext context) =>
        {
            context.Response.ContentType = "application/octet-stream";
            context.Response.Headers.ContentDisposition = "attachment; filename=\"large-file.bin\"";
            
            var chunk = new byte[1024];
            Array.Fill(chunk, (byte)'X');
            
            // Stream 100 KB in chunks
            for (int i = 0; i < 100; i++)
            {
                await context.Response.Body.WriteAsync(chunk);
                await context.Response.Body.FlushAsync();
            }
        });
    }
}
