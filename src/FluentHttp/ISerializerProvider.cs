namespace Fuzn.FluentHttp;

public interface ISerializerProvider
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string json);
}
