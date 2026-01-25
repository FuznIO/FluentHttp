using System.Text;

namespace Fuzn.FluentHttp.Tests;

[TestClass]
public class FileUploadTests : Test
{
    [Test]
    public async Task AttachFile_ByteArray_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file from byte array", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("Test file content");
                
                var response = await client.Url("/api/files/upload-single")
                    .AttachFile("file", "test.txt", fileContent, "text/plain")
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("test.txt", (string)body!.fileName);
                Assert.AreEqual("text/plain", (string)body!.contentType);
                Assert.AreEqual(fileContent.Length, (long)body!.length);
            })
            .Run();
    }

    [Test]
    public async Task AttachFile_Stream_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file from stream", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("Stream file content");
                using var stream = new MemoryStream(fileContent);
                
                var response = await client.Url("/api/files/upload-single")
                    .AttachFile("file", "stream-test.txt", stream, "text/plain")
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("stream-test.txt", (string)body!.fileName);
            })
            .Run();
    }

    [Test]
    public async Task AttachFile_FileContentRecord_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file using FileContent record", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("FileContent record test");
                var file = new FileContent("document", "document.txt", fileContent, "text/plain");
                
                var response = await client.Url("/api/files/upload-single")
                    .AttachFile(file)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                Assert.AreEqual("document", (string)body!.name);
                Assert.AreEqual("document.txt", (string)body!.fileName);
            })
            .Run();
    }

    [Test]
    public async Task AttachFile_MultipleFiles_UploadCorrectly()
    {
        await Scenario()
            .Step("Upload multiple files", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var file1Content = Encoding.UTF8.GetBytes("File 1 content");
                var file2Content = Encoding.UTF8.GetBytes("File 2 content");
                
                var response = await client.Url("/api/files/upload")
                    .AttachFile("file1", "file1.txt", file1Content)
                    .AttachFile("file2", "file2.txt", file2Content)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                var files = (IEnumerable<dynamic>)body!.files;
                Assert.AreEqual(2, files.Count());
            })
            .Run();
    }

    [Test]
    public async Task FormField_WithFile_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload file with form fields", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("File with form data");
                
                var response = await client.Url("/api/files/upload")
                    .AttachFile("file", "test.txt", fileContent)
                    .FormField("description", "Test file description")
                    .FormField("category", "documents")
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                var fields = body!.fields;
                Assert.AreEqual("Test file description", (string)fields["description"]);
                Assert.AreEqual("documents", (string)fields["category"]);
            })
            .Run();
    }

    [Test]
    public async Task FormFields_Dictionary_UploadsCorrectly()
    {
        await Scenario()
            .Step("Upload with form fields from dictionary", async _ =>
            {
                var client = SuiteData.Factory.CreateClient();
                
                var fileContent = Encoding.UTF8.GetBytes("File content");
                var formFields = new Dictionary<string, string>
                {
                    ["field1"] = "value1",
                    ["field2"] = "value2"
                };
                
                var response = await client.Url("/api/files/upload")
                    .AttachFile("file", "test.txt", fileContent)
                    .FormFields(formFields)
                    .Post();

                Assert.IsTrue(response.Ok);
                
                var body = response.BodyAsJson();
                var fields = body!.fields;
                Assert.AreEqual("value1", (string)fields["field1"]);
                Assert.AreEqual("value2", (string)fields["field2"]);
            })
            .Run();
    }
}
