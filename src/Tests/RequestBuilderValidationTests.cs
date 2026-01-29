using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class RequestBuilderValidationTests : Test
{
    [Test]
    public async Task Url_AbsoluteUrl_WithoutBaseAddress_Works()
    {
        await Scenario()
            .Step("Create request with absolute URL when no base address", async _ =>
            {
                var client = new HttpClient();
                
                // This should work because we provide an absolute URL
                var builder = client.Url("https://example.com/api/test");
                
                Assert.IsNotNull(builder);
            })
            .Run();
    }

    [Test]
    public async Task Url_RelativeUrl_WithBaseAddress_Works()
    {
        await Scenario()
            .Step("Create request with relative URL when base address is set", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var response = await client.Url("/api/status/ok").Get();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task Url_InvalidRelativeUrl_WithoutBaseAddress_ThrowsArgumentException()
    {
        await Scenario()
            .Step("Invalid relative URL throws exception", async _ =>
            {
                var client = new HttpClient(); // No base address
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/relative-only");
                }
                catch (ArgumentException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected ArgumentException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task WithAuthBearer_DuplicateAuth_ThrowsInvalidOperationException()
    {
        await Scenario()
            .Step("Setting auth twice throws exception", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/auth/bearer")
                        .WithAuthBearer("token1")
                        .WithAuthBearer("token2");
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task WithAuthBasic_AfterBearer_ThrowsInvalidOperationException()
    {
        await Scenario()
            .Step("Setting basic auth after bearer throws exception", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/auth/bearer")
                        .WithAuthBearer("token")
                        .WithAuthBasic("user", "pass");
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task WithAuthBearer_AfterBasic_ThrowsInvalidOperationException()
    {
        await Scenario()
            .Step("Setting bearer auth after basic throws exception", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/auth/basic")
                        .WithAuthBasic("user", "pass")
                        .WithAuthBearer("token");
                }
                catch (InvalidOperationException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected InvalidOperationException to be thrown");
            })
            .Run();
    }
}
