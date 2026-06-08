using System.Text;
using System.Text.RegularExpressions;

namespace Fuzn.FluentHttp.Testing.Internals;

/// <summary>
/// Compiles a URL pattern (optionally containing <c>*</c> wildcards) into a matcher that can
/// be tested against a request URI. Absolute patterns are matched against the full request URI;
/// relative patterns are matched against the request's path and query.
/// </summary>
internal sealed class UrlMatcher
{
    private readonly Regex _regex;
    private readonly bool _isAbsolute;

    internal UrlMatcher(string pattern)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pattern);

        Pattern = pattern;
        _isAbsolute = Uri.TryCreate(pattern, UriKind.Absolute, out _);
        _regex = BuildRegex(pattern);
    }

    /// <summary>
    /// Gets the original pattern, used for diagnostics.
    /// </summary>
    internal string Pattern { get; }

    internal bool IsMatch(Uri requestUri)
    {
        var target = _isAbsolute
            ? requestUri.GetLeftPart(UriPartial.Query)
            : requestUri.PathAndQuery;

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
