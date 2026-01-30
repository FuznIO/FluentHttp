using System.Text.Json;

namespace Fuzn.FluentHttp.Internals;

internal class SystemTextJsonSerializerProvider : ISerializerProvider
{
    private static readonly JsonSerializerOptions _defaultOptions = new(JsonSerializerDefaults.Web);
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonSerializerProvider()
    {
        _options = _defaultOptions;
    }

    public SystemTextJsonSerializerProvider(JsonSerializerOptions jsonSerializerOptions)
    {
        _options = jsonSerializerOptions;
    }

    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, _options);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}
