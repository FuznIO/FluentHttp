namespace Fuzn.FluentHttp.Testing;

/// <summary>
/// Controls how a <see cref="MockHttpHandler"/> behaves when an incoming request
/// does not match any configured rule.
/// </summary>
public enum MockFallbackBehavior
{
    /// <summary>
    /// Throw a <see cref="MockHttpException"/> describing the unmatched request.
    /// This is the default and surfaces missing rules early during testing.
    /// </summary>
    Throw,

    /// <summary>
    /// Return an empty <c>404 Not Found</c> response instead of throwing.
    /// </summary>
    RespondNotFound
}
