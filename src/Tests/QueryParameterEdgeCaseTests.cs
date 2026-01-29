using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class QueryParameterEdgeCaseTests : Test
{
    [Test]
    public async Task WithQueryParam_DateTimeValue_FormatsAsIso8601()
    {
        await Scenario()
            .Step("DateTime query parameter is formatted as ISO 8601", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var testDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
                
                var response = await client.Url("/api/echo")
                    .WithQueryParam("date", testDate)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                // The response will contain the query string which should have the ISO 8601 format
                Assert.Contains("2024-06-15", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_DateTimeOffsetValue_FormatsAsIso8601()
    {
        await Scenario()
            .Step("DateTimeOffset query parameter is formatted as ISO 8601", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                var testDate = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);
                
                var response = await client.Url("/api/echo")
                    .WithQueryParam("date", testDate)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("2024-06-15", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_BooleanValue_FormatsAsLowercase()
    {
        await Scenario()
            .Step("Boolean query parameter is formatted as lowercase", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .WithQueryParam("enabled", true)
                    .WithQueryParam("disabled", false)
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
                    .WithQueryParam("count", 1)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleQueryParamsResponse>();
                Assert.AreEqual("Test & Value", body!.Name);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParams_NullValuesInCollection_AreIgnored()
    {
        await Scenario()
            .Step("Null values in collection are ignored", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var values = new object?[] { "value1", null, "value2" };
                
                var response = await client.Url("/api/query/multiple")
                    .WithQueryParam("tags", values)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MultipleQueryParamsResponse>();
                Assert.HasCount(2, body!.Tags);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParam_NullCollection_IsIgnored()
    {
        await Scenario()
            .Step("Null collection is ignored", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                IEnumerable<object?>? nullCollection = null;
                
                var response = await client.Url("/api/status/ok")
                    .WithQueryParam("tags", nullCollection)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task WithQueryParams_NullObject_IsIgnored()
    {
        await Scenario()
            .Step("Null object for WithQueryParams is ignored", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                object? nullObj = null;
                
                var response = await client.Url("/api/status/ok")
                    .WithQueryParams(nullObj)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }
}
