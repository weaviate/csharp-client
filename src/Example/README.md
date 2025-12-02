# Weaviate C# Client Examples

This project contains multiple examples demonstrating different ways to use the Weaviate C# client.

## Running the Examples

### Interactive Menu

Run without arguments to see an interactive menu:

```bash
dotnet run --project src/Example
```

You'll see:
```
=== Weaviate C# Client Examples ===

Choose an example to run:
  1. Traditional Example (cats.json batch insert)
  2. Dependency Injection Example
  3. Multiple Clients Example
  4. Different Configs Example
  5. Configuration Example (from appsettings.json)
  6. Lazy Initialization Example
  7. Connect Helper Example

Enter your choice (1-7):
```

### Command Line

Run a specific example by name:

```bash
dotnet run --project src/Example -- traditional
dotnet run --project src/Example -- di
dotnet run --project src/Example -- multiple
dotnet run --project src/Example -- configs
dotnet run --project src/Example -- configuration
dotnet run --project src/Example -- lazy
dotnet run --project src/Example -- connect
```

## Examples Overview

### 1. Traditional Example

**File**: `TraditionalExample.cs`

Classic usage without dependency injection. Demonstrates:
- Using `Connect.Local()` to create a client
- Batch insertion of data
- Basic queries (FetchObjects, FetchObjectByID, FetchObjectsByIDs)
- Vector similarity search with NearVector
- BM25 full-text search
- Iterator pattern for large result sets

**When to use**: Simple scripts, console applications, quick prototypes.

```csharp
var client = await Connect.Local();
var collection = client.Collections.Use<Cat>("Cat");
var results = await collection.Query.FetchObjects(limit: 10);
```

---

### 2. Dependency Injection Example

**File**: `DependencyInjectionExample.cs`

Modern ASP.NET Core-style dependency injection with eager initialization.

**What it shows**:
- Registering Weaviate with `AddWeaviateLocal()`
- Eager initialization via `IHostedService` (client initializes on app startup)
- Injecting `WeaviateClient` into services
- Using ILogger for structured logging

**When to use**: ASP.NET Core applications, web APIs, long-running services.

```csharp
// Startup
services.AddWeaviateLocal(
    hostname: "localhost",
    restPort: 8080,
    grpcPort: 50051,
    eagerInitialization: true
);

// In your service
public class CatService
{
    private readonly WeaviateClient _weaviate;

    public CatService(WeaviateClient weaviate)
    {
        _weaviate = weaviate; // Already initialized!
    }
}
```

---

### 3. Multiple Clients Example

**File**: `MultipleClientsExample.cs`

Using multiple named Weaviate clients simultaneously.

**What it shows**:
- Registering multiple clients with different configurations
- Using `IWeaviateClientFactory` to get clients by name
- Multi-environment scenarios (prod, staging, local)
- Syncing data between environments

**When to use**:
- Multi-region deployments
- Multi-tenant architectures
- Different databases for analytics vs operations
- Working with multiple Weaviate clusters

```csharp
// Register multiple clients
services.AddWeaviateClient("production", options => { ... });
services.AddWeaviateClient("staging", options => { ... });
services.AddWeaviateClient("local", "localhost", 8080, 50051);

// Use in service
public class MyService
{
    private readonly IWeaviateClientFactory _factory;

    public async Task ProcessAsync()
    {
        var prodClient = await _factory.GetClientAsync("production");
        var stagingClient = await _factory.GetClientAsync("staging");
        // Each has independent configuration
    }
}
```

---

### 4. Different Configs Example

**File**: `DifferentConfigsExample.cs`

Demonstrates how each named client can have completely different settings.

**What it shows**:
- Different hosts and ports per client
- Different SSL settings
- Different authentication (API key, OAuth, OIDC, none)
- Different timeouts per client
- Custom headers per client
- Different retry policies

**When to use**: When you need fine-grained control over each client's behavior.

```csharp
// Production: SSL + API key + short timeouts
services.AddWeaviateClient("production", options =>
{
    options.RestEndpoint = "prod.weaviate.cloud";
    options.UseSsl = true;
    options.Credentials = Auth.ApiKey("key");
    options.QueryTimeout = TimeSpan.FromSeconds(30);
});

// Local: No SSL, no auth, long timeouts for debugging
services.AddWeaviateClient("local", options =>
{
    options.RestEndpoint = "localhost";
    options.UseSsl = false;
    options.Credentials = null;
    options.QueryTimeout = TimeSpan.FromMinutes(5);
});
```

---

### 5. Configuration Example

**File**: `DependencyInjectionExample.cs` → `ConfigurationExample` class

Reading Weaviate settings from `appsettings.json`.

**What it shows**:
- Using `IConfiguration` with `AddWeaviate()`
- Externalizing connection settings
- Environment-specific configuration

**When to use**: Production applications where settings should be configurable without code changes.

**appsettings.json**:
```json
{
  "Weaviate": {
    "RestEndpoint": "localhost",
    "RestPort": 8080,
    "GrpcPort": 50051,
    "UseSsl": false
  }
}
```

**Code**:
```csharp
services.AddWeaviate(
    context.Configuration.GetSection("Weaviate"),
    eagerInitialization: true
);
```

---

### 6. Lazy Initialization Example

**File**: `DependencyInjectionExample.cs` → `LazyInitializationExample` class

Client initializes on first use instead of startup.

**What it shows**:
- Setting `eagerInitialization: false`
- Manually triggering initialization with `InitializeAsync()`
- Checking initialization status with `IsInitialized`

**When to use**:
- Applications where Weaviate might not always be needed
- Faster startup times
- On-demand client creation

```csharp
services.AddWeaviateLocal(eagerInitialization: false);

// Later...
var client = serviceProvider.GetRequiredService<WeaviateClient>();
Console.WriteLine(client.IsInitialized); // False

await client.InitializeAsync(); // Initialize now
Console.WriteLine(client.IsInitialized); // True
```

---

### 7. Connect Helper Example

**File**: `DependencyInjectionExample.cs` → `ConnectHelperExample` class

Shows that traditional `Connect.Local()` and `Connect.Cloud()` still work.

**What it shows**:
- Backward compatibility
- Fully async, no blocking calls
- Simple one-liner client creation

**When to use**: Quick scripts, simple applications, backward compatibility.

```csharp
// Local
var client = await Connect.Local();

// Cloud
var client = await Connect.Cloud(
    clusterEndpoint: "my-cluster.weaviate.cloud",
    apiKey: "my-api-key"
);
```

---

## Prerequisites

All examples require:
- A running Weaviate instance (default: `localhost:8080`)
- .NET 8.0 or later

### Starting Weaviate Locally

```bash
docker run -d \
  -p 8080:8080 \
  -p 50051:50051 \
  -e ENABLE_MODULES=text2vec-weaviate \
  -e DEFAULT_VECTORIZER_MODULE=text2vec-weaviate \
  cr.weaviate.io/semitechnologies/weaviate:latest
```

## Example Selection Guide

| Scenario | Recommended Example |
|----------|-------------------|
| Quick script or console app | Traditional (#1) or Connect Helper (#7) |
| ASP.NET Core web API | Dependency Injection (#2) |
| Multiple Weaviate clusters | Multiple Clients (#3) |
| Per-client custom settings | Different Configs (#4) |
| Configuration-driven setup | Configuration (#5) |
| Faster startup, on-demand init | Lazy Initialization (#6) |
| Learning the basics | Traditional (#1) |
| Production applications | Dependency Injection (#2) + Configuration (#5) |

## Common Patterns

### ASP.NET Core Web API

```csharp
// Program.cs or Startup.cs
builder.Services.AddWeaviateLocal(
    hostname: builder.Configuration["Weaviate:Host"] ?? "localhost",
    restPort: ushort.Parse(builder.Configuration["Weaviate:Port"] ?? "8080"),
    eagerInitialization: true
);

// Controller
[ApiController]
[Route("[controller]")]
public class SearchController : ControllerBase
{
    private readonly WeaviateClient _weaviate;

    public SearchController(WeaviateClient weaviate)
    {
        _weaviate = weaviate;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string query)
    {
        var collection = _weaviate.Collections.Use<Product>("Product");
        var results = await collection.Query.BM25(query, limit: 10);
        return Ok(results.Objects);
    }
}
```

### Background Service

```csharp
public class WeaviateBackgroundService : BackgroundService
{
    private readonly WeaviateClient _weaviate;
    private readonly ILogger<WeaviateBackgroundService> _logger;

    public WeaviateBackgroundService(
        WeaviateClient weaviate,
        ILogger<WeaviateBackgroundService> logger)
    {
        _weaviate = weaviate;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Process data periodically
            await ProcessDataAsync();
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Documentation

For more information:
- **DEPENDENCY_INJECTION.md** - Comprehensive DI guide
- **MULTIPLE_CLIENTS.md** - Multi-client patterns and use cases
- [Official Weaviate Docs](https://weaviate.io/developers/weaviate)

## Troubleshooting

**Can't connect to Weaviate**:
- Ensure Weaviate is running: `docker ps`
- Check ports are correct: 8080 (REST), 50051 (gRPC)
- Verify endpoint: `curl http://localhost:8080/v1/.well-known/ready`

**Build errors about `Host`**:
- Ensure `Microsoft.Extensions.Hosting` package is referenced in `Example.csproj`

**Initialization timeout**:
- Increase `initTimeout` in options
- Check network connectivity to Weaviate
- Verify Weaviate is fully started and healthy
