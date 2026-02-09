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

## Custom Serialization

By default, FluentHttp uses `System.Text.Json` with `JsonSerializerDefaults.Web` (camelCase, case-insensitive).

### Per-Request Options

Customize serialization on a per-request basis:

```csharp
.WithJsonOptions(new JsonSerializerOptions { PropertyNamingPolicy = null })
.WithSerializer(new MyCustomSerializer())
```

### Global Defaults

Configure defaults for all requests:

```csharp
FluentHttpDefaults.JsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = null };
FluentHttpDefaults.Serializer = new NewtonsoftSerializerProvider();
```

### Custom Serializer

Implement `ISerializerProvider` for custom serialization:

```csharp
public class NewtonsoftSerializerProvider : ISerializerProvider
{
    public string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj);
    public T? Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json);
}
```

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

## License

MIT License - see [LICENSE](https://github.com/FuznIO/FluentHttp/blob/main/LICENSE) for details.