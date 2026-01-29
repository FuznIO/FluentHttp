using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class QueryParameterTests : Test
{
    [Test]
    public async Task WithQueryParam_SingleValues_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with single query parameters", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/query/single")
                    .WithQueryParam("name", "TestName")
                    .WithQueryParam("count", 42)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleQueryParamsResponse>();
                Assert.AreEqual("TestName", body!.Name);
                Assert.AreEqual(42, body.Count);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_MultipleValues_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple values for same parameter", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/query/multiple")
                    .WithQueryParam("tags", new[] { "csharp", "dotnet", "http" })
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MultipleQueryParamsResponse>();
                Assert.HasCount(3, body!.Tags);
                Assert.Contains("csharp", body.Tags);
                Assert.Contains("dotnet", body.Tags);
                Assert.Contains("http", body.Tags);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParams_FromDictionary_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with query parameters from dictionary", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var parameters = new Dictionary<string, object?>
                {
                    ["search"] = "test query",
                    ["page"] = 2,
                    ["pageSize"] = 25,
                    ["includeDeleted"] = true
                };

                var response = await client.Url("/api/query/complex")
                    .WithQueryParams(parameters)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<ComplexQueryParamsResponse>();
                Assert.AreEqual("test query", body!.Search);
                Assert.AreEqual(2, body.Page);
                Assert.AreEqual(25, body.PageSize);
                Assert.IsTrue(body.IncludeDeleted);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParams_FromAnonymousObject_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with query parameters from anonymous object", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/query/complex")
                    .WithQueryParams(new { search = "anonymous", page = 3, pageSize = 50, includeDeleted = false })
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<ComplexQueryParamsResponse>();
                Assert.AreEqual("anonymous", body!.Search);
                Assert.AreEqual(3, body.Page);
                Assert.AreEqual(50, body.PageSize);
                Assert.IsFalse(body.IncludeDeleted);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_WithNullValue_IsIgnored()
    {
        await Scenario()
            .Step("Send request with null query parameter", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/query/single")
                    .WithQueryParam("name", "TestName")
                    .WithQueryParam("count", null!)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleQueryParamsResponse>();
                Assert.AreEqual("TestName", body!.Name);
            })
            .Run();
    }
}
