using Fuzn.FluentHttp.TestApi.Models;

namespace Fuzn.FluentHttp.TestApi.Endpoints;

public static class FileUploadEndpoints
{
    public static void MapFileUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/api/files/upload", async (HttpContext context) =>
        {
            var form = await context.Request.ReadFormAsync();
            var files = form.Files.Select(f => new MultipleFileUploadResponse.FileInfo
            {
                Name = f.Name,
                FileName = f.FileName
            }).ToList();
            
            var fields = form.Where(f => f.Key != null && !form.Files.Any(file => file.Name == f.Key))
                             .ToDictionary(f => f.Key, f => f.Value.ToString());
            
            return Results.Ok(new MultipleFileUploadResponse { Files = files, Fields = fields });
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
            
            return Results.Ok(new SingleFileUploadResponse
            {
                Name = file.Name,
                FileName = file.FileName,
                ContentType = file.ContentType,
                Length = file.Length
            });
        });
    }
}
