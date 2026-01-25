namespace Fuzn.FluentHttp.TestApi.Models;

public class SingleFileUploadResponse
{
    public string Name { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Length { get; set; }
}
