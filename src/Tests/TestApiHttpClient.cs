using Fuzn.FluentHttp.TestApi.Models;

namespace Fuzn.FluentHttp.Tests;

public class TestApiHttpClient
{
    private readonly HttpClient _client;

    public TestApiHttpClient(HttpClient client)
    {
        _client = client;
    }

    private FluentHttpRequest GetRequest()
    {
        return _client
            .Request()
            .WithSerializer(new CustomJsonSerializerProvider());
    }

    public async Task<PersonDto> GetPerson()
    {
        var response = await GetRequest().WithUrl("/api/deserialize/person").Get();

        if (!response.IsSuccessful)
        {
            throw new Exception($"Request failed with status code: {response.StatusCode}");
        }

        return response.ContentAs<PersonDto>() ?? throw new Exception("Failed to deserialize response content.");
    }
}
