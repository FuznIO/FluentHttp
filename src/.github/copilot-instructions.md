# Copilot Instructions for FluentHttp

## Project Overview
FluentHttp is an open source fluent HTTP client library for .NET.
The version is still in beta, so feel free to propose breaking changes; you don't need to maintain backward compatibility.

## Technical Stack
- .NET 10

## Coding Standards
- Include XML documentation comments on all public members.
- Private or internal types or members should not have XML documentation comments.
- Use file-scoped namespaces

## Naming Conventions
- Prefix private fields with underscore (`_fieldName`)
- Use PascalCase for public members
- Use camelCase for local variables and parameters

## Patterns
- Use `ArgumentNullException.ThrowIfNull()` for null validation
- Only classes, members and methods relevant for the public API should be public; everything else should be internal or private.

## Testing
- Test framework: MSTest with TestFuzn adapter (`Fuzn.TestFuzn.Adapters.MSTest`)
- Tests are located in the `Tests` project
- Test classes inherit from `Test` base class
- Use `[TestClass]` attribute on test classes
- Use `[Test]` attribute on test methods (not `[TestMethod]`)
- Tests use the `Scenario().Step().Run()` pattern from TestFuzn
- Shared test data is accessed via `SuiteData` static class
- The `TestApi` project is a minimal ASP.NET Core Web API used for integration testing

## Documentation
- Always update the `README.md` file when adding new features or changing existing functionality
