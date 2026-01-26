# Fuzn.FluentHttp

A lightweight fluent API for building and sending HTTP requests with `HttpClient`. Provides a clean, chainable interface for configuring URLs, headers, content types, authentication, and serialization.

## Installation

```bash
dotnet add package Fuzn.FluentHttp
```

## Quick Start

```csharp
using Fuzn.FluentHttp;

var httpClient = new HttpClient();

// Simple GET request
var response = await httpClient
    .Url("https://api.example.com/users")
    .Get();

if (response.Ok)
{
    var users = response.As<List<User>>();
}
```

## Features

### HTTP Methods

All standard HTTP methods are supported:

```csharp
await httpClient.Url("/api/resource").Get();
await httpClient.Url("/api/resource").Post();
await httpClient.Url("/api/resource").Put();
await httpClient.Url("/api/resource").Patch();
await httpClient.Url("/api/resource").Delete();
await httpClient.Url("/api/resource").Head();
await httpClient.Url("/api/resource").Options();
```

### Request Body

```csharp
var response = await httpClient
    .Url("https://api.example.com/users")
    .Body(new { Name = "John", Email = "john@example.com" })
    .Post();
```

### Query Parameters

```csharp
// Single parameter
var response = await httpClient
    .Url("https://api.example.com/search")
    .QueryParam("q", "dotnet")
    .QueryParam("page", 1)
    .Get();

// Multiple parameters via dictionary
var response = await httpClient
    .Url("https://api.example.com/search")
    .QueryParams(new Dictionary<string, object?> 
    { 
        ["q"] = "dotnet", 
        ["page"] = 1 
    })
    .Get();

// Anonymous object
var response = await httpClient
    .Url("https://api.example.com/search")
    .QueryParams(new { q = "dotnet", page = 1 })
    .Get();

// Multiple values for same parameter
var response = await httpClient
    .Url("https://api.example.com/items")
    .QueryParam("tags", new[] { "c#", "dotnet", "http" })
    .Get();
```

### Headers

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .Header("X-Custom-Header", "value")
    .Headers(new Dictionary<string, string> 
    { 
        ["X-Another"] = "another-value" 
    })
    .Get();
```

### Authentication

```csharp
// Bearer token
var response = await httpClient
    .Url("https://api.example.com/protected")
    .AuthBearer("your-jwt-token")
    .Get();

// Basic authentication
var response = await httpClient
    .Url("https://api.example.com/protected")
    .AuthBasic("username", "password")
    .Get();

// API Key
var response = await httpClient
    .Url("https://api.example.com/protected")
    .AuthApiKey("your-api-key")
    .Get();

// Custom header name for API key
var response = await httpClient
    .Url("https://api.example.com/protected")
    .AuthApiKey("your-api-key", "Authorization")
    .Get();
```

### Content Types

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .ContentType(ContentTypes.Json)
    .Body(data)
    .Post();

// Custom content type
var response = await httpClient
    .Url("https://api.example.com/graphql")
    .ContentType("application/graphql")
    .Body(query)
    .Post();
```

### Accept Headers

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .Accept(AcceptTypes.Json)
    .Get();

// Custom accept type
var response = await httpClient
    .Url("https://api.example.com/report")
    .Accept("application/pdf")
    .Get();
```

### File Uploads

```csharp
// Upload file from stream
var response = await httpClient
    .Url("https://api.example.com/upload")
    .AttachFile("file", "document.pdf", fileStream, "application/pdf")
    .Post();

// Upload file from byte array
var response = await httpClient
    .Url("https://api.example.com/upload")
    .AttachFile("file", "image.png", imageBytes, "image/png")
    .Post();

// Multiple files with form fields
var response = await httpClient
    .Url("https://api.example.com/upload")
    .AttachFile("file1", "doc1.pdf", stream1)
    .AttachFile("file2", "doc2.pdf", stream2)
    .FormField("description", "My documents")
    .Post();
```

### Cookies

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .Cookie("session", "abc123")
    .Cookie("preference", "dark-mode", path: "/", duration: TimeSpan.FromDays(30))
    .Get();
```

### Timeouts

```csharp
var response = await httpClient
    .Url("https://api.example.com/slow-endpoint")
    .Timeout(TimeSpan.FromSeconds(30))
    .Get();
```

### Custom User-Agent

```csharp
var response = await httpClient
    .Url("https://api.example.com/data")
    .UserAgent("MyApp/1.0")
    .Get();
```

## Working with Responses

```csharp
var response = await httpClient
    .Url("https://api.example.com/users/1")
    .Get();

// Check if successful
if (response.Ok)
{
    // Deserialize to type
    var user = response.As<User>();
    
    // Get raw body
    string body = response.Body;
    
    // Get as bytes
    byte[] bytes = response.AsBytes();
}

// Access status code
HttpStatusCode status = response.StatusCode;

// Access headers
var headers = response.Headers;
var contentHeaders = response.ContentHeaders;

// Access cookies
var cookies = response.Cookies;
```

## Custom Serialization

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
    .SerializerProvider(new NewtonsoftSerializerProvider())
    .Body(data)
    .Post();
```

## Cancellation Support

All HTTP methods support cancellation tokens:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

var response = await httpClient
    .Url("https://api.example.com/data")
    .Get(cts.Token);
```

You can also set a cancellation token on the builder, which will be linked with any token passed to the HTTP method:

```csharp
var builderCts = new CancellationTokenSource();
var methodCts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

// Both tokens are linked - cancelling either will cancel the request
var response = await httpClient
    .Url("https://api.example.com/data")
    .CancellationToken(builderCts.Token)
    .Get(methodCts.Token);
```

## License

MIT License - see [LICENSE](https://github.com/FuznIO/FluentHttp/blob/main/LICENSE) for details.
