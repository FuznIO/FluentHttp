using Fuzn.TestFuzn;
using System.Net;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class HttpVersionTests : Test
{
    [Test]
    public async Task Version_Http11_SetsVersionOnRequest()
    {
        await Scenario()
            .Step("Set HTTP/1.1 version on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version11)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersion.Version11, request.Version);
            })
            .Run();
    }

    [Test]
    public async Task Version_Http20_SetsVersionOnRequest()
    {
        await Scenario()
            .Step("Set HTTP/2 version on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version20)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersion.Version20, request.Version);
            })
            .Run();
    }

    [Test]
    public async Task Version_Http30_SetsVersionOnRequest()
    {
        await Scenario()
            .Step("Set HTTP/3 version on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version30)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersion.Version30, request.Version);
            })
            .Run();
    }

    [Test]
    public async Task VersionPolicy_RequestVersionExact_SetsPolicyOnRequest()
    {
        await Scenario()
            .Step("Set RequestVersionExact policy on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version20)
                    .VersionPolicy(HttpVersionPolicy.RequestVersionExact)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersionPolicy.RequestVersionExact, request.VersionPolicy);
            })
            .Run();
    }

    [Test]
    public async Task VersionPolicy_RequestVersionOrLower_SetsPolicyOnRequest()
    {
        await Scenario()
            .Step("Set RequestVersionOrLower policy on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version20)
                    .VersionPolicy(HttpVersionPolicy.RequestVersionOrLower)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersionPolicy.RequestVersionOrLower, request.VersionPolicy);
            })
            .Run();
    }

    [Test]
    public async Task VersionPolicy_RequestVersionOrHigher_SetsPolicyOnRequest()
    {
        await Scenario()
            .Step("Set RequestVersionOrHigher policy on request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version11)
                    .VersionPolicy(HttpVersionPolicy.RequestVersionOrHigher)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersionPolicy.RequestVersionOrHigher, request.VersionPolicy);
            })
            .Run();
    }

    [Test]
    public async Task Version_WithoutVersionPolicy_UsesDefaultPolicy()
    {
        await Scenario()
            .Step("Version without policy uses default", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Version(HttpVersion.Version20)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersion.Version20, request.Version);
                // Default policy is RequestVersionOrLower
                Assert.AreEqual(HttpVersionPolicy.RequestVersionOrLower, request.VersionPolicy);
            })
            .Run();
    }

    [Test]
    public async Task Version_ChainingWithOtherMethods_WorksCorrectly()
    {
        await Scenario()
            .Step("Chain version with other builder methods", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var request = client.Url("/api/echo")
                    .Header("X-Custom", "value")
                    .Version(HttpVersion.Version20)
                    .VersionPolicy(HttpVersionPolicy.RequestVersionExact)
                    .Accept(AcceptTypes.Json)
                    .BuildRequest(HttpMethod.Get);

                Assert.AreEqual(HttpVersion.Version20, request.Version);
                Assert.AreEqual(HttpVersionPolicy.RequestVersionExact, request.VersionPolicy);
                Assert.IsTrue(request.Headers.Contains("X-Custom"));
            })
            .Run();
    }

    [Test]
    public async Task Version_Http11_RequestSucceeds()
    {
        await Scenario()
            .Step("Send request with HTTP/1.1 version", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                // HTTP/1.1 should work with most servers
                var response = await client.Url("/api/echo")
                    .Version(HttpVersion.Version11)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }
}
