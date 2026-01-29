using Fuzn.FluentHttp.TestApi.Models;
using System.Text;
using Fuzn.TestFuzn;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class FileUploadTests : Test
{
    [Test]
    public async Task WithFile_ByteArray_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file from byte array", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("Test file content");
                
                var response = await client.Url("/api/files/upload-single")
                    .WithFile("file", "test.txt", fileContent, "text/plain")
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleFileUploadResponse>();
                Assert.AreEqual("test.txt", body!.FileName);
                Assert.AreEqual("text/plain", body.ContentType);
                Assert.AreEqual(fileContent.Length, body.Length);
            })
            .Run();
    }

    [Test]
    public async Task WithFile_Stream_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file from stream", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("Stream file content");
                using var stream = new MemoryStream(fileContent);
                
                var response = await client.Url("/api/files/upload-single")
                    .WithFile("file", "stream-test.txt", stream, "text/plain")
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleFileUploadResponse>();
                Assert.AreEqual("stream-test.txt", body!.FileName);
            })
            .Run();
    }

    [Test]
    public async Task WithFile_FileContentRecord_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file using FileContent record", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("FileContent record test");
                var file = new FileContent("document", "document.txt", fileContent, "text/plain");
                
                var response = await client.Url("/api/files/upload-single")
                    .WithFile(file)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<SingleFileUploadResponse>();
                Assert.AreEqual("document", body!.Name);
                Assert.AreEqual("document.txt", body.FileName);
            })
            .Run();
    }

    [Test]
    public async Task WithFile_MultipleFiles_UploadCorrectly()
    {
        await Scenario()
            .Step("Upload multiple files", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var file1Content = Encoding.UTF8.GetBytes("File 1 content");
                var file2Content = Encoding.UTF8.GetBytes("File 2 content");
                
                var response = await client.Url("/api/files/upload")
                    .WithFile("file1", "file1.txt", file1Content)
                    .WithFile("file2", "file2.txt", file2Content)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MultipleFileUploadResponse>();
                Assert.HasCount(2, body!.Files);
            })
            .Run();
    }

    [Test]
    public async Task WithFormField_WithFile_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file with form fields", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("File with form data");
                
                var response = await client.Url("/api/files/upload")
                    .WithFile("file", "test.txt", fileContent)
                    .WithFormField("description", "Test file description")
                    .WithFormField("category", "documents")
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MultipleFileUploadResponse>();
                Assert.AreEqual("Test file description", body!.Fields["description"]);
                Assert.AreEqual("documents", body.Fields["category"]);
            })
            .Run();
    }

    [Test]
    public async Task WithFormFields_Dictionary_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload with form fields from dictionary", async _ =>
            {
                var client = SuiteData.HttpClientFactory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("File content");
                var formFields = new Dictionary<string, string>
                {
                    ["field1"] = "value1",
                    ["field2"] = "value2"
                };
                
                var response = await client.Url("/api/files/upload")
                    .WithFile("file", "test.txt", fileContent)
                    .WithFormFields(formFields)
                    .Post();

                Assert.IsTrue(response.IsSuccessful);
                
                var body = response.ContentAs<MultipleFileUploadResponse>();
                Assert.AreEqual("value1", body!.Fields["field1"]);
                Assert.AreEqual("value2", body.Fields["field2"]);
            })
            .Run();
    }
}
