using System.Net;
using System.Text;
using Fuzn.FluentHttp.Testing;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests.Mock;

[TestClass]
public class MockHandlerStreamingTests : Test
{
    [Test]
    public async Task GetStream_ReadsMockContentAsStream()
    {
        await Scenario()
            .Step("Streaming download reads the mocked body", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/download")
                    .RespondWithContent("streamed-payload", "application/octet-stream");
                var client = handler.CreateClient("https://api.example.com/");

                await using var response = await client.Url("/api/download").GetStream();

                Assert.IsTrue(response.IsSuccessful);
                var bytes = await response.GetBytes();
                Assert.AreEqual("streamed-payload", Encoding.UTF8.GetString(bytes));
            })
            .Run();
    }

    [Test]
    public async Task GetStream_HonorsStatusCode()
    {
        await Scenario()
            .Step("Streaming download surfaces the mocked status code", async _ =>
            {
                var handler = new MockHttpHandler();
                handler.WhenGet("/api/missing").RespondWith(HttpStatusCode.NotFound);
                var client = handler.CreateClient("https://api.example.com/");

                await using var response = await client.Url("/api/missing").GetStream();

                Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
                Assert.IsFalse(response.IsSuccessful);
            })
            .Run();
    }
}
