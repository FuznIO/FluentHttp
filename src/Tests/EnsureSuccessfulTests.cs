using System.Net;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class EnsureSuccessfulTests : Test
{
    [Test]
    public async Task EnsureSuccessful_SuccessfulResponse_ReturnsSameInstance()
    {
        await Scenario()
            .Step("EnsureSuccessful returns same instance for successful response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/ok").Get();
                var result = response.EnsureSuccessful();

                Assert.AreSame(response, result);
            })
            .Run();
    }

    [Test]
    public async Task EnsureSuccessful_SuccessfulResponse_DoesNotThrow()
    {
        await Scenario()
            .Step("EnsureSuccessful does not throw for 2xx responses", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/ok").Get();

                // Should not throw
                response.EnsureSuccessful();

                Assert.IsTrue(response.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task EnsureSuccessful_Created_DoesNotThrow()
    {
        await Scenario()
            .Step("EnsureSuccessful does not throw for 201 Created", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/created").Get();

                response.EnsureSuccessful();

                Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task EnsureSuccessful_BadRequest_ThrowsHttpRequestException()
    {
        await Scenario()
            .Step("EnsureSuccessful throws HttpRequestException for 400 Bad Request", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/badrequest").Get();

                HttpRequestException? caughtException = null;
                try
                {
                    response.EnsureSuccessful();
                }
                catch (HttpRequestException ex)
                {
                    caughtException = ex;
                }

                Assert.IsNotNull(caughtException, "Expected HttpRequestException to be thrown");
                Assert.AreEqual(HttpStatusCode.BadRequest, caughtException!.StatusCode);
                Assert.IsTrue(caughtException.Message.Contains("400"));
            })
            .Run();
    }

    [Test]
    public async Task EnsureSuccessful_NotFound_ThrowsHttpRequestException()
    {
        await Scenario()
            .Step("EnsureSuccessful throws HttpRequestException for 404 Not Found", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/notfound").Get();

                HttpRequestException? caughtException = null;
                try
                {
                    response.EnsureSuccessful();
                }
                catch (HttpRequestException ex)
                {
                    caughtException = ex;
                }

                Assert.IsNotNull(caughtException, "Expected HttpRequestException to be thrown");
                Assert.AreEqual(HttpStatusCode.NotFound, caughtException!.StatusCode);
                Assert.IsTrue(caughtException.Message.Contains("404"));
            })
            .Run();
    }

    [Test]
    public async Task EnsureSuccessful_InternalServerError_ThrowsHttpRequestException()
    {
        await Scenario()
            .Step("EnsureSuccessful throws HttpRequestException for 500 Internal Server Error", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/error").Get();

                HttpRequestException? caughtException = null;
                try
                {
                    response.EnsureSuccessful();
                }
                catch (HttpRequestException ex)
                {
                    caughtException = ex;
                }

                Assert.IsNotNull(caughtException, "Expected HttpRequestException to be thrown");
                Assert.AreEqual(HttpStatusCode.InternalServerError, caughtException!.StatusCode);
                Assert.IsTrue(caughtException.Message.Contains("500"));
            })
            .Run();
    }

    [Test]
    public async Task EnsureSuccessful_MethodChaining_Works()
    {
        await Scenario()
            .Step("EnsureSuccessful supports method chaining", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/response/json").Get();

                var content = response.EnsureSuccessful().Content;

                Assert.IsNotNull(content);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_EnsureSuccessful_SuccessfulResponse_ReturnsSameInstance()
    {
        await Scenario()
            .Step("HttpStreamResponse EnsureSuccessful returns same instance for successful response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                await using var streamResponse = await client.Url("/api/stream/download").GetStream();
                var result = streamResponse.EnsureSuccessful();

                Assert.AreSame(streamResponse, result);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_EnsureSuccessful_SuccessfulResponse_DoesNotThrow()
    {
        await Scenario()
            .Step("HttpStreamResponse EnsureSuccessful does not throw for 2xx responses", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                streamResponse.EnsureSuccessful();

                Assert.IsTrue(streamResponse.IsSuccessful);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_EnsureSuccessful_NotFound_ThrowsHttpRequestException()
    {
        await Scenario()
            .Step("HttpStreamResponse EnsureSuccessful throws HttpRequestException for 404", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                await using var streamResponse = await client.Url("/api/status/notfound").GetStream();

                HttpRequestException? caughtException = null;
                try
                {
                    streamResponse.EnsureSuccessful();
                }
                catch (HttpRequestException ex)
                {
                    caughtException = ex;
                }

                Assert.IsNotNull(caughtException, "Expected HttpRequestException to be thrown");
                Assert.AreEqual(HttpStatusCode.NotFound, caughtException!.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task StreamResponse_EnsureSuccessful_MethodChaining_Works()
    {
        await Scenario()
            .Step("HttpStreamResponse EnsureSuccessful supports method chaining", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                await using var streamResponse = await client.Url("/api/stream/download").GetStream();

                var bytes = await streamResponse.EnsureSuccessful().GetBytes();

                Assert.IsNotNull(bytes);
                Assert.IsGreaterThan(0, bytes.Length);
            })
            .Run();
    }
}
