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
                var client = SuiteData.Factory.CreateClient();
                
                var response = await client.Url("/api/status/ok").Get();

                Assert.IsTrue(response.Ok);
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
    public async Task AuthBearer_DuplicateAuth_ThrowsInvalidOperationException()
    {
        await Scenario()
            .Step("Setting auth twice throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/auth/bearer")
                        .AuthBearer("token1")
                        .AuthBearer("token2");
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
    public async Task AuthBasic_AfterBearer_ThrowsInvalidOperationException()
    {
        await Scenario()
            .Step("Setting basic auth after bearer throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/auth/bearer")
                        .AuthBearer("token")
                        .AuthBasic("user", "pass");
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
    public async Task AuthBearer_AfterBasic_ThrowsInvalidOperationException()
    {
        await Scenario()
            .Step("Setting bearer auth after basic throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var exceptionThrown = false;
                try
                {
                    client.Url("/api/auth/basic")
                        .AuthBasic("user", "pass")
                        .AuthBearer("token");
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
