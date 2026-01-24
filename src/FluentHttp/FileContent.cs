namespace Fuzn.FluentHttp;

/// <summary>
/// Represents a file to be uploaded in a multipart/form-data request.
/// </summary>
public class FileContent
{
    /// <summary>
    /// Gets or sets the form field name for the file.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the file name to be sent in the Content-Disposition header.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the file content as a stream.
    /// </summary>
    public Stream Content { get; set; }

    /// <summary>
    /// Gets or sets the MIME type of the file. Defaults to "application/octet-stream".
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Initializes a new instance of the <see cref="FileContent"/> class.
    /// </summary>
    /// <param name="name">The form field name for the file.</param>
    /// <param name="fileName">The file name to be sent in the Content-Disposition header.</param>
    /// <param name="content">The file content as a stream.</param>
    /// <param name="contentType">The MIME type of the file. Defaults to "application/octet-stream".</param>
    public FileContent(string name, string fileName, Stream content, string contentType = "application/octet-stream")
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        ContentType = contentType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileContent"/> class from a byte array.
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
