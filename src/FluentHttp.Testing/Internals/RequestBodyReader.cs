using System.Text;

namespace Fuzn.FluentHttp.Testing.Internals;

/// <summary>
/// Buffers an <see cref="HttpContent"/> body into bytes (and a decoded string) for capture and matching.
/// </summary>
internal static class RequestBodyReader
{
    /// <summary>
    /// Reads the content as a byte array, returning <c>null</c> when there is no content body.
    /// </summary>
    internal static async Task<byte[]?> ReadAsByteArray(HttpContent? content, CancellationToken cancellationToken)
    {
        if (content is null)
            return null;

        return await content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Decodes body bytes into a string using the content's declared charset, falling back to UTF-8.
    /// Used for string-based matchers and the captured <c>Content</c> string.
    /// </summary>
    internal static string Decode(HttpContent? content, byte[] bytes)
    {
        var charSet = content?.Headers.ContentType?.CharSet;
        if (!string.IsNullOrWhiteSpace(charSet))
        {
            try
            {
                return Encoding.GetEncoding(charSet.Trim('"')).GetString(bytes);
            }
            catch (ArgumentException)
            {
                // Unknown charset - fall back to UTF-8 below.
            }
        }

        return Encoding.UTF8.GetString(bytes);
    }
}
