namespace Fuzn.FluentHttp.TestApi.Models;

public class MultipleFileUploadResponse
{
    public List<FileInfo> Files { get; set; } = new();
    public Dictionary<string, string> Fields { get; set; } = new();

    public class FileInfo
    {
        public string Name { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}
