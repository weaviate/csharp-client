# Contributing to Weaviate C# Client

Thank you for contributing! This document provides guidelines for contributing to the Weaviate C# client.

## Public API Tracking

This project uses [Microsoft.CodeAnalysis.PublicApiAnalyzers](https://github.com/dotnet/roslyn-analyzers/blob/main/src/PublicApiAnalyzers/PublicApiAnalyzers.Help.md) to track changes to the public API surface. This ensures that breaking changes are intentional and reviewed.

### How It Works

The analyzer tracks public API members in two files located in `src/Weaviate.Client/`:

| File | Purpose |
|------|---------|
| `PublicAPI.Shipped.txt` | APIs that have been released in a published version |
| `PublicAPI.Unshipped.txt` | APIs that are new/changed since the last release |

### When Adding New Public APIs

When you add a new public class, method, property, or other member, you'll see an **RS0016** warning at build time:

```text
warning RS0016: Symbol 'YourNewMethod' is not part of the declared public API
```

**To fix this:**

1. Use the IDE quick-fix (lightbulb icon) to add the symbol to `PublicAPI.Unshipped.txt`
2. Or run `dotnet format analyzers --diagnostics RS0016` to auto-fix all missing entries

### When Modifying or Removing Public APIs

If you change or remove a public API member, you'll see an **RS0017** warning:

```text
warning RS0017: Symbol 'OldMethod' is part of the declared API, but could not be found
```

This is intentional - it alerts you to a potential **breaking change**. Before proceeding:

1. Consider if the change is backward-compatible
2. If removing/changing is intentional, update the corresponding line in `PublicAPI.Unshipped.txt`
3. Document the breaking change in the changelog

### Release Process

When preparing a release:

1. Review all entries in `PublicAPI.Unshipped.txt`
2. Move the entries to `PublicAPI.Shipped.txt`
3. Clear `PublicAPI.Unshipped.txt` (keeping only `#nullable enable`)

### Suppressed Warnings

The following analyzer warnings are currently suppressed in the project:

| Warning | Reason |
|---------|--------|
| RS0026 | Multiple overloads with optional parameters (API design advisory) |
| RS0027 | Optional parameter ordering (API design advisory) |
| RS0041 | Oblivious reference types (nullability advisory) |

These are design recommendations, not API tracking issues. They may be addressed in future refactoring efforts.

## Building the Project

```bash
# Build the main library
dotnet build src/Weaviate.Client/Weaviate.Client.csproj

# Build and run tests
dotnet test src/Weaviate.Client.Tests/Weaviate.Client.Tests.csproj
```

## Running Tests

The test project includes both unit tests and integration tests. Integration tests require a running Weaviate instance.

```bash
# Run all tests
dotnet test

# Run only unit tests
dotnet test --filter "Category!=Integration"
```

## Request/Response Logging

By default, request and response logging is **disabled** to keep CI and test output clean.

### Using .NET Configuration System

The client integrates with .NET's configuration system through `IConfiguration`.

#### 1. Environment Variables (via IConfiguration)

.NET's configuration system automatically reads environment variables when configured:

```bash
export Weaviate__LogRequests=true
dotnet run
```

Note the double underscore `__` which maps to nested configuration (colon `:` on Windows).

#### 2. appsettings.json

```json
{
  "Weaviate": {
    "RestEndpoint": "localhost",
    "RestPort": 8080,
    "GrpcPort": 50051,
    "LogRequests": true,
    "RequestLoggingLevel": "Information"
  }
}
```

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<WeaviateOptions>(
    builder.Configuration.GetSection("Weaviate")
);
builder.Services.AddSingleton<WeaviateClient>();
```

#### 3. IConfiguration with Builder

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()  // Automatically reads Weaviate__LogRequests
    .Build();

var logRequests = configuration.GetValue<bool>("Weaviate:LogRequests");

var client = await new WeaviateClientBuilder()
    .Local()
    .UseRequestLogging(logRequests ? LogLevel.Information : LogLevel.Debug)
    .BuildAsync();
```

### Manual Configuration

You can also enable logging programmatically:

```csharp
var client = await new WeaviateClientBuilder()
    .Local()
    .UseRequestLogging(LogLevel.Information)
    .BuildAsync();
```

### Configuring Test Logging

Integration tests support optional HTTP/gRPC request/response logging for debugging. Configuration uses .NET's standard `IConfiguration` system.

Logging output is captured by xUnit via `[assembly: CaptureConsole]` and displayed in test results under "Standard Output Messages".

#### Enable via Environment Variable

```bash
Weaviate__LogRequests=true dotnet test --logger "console;verbosity=detailed"
```

You can also set the logging level:

```bash
Weaviate__LogRequests=true Weaviate__RequestLoggingLevel=Debug dotnet test --logger "console;verbosity=detailed"
```

Available levels: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`

#### Enable via Configuration File

Create `src/Weaviate.Client.Tests/appsettings.Test.json`:

```json
{
  "Weaviate": {
    "LogRequests": true,
    "RequestLoggingLevel": "Information"
  }
}
```

**Note:** Environment variables override file-based configuration. Use the hierarchical key format: `Weaviate__LogRequests=true` (double underscore separates sections).

#### Viewing Captured Output

Use the detailed console logger to see captured output:

```bash
dotnet test --logger "console;verbosity=detailed"
```

Logged requests appear in the "Standard Output Messages" section of each test:

```text
Standard Output Messages:
 info: Weaviate.Client.Internal.HttpLoggingHandler[0]
       HTTP GET http://localhost:8080/v1/meta
 info: Weaviate.Client.Internal.HttpLoggingHandler[0]
       -> 200 in 15ms
```

**Do not enable request logging in CI or normal test runs.** This keeps logs clean and only outputs requests/responses when needed for debugging.
