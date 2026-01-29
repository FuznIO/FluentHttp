using Fuzn.FluentHttp.TestApi.Models;
using Fuzn.TestFuzn;
using System.Net;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class GenericResponseTests : Test
{
    [Test]
    public async Task Get_Generic_DeserializesResponse()
    {
        await Scenario()
            .Step("GET with generic type returns typed response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/deserialize/person").Get<PersonDto>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual(1, response.Data!.Id);
                Assert.AreEqual("John Doe", response.Data.Name);
            })
            .Run();
    }

    [Test]
    public async Task Post_Generic_DeserializesResponse()
    {
        await Scenario()
            .Step("POST with generic type returns typed response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/deserialize/person")
                    .WithContent(new PersonDto { Id = 1, Name = "Test", Email = "test@test.com", Age = 25 })
                    .Post<DeserializeResponse>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("PersonDto", response.Data!.Type);
            })
            .Run();
    }

    [Test]
    public async Task Put_Generic_DeserializesResponse()
    {
        await Scenario()
            .Step("PUT with generic type returns typed response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/methods/put")
                    .WithContent(new { id = 1, name = "Updated" })
                    .Put<MethodResponseWithBody>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("PUT", response.Data!.Method);
            })
            .Run();
    }

    [Test]
    public async Task Delete_Generic_DeserializesResponse()
    {
        await Scenario()
            .Step("DELETE with generic type returns typed response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/methods/delete").Delete<MethodResponse>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("DELETE", response.Data!.Method);
            })
            .Run();
    }

    [Test]
    public async Task Patch_Generic_DeserializesResponse()
    {
        await Scenario()
            .Step("PATCH with generic type returns typed response", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/methods/patch")
                    .WithContent(new { field = "value" })
                    .Patch<MethodResponseWithBody>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("PATCH", response.Data!.Method);
            })
            .Run();
    }

    [Test]
    public async Task GenericResponse_InheritsHttpResponseProperties()
    {
        await Scenario()
            .Step("HttpResponse<T> inherits all HttpResponse properties", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/deserialize/person").Get<PersonDto>();

                // Inherited properties
                Assert.IsNotNull(response.Headers);
                Assert.IsNotNull(response.InnerResponse);
                Assert.IsNotNull(response.Content);
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            })
            .Run();
    }

    [Test]
    public async Task GenericResponse_ContentAs_CanDeserializeToDifferentType()
    {
        await Scenario()
            .Step("Can use inherited ContentAs<T> to deserialize to different type", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                // Get response typed as PersonDto
                var response = await client.Url("/api/deserialize/person").Get<PersonDto>();

                Assert.IsTrue(response.IsSuccessful);

                // But can also deserialize to a different type using inherited ContentAs<T>
                var asDict = response.ContentAs<Dictionary<string, object>>();
                Assert.IsNotNull(asDict);
            })
            .Run();
    }

    [Test]
    public async Task GenericResponse_FailedRequest_StillDeserializesData()
    {
        await Scenario()
            .Step("Data deserializes content even when request fails", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/badrequest").Get<ErrorResponse>();

                Assert.IsFalse(response.IsSuccessful);
                Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

                // Data is deserialized even on error responses
                Assert.IsNotNull(response.Data);
                Assert.AreEqual("Bad request", response.Data!.Error);
            })
            .Run();
    }

    [Test]
    public async Task GenericResponse_EmptyContent_DataIsDefault()
    {
        await Scenario()
            .Step("Data returns default when content is empty", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/nocontent").Get<PersonDto>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

                // Empty content returns default
                Assert.IsNull(response.Data);
            })
            .Run();
    }

    [Test]
    public async Task GenericResponse_CanDeserializeToAlternateType()
    {
        await Scenario()
            .Step("Can use ContentAs<T> to deserialize to different type", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/badrequest").Get<PersonDto>();

                Assert.IsFalse(response.IsSuccessful);

                // Use ContentAs<T> to deserialize to error type
                var error = response.ContentAs<ErrorResponse>();
                Assert.IsNotNull(error);
                Assert.AreEqual("Bad request", error!.Error);
            })
            .Run();
    }

    [Test]
    public async Task GenericResponse_InvalidJson_ThrowsOnDataAccess()
    {
        await Scenario()
            .Step("Accessing Data throws when content cannot be deserialized", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();

                var response = await client.Url("/api/status/invalid-json").Get<PersonDto>();

                Assert.IsTrue(response.IsSuccessful);
                Assert.AreEqual("this is not valid json {{{", response.Content);

                // Accessing Data should throw because JSON is invalid
                var exceptionThrown = false;
                try
                {
                    var data = response.Data;
                }
                catch (Exception ex)
                {
                    exceptionThrown = true;
                    Assert.Contains("Unable to deserialize", ex.Message);
                }

                Assert.IsTrue(exceptionThrown, "Expected exception when deserializing invalid JSON");
            })
            .Run();
    }
}
