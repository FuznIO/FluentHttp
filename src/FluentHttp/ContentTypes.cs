namespace Fuzn.FluentHttp;

/// <summary>
/// Specifies the content type for HTTP request bodies.
/// </summary>
public enum ContentTypes
{
    /// <summary>
    /// URL-encoded form data (application/x-www-form-urlencoded).
    /// </summary>
    XFormUrlEncoded,

    /// <summary>
    /// JSON content (application/json).
    /// </summary>
    Json,

    /// <summary>
    /// Multipart form data (multipart/form-data). Used for file uploads.
    /// </summary>
    Multipart,

    /// <summary>
    /// XML content (application/xml).
    /// </summary>
    Xml,

    /// <summary>
    /// Plain text content (text/plain).
    /// </summary>
    PlainText,

    /// <summary>
    /// Binary content (application/octet-stream).
    /// </summary>
    OctetStream
}
