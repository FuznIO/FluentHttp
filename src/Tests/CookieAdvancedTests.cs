using Fuzn.FluentHttp.TestApi.Models;
using System.Net;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class CookieAdvancedTests : Test
{
    [Test]
    public async Task Cookie_WithDuration_SetsExpiration()
    {
        await Scenario()
            .Step("Cookie with duration has expiration set", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "abc123", duration: TimeSpan.FromHours(1))
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<CookiesEchoResponse>();
                Assert.AreEqual("abc123", body!.Cookies["session"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_WithPath_SetsPathCorrectly()
    {
        await Scenario()
            .Step("Cookie with path is sent correctly", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "value123", path: "/")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<CookiesEchoResponse>();
                Assert.AreEqual("value123", body!.Cookies["session"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_SessionCookie_NoExpiration()
    {
        await Scenario()
            .Step("Cookie without duration is session cookie", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                // No duration specified = session cookie
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "sessionValue")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<CookiesEchoResponse>();
                Assert.AreEqual("sessionValue", body!.Cookies["session"]);
            })
            .Run();
    }
}
