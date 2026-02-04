using System.Net;
using System.Net.Http.Headers;

namespace Fuzn.FluentHttp;

/// <summary>
/// Represents an HTTP response optimized for streaming downloads.
/// The response stream should be disposed after use.
/// </summary>
public class FluentHttpStreamResponse : IDisposable, IAsyncDisposable
{
    private Stream? _contentStream;
    private int _disposed;

    internal FluentHttpStreamResponse(HttpResponseMessage response)
    {
        ArgumentNullException.ThrowIfNull(response);
        InnerResponse = response;
    }

    /// <summary>
    /// Gets the underlying <see cref="HttpResponseMessage"/>.
    /// </summary>
    public HttpResponseMessage InnerResponse { get; }

    /// <summary>
    /// Gets the response headers.
    /// </summary>
    public HttpResponseHeaders Headers => InnerResponse.Headers;

    /// <summary>
    /// Gets the content headers (e.g., Content-Type, Content-Length, Content-Disposition).
    /// </summary>
    public HttpContentHeaders ContentHeaders => InnerResponse.Content.Headers;

    /// <summary>
    /// Gets the HTTP status code of the response.
    /// </summary>
    public HttpStatusCode StatusCode => InnerResponse.StatusCode;

    /// <summary>
    /// Gets the HTTP status reason phrase (e.g., "OK", "Not Found").
    /// </summary>
    public string? ReasonPhrase => InnerResponse.ReasonPhrase;

    /// <summary>
    /// Gets a value indicating whether the response was successful (status code 2xx).
    /// </summary>
    public bool IsSuccessful => InnerResponse.IsSuccessStatusCode;

    /// <summary>
    /// Gets the HTTP version of the response.
    /// </summary>
    public Version Version => InnerResponse.Version;

    /// <summary>
    /// Gets the content length in bytes, if available.
    /// </summary>
    public long? ContentLength => InnerResponse.Content.Headers.ContentLength;

    /// <summary>
    /// Gets the content type of the response, if available.
    /// </summary>
    public string? ContentType => InnerResponse.Content.Headers.ContentType?.MediaType;

    /// <summary>
    /// Gets the suggested file name from the Content-Disposition header, if available.
    /// </summary>
    public string? FileName
    {
        get
        {
            var contentDisposition = InnerResponse.Content.Headers.ContentDisposition;
            return contentDisposition?.FileNameStar ?? contentDisposition?.FileName?.Trim('"');
        }
    }

    /// <summary>
    /// Throws an <see cref="HttpRequestException"/> if the response status code does not indicate success (2xx).
    /// </summary>
    /// <returns>The current <see cref="FluentHttpStreamResponse"/> instance for method chaining.</returns>
    /// <exception cref="HttpRequestException">Thrown when the response status code is not successful.</exception>
    public FluentHttpStreamResponse EnsureSuccessful()
    {
        if (!IsSuccessful)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)StatusCode} ({ReasonPhrase}).",
                inner: null,
                StatusCode);
        }

        return this;
    }

    /// <summary>
    /// Gets the response content as a stream for reading.
    /// The caller is responsible for disposing the stream.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A stream containing the response content.</returns>
    public async Task<Stream> GetStream(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        _contentStream = await InnerResponse.Content.ReadAsStreamAsync(cancellationToken);
        return _contentStream;
    }

    /// <summary>
    /// Reads the entire response content as a byte array.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A byte array containing the response content.</returns>
    public async Task<byte[]> GetBytes(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed == 1, this);
        return await InnerResponse.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    /// <summary>
    /// Releases all resources used by the <see cref="FluentHttpStreamResponse"/>.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;
        _contentStream?.Dispose();
        InnerResponse.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously releases all resources used by the <see cref="FluentHttpStreamResponse"/>.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
            return;
        if (_contentStream != null)
            await _contentStream.DisposeAsync();
        InnerResponse.Dispose();
        GC.SuppressFinalize(this);
    }
}
