using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class DeserializationTests : Test
{
    [Test]
    public async Task ContentAs_DeserializesToStronglyTypedObject()
    {
        await Scenario()
            .Step("Deserialize response to typed object", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/deserialize/person").Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var person = response.ContentAs<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual(1, person!.Id);
                Assert.AreEqual("John Doe", person.Name);
                Assert.AreEqual("john@example.com", person.Email);
                Assert.AreEqual(30, person.Age);
            })
            .Run();
    }

    [Test]
    public async Task ContentAs_DeserializesToList()
    {
        await Scenario()
            .Step("Deserialize response to list of typed objects", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/deserialize/list").Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var people = response.ContentAs<PersonDto[]>();
                Assert.IsNotNull(people);
                Assert.HasCount(2, people);
                Assert.AreEqual("John Doe", people[0].Name);
                Assert.AreEqual("Jane Doe", people[1].Name);
            })
            .Run();
    }

    [Test]
    public async Task Content_ReturnsStringContent()
    {
        await Scenario()
            .Step("Get response content as string", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/response/text").Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual("This is plain text response", response.Content);
            })
            .Run();
    }
}
