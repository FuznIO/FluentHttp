using System.Net;
using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

/// <summary>
/// Mirrors the live deserialization scenarios using a mock response (no TestApi).
/// </summary>
[TestClass]
public class MockDeserializationTests : Test
{
    [Test]
    public async Task ContentAs_DeserializesPerson()
    {
        await Scenario()
            .Step("Response body deserializes into a typed object", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/deserialize/person")
                    .RespondWithContent(new PersonDto { Id = 1, Name = "John Doe", Email = "john@example.com", Age = 42 });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/deserialize/person").Get();
                var person = response.ContentAs<PersonDto>();

                Assert.IsNotNull(person);
                Assert.AreEqual("John Doe", person!.Name);
                Assert.AreEqual(42, person.Age);
            })
            .Run();
    }

    [Test]
    public async Task GenericGet_ExposesTypedData()
    {
        await Scenario()
            .Step("Generic Get<T> exposes typed data", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/deserialize/person")
                    .RespondWithContent(new PersonDto { Id = 5, Name = "Jane" });
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/deserialize/person").Get<PersonDto>();

                Assert.AreEqual(5, response.Data!.Id);
                Assert.AreEqual("Jane", response.Data.Name);
            })
            .Run();
    }
}
