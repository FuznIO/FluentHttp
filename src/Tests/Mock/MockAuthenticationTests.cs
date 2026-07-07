using System.Net;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

/// <summary>
/// Mirrors the live authentication scenarios by asserting on the captured Authorization header (no TestApi).
/// </summary>
[TestClass]
public class MockAuthenticationTests : Test
{
    [Test]
    public async Task WithAuthBearer_SendsBearerHeader()
    {
        await Scenario()
            .Step("Bearer token is sent in the Authorization header", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/auth/bearer").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/auth/bearer").WithAuthBearer("my-token").Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.Headers["Authorization"].Contains("Bearer my-token"));
            })
            .Run();
    }

    [Test]
    public async Task WithAuthBasic_SendsBasicHeader()
    {
        await Scenario()
            .Step("Basic credentials are base64-encoded in the Authorization header", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/auth/basic").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/auth/basic").WithAuthBasic("user", "pass").Get();

                var expected = "Basic " + Convert.ToBase64String("user:pass"u8.ToArray());
                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.Headers["Authorization"].Contains(expected));
            })
            .Run();
    }

    [Test]
    public async Task WithAuthApiKey_SendsApiKeyHeader()
    {
        await Scenario()
            .Step("API key is sent in the configured header", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/auth/apikey").RespondWith(HttpStatusCode.OK);
                var client = handler.CreateClient("https://api.example.com/");

                await client.Url("/api/auth/apikey").WithAuthApiKey("secret-key").Get();

                var sent = handler.Requests.Single();
                Assert.IsTrue(sent.Headers["X-API-Key"].Contains("secret-key"));
            })
            .Run();
    }

    [Test]
    public async Task Unauthorized_WhenRuleReturns401()
    {
        await Scenario()
            .Step("Missing auth maps to a 401 rule", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/auth/bearer").WithHeader("Authorization").RespondWith(HttpStatusCode.OK);
                handler.WhenGet("/api/auth/bearer").RespondWith(HttpStatusCode.Unauthorized);
                var client = handler.CreateClient("https://api.example.com/");

                var response = await client.Url("/api/auth/bearer").Get();

                Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            })
            .Run();
    }
}
