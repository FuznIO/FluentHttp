namespace Fuzn.FluentHttp;

/// <summary>
/// Specifies the accepted response content types for HTTP requests.
/// </summary>
public enum AcceptTypes
{
    /// <summary>
    /// Accept JSON responses (application/json).
    /// </summary>
    Json,

    /// <summary>
    /// Accept HTML responses (text/html).
    /// </summary>
    Html,

    /// <summary>
    /// Accept XML responses (application/xml).
    /// </summary>
    Xml,

    /// <summary>
    /// Accept plain text responses (text/plain).
    /// </summary>
    PlainText,

    /// <summary>
    /// Accept any content type (*/*).
    /// </summary>
    Any,

    /// <summary>
    /// Accept binary content (application/octet-stream).
    /// </summary>
    OctetStream
}
