namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// Thrown by <see cref="FluentHttpMockHandler"/> when a request does not match any stub
/// (under <see cref="MockFallbackBehavior.Throw"/>) or when a verification assertion fails.
/// </summary>
public class FluentHttpMockException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FluentHttpMockException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public FluentHttpMockException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FluentHttpMockException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public FluentHttpMockException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
