using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class NoBaseAddressTests : Test
{
    [Test]
    public async Task Url_WithAbsoluteUrl_WhenNoBaseAddress_CreatesRequestSuccessfully()
    {
        await Scenario()
            .Step("Absolute URL works when HttpClient has no BaseAddress", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var response = await client.Url("https://localhost:5201/api/status/ok")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithAbsoluteUrl_WhenNoBaseAddress_SetsCorrectBaseUri()
    {
        await Scenario()
            .Step("BaseUri is extracted correctly from absolute URL", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var request = client.Url("https://localhost:5201/api/status/ok")
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(new Uri("https://localhost:5201/api/status/ok"), request.RequestUri);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithAbsoluteUrlAndPort_WhenNoBaseAddress_PreservesPort()
    {
        await Scenario()
            .Step("Non-default port is preserved in request URI", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var request = client.Url("https://localhost:5201/api/test")
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(5201, request.RequestUri!.Port);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithAbsoluteUrlAndDefaultPort_WhenNoBaseAddress_OmitsDefaultPort()
    {
        await Scenario()
            .Step("Default port is omitted from request URI", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var request = client.Url("https://example.com/api/test")
                    .BuildRequest(HttpMethod.Get);

                Assert.IsTrue(request.RequestUri!.IsDefaultPort);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithRelativeUrl_WhenNoBaseAddress_ThrowsArgumentException()
    {
        await Scenario()
            .Step("Relative URL without BaseAddress throws ArgumentException", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                Assert.Throws<ArgumentException>(() => client.Url("/api/status/ok"));
            })
            .Run();
    }

    [Test]
    public async Task Url_WithInvalidUrl_WhenNoBaseAddress_ThrowsArgumentException()
    {
        await Scenario()
            .Step("Invalid URL without BaseAddress throws ArgumentException", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                Assert.Throws<ArgumentException>(() => client.Url("not-a-valid-url"));
            })
            .Run();
    }

    [Test]
    public async Task Url_WithQueryParams_WhenNoBaseAddress_WorksCorrectly()
    {
        await Scenario()
            .Step("Query parameters work with absolute URL and no BaseAddress", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var response = await client.Url("https://localhost:5201/api/echo")
                    .WithQueryParam("param1", "value1")
                    .WithQueryParam("param2", "value2")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.Contains("param1", response.Content);
                Assert.Contains("param2", response.Content);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithNullUrl_WhenNoBaseAddress_ThrowsArgumentNullException()
    {
        await Scenario()
            .Step("Null URL throws ArgumentNullException", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                Assert.Throws<ArgumentNullException>(() => client.Url(null!));
            })
            .Run();
    }

    [Test]
    public async Task Url_WithNonHttpScheme_WhenNoBaseAddress_ThrowsArgumentException()
    {
        await Scenario()
            .Step("Non-HTTP scheme throws ArgumentException", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                Assert.Throws<ArgumentException>(() => client.Url("ftp://example.com/file.txt"));
            })
            .Run();
    }

    [Test]
    public async Task Url_WithHeaders_WhenNoBaseAddress_WorksCorrectly()
    {
        await Scenario()
            .Step("Headers work with absolute URL and no BaseAddress", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var response = await client.Url("https://localhost:5201/api/echo")
                    .WithHeader("X-Custom-Header", "test-value")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithContent_WhenNoBaseAddress_WorksCorrectly()
    {
        await Scenario()
            .Step("Content works with absolute URL and no BaseAddress", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var response = await client.Url("https://localhost:5201/api/echo")
                    .WithContent(new { Name = "Test", Value = 123 })
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task Url_WithHttpScheme_WhenNoBaseAddress_WorksCorrectly()
    {
        await Scenario()
            .Step("HTTP scheme (not HTTPS) works correctly", _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient(SuiteData.NoBaseAddressClientName);

                var request = client.Url("http://example.com/api/test")
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual("http", request.RequestUri!.Scheme);
                Assert.AreEqual(new Uri("http://example.com/api/test"), request.RequestUri);
            })
            .Run();
    }
}
