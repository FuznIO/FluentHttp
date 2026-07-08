namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// Thrown by <see cref="MockHttpHandler"/> when a request does not match any configured rule.
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
}
