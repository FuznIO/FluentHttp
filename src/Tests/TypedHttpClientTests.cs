using Fuzn.TestFuzn;
using Microsoft.Extensions.DependencyInjection;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class TypedHttpClientTests : Test
{
    [Test]
    public async Task TypedClient_ResolvesFromDI_AndDeserializesPerson()
    {
        await Scenario()
            .Step("Test", async _ =>
            {
                var client = SuiteData.ServiceProvider.GetRequiredService<TestApiHttpClient>();

                var person = await client.GetPerson();

                Assert.IsNotNull(person);
            })
            .Run();
    }
}