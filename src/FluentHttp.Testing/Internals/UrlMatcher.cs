using System.Text;
using System.Text.RegularExpressions;

namespace Fuzn.FluentHttp.Testing.Internals;

/// <summary>
/// Compiles a URL pattern (optionally containing <c>*</c> wildcards) into a matcher that can
/// be tested against a request URI. Absolute patterns are matched against the request URI, relative
/// patterns against the request path. A pattern that contains a <c>?</c> also matches against the
/// request's query string; a pattern without one matches the path alone and ignores any query, so the
/// query can be constrained separately with <c>WithQueryParam</c>.
/// </summary>
internal sealed class UrlMatcher
{
    private readonly Regex _regex;
    private readonly bool _isAbsolute;
    private readonly bool _includesQuery;

    internal UrlMatcher(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        Pattern = pattern;

        // A pattern is absolute only when it carries a scheme separator. We deliberately avoid
        // Uri.TryCreate(..., UriKind.Absolute) here: on Unix it reports relative paths like
        // "/api/x" as absolute "file" URIs (see FluentHttpRequest's IsHttpScheme guard and the
        // uri-tryparse-linux fix), and it rejects wildcard hosts like "https://*/x". A simple
        // "contains '://'" test classifies relative paths, absolute URLs, and wildcard hosts correctly.
        _isAbsolute = pattern.Contains("://", StringComparison.Ordinal);

        // Only match against the query string when the pattern explicitly includes one. Otherwise a
        // request's query would defeat an exact path pattern and make WithQueryParam unusable.
        _includesQuery = pattern.Contains('?', StringComparison.Ordinal);

        _regex = BuildRegex(pattern);
    }

    /// <summary>
    /// Gets the original pattern, used for diagnostics.
    /// </summary>
    internal string Pattern { get; }

    internal bool IsMatch(Uri requestUri)
    {
        var target = (_isAbsolute, _includesQuery) switch
        {
            (true, true) => requestUri.GetLeftPart(UriPartial.Query),
            (true, false) => requestUri.GetLeftPart(UriPartial.Path),
            (false, true) => requestUri.PathAndQuery,
            (false, false) => requestUri.AbsolutePath,
        };

        return _regex.IsMatch(target);
    }

    private static Regex BuildRegex(string pattern)
    {
        // Translate a glob (only '*' is special) into an anchored regex.
        var sb = new StringBuilder();
        sb.Append('^');

        foreach (var ch in pattern)
        {
            if (ch == '*')
                sb.Append(".*");
            else
                sb.Append(Regex.Escape(ch.ToString()));
        }

        sb.Append('$');

        return new Regex(sb.ToString(), RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}
