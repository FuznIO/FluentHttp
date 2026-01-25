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
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/auth/bearer")
                    .AuthBearer("my-test-token-12345")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.IsTrue((bool)body!.authenticated);
                Assert.AreEqual("my-test-token-12345", (string)body!.tokenReceived);
            })
            .Run();
    }

    [Test]
    public async Task AuthBearer_MissingToken_ReturnsUnauthorized()
    {
        await Scenario()
            .Step("Send request without Bearer token", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
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
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/auth/basic")
                    .AuthBasic("testuser", "testpassword")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.IsTrue((bool)body!.authenticated);
                Assert.AreEqual("testuser", (string)body!.username);
                Assert.AreEqual("testpassword", (string)body!.password);
            })
            .Run();
    }

    [Test]
    public async Task AuthBasic_MissingCredentials_ReturnsUnauthorized()
    {
        await Scenario()
            .Step("Send request without Basic auth", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
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
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/auth/apikey")
                    .AuthApiKey("my-api-key-secret")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.IsTrue((bool)body!.authenticated);
                Assert.AreEqual("my-api-key-secret", (string)body!.apiKey);
            })
            .Run();
    }

    [Test]
    public async Task AuthApiKey_CustomHeader_IsSentCorrectly()
    {
        await Scenario()
            .Step("Send request with API key using custom header name", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/auth/apikey-custom")
                    .AuthApiKey("custom-api-key", "X-My-Api-Key")
                    .Get();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.IsTrue((bool)body!.authenticated);
                Assert.AreEqual("custom-api-key", (string)body!.apiKey);
            })
            .Run();
    }
}
