using System.Text.Json;

namespace Fuzn.FluentHttp;

/// <summary>
/// Default serializer provider using System.Text.Json.
/// Uses <see cref="JsonSerializerDefaults.Web"/> by default (camelCase, case-insensitive).
/// </summary>
public class SystemTextJsonSerializerProvider : ISerializerProvider
{
    private static readonly JsonSerializerOptions _defaultOptions = new(JsonSerializerDefaults.Web);
    private readonly JsonSerializerOptions _options;

    /// <summary>
    /// Initializes a new instance with default web options (camelCase, case-insensitive).
    /// </summary>
    public SystemTextJsonSerializerProvider()
    {
        _options = _defaultOptions;
    }

    /// <summary>
    /// Initializes a new instance with custom JSON serializer options.
    /// </summary>
    /// <param name="jsonSerializerOptions">The JSON serializer options to use.</param>
    public SystemTextJsonSerializerProvider(JsonSerializerOptions jsonSerializerOptions)
    {
        ArgumentNullException.ThrowIfNull(jsonSerializerOptions);
        _options = jsonSerializerOptions;
    }

    /// <inheritdoc />
    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    /// <inheritdoc />
    public T? Deserialize<T>(string content)
    {
        return JsonSerializer.Deserialize<T>(content, _options);
    }
}
