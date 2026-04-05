# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Test Commands

```bash
# Build
dotnet build src

# Run all tests
dotnet test src

# Run a single test by name
dotnet test src --filter "FullyQualifiedName~Fuzn.FluentHttp.Tests.ClassName.MethodName"

# Pack NuGet package
dotnet pack src/FluentHttp/FluentHttp.csproj --configuration Release
```

Solution file: `src/FluentHttp.slnx`. All dotnet commands use `src` as the path argument.

## Project Structure

- **src/FluentHttp/** - Main library (NuGet package: `Fuzn.FluentHttp`)
- **src/Tests/** - Integration tests using MSTest + TestFuzn adapter
- **src/TestApi/** - ASP.NET Core minimal API that serves as the test server (https://localhost:5201/)

## Architecture

The library provides a fluent builder API over `HttpClient` for constructing and sending HTTP requests.

**Request flow:** `HttpClient` -> `HttpClientExtensions.Request()`/`.Url()` -> `FluentHttpRequest` (builder with chaining) -> HTTP method call (`Get()`, `Post()`, etc.) -> `FluentHttpResponse` / `FluentHttpResponse<T>` / `FluentHttpStreamResponse`

Key types:
- **FluentHttpRequest** - Core builder. Holds `FluentHttpRequestData` internally. All `With*()` methods configure the request; HTTP method calls (`Get()`, `Post()`, etc.) execute it.
- **FluentHttpResponse** - Response wrapper with `ContentAs<T>()` for deserialization. Implements `IDisposable`.
- **FluentHttpResponse\<T>** - Generic typed response with lazy `Data` property (deserializes on first access).
- **FluentHttpStreamResponse** - For streaming/large downloads. Implements `IDisposable` and `IAsyncDisposable`.
- **FluentHttpDefaults** - Static global defaults for JSON options and serializer.
- **ISerializerProvider** - Interface for pluggable serialization (default: `SystemTextJsonSerializerProvider`).

Serializer resolution order: per-request custom serializer > per-request JSON options > global custom serializer > global JSON options > default System.Text.Json.

## Coding Standards

- XML documentation comments on all public members; none on private/internal
- File-scoped namespaces
- Private fields prefixed with underscore (`_fieldName`)
- Use `ArgumentNullException.ThrowIfNull()` for null validation
- Only public API surfaces should be public; everything else internal or private
- Beta version: breaking changes are acceptable

## Testing Conventions

- Framework: MSTest with TestFuzn adapter (`Fuzn.TestFuzn.Adapters.MSTest`)
- Test classes inherit from `Test` base class
- Use `[TestClass]` on classes and `[Test]` on methods (not `[TestMethod]`)
- Pattern: `Scenario().Step().Run()` from TestFuzn
- Shared state via `SuiteData` static class (provides `HttpClientFactory`, `ServiceProvider`)
- TestApi endpoints are organized in `src/TestApi/Endpoints/` by feature

## Documentation

Always update `README.md` when adding new features or changing existing functionality.
