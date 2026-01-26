using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class AuthenticationTests : Test
{
    [Test]
    public async Task AuthBearer_Token_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Bearer token", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/auth/bearer")
                    .AuthBearer("my-test-token-12345")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<BearerAuthResponse>();
                Assert.IsTrue(body!.Authenticated);
                Assert.AreEqual("my-test-token-12345", body.TokenReceived);
            })
            .Run();
    }

    [Test]
    public async Task AuthBearer_MissingToken_ReturnsUnauthorized()
    {
        await Scenario()
            .Step("Send request without Bearer token", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/auth/bearer").Get();

                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task AuthBasic_Credentials_AreSentCorrectly()
    {
        await Scenario()
            .Step("Send request with Basic auth credentials", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/auth/basic")
                    .AuthBasic("testuser", "testpassword")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<BasicAuthResponse>();
                Assert.IsTrue(body!.Authenticated);
                Assert.AreEqual("testuser", body.Username);
                Assert.AreEqual("testpassword", body.Password);
            })
            .Run();
    }

    [Test]
    public async Task AuthBasic_MissingCredentials_ReturnsUnauthorized()
    {
        await Scenario()
            .Step("Send request without Basic auth", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/auth/basic").Get();

                Assert.AreEqual(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task AuthApiKey_DefaultHeader_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with API key using default header", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/auth/apikey")
                    .AuthApiKey("my-api-key-secret")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<ApiKeyAuthResponse>();
                Assert.IsTrue(body!.Authenticated);
                Assert.AreEqual("my-api-key-secret", body.ApiKey);
            })
            .Run();
    }

    [Test]
    public async Task AuthApiKey_CustomHeader_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with API key using custom header name", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/auth/apikey-custom")
                    .AuthApiKey("custom-api-key", "X-My-Api-Key")
                    .Get();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.As<ApiKeyAuthResponse>();
                Assert.IsTrue(body!.Authenticated);
                Assert.AreEqual("custom-api-key", body.ApiKey);
            })
            .Run();
    }
}
