using System.Net;

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
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "abc123")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                var cookies = body!.cookies;
                Assert.AreEqual("abc123", (string)cookies["session"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_MultipleCookies_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with multiple cookies", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie("session", "abc123")
                    .Cookie("user", "john")
                    .Get();

                Assert.IsTrue(response.Ok);

                var cookies = response.As<EchoResponse>();
                Assert.AreEqual("abc123", (string)cookies.cookies["session"]);
                Assert.AreEqual("john", (string)cookies.cookies["user"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_WithCookieObject_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Cookie object", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var cookie = new Cookie("auth", "token123");
                
                var response = await client.Url("/api/cookies/echo")
                    .Cookie(cookie)
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                var cookies = body!.cookies;
                Assert.AreEqual("token123", (string)cookies["auth"]);
            })
            .Run();
    }

    [Test]
    public async Task Cookie_ResponseSetsCookie_IsReceived()
    {
        await Scenario()
            .Step("Receive cookie from server response", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/cookies/set")
                    .QueryParam("name", "testCookie")
                    .QueryParam("value", "testValue")
                    .Get();

                Assert.IsTrue(response.Ok);
                Assert.IsTrue(response.Cookies.Count > 0);
                
                var cookie = response.Cookies.FirstOrDefault(c => c.Name == "testCookie");
                Assert.IsNotNull(cookie);
                Assert.AreEqual("testValue", cookie!.Value);
            })
            .Run();
    }

    internal class EchoResponse
    {
        public Dictionary<string, string> cookies { get; set; } = new();
    }
}
