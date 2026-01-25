namespace Fuzn.FluentHttp;

/// <summary>
/// Represents a file to be uploaded in a multipart/form-data request.
/// </summary>
/// <param name="Name">The form field name for the file.</param>
/// <param name="FileName">The file name to be sent in the Content-Disposition header.</param>
/// <param name="Content">The file content as a stream.</param>
/// <param name="ContentType">The MIME type of the file. Defaults to "application/octet-stream".</param>
public record FileContent(string Name, string FileName, Stream Content, string ContentType = "application/octet-stream")
{
    /// <summary>
    /// Creates a new instance of the <see cref="FileContent"/> class from a byte array.
    /// </summary>
    /// <param name="name">The form field name for the file.</param>
    /// <param name="fileName">The file name to be sent in the Content-Disposition header.</param>
    /// <param name="content">The file content as a byte array.</param>
    /// <param name="contentType">The MIME type of the file. Defaults to "application/octet-stream".</param>
    public FileContent(string name, string fileName, byte[] content, string contentType = "application/octet-stream")
        : this(name, fileName, new MemoryStream(content), contentType)
    {
    }
}
