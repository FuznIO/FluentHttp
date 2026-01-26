namespace Fuzn.FluentHttp;

/// <summary>
/// Provides methods for serializing and deserializing objects.
/// </summary>
public interface ISerializerProvider
{
    /// <summary>
    /// Serializes an object to a string representation.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A string representation of the serialized object.</returns>
    string Serialize<T>(T obj);
    
    /// <summary>
    /// Deserializes a string representation to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type to deserialize to.</typeparam>
    /// <param name="json">The string representation to deserialize.</param>
    /// <returns>The deserialized object, or null if deserialization fails.</returns>
    T? Deserialize<T>(string json);
}
