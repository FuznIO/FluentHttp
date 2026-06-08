namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// Controls how a <see cref="FluentHttpMockHandler"/> behaves when an incoming request
/// does not match any configured stub.
/// </summary>
public enum MockFallbackBehavior
{
    /// <summary>
    /// Throw a <see cref="FluentHttpMockException"/> describing the unmatched request.
    /// This is the default and surfaces missing stubs early during testing.
    /// </summary>
    Throw,

    /// <summary>
    /// Return an empty <c>404 Not Found</c> response instead of throwing.
    /// </summary>
    RespondNotFound
}
