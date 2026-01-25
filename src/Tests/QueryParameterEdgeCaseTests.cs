using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class QueryParameterEdgeCaseTests : Test
{
    [Test]
    public async Task QueryParam_DateTimeValue_FormatsAsIso8601()
    {
        await Scenario()
            .Step("DateTime query parameter is formatted as ISO 8601", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                var testDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
                
                var response = await client.Url("/api/echo")
                    .QueryParam("date", testDate)
                    .Get();

                Assert.IsTrue(response.Ok);
                // The response will contain the query string which should have the ISO 8601 format
                Assert.Contains("2024-06-15", response.Body);
            })
            .Run();
    }

    [Test]
    public async Task QueryParam_DateTimeOffsetValue_FormatsAsIso8601()
    {
        await Scenario()
            .Step("DateTimeOffset query parameter is formatted as ISO 8601", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                var testDate = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero);
                
                var response = await client.Url("/api/echo")
                    .QueryParam("date", testDate)
                    .Get();

                Assert.IsTrue(response.Ok);
                Assert.Contains("2024-06-15", response.Body);
            })
            .Run();
    }

    [Test]
    public async Task QueryParam_BooleanValue_FormatsAsLowercase()
    {
        await Scenario()
            .Step("Boolean query parameter is formatted as lowercase", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/echo")
                    .QueryParam("enabled", true)
                    .QueryParam("disabled", false)
                    .Get();

                Assert.IsTrue(response.Ok);
                Assert.Contains("true", response.Body);
                Assert.Contains("false", response.Body);
            })
            .Run();
    }

    [Test]
    public async Task QueryParam_SpecialCharacters_AreUrlEncoded()
    {
        await Scenario()
            .Step("Special characters in query parameters are URL encoded", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/query/single")
                    .QueryParam("name", "Test & Value")
                    .QueryParam("count", 1)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<SingleQueryParamsResponse>();
                Assert.AreEqual("Test & Value", body!.Name);
            })
            .Run();
    }

    [Test]
    public async Task QueryParams_NullValuesInCollection_AreIgnored()
    {
        await Scenario()
            .Step("Null values in collection are ignored", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var values = new object?[] { "value1", null, "value2" };
                
                var response = await client.Url("/api/query/multiple")
                    .QueryParam("tags", values)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.As<MultipleQueryParamsResponse>();
                Assert.HasCount(2, body!.Tags);
            })
            .Run();
    }

    [Test]
    public async Task QueryParam_NullCollection_IsIgnored()
    {
        await Scenario()
            .Step("Null collection is ignored", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                IEnumerable<object?>? nullCollection = null;
                
                var response = await client.Url("/api/status/ok")
                    .QueryParam("tags", nullCollection)
                    .Get();

                Assert.IsTrue(response.Ok);
            })
            .Run();
    }

    [Test]
    public async Task QueryParams_NullObject_IsIgnored()
    {
        await Scenario()
            .Step("Null object for QueryParams is ignored", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                object? nullObj = null;
                
                var response = await client.Url("/api/status/ok")
                    .QueryParams(nullObj)
                    .Get();

                Assert.IsTrue(response.Ok);
            })
            .Run();
    }
}
