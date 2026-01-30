# Fuzn.FluentHttp

A lightweight fluent API for building and sending HTTP requests with `HttpClient`. Provides a clean, chainable interface for configuring URLs, headers, content types, authentication, and serialization.

## Installation

```bash
dotnet add package Fuzn.FluentHttp
```

## Quick Start

### With Dependency Injection (Recommended)

Register `HttpClient` in your DI container:

```csharp
// In Program.cs or Startup.cs
services.AddHttpClient();

// Or with a named client and base address
services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});
```

Inject and use in your service:

```csharp
using Fuzn.FluentHttp;

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

// Register your service with a typed client
services.AddHttpClient<UserHttpClient>(client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
});
```

### With IHttpClientFactory

For scenarios where you need to create clients manually:

```csharp
using Fuzn.FluentHttp;

public class UserHttpClient(IHttpClientFactory httpClientFactory)
{
    public async Task<User?> GetUserAsync(int id)
    {
        var httpClient = httpClientFactory.CreateClient("MyApi");
        
        var response = await httpClient
            .Url($"/users/{id}")
            .Get<User>();

        return response.IsSuccessful ? response.Data : null;
    }
}
```

> **Why use `IHttpClientFactory`?** Creating `HttpClient` instances with `new HttpClient()` can lead to socket exhaustion and DNS caching issues. `IHttpClientFactory` manages the underlying `HttpMessageHandler` instances, providing proper pooling and lifetime management. See [Microsoft's guidance](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines) for details.

## Features

### Typed Responses

Use generic HTTP methods to get strongly-typed responses:

```csharp
var response = await httpClient.Url("/api/users/1").Get<User>();

if (response.IsSuccessful)
{
    User user = response.Data!;
}
else
{
    Console.WriteLine($"Error {response.StatusCode}: {response.Content}");
}
```

### HTTP Methods

All standard HTTP methods are supported, with both generic and non-generic versions:

```csharp
// Non-generic (returns HttpResponse)
await httpClient.Url("/api/resource").Get();
await httpClient.Url("/api/resource").Post();
await httpClient.Url("/api/resource").Put();
await httpClient.Url("/api/resource").Patch();
await httpClient.Url("/api/resource").Delete();
await httpClient.Url("/api/resource").Head();
await httpClient.Url("/api/resource").Options();

// Generic (returns HttpResponse<T>)
await httpClient.Url("/api/resource").Get<MyType>();
await httpClient.Url("/api/resource").Post<MyType>();
await httpClient.Url("/api/resource").Put<MyType>();
await httpClient.Url("/api/resource").Patch<MyType>();
await httpClient.Url("/api/resource").Delete<MyType>();
```

### Custom HTTP Methods

For non-standard HTTP methods (e.g., WebDAV's PROPFIND, MKCOL), use the `Send` method:

```csharp
// Custom HTTP method
var response = await httpClient
    .Url("https://webdav.example.com/folder")
    .Send(new HttpMethod("PROPFIND"));

// With typed response
var response = await httpClient
    .Url("https://webdav.example.com/folder")
    .WithContent(propfindRequest)
    .Send<PropfindResponse>(new HttpMethod("MKCOL"));

// With streaming response
await using var streamResponse = await httpClient
    .Url("https://webdav.example.com/largefile")
    .SendStream(new HttpMethod("PROPFIND"));
```

### Request Content

```csharp
var response = await httpClient
    .Url("https://api.example.com/users")
    .WithContent(new { Name = "John", Email = "john@example.com" })
    .Post<User>();
```

### Query Parameters

```csharp
// Single parameter
var response = await httpClient
    .Url("https://api.example.com/search")
    .WithQueryParam("q", "dotnet")
    .WithQueryParam("page", 1)
    .Get<SearchResult>();

// Multiple parameters via dictionary
var response = await httpClient
    .Url("https://api.example.com/search")
    .WithQueryParams(new Dictionary<string, object?> 
    { 
        ["q"] = "dotnet", 
        ["page"] = 1 
    })
    .Get<SearchResult>();

// Anonymous object
var response = await httpClient
    .Url("https://api.example.com/search")
    .WithQueryParams(new { q = "dotnet", page = 1 })
    .Get<SearchResult>();

// Multiple values for same parameter
var response = await httpClient
    .Url("https://api.example.com/items")
    .WithQueryParam("tags", new[] { "c#", "dotnet", "http" })
    .Get<ItemList>();
```

### Headers

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithHeader("X-Custom-Header", "value")
    .WithHeaders(new Dictionary<string, string> 
    { 
        ["X-Another"] = "another-value" 
    })
    .Get<Data>();
```

### Authentication

```csharp
// Bearer token
var response = await httpClient
    .Url("https://api.example.com/protected")
    .WithAuthBearer("your-jwt-token")
    .Get<ProtectedData>();

// Basic authentication
var response = await httpClient
    .Url("https://api.example.com/protected")
    .WithAuthBasic("username", "password")
    .Get<ProtectedData>();

// API Key
var response = await httpClient
    .Url("https://api.example.com/protected")
    .WithAuthApiKey("your-api-key")
    .Get<ProtectedData>();

// Custom header name for API key
var response = await httpClient
    .Url("https://api.example.com/protected")
    .WithAuthApiKey("your-api-key", "Authorization")
    .Get<ProtectedData>();
```

### Content Types

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithContentType(ContentTypes.Json)
    .WithContent(data)
    .Post<Result>();

// Custom content type
var response = await httpClient
    .Url("https://api.example.com/graphql")
    .WithContentType("application/graphql")
    .WithContent(query)
    .Post<GraphQLResponse>();
```

### Accept Headers

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithAccept(AcceptTypes.Json)
    .Get<Data>();

// Custom accept type
var response = await httpClient
    .Url("https://api.example.com/report")
    .WithAccept("application/pdf")
    .Get();
```

### File Uploads

```csharp
// Upload file from stream
var response = await httpClient
    .Url("https://api.example.com/upload")
    .WithFile("file", "document.pdf", fileStream, "application/pdf")
    .Post<UploadResult>();

// Upload file from byte array
var response = await httpClient
    .Url("https://api.example.com/upload")
    .WithFile("file", "image.png", imageBytes, "image/png")
    .Post<UploadResult>();

// Multiple files with form fields
var response = await httpClient
    .Url("https://api.example.com/upload")
    .WithFile("file1", "doc1.pdf", stream1)
    .WithFile("file2", "doc2.pdf", stream2)
    .WithFormField("description", "My documents")
    .Post<UploadResult>();
```

### Cookies

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithCookie("session", "abc123")
    .WithCookie("preference", "dark-mode", path: "/", duration: TimeSpan.FromDays(30))
    .Get<Data>();
```

### Timeouts

```csharp
var response = await httpClient
    .Url("https://api.example.com/slow-endpoint")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .Get<Data>();
```

### Custom User-Agent

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithUserAgent("MyApp/1.0")
    .Get<Data>();
```

### HTTP Version

Control the HTTP protocol version for the request. Useful for HTTP/2, HTTP/3, or legacy system compatibility:

```csharp
using System.Net;

// Force HTTP/2
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithVersion(HttpVersion.Version20)
    .Get<Data>();

// Force HTTP/3 (QUIC)
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithVersion(HttpVersion.Version30)
    .Get<Data>();

// HTTP/1.1 for legacy systems
var response = await httpClient
    .Url("https://legacy-api.example.com/data")
    .WithVersion(HttpVersion.Version11)
    .Get<Data>();
```

### HTTP Version Policy

Control how the HTTP version is negotiated with the server:

```csharp
// Require exact version match (fail if server doesn't support it)
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithVersion(HttpVersion.Version20)
    .WithVersionPolicy(HttpVersionPolicy.RequestVersionExact)
    .Get<Data>();

// Allow downgrade to lower versions
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithVersion(HttpVersion.Version20)
    .WithVersionPolicy(HttpVersionPolicy.RequestVersionOrLower)
    .Get<Data>();

// Allow upgrade to higher versions
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithVersion(HttpVersion.Version11)
    .WithVersionPolicy(HttpVersionPolicy.RequestVersionOrHigher)
    .Get<Data>();
```

## Working with Responses

### `HttpResponse`

```csharp
var response = await httpClient
    .Url("https://api.example.com/users/1")
    .Get();

// Check if successful (2xx status code)
if (response.IsSuccessful)
{
    // Deserialize to type
    var user = response.ContentAs<User>();
    
    // Get content as string
    string content = response.Content;
}

// Access status code and reason
HttpStatusCode status = response.StatusCode;
string? reason = response.ReasonPhrase;

// Access content metadata
string? contentType = response.ContentType;
long? contentLength = response.ContentLength;

// Access HTTP version
Version version = response.Version;

// Access headers
var headers = response.Headers;
var contentHeaders = response.ContentHeaders;

// Access cookies
var cookies = response.Cookies;

// Access the underlying request message
HttpRequestMessage request = response.RequestMessage;
```

### `HttpResponse<T>`

```csharp
var response = await httpClient
    .Url("https://api.example.com/users/1")
    .Get<User>();

// Typed data (auto-deserialized)
User? user = response.Data;

// Check success
bool success = response.IsSuccessful;

// Status code
HttpStatusCode status = response.StatusCode;

// Content as string
string content = response.Content;

// Headers and cookies
var headers = response.Headers;
var cookies = response.Cookies;

// Deserialize to a different type
var error = response.ContentAs<ProblemDetails>();
```

## Debugging

### ToString() for Request Inspection

The builder overrides `ToString()` to provide a formatted view of the current request configuration. This is useful for logging and debugging:

```csharp
var builder = httpClient
    .Url("https://api.example.com/users")
    .WithContent(new { Name = "John" })
    .WithAuthBearer("token123");

// Log the request
Console.WriteLine(builder);

// Output:
// === FluentHttp Request ===
// Method: (not set)
// URL: https://api.example.com/users
// Headers:
//   Authorization: [REDACTED]
// Content-Type: application/json
// Accept: application/json
// Content: {"Name":"John"}
```

In Visual Studio, you can also hover over the builder variable in the debugger to see this information.

### ToString() for Response Inspection

Similarly, `HttpResponse` overrides `ToString()` to provide a formatted view of the response for debugging and logging:

```csharp
var response = await httpClient
    .Url("https://api.example.com/users/1")
    .Get();

// Log the response
Console.WriteLine(response);

// Output:
// === FluentHttp Response ===
// Status: 200 OK
// Success: True
// Version: HTTP/1.1
// Headers:
//   Date: Mon, 01 Jan 2024 12:00:00 GMT
// Content Headers:
//   Content-Type: application/json; charset=utf-8
//   Content-Length: 42
// Cookies: 1
//   sessionId = abc123
// Content: {"id":1,"name":"John Doe"}
```

This is especially useful when debugging in Visual Studio - just hover over the `response` variable to see the complete formatted output, including headers, cookies, and the actual response body content (truncated to 500 characters).

### BuildRequest() for Advanced Inspection

To get the actual `HttpRequestMessage` that would be sent (after the `BeforeSend` interceptor runs):

```csharp
var builder = httpClient
    .Url("https://api.example.com/users")
    .WithContent(new { Name = "John" });

// Get the HttpRequestMessage without sending
var request = builder.BuildRequest(HttpMethod.Post);

// Inspect the actual request
Console.WriteLine($"URL: {request.RequestUri}");
Console.WriteLine($"Content: {await request.Content!.ReadAsStringAsync()}");

// You can still send the request afterwards
var response = await builder.Post();
```

## Custom Serialization

By default, FluentHttp uses `System.Text.Json` with `JsonSerializerDefaults.Web`, which provides:
- **camelCase property names** when serializing (e.g., `FirstName` becomes `"firstName"`)
- **Case-insensitive** property matching when deserializing
- **Number handling** that allows reading numbers from strings

This works well with most REST APIs. You can override these defaults per-request or globally.

### Using System.Text.Json Options (Per-Request)

```csharp
var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = null, // Preserve PascalCase
    PropertyNameCaseInsensitive = true
};

var response = await httpClient
    .Url("https://api.example.com/data")
    .WithJsonOptions(options)
    .WithContent(data)
    .Post<Result>();
```

### Using a Custom Serializer

Implement `ISerializerProvider` to use your preferred serializer:

```csharp
public class NewtonsoftSerializerProvider : ISerializerProvider
{
    public string Serialize<T>(T obj) => 
        JsonConvert.SerializeObject(obj);

    public T? Deserialize<T>(string json) => 
        JsonConvert.DeserializeObject<T>(json);
}

// Use custom serializer
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithSerializer(new NewtonsoftSerializerProvider())
    .WithContent(data)
    .Post<Result>();
```

### Setting Global Serializer Options

Use `FluentHttpDefaults.BeforeSend` to configure serialization globally:

```csharp
// Use PascalCase globally instead of the default camelCase
var globalOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = null, // Preserve PascalCase
    PropertyNameCaseInsensitive = true
};

FluentHttpDefaults.BeforeSend = builder =>
{
    // Only set if not already configured per-request
    if (builder.Data.JsonOptions is null && builder.Data.Serializer is null)
    {
        builder.WithJsonOptions(globalOptions);
    }
};
```

## Global Defaults

Use `FluentHttpDefaults.BeforeSend` to configure global behavior for all requests. The interceptor receives the builder, allowing you to inspect current request state via `builder.Data` and modify using builder methods.

### Adding Default Headers

```csharp
FluentHttpDefaults.BeforeSend = builder =>
{
    // Add correlation ID to all requests
    if (!builder.Data.Headers.ContainsKey("X-Correlation-Id"))
    {
        builder.WithHeader("X-Correlation-Id", Guid.NewGuid().ToString());
    }
    
    // Add app version header
    builder.Data.Headers.TryAdd("X-App-Version", "1.0.0");
};
```

### URL-Based Conditional Logic

```csharp
FluentHttpDefaults.BeforeSend = builder =>
{
    // Different serializer for legacy API
    if (builder.Data.AbsoluteUri.Host.Contains("legacy-api"))
    {
        builder.WithSerializer(new LegacySerializerProvider());
    }
    
    // Longer timeout for report endpoints
    if (builder.Data.RequestUrl.Contains("/reports/"))
    {
        builder.WithTimeout(TimeSpan.FromMinutes(5));
    }
};
```

### Clearing the Interceptor

```csharp
FluentHttpDefaults.BeforeSend = null;
```

> **Note:** Per-request settings always take precedence. The pattern is to check `builder.Data` first, then only set defaults if not already configured. For async operations like token refresh, use a `DelegatingHandler` instead.

## Resilience and Retry Policies

FluentHttp works seamlessly with resilience libraries like [Polly](https://github.com/App-vNext/Polly) through `HttpClient`'s `DelegatingHandler` pipeline. This is the recommended approach for implementing retry policies, circuit breakers, and other resilience patterns.

### Using Microsoft.Extensions.Http.Resilience (Recommended)

```csharp
// Install: dotnet add package Microsoft.Extensions.Http.Resilience

services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddStandardResilienceHandler(); // Adds retry, circuit breaker, and timeout policies
```

### Using Polly Directly

```csharp
// Install: dotnet add package Microsoft.Extensions.Http.Polly

services.AddHttpClient("MyApi", client =>
{
    client.BaseAddress = new Uri("https://api.example.com");
})
.AddTransientHttpErrorPolicy(builder => 
    builder.WaitAndRetryAsync(3, retryAttempt => 
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

### Custom DelegatingHandler

```csharp
public class RetryHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        HttpResponseMessage response = null!;
        for (int i = 0; i < 3; i++)
        {
            response = await base.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return response;
            
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)), cancellationToken);
        }
        return response;
    }
}

// Register
services.AddHttpClient("MyApi")
    .AddHttpMessageHandler<RetryHandler>();
```

Then use FluentHttp with the configured client:

```csharp
var client = httpClientFactory.CreateClient("MyApi");
var response = await client
    .Url("/api/users")
    .Get<User[]>();
```

## Cancellation Support

All HTTP methods support cancellation tokens:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

var response = await httpClient
    .Url("https://api.example.com/data")
    .Get<Data>(cts.Token);
```

You can also set a cancellation token on the builder, which will be linked with any token passed to the HTTP method:

```csharp
var builderCts = new CancellationTokenSource();
var methodCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

// Both tokens are linked - cancelling either will cancel the request
var response = await httpClient
    .Url("https://api.example.com/data")
    .WithCancellationToken(builderCts.Token)
    .Get<Data>(methodCts.Token);
```

## License

MIT License - see [LICENSE](https://github.com/FuznIO/FluentHttp/blob/main/LICENSE) for details.