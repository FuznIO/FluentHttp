using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class QueryParameterEdgeCaseTests : Test
{
    [Test]
    public async Task WithQueryParam_DateTimeValue_CanBeFormattedByDeveloper()
    {
        await Scenario()
            .Step("DateTime query parameter formatted by developer as ISO 8601", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var testDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
                
                var response = await client.Url("/api/echo")
                    .WithQueryParam("date", testDate.ToString("O"))
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                // The response will contain the query string which should have the ISO 8601 format
                Assert.Contains("2024-06-15", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_BooleanValue_CanBeFormattedByDeveloper()
    {
        await Scenario()
            .Step("Boolean query parameter formatted by developer as lowercase", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .WithQueryParam("enabled", "true")
                    .WithQueryParam("disabled", "false")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("true", response.Content);
                Assert.Contains("false", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_SpecialCharacters_AreUrlEncoded()
    {
        await Scenario()
            .Step("Special characters in query parameters are URL encoded", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/query/single")
                    .WithQueryParam("name", "Test & Value")
                    .WithQueryParam("count", "1")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleQueryParamsResponse>();
                Assert.AreEqual("Test & Value", body!.Name);
            })
            .Run();
    }
}
