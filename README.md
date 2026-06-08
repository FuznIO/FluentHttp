# Fuzn.FluentHttp

A lightweight fluent API for building and sending HTTP requests with `HttpClient`. Provides a clean, chainable interface for configuring URLs, headers, content types, authentication, and serialization.

## Installation

To get started, add the Fuzn.FluentHttp package to your project using the following command:

```bash
dotnet add package Fuzn.FluentHttp
```

## Quick Start

The following example demonstrates how to register an `HttpClient` with dependency injection and use it to make a simple GET request:

```csharp
using Fuzn.FluentHttp;

// Register HttpClient with DI
services.AddHttpClient<UserHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});

// Use in your service
public class UserHttpClient(HttpClient httpClient)
{
    public async Task<User?> GetUserAsync(int id)
    {
        var response = await httpClient
            .Url($"/users/{id}")
            .Get<User>();

        return response.IsSuccessful ? response.Data : null;
    }
}
```

**Alternative syntax** using `Request().WithUrl()`:
```csharp
var response = await httpClient.Request().WithUrl("/users/1").Get<User>();
```

> **Note:** When `HttpClient` has no `BaseAddress`, you must use absolute URLs.

## HTTP Methods

All standard HTTP methods are supported with both generic and non-generic versions. Generic methods automatically deserialize the response body:

```csharp
// Non-generic returns FluentHttpResponse
await httpClient.Url("/resource").Get();
await httpClient.Url("/resource").Post();
await httpClient.Url("/resource").Put();
await httpClient.Url("/resource").Patch();
await httpClient.Url("/resource").Delete();
await httpClient.Url("/resource").Head();
await httpClient.Url("/resource").Options();

// Generic returns FluentHttpResponse<T> with deserialized Data property
await httpClient.Url("/resource").Get<T>();
await httpClient.Url("/resource").Post<T>();

// Custom HTTP methods (e.g., WebDAV)
await httpClient.Url("/resource").Send(new HttpMethod("PROPFIND"));
await httpClient.Url("/resource").Send<T>(new HttpMethod("MKCOL"));
```

## Request Configuration

### Content

Objects are automatically serialized to JSON:

```csharp
await httpClient
    .Url("/users")
    .WithContent(new { Name = "John", Email = "john@example.com" })
    .Post<User>();
```

### Query Parameters

Add query parameters to the request URL:

```csharp
// Individual parameters (values must be strings)
.WithQueryParam("q", "dotnet")
.WithQueryParam("page", "1")

// Multiple values for same key (e.g., ?tags=c%23&tags=dotnet)
.WithQueryParam("tags", "c#")
.WithQueryParam("tags", "dotnet")

// For non-string values, convert to string yourself
.WithQueryParam("date", DateTime.UtcNow.ToString("O"))
.WithQueryParam("active", true.ToString().ToLower())
```

### Headers

```csharp
.WithHeader("X-Custom", "value")
.WithHeaders(new Dictionary<string, string> { ["X-Another"] = "value" })
```

### Authentication

Built-in support for common authentication schemes:

```csharp
.WithAuthBearer("jwt-token")
.WithAuthBasic("username", "password")
.WithAuthApiKey("api-key")                    // Uses X-API-Key header
.WithAuthApiKey("api-key", "Authorization")   // Custom header name
```

### Content & Accept Types

Control request and response content types:

```csharp
.WithContentType(ContentTypes.Json)
.WithContentType("application/graphql")
.WithAccept(AcceptTypes.Json)
.WithAccept("application/pdf")
```

### File Uploads

Upload files with automatic multipart/form-data handling:

```csharp
await httpClient
    .Url("/upload")
    .WithFile("file", "doc.pdf", fileStream, "application/pdf")
    .WithFormField("description", "My document")
    .Post<UploadResult>();
```

### Other Options

```csharp
.WithTimeout(TimeSpan.FromSeconds(30))
.WithUserAgent("MyApp/1.0")
.WithCookie("session", "abc123")
.WithVersion(HttpVersion.Version20)
.WithVersionPolicy(HttpVersionPolicy.RequestVersionExact)
.WithCancellationToken(cancellationToken)
```

## Working with Responses

### FluentHttpResponse / FluentHttpResponse&lt;T&gt;

Responses provide easy access to status, content, headers, and cookies. The response body is only deserialized when you access the `Data` property, not automatically upon receiving the response:

```csharp
var response = await httpClient.Url("/users/1").Get<User>();

// Check status
if (response.IsSuccessful)
{
    User user = response.Data!;  // Deserialization happens here
}

// Or throw HttpRequestException on failure
response.EnsureSuccessful();

// Access response properties
HttpStatusCode status = response.StatusCode;
string? reason = response.ReasonPhrase;
string content = response.Content;
string? contentType = response.ContentType;
long? contentLength = response.ContentLength;
Version version = response.Version;

// Access headers and cookies
var headers = response.Headers;
var contentHeaders = response.ContentHeaders;
var cookies = response.Cookies;

// Access underlying messages
HttpResponseMessage inner = response.InnerResponse;
HttpRequestMessage request = response.RequestMessage;

// Deserialize to different type (useful for error responses)
var error = response.ContentAs<ProblemDetails>();

// Try deserialize without throwing
if (response.TryContentAs<User>(out var user))
{
    // Use user
}
```

### Streaming Responses

For large files, use streaming to avoid loading the entire response into memory. The `FluentHttpStreamResponse` must be disposed after use:

```csharp
await using var response = await httpClient.Url("/files/large.zip").GetStream();

if (response.IsSuccessful)
{
    // Access metadata
    long? size = response.ContentLength;
    string? type = response.ContentType;
    string? fileName = response.FileName;  // From Content-Disposition header

    // Read as stream or bytes
    var stream = await response.GetStream();
    // Or: var bytes = await response.GetBytes();
}
```

## Serialization

By default, FluentHttp uses `System.Text.Json` with `JsonSerializerDefaults.Web` (camelCase, case-insensitive). No configuration is needed for standard JSON APIs.

### Changing the Default Serializer

The default serializer is used when no content-type-specific serializer matches. To customize JSON serialization options, replace the default with a new `SystemTextJsonSerializerProvider` configured with your options:

```csharp
// Change JSON options (e.g., use PascalCase instead of camelCase)
FluentHttpDefaults.Serializers.Default = new SystemTextJsonSerializerProvider(
    new JsonSerializerOptions { PropertyNamingPolicy = null });

// Or swap to a completely different serializer
FluentHttpDefaults.Serializers.Default = new NewtonsoftSerializerProvider();
```

### Custom Serializer

Implement `ISerializerProvider` to use a different serialization library:

```csharp
public class NewtonsoftSerializerProvider : ISerializerProvider
{
    public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj);
    public T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);
}

FluentHttpDefaults.Serializers.Default = new NewtonsoftSerializerProvider();
```

### Serializers by Content Type

Register serializers for specific content types. The correct serializer is automatically selected based on the request's content type for serialization and the response's `Content-Type` header for deserialization:

```csharp
FluentHttpDefaults.Serializers
    .Register("application/json", new SystemTextJsonSerializerProvider())
    .Register("application/xml", new XmlSerializerProvider());
```

### Per-Request Overrides

Override serializer resolution for a single request. You can override by content type or override all resolution entirely:

```csharp
// Override by content type
client.Url("/api/data")
    .WithSerializer("application/json", new SystemTextJsonSerializerProvider(myJsonOptions))
    .WithSerializer("application/xml", new XmlSerializerProvider())
    .WithContentType("application/xml")
    .WithContent(payload)
    .Post<MyResponse>();

// Override all serializer resolution for both request and response
client.Url("/api/data")
    .WithSerializer(new MyCustomSerializer())
    .Get();
```

### Serializer Resolution Order

1. Per-request `WithSerializer()` (overrides everything)
2. Per-request registry (via `WithSerializer(contentType, serializer)`)
3. Global registry (`FluentHttpDefaults.Serializers`)
4. Default (`FluentHttpDefaults.Serializers.Default`)

## Resilience & Retry

FluentHttp works seamlessly with `HttpClient`'s `DelegatingHandler` pipeline. Use libraries like Polly for retry policies, circuit breakers, and other resilience patterns:

```csharp
services.AddHttpClient("MyApi")
    .AddStandardResilienceHandler();  // Microsoft.Extensions.Http.Resilience
    // Or: .AddTransientHttpErrorPolicy(...) // Microsoft.Extensions.Http.Polly
```

## Debugging

Both requests and responses override `ToString()` for easy inspection:

```csharp
// Inspect request configuration
var builder = httpClient.Url("/users").WithAuthBearer("token");
Console.WriteLine(builder);  // Prints formatted request details (auth is redacted)

// Inspect response
Console.WriteLine(response);  // Prints status, headers, and content

// Get HttpRequestMessage without sending
var request = builder.BuildRequest(HttpMethod.Post);
```

## Unit Testing

The companion package **`Fuzn.FluentHttp.Testing`** lets you unit test code that uses FluentHttp without making live HTTP calls. It provides `FluentHttpMockHandler`, an `HttpMessageHandler` that returns canned responses and captures the requests your code sends.

```bash
dotnet add package Fuzn.FluentHttp.Testing
```

### Stub a response

```csharp
var handler = new FluentHttpMockHandler();
handler.WhenGet("/api/person/1")
    .RespondWithJson(new PersonDto { Id = 1, Name = "John Doe" });

// Build an HttpClient backed by the mock (or pass `handler` to your own HttpClient).
var client = handler.CreateClient("https://api.example.com/");

var response = await client.Url("/api/person/1").Get();
Assert.AreEqual("John Doe", response.ContentAs<PersonDto>()!.Name);
```

`When(method, url)`, `WhenGet`/`WhenPost`/`WhenPut`/`WhenPatch`/`WhenDelete`, and `WhenAny` register stubs. URL patterns may be relative or absolute and may contain `*` wildcards. Constrain further with `WithHeader`, `WithQueryParam`, and `WithContent`. Respond with `RespondWith` (status, optional JSON), `RespondWithJson`, `RespondWithContent` (raw string + content type), a custom `HttpResponseMessage`, or a factory.

### Verify what was sent

```csharp
var handler = new FluentHttpMockHandler();
var stub = handler.WhenPost("/api/person*").WithHeader("Authorization");
stub.RespondWith(HttpStatusCode.Created);

var client = handler.ToHttpClient();
await client.Url("https://api.example.com/api/person")
    .WithAuthBearer("token")
    .WithContent(new PersonDto { Name = "Jane" })
    .Post();

handler.VerifyMatched(stub, 1);
var sent = handler.Requests.Single();
Assert.IsTrue(sent.HasHeader("Authorization", "Bearer token"));
Assert.AreEqual("Jane", sent.ContentAs<PersonDto>()!.Name);
```

### Simulate failures and timeouts

```csharp
var handler = new FluentHttpMockHandler()
    .WithFallback(MockFallbackBehavior.RespondNotFound); // unmatched requests → 404 (default: throw)

handler.WhenGet("/api/down").RespondWithException(new HttpRequestException("connection refused"));
handler.WhenGet("/api/slow").RespondWithTimeout();

var client = handler.CreateClient("https://api.example.com/");

await Assert.ThrowsAsync<HttpRequestException>(() => client.Url("/api/down").Get());
await Assert.ThrowsAsync<TaskCanceledException>(() => client.Url("/api/slow").Get());
```

### Different response per call (sequences)

Use `ThenRespondWith*` to return a different response on each successive matched request — handy for testing retry, polling, or token-refresh flows. The last response repeats once the sequence is exhausted.

```csharp
var handler = new FluentHttpMockHandler();

// Polling: pending on the first call, done on the next.
handler.WhenGet("/api/job/status")
    .RespondWithJson(new { status = "pending" })
    .ThenRespondWithJson(new { status = "done" });

// Retry: transient failure first, success on retry.
handler.WhenGet("/api/flaky")
    .RespondWithException(new HttpRequestException("transient"))
    .ThenRespondWith(HttpStatusCode.OK);
```

Every `RespondWith*`/`ThenRespondWith*` variant has a sequence form (`ThenRespondWith`, `ThenRespondWithJson`, `ThenRespondWithContent`, `ThenRespondWithException`, `ThenRespondWithTimeout`, …). A fresh `RespondWith*` call restarts the sequence.

### Serialization

Response bodies are serialized with FluentHttp's own serializer resolution (`FluentHttpDefaults`), so mocked JSON matches what the real server would produce. Override it for a single handler with `handler.WithSerializer(...)`.

## License

MIT License - see [LICENSE](https://github.com/FuznIO/FluentHttp/blob/main/LICENSE) for details.