using Fuzn.FluentHttp.TestApi.Models;
using System.Net;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class CookieTests : Test
{
    [Test]
    public async Task Cookie_SingleCookie_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with single cookie", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "abc123")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<CookiesEchoResponse>();
                Assert.AreEqual("abc123", body!.Cookies["session"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_MultipleCookies_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple cookies", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "abc123")
                    .Cookie("user", "john")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);

                var cookies = response.As<CookiesEchoResponse>();
                Assert.AreEqual("abc123", cookies!.Cookies["session"]);
                Assert.AreEqual("john", cookies.Cookies["user"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_WithCookieObject_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Cookie object", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var cookie = new Cookie("auth", "token123");
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie(cookie)
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<CookiesEchoResponse>();
                Assert.AreEqual("token123", body!.Cookies["auth"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_ResponseSetsCookie_IsReceived()
    {
        await Scenario()
            .Step("Receive cookie from server response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/cookies/set")
                    .QueryParam("name", "testCookie")
                    .QueryParam("value", "testValue")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotEmpty(response.Cookies);
                
                var cookie = response.Cookies.FirstOrDefault(c => c.Name == "testCookie");
                Assert.IsNotNull(cookie);
                Assert.AreEqual("testValue", cookie!.Value);
            })
            .Run();
    }
}
