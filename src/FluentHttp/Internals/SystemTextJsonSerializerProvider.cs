using System.Text.Json;

namespace Fuzn.FluentHttp.Internals;

internal class SystemTextJsonSerializerProvider : ISerializerProvider
{
    private readonly JsonSerializerOptions _options;

    public SystemTextJsonSerializerProvider()
    {
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
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
