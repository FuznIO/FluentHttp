# Fuzn.FluentHttp

A lightweight fluent API for building and sending HTTP requests with `HttpClient`. Provides a clean, chainable interface for configuring URLs, headers, content types, authentication, and serialization.

It's built to be easy to test: the companion **`Fuzn.FluentHttp.Testing`** package gives you an in-memory `MockHttpHandler` to unit-test your HTTP code with no live server — mock responses in any format, simulate failures and timeouts, and assert on exactly what your code sent. It works with any HttpClient and any test framework. See [Unit Testing](#unit-testing).

## Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [HTTP Methods](#http-methods)
- [Request Configuration](#request-configuration)
  - [Content](#content)
  - [Query Parameters](#query-parameters)
  - [Headers](#headers)
  - [Authentication](#authentication)
  - [Content & Accept Types](#content--accept-types)
  - [File Uploads](#file-uploads)
  - [Other Options](#other-options)
- [Working with Responses](#working-with-responses)
  - [FluentHttpResponse / FluentHttpResponse&lt;T&gt;](#fluenthttpresponse--fluenthttpresponset)
  - [Streaming Responses](#streaming-responses)
- [Serialization](#serialization)
  - [Changing the Default Serializer](#changing-the-default-serializer)
  - [Custom Serializer](#custom-serializer)
  - [Serializers by Content Type](#serializers-by-content-type)
  - [Per-Request Overrides](#per-request-overrides)
  - [Serializer Resolution Order](#serializer-resolution-order)
- [Resilience & Retry](#resilience--retry)
- [Debugging](#debugging)
- [Unit Testing](#unit-testing)
  - [At a glance](#at-a-glance)
  - [Mock a response](#mock-a-response)
  - [Match requests](#match-requests)
  - [Respond in different ways](#respond-in-different-ways)
  - [Verify what was sent](#verify-what-was-sent)
  - [Test typed and named clients (dependency injection)](#test-typed-and-named-clients-dependency-injection)
  - [Simulate failures and timeouts](#simulate-failures-and-timeouts)
  - [Different response per call (sequences)](#different-response-per-call-sequences)
  - [Serialization](#serialization-1)
- [License](#license)

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

The companion package **`Fuzn.FluentHttp.Testing`** lets you unit test code that uses FluentHttp without making live HTTP calls. It provides `MockHttpHandler`, an `HttpMessageHandler` that returns predefined responses and captures the requests your code sends.

```bash
dotnet add package Fuzn.FluentHttp.Testing
```

### At a glance

**Register & match** — `When*` selects the endpoint; chain matchers to narrow it.

| Do this | Code |
|---|---|
| Register a rule by method | `handler.WhenGet(url)` · `WhenPost` · `WhenPut` · `WhenPatch` · `WhenDelete` · `WhenHead` |
| Any method, or an explicit one | `handler.WhenAny(url)` · `handler.When(HttpMethod.Head, url)` |
| Match a method against any URL | `handler.WhenGet()` · `handler.WhenAny()` · `handler.When(HttpMethod.Head)` |
| Require a header (exact or predicate) | `.WithHeader("Authorization", "Bearer t")` · `.WithHeader("Authorization", v => v.StartsWith("Bearer"))` |
| Require a query parameter (exact or predicate) | `.WithQueryParam("page", "2")` · `.WithQueryParam("page", v => v != "0")` |
| Match the body | `.WithContent(dto)` · `.WithContent("{…}")` · `.WithContent(b => b.Contains("x"))` |
| Match on arbitrary request logic | `.WithRequest(req => req.Headers.Contains("X-A"))` |

**Respond**

| Do this | Code |
|---|---|
| Status only | `.RespondWith(HttpStatusCode.Created)` |
| Body — JSON by default, or any content type | `.RespondWithContent(dto)` · `.RespondWithContent(dto, "application/xml")` |
| Raw body (string sent as-is) | `.RespondWithContent("<rss/>", "application/xml")` |
| Custom message or factory | `.RespondWith(message)` · `.RespondWith(req => …)` · `.RespondWith(async (req, ct) => …)` |
| Transport error or timeout | `.RespondWithException(new HttpRequestException())` · `.RespondWithTimeout()` |
| Add a response header | `.WithResponseHeader("ETag", "v1")` |
| Delay the response | `.WithResponseDelay(TimeSpan.FromSeconds(2))` |
| A different response per call | `.RespondWith(a).ThenRespondWith(b)` (also `ThenRespondWithContent`/`Exception`/`Timeout`) |
| Respond to otherwise-unmatched requests | register a catch-all last: `handler.WhenAny().RespondWith(HttpStatusCode.NotFound)` |

**Verify & inspect**

| Do this | Code |
|---|---|
| Assert a rule was hit N times | `rule.MatchCount` |
| Assert over what was sent | `handler.Requests.Any(r => r.Headers.ContainsKey("X-Id"))` · `handler.Requests.Count(pred)` |
| Inspect captured requests | `handler.Requests` → `.Method` `.RequestUri` `.Headers` `.Query` `.Content` `.ContentBytes` `.ContentType` |
| Read a captured body / header / query | `req.ContentAs<T>()` · `req.Headers["X"].Contains(v)` · `req.Query["page"].Single()` |

**Build the client**

| Do this | Code |
|---|---|
| Client with a base address | `handler.CreateClient("https://api.example.com/")` |
| Client without a base address | `handler.CreateClient()` (use absolute URLs) |
| Wire into DI (typed / named clients) | `services.AddHttpClient<T>().UseMockHandler(handler)` |
| Override the serializer | `handler.WithSerializer(serializerProvider)` |

### Mock a response

Say the class you want to test takes an `HttpClient`:

```csharp
public class PersonApiClient(HttpClient httpClient)
{
    public Task<FluentHttpResponse<PersonDto>> GetPersonAsync(int id) =>
        httpClient.Url($"/api/person/{id}").Get<PersonDto>();
}
```

Set up the mock, build an `HttpClient` from it, and inject that into your class:

```csharp
var handler = new MockHttpHandler();
handler.WhenGet("/api/person/1")
    .RespondWithContent(new PersonDto { Id = 1, Name = "John Doe" });

// Build an HttpClient backed by the mock and inject it into the class under test.
var personApi = new PersonApiClient(handler.CreateClient("https://api.example.com/"));

var response = await personApi.GetPersonAsync(1);
Assert.AreEqual("John Doe", response.Data!.Name);
```

`CreateClient(baseAddress)` wires the handler and sets the base address in one call (use `CreateClient()` with no argument when your code uses absolute URLs). If your code instead resolves its `HttpClient` from `IHttpClientFactory`, skip this and swap the handler in your service registration with `UseMockHandler` — see [Test typed and named clients](#test-typed-and-named-clients-dependency-injection).

`WhenGet`/`WhenPost`/`WhenPut`/`WhenPatch`/`WhenDelete`, `When(method, url)`, and `WhenAny` register rules; chain matchers and finish with a `RespondWith*`. See [Match requests](#match-requests) for matching and rule ordering, and [Respond in different ways](#respond-in-different-ways) for the response options.

### Match requests

Target a rule precisely by combining the URL pattern with header, query, and body matchers. URL patterns may be relative or absolute and may contain `*` wildcards.

```csharp
var handler = new MockHttpHandler();

// Path only — matches regardless of any query string on the request.
handler.WhenGet("/api/persons").RespondWithContent(allPeople);

// Wildcard path segment — matches /api/persons/1, /api/persons/42, ...
handler.WhenGet("/api/persons/*").RespondWithContent(onePerson);

// Constrain by query parameter (no trailing wildcard needed).
handler.WhenGet("/api/persons").WithQueryParam("page", "2").RespondWithContent(page2);

// Or pin the query string in the pattern itself.
handler.WhenGet("/api/persons?page=2").RespondWithContent(page2);

// Match on a header.
handler.WhenGet("/api/me")
    .WithHeader("Authorization", "Bearer token")
    .RespondWith(HttpStatusCode.OK);

// Match on the request body — as an object (serialized), an exact string, or a predicate.
handler.WhenPost("/api/persons").WithContent(new PersonDto { Name = "Jane" }).RespondWith(HttpStatusCode.Created);
handler.WhenPost("/api/persons").WithContent(body => body.Contains("Jane")).RespondWith(HttpStatusCode.Created);

// Header and query values also take a predicate (no wildcard ambiguity — a literal value always matches literally).
handler.WhenGet("/api/me").WithHeader("Authorization", v => v.StartsWith("Bearer ")).RespondWith(HttpStatusCode.OK);
```

> **Path vs. query:** `WhenGet("/api/persons")` matches `/api/persons?page=2&size=10` because a pattern with no `?` ignores the query. Add `WithQueryParam(...)` to constrain it, or put the query in the pattern for an exact match.

**Order matters.** Rules are evaluated top to bottom in registration order, and the **first** rule whose method, URL, and matchers all pass handles the request — the rest are skipped. A matched rule is not "used up"; it keeps handling every request it matches. So a broad rule registered before a specific one hides it — register specific rules first:

```csharp
// Wrong order: the wildcard also matches /api/persons/1, so the second rule never runs.
handler.WhenGet("/api/persons/*").RespondWith(HttpStatusCode.OK);
handler.WhenGet("/api/persons/1").RespondWithContent(person);

// Correct order: /api/persons/1 gets its own response; every other id hits the wildcard.
handler.WhenGet("/api/persons/1").RespondWithContent(person);
handler.WhenGet("/api/persons/*").RespondWith(HttpStatusCode.OK);
```

A request that matches no rule throws a `MockHttpException`, so a call your test didn't set up fails loudly instead of silently getting an unexpected response. To return a response for otherwise-unmatched requests instead, register a catch-all rule **last** — `handler.WhenAny().RespondWith(HttpStatusCode.NotFound)` — and, because first-match-wins, your specific rules still take precedence.

### Respond in different ways

```csharp
var handler = new MockHttpHandler();

// Status only, a serialized body (JSON by default), or a body with an explicit status.
handler.WhenDelete("/api/person/1").RespondWith(HttpStatusCode.NoContent);
handler.WhenGet("/api/person/1").RespondWithContent(new PersonDto { Id = 1 });
handler.WhenPost("/api/person").RespondWithContent(new PersonDto { Id = 2 }, statusCode: HttpStatusCode.Created);

// Any content type — serialized with the serializer registered for it; a string body is sent as-is.
handler.WhenGet("/api/person.xml").RespondWithContent(new PersonDto { Id = 1 }, "application/xml");
handler.WhenGet("/api/feed").RespondWithContent("<rss/>", "application/xml");

// An extra response header, and an artificial delay (e.g. to exercise client timeouts).
handler.WhenGet("/api/slow")
    .WithResponseHeader("X-Cache", "MISS")
    .WithResponseDelay(TimeSpan.FromMilliseconds(200))
    .RespondWith(HttpStatusCode.OK);

// Build the response from the incoming request (sync or async factory).
handler.WhenPost("/api/echo")
    .RespondWith(async (req, ct) =>
    {
        var body = req.Content is null ? "" : await req.Content.ReadAsStringAsync(ct);
        return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body) };
    });

// Or return a fully custom HttpResponseMessage.
handler.WhenGet("/api/teapot").RespondWith(new HttpResponseMessage((HttpStatusCode)418));
```

### Verify what was sent

```csharp
var handler = new MockHttpHandler();
var rule = handler.WhenPost("/api/person*").WithHeader("Authorization");
rule.RespondWith(HttpStatusCode.Created);

var httpClient = handler.CreateClient();
await httpClient.Url("https://api.example.com/api/person")
    .WithAuthBearer("token")
    .WithContent(new PersonDto { Name = "Jane" })
    .Post();

Assert.AreEqual(1, rule.MatchCount);
var sent = handler.Requests.Single();
Assert.IsTrue(sent.Headers["Authorization"].Contains("Bearer token"));
Assert.AreEqual("Jane", sent.ContentAs<PersonDto>()!.Name);
```

Use `rule.MatchCount` to assert how many times a rule fired, and assert over `handler.Requests` with your test framework and LINQ (e.g. `handler.Requests.Count(r => …)`). Each `CapturedRequest` exposes `Method`, `RequestUri`, `Headers`, `Query`, `Content`, `ContentBytes` (raw bytes for binary/multipart bodies), and `ContentAs<T>()`.

### Test typed and named clients (dependency injection)

When your code resolves an `HttpClient` from `IHttpClientFactory` (named or typed clients), register the mock as the **primary** message handler with `UseMockHandler` — any `DelegatingHandler`s on the client still run in front of it:

```csharp
var handler = new MockHttpHandler();
handler.WhenGet("/api/person/1").RespondWithContent(new PersonDto { Id = 1, Name = "John Doe" });

var services = new ServiceCollection();
services.AddHttpClient<PersonApiClient>(c => c.BaseAddress = new Uri("https://api.example.com/"))
    .UseMockHandler(handler);

using var provider = services.BuildServiceProvider();
var personApi = provider.GetRequiredService<PersonApiClient>();
// PersonApiClient's requests are now served by the mock — no network, no test server.
```

### Simulate failures and timeouts

```csharp
var handler = new MockHttpHandler();

handler.WhenGet("/api/down").RespondWithException(new HttpRequestException("connection refused"));
handler.WhenGet("/api/slow").RespondWithTimeout();

var httpClient = handler.CreateClient("https://api.example.com/");

await Assert.ThrowsAsync<HttpRequestException>(() => httpClient.Url("/api/down").Get());
await Assert.ThrowsAsync<TaskCanceledException>(() => httpClient.Url("/api/slow").Get());
```

### Different response per call (sequences)

Use `ThenRespondWith*` to return a different response on each successive matched request — handy for testing retry, polling, or token-refresh flows. The last response repeats once the sequence is exhausted.

```csharp
var handler = new MockHttpHandler();

// Polling: pending on the first call, done on the next.
handler.WhenGet("/api/job/status")
    .RespondWithContent(new { status = "pending" })
    .ThenRespondWithContent(new { status = "done" });

// Retry: transient failure first, success on retry.
handler.WhenGet("/api/flaky")
    .RespondWithException(new HttpRequestException("transient"))
    .ThenRespondWith(HttpStatusCode.OK);
```

Every `RespondWith*` variant has a sequence form (`ThenRespondWith`, `ThenRespondWithContent`, `ThenRespondWithException`, `ThenRespondWithTimeout`). A fresh `RespondWith*` call restarts the sequence.

### Serialization

Object-based `RespondWithContent(...)`, object-based `WithContent(...)` matching, and `CapturedRequest.ContentAs<T>()` all use the same serializer FluentHttp resolves for the message's Content-Type against `FluentHttpDefaults.Serializers` (falling back to its default). Resolution happens per request, so tests use whatever serializer your code is configured with, without any setup. Override it for a single handler with `handler.WithSerializer(...)` — only needed when the code under test sets a per-request serializer the handler can't otherwise see.

One caveat: an object response body is produced with the serializer's `Serialize` (the same one used to send requests), so it accurately mirrors a real response only when that serializer round-trips symmetrically. To pin an exact wire payload, pass a **string** body — `RespondWithContent("<literal>", contentType)` is sent as-is, unserialized.

## License

MIT License - see [LICENSE](https://github.com/FuznIO/FluentHttp/blob/main/LICENSE) for details.