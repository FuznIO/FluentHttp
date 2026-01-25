namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class QueryParameterTests : Test
{
    [Test]
    public async Task QueryParam_SingleValues_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with single query parameters", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/query/single")
                    .QueryParam("name", "TestName")
                    .QueryParam("count", 42)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("TestName", (string)body!.name);
                Assert.AreEqual(42, (int)body!.count);
            })
            .Run();
    }

    [Test]
    public async Task QueryParam_MultipleValues_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple values for same parameter", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/query/multiple")
                    .QueryParam("tags", new[] { "csharp", "dotnet", "http" })
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                var tags = ((IEnumerable<dynamic>)body!.tags).Select(t => (string)t).ToList();
                Assert.AreEqual(3, tags.Count);
                Assert.IsTrue(tags.Contains("csharp"));
                Assert.IsTrue(tags.Contains("dotnet"));
                Assert.IsTrue(tags.Contains("http"));
            })
            .Run();
    }

    [Test]
    public async Task QueryParams_FromDictionary_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with query parameters from dictionary", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var parameters = new Dictionary<string, object?>
                {
                    ["search"] = "test query",
                    ["page"] = 2,
                    ["pageSize"] = 25,
                    ["includeDeleted"] = true
                };

                var response = await client.Url("/api/query/complex")
                    .QueryParams(parameters)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("test query", (string)body!.search);
                Assert.AreEqual(2, (int)body!.page);
                Assert.AreEqual(25, (int)body!.pageSize);
                Assert.IsTrue((bool)body!.includeDeleted);
            })
            .Run();
    }

    [Test]
    public async Task QueryParams_FromAnonymousObject_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with query parameters from anonymous object", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/query/complex")
                    .QueryParams(new { search = "anonymous", page = 3, pageSize = 50, includeDeleted = false })
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("anonymous", (string)body!.search);
                Assert.AreEqual(3, (int)body!.page);
                Assert.AreEqual(50, (int)body!.pageSize);
                Assert.IsFalse((bool)body!.includeDeleted);
            })
            .Run();
    }

    [Test]
    public async Task QueryParam_WithNullValue_IsIgnored()
    {
        await Scenario()
            .Step("Send request with null query parameter", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/query/single")
                    .QueryParam("name", "TestName")
                    .QueryParam("count", null)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("TestName", (string)body!.name);
            })
            .Run();
    }
}
