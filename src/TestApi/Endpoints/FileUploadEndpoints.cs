namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class FileUploadEndpoints
{
    public static void MapFileUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/api/files/upload", async (HttpContext context) =>
        {
            var form = await context.Request.ReadFormAsync();
            var files = form.Files.Select(f => new
            {
                name = f.Name,
                fileName = f.FileName,
                contentType = f.ContentType,
                length = f.Length
            }).ToList();
            
            var fields = form.Where(f => f.Key != null && !form.Files.Any(file => file.Name == f.Key))
                             .ToDictionary(f => f.Key, f => f.Value.ToString());
            
            return Results.Ok(new { files, fields });
        });

        app.MapPost("/api/files/upload-single", async (HttpContext context) =>
        {
            var form = await context.Request.ReadFormAsync();
            var file = form.Files.FirstOrDefault();
            
            if (file == null)
                return Results.BadRequest(new { error = "No file uploaded" });
            
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var content = ms.ToArray();
            
            return Results.Ok(new
            {
                name = file.Name,
                fileName = file.FileName,
                contentType = file.ContentType,
                length = file.Length,
                contentPreview = content.Length <= 100 ? Convert.ToBase64String(content) : Convert.ToBase64String(content.Take(100).ToArray()) + "..."
            });
        });
    }
}
