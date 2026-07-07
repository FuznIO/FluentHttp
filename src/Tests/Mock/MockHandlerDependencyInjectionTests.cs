using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.FluentHttp.Testing;
using Fuzn.FluentHttp.Tests.Live;
using Fuzn.TestFuzn;
using Microsoft.Extensions.DependencyInjection;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerDependencyInjectionTests : Test
{
    [Test]
    public async Task UseMockHandler_ServesNamedClientFromFactory()
    {
        await Scenario()
            .Step("A named HttpClient from IHttpClientFactory is served by the mock", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/person/1").RespondWithContent(new PersonDto { Id = 1, Name = "DI" });

                var services = new ServiceCollection();
                services.AddHttpClient("api", c => c.BaseAddress = new Uri("https://api.example.com/"))
                    .UseMockHandler(handler);

                using var provider = services.BuildServiceProvider();
                var client = provider.GetRequiredService<IHttpClientFactory>().CreateClient("api");

                var response = await client.Url("/api/person/1").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual("DI", response.ContentAs<PersonDto>()!.Name);
                Assert.IsTrue(handler.Requests.Any(r => r.Method == HttpMethod.Get && r.RequestUri.AbsolutePath == "/api/person/1"));
            })
            .Run();
    }

    [Test]
    public async Task UseMockHandler_ServesTypedClientFromDI()
    {
        await Scenario()
            .Step("A typed HttpClient resolved from DI is served by the mock", async _ =>
            {
                var handler = new MockHttpHandler()
                    .WithSerializer(new CustomJsonSerializerProvider());
                handler.WhenGet("/api/deserialize/person")
                    .RespondWithContent(new PersonDto { Id = 9, Name = "Typed" });

                var services = new ServiceCollection();
                services.AddHttpClient<TestApiHttpClient>(c => c.BaseAddress = new Uri("https://api.example.com/"))
                    .UseMockHandler(handler);

                using var provider = services.BuildServiceProvider();
                var client = provider.GetRequiredService<TestApiHttpClient>();

                var person = await client.GetPerson();

                Assert.IsNotNull(person);
                Assert.AreEqual("Typed", person.Name);
            })
            .Run();
    }
}
