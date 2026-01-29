namespace Fuzn.FluentHttp;

/// <summary>
/// Global defaults and interceptors for FluentHttp requests.
/// </summary>
public static class FluentHttpDefaults
{
    private static readonly Lock _lock = new();
    private static Action<HttpRequestBuilder>? _beforeSend;

    /// <summary>
    /// Gets or sets the interceptor that runs before each request is sent.
    /// Use <c>builder.Data</c> to inspect the current request state and builder methods to modify.
    /// For async operations like token refresh, use a <see cref="DelegatingHandler"/> instead.
    /// </summary>
    /// <example>
    /// <code>
    /// FluentHttpDefaults.BeforeSend = builder =>
    /// {
    ///     // Set default serializer if not already configured per-request
    ///     if (builder.Data.SerializerOptions is null)
    ///     {
    ///         builder.WithJsonOptions(myGlobalOptions);
    ///     }
    ///     
    ///     // Add correlation ID to all requests
    ///     if (!builder.Data.Headers.ContainsKey("X-Correlation-Id"))
    ///     {
    ///         builder.WithHeader("X-Correlation-Id", Guid.NewGuid().ToString());
    ///     }
    /// };
    /// </code>
    /// </example>
    public static Action<HttpRequestBuilder>? BeforeSend
    {
        get { lock (_lock) return _beforeSend; }
        set { lock (_lock) _beforeSend = value; }
    }

    internal static void ExecuteInterceptor(HttpRequestBuilder builder)
    {
        if (builder.Data.InterceptorExecuted)
            return;

        Action<HttpRequestBuilder>? action;
        lock (_lock)
        {
            action = _beforeSend;
        }

        action?.Invoke(builder);
        builder.Data.InterceptorExecuted = true;
    }
}
