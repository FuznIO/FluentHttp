namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class DeserializationTests : Test
{
    [Test]
    public async Task As_DeserializesToStronglyTypedObject()
    {
        await Scenario()
            .Step("Deserialize response to typed object", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/deserialize/person").Get();

                Assert.IsTrue(response.Ok);
                
                var person = response.As<PersonDto>();
                Assert.IsNotNull(person);
                Assert.AreEqual(1, person!.Id);
                Assert.AreEqual("John Doe", person.Name);
                Assert.AreEqual("john@example.com", person.Email);
                Assert.AreEqual(30, person.Age);
            })
            .Run();
    }

    [Test]
    public async Task As_DeserializesToList()
    {
        await Scenario()
            .Step("Deserialize response to list of typed objects", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/deserialize/list").Get();

                Assert.IsTrue(response.Ok);
                
                var people = response.As<PersonDto[]>();
                Assert.IsNotNull(people);
                Assert.AreEqual(2, people!.Length);
                Assert.AreEqual("John Doe", people[0].Name);
                Assert.AreEqual("Jane Doe", people[1].Name);
            })
            .Run();
    }

    [Test]
    public async Task AsBytes_ReturnsRawBytes()
    {
        await Scenario()
            .Step("Get response as raw bytes", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/response/bytes").Get();

                Assert.IsTrue(response.Ok);
                
                var bytes = response.AsBytes();
                Assert.IsNotNull(bytes);
                Assert.IsTrue(bytes.Length > 0);
            })
            .Run();
    }

    [Test]
    public async Task Body_ReturnsStringContent()
    {
        await Scenario()
            .Step("Get response body as string", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/response/text").Get();

                Assert.IsTrue(response.Ok);
                Assert.AreEqual("This is plain text response", response.Body);
            })
            .Run();
    }
}

public record PersonDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public int Age { get; set; }
}
