using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class CancellationTokenTests : Test
{
    [Test]
    public async Task Get_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled GET request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/1000").Get(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Post_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled POST request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { test = "data" })
                        .Post(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Put_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled PUT request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { id = 1 })
                        .Put(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Delete_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled DELETE request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo").Delete(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Patch_WithCancellation_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Cancelled PATCH request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { field = "value" })
                        .Patch(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Get_WithBuilderCancellationToken_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Builder-level cancelled GET request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/1000")
                        .CancellationToken(cts.Token)
                        .Get();
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Post_WithBuilderCancellationToken_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Builder-level cancelled POST request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { test = "data" })
                        .CancellationToken(cts.Token)
                        .Post();
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Get_WithBothCancellationTokens_BuilderTokenCancelled_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Request with both tokens cancels when builder token is cancelled", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var builderCts = new CancellationTokenSource();
                using var methodCts = new CancellationTokenSource();
                builderCts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/1000")
                        .CancellationToken(builderCts.Token)
                        .Get(methodCts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task Get_WithBothCancellationTokens_MethodTokenCancelled_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Request with both tokens cancels when method token is cancelled", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var builderCts = new CancellationTokenSource();
                using var methodCts = new CancellationTokenSource();
                methodCts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/1000")
                        .CancellationToken(builderCts.Token)
                        .Get(methodCts.Token);
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task GetStream_WithBuilderCancellationToken_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Builder-level cancelled GetStream request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/delay/1000")
                        .CancellationToken(cts.Token)
                        .GetStream();
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }

    [Test]
    public async Task PostStream_WithBuilderCancellationToken_ThrowsOperationCanceled()
    {
        await Scenario()
            .Step("Builder-level cancelled PostStream request throws exception", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                using var cts = new CancellationTokenSource();
                cts.Cancel();
                
                var exceptionThrown = false;
                try
                {
                    await client.Url("/api/echo")
                        .Body(new { test = "data" })
                        .CancellationToken(cts.Token)
                        .PostStream();
                }
                catch (OperationCanceledException)
                {
                    exceptionThrown = true;
                }
                
                Assert.IsTrue(exceptionThrown, "Expected OperationCanceledException to be thrown");
            })
            .Run();
    }
}
