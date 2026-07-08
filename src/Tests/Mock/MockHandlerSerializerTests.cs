using System.Net;
using System.Text.Json;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerSerializerTests : Test
{
    [Test]
    public async Task DefaultSerializer_UsesWebCamelCase()
    {
        await Scenario()
            .Step("Default mock serializer matches the library default (camelCase)", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/person").RespondWithContent(new PersonDto { Name = "John" });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person").Get();

                Assert.Contains("\"name\"", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithSerializer_OverridesResponseSerialization()
    {
        await Scenario()
            .Step("Custom serializer controls response body serialization", async _ =>
            {
                var pascalCase = new SystemTextJsonSerializerProvider(
                    new JsonSerializerOptions { PropertyNamingPolicy = null });

                var handler = new MockHttpHandler().WithSerializer(pascalCase);
                handler.WhenGet("/api/person").RespondWithContent(new PersonDto { Name = "John" });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/person").Get();

                Assert.Contains("\"Name\"", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithSerializer_UsedForBodyMatching()
    {
        await Scenario()
            .Step("Custom serializer is used when matching request bodies", async _ =>
            {
                var pascalCase = new SystemTextJsonSerializerProvider(
                    new JsonSerializerOptions { PropertyNamingPolicy = null });

                var handler = new MockHttpHandler().WithSerializer(pascalCase);
                handler.WhenPost("/api/person")
                    .WithContent(new PersonDto { Name = "Jane" })
                    .RespondWith(HttpStatusCode.Created);
                var client = handler.CreateClient("https://api.example.com/");

                // The request body must be serialized PascalCase to match.
                var response = await client.Url("/api/person")
                    .WithSerializer(pascalCase)
                    .WithContent(new PersonDto { Name = "Jane" })
                    .Post();

                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            })
            .Run();
    }
}
