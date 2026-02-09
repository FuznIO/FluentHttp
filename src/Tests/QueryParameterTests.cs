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
                    .WithQueryParam("count", "42")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleQueryParamsResponse>();
                Assert.AreEqual("TestName", body!.Name);
                Assert.AreEqual(42, body.Count);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_MultipleValuesForSameKey_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple values for same parameter", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/query/multiple")
                    .WithQueryParam("tags", "csharp")
                    .WithQueryParam("tags", "dotnet")
                    .WithQueryParam("tags", "http")
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
    public async Task WithQueryParam_ComplexParameters_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple query parameters", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/query/complex")
                    .WithQueryParam("search", "test query")
                    .WithQueryParam("page", "2")
                    .WithQueryParam("pageSize", "25")
                    .WithQueryParam("includeDeleted", "true")
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
}
