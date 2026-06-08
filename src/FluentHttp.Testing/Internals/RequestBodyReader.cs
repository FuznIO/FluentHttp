namespace Fuzn.FluentHttp.Testing.Internals;

/// <summary>
/// Buffers an <see cref="HttpContent"/> body into a string for capture and matching.
/// </summary>
internal static class RequestBodyReader
{
    /// <summary>
    /// Reads the content as a string, returning <c>null</c> when there is no content body.
    /// </summary>
    internal static async Task<string?> ReadAsStringAsync(HttpContent? content, CancellationToken cancellationToken)
    {
        if (content is null)
            return null;

        return await content.ReadAsStringAsync(cancellationToken);
    }
}
