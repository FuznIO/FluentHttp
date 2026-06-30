namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// Thrown by <see cref="MockHttpHandler"/> when a request does not match any rule
/// (under <see cref="MockFallbackBehavior.Throw"/>) or when a verification assertion fails.
/// </summary>
public class MockHttpException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public MockHttpException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MockHttpException"/> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MockHttpException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
