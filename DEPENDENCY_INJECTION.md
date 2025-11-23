# Dependency Injection with Weaviate Client

The Weaviate C# client now supports modern dependency injection patterns with async initialization.

## Table of Contents

- [Quick Start](#quick-start)
- [Configuration Options](#configuration-options)
- [Eager vs Lazy Initialization](#eager-vs-lazy-initialization)
- [Using Connect Helpers](#using-connect-helpers)
- [Advanced Scenarios](#advanced-scenarios)

---

## Quick Start

### 1. Register Weaviate in `Program.cs`

```csharp
using Weaviate.Client.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Option 1: Configure inline
builder.Services.AddWeaviate(options =>
{
    options.RestEndpoint = "localhost";
    options.GrpcEndpoint = "localhost";
    options.RestPort = 8080;
    options.GrpcPort = 50051;
});

// Option 2: From appsettings.json
builder.Services.AddWeaviate(builder.Configuration.GetSection("Weaviate"));

// Option 3: Helper methods for common scenarios
builder.Services.AddWeaviateLocal(); // Connects to localhost:8080
builder.Services.AddWeaviateCloud("my-cluster.weaviate.cloud", "api-key-here");

var app = builder.Build();
```

### 2. Configure in `appsettings.json` (Optional)

```json
{
  "Weaviate": {
    "RestEndpoint": "localhost",
    "GrpcEndpoint": "localhost",
    "RestPort": 8080,
    "GrpcPort": 50051,
    "UseSsl": false,
    "DefaultTimeout": "00:00:30",
    "InitTimeout": "00:00:02",
    "DataTimeout": "00:02:00",
    "QueryTimeout": "00:01:00"
  }
}
```

### 3. Inject and Use in Your Services

```csharp
public class CatService
{
    private readonly WeaviateClient _weaviate;

    public CatService(WeaviateClient weaviate)
    {
        _weaviate = weaviate;
        // Client is already initialized and ready to use!
    }

    public async Task<List<Cat>> SearchCatsAsync(string query)
    {
        var collection = _weaviate.Collections.Use<Cat>("Cat");

        var results = await collection.Query.NearText(new NearTextOptions
        {
            Text = query,
            Limit = 10
        });

        return results.Objects.Select(o => o.As<Cat>()!).ToList();
    }

    public async Task<Guid> AddCatAsync(Cat cat)
    {
        var collection = _weaviate.Collections.Use<Cat>("Cat");
        return await collection.Data.Insert(cat);
    }
}
```

---

## Configuration Options

### WeaviateOptions Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `RestEndpoint` | `string` | `"localhost"` | REST API endpoint address |
| `RestPort` | `ushort` | `8080` | REST API port |
| `GrpcEndpoint` | `string` | `"localhost"` | gRPC endpoint address |
| `GrpcPort` | `ushort` | `50051` | gRPC port |
| `UseSsl` | `bool` | `false` | Whether to use SSL/TLS |
| `Credentials` | `ICredentials?` | `null` | Authentication credentials |
| `DefaultTimeout` | `TimeSpan?` | `30s` | Default timeout for all operations |
| `InitTimeout` | `TimeSpan?` | `2s` | Timeout for initialization |
| `DataTimeout` | `TimeSpan?` | `120s` | Timeout for data operations |
| `QueryTimeout` | `TimeSpan?` | `60s` | Timeout for query operations |
| `Headers` | `Dictionary<string, string>?` | `null` | Additional HTTP headers |
| `RetryPolicy` | `RetryPolicy?` | Default | Retry policy for failed requests |

### Authentication

```csharp
builder.Services.AddWeaviate(options =>
{
    options.RestEndpoint = "my-cluster.weaviate.cloud";
    options.GrpcEndpoint = "grpc-my-cluster.weaviate.cloud";
    options.UseSsl = true;

    // API Key authentication
    options.Credentials = Auth.ApiKey("your-api-key");

    // Or OAuth2 Client Credentials
    options.Credentials = Auth.ClientCredentials("client-secret", "scope1", "scope2");

    // Or OAuth2 Password Flow
    options.Credentials = Auth.ClientPassword("username", "password", "scope1");
});
```

---

## Eager vs Lazy Initialization

### Eager Initialization (Default - Recommended)

The client initializes during application startup before handling requests.

```csharp
// Eager initialization is enabled by default
builder.Services.AddWeaviate(options => { ... });

// Explicitly enable
builder.Services.AddWeaviate(options => { ... }, eagerInitialization: true);
```

**Benefits:**
- ✅ Client is ready when first request arrives
- ✅ Fails fast if connection issues exist
- ✅ Simpler usage - no need to await initialization

**How it works:**
1. `WeaviateClient` is constructed with DI
2. `IHostedService` runs on app startup
3. Calls `client.InitializeAsync()` which:
   - Creates REST client
   - Fetches server metadata
   - Creates gRPC client with correct max message size
4. Client is fully initialized before app accepts requests

### Lazy Initialization

The client initializes on first use.

```csharp
builder.Services.AddWeaviate(options => { ... }, eagerInitialization: false);
```

**When to use:**
- Application startup time is critical
- Weaviate connection isn't needed immediately
- You want to handle connection failures gracefully

**Usage with lazy initialization:**
```csharp
public class MyService
{
    private readonly WeaviateClient _client;

    public MyService(WeaviateClient client)
    {
        _client = client;
    }

    public async Task DoWorkAsync()
    {
        // Manually ensure initialization
        if (!_client.IsInitialized)
        {
            await _client.InitializeAsync();
        }

        // Now use the client
        var collections = _client.Collections.Use<Cat>("Cat");
        // ...
    }
}
```

---

## Using Connect Helpers

The `Connect.Local()` and `Connect.Cloud()` helpers **still work** and are compatible with the new async pattern!

```csharp
// These work exactly as before - fully async, no blocking
var client = await Connect.Local();
var client = await Connect.Cloud("my-cluster.weaviate.cloud", "api-key");

// With timeouts
var client = await Connect.Local(
    hostname: "localhost",
    defaultTimeout: TimeSpan.FromSeconds(60),
    queryTimeout: TimeSpan.FromSeconds(30)
);
```

These methods:
- ✅ Return a fully initialized client
- ✅ Use async initialization (no blocking)
- ✅ Follow the same REST → Meta → gRPC initialization flow
- ✅ Are perfect for console apps, scripts, and testing

---

## Advanced Scenarios

### Custom HttpMessageHandler

```csharp
builder.Services.AddWeaviate(options =>
{
    options.RestEndpoint = "localhost";
    // Note: CustomHandlers property exists on ClientConfiguration, not WeaviateOptions
    // For now, use WeaviateClientBuilder for custom handlers
});

// Alternative: Build client manually
builder.Services.AddSingleton<WeaviateClient>(sp =>
{
    var client = await WeaviateClientBuilder
        .Local()
        .WithHttpMessageHandler(myCustomHandler)
        .BuildAsync();

    return client;
});
```

### Check Initialization Status

```csharp
public class MyService
{
    private readonly WeaviateClient _client;

    public MyService(WeaviateClient client)
    {
        _client = client;
    }

    public async Task<string> GetStatusAsync()
    {
        if (!_client.IsInitialized)
        {
            return "Client is initializing...";
        }

        var version = _client.WeaviateVersion;
        return $"Connected to Weaviate {version}";
    }
}
```

### Manual Initialization

```csharp
// Create client without DI
var options = Options.Create(new WeaviateOptions
{
    RestEndpoint = "localhost"
});

var client = new WeaviateClient(options);

// Manually initialize
await client.InitializeAsync();

// Now use the client
var collection = client.Collections.Use<Cat>("Cat");
```

### Integration Testing

```csharp
public class MyIntegrationTests : IAsyncLifetime
{
    private WeaviateClient _client = null!;

    public async Task InitializeAsync()
    {
        // Setup test client
        _client = await Connect.Local();

        // Verify it's ready
        Assert.True(_client.IsInitialized);
        Assert.True(await _client.IsReady());
    }

    [Fact]
    public async Task CanSearchCats()
    {
        var collection = _client.Collections.Use<Cat>("Cat");
        var results = await collection.Query.FetchObjects(limit: 10);

        Assert.NotEmpty(results.Objects);
    }

    public async Task DisposeAsync()
    {
        _client?.Dispose();
    }
}
```

---

## How It Works

### Initialization Flow

1. **Constructor** (`WeaviateClient(IOptions<WeaviateOptions>)`)
   - Creates a `Lazy<Task>` for async initialization
   - Sets up sub-clients (Collections, Cluster, etc.)
   - Returns immediately

2. **First Async Method Call** or **IHostedService**
   - Triggers `EnsureInitializedAsync()`
   - Runs initialization task (only once, thread-safe)

3. **Initialization Task**
   ```
   ┌─────────────────────────────────────────┐
   │ 1. Initialize Token Service (OAuth, etc)│
   ├─────────────────────────────────────────┤
   │ 2. Create REST Client                   │
   ├─────────────────────────────────────────┤
   │ 3. Fetch Metadata from REST /v1/meta    │
   │    - Get GrpcMaxMessageSize             │
   │    - Get Server Version                 │
   ├─────────────────────────────────────────┤
   │ 4. Create gRPC Client                   │
   │    - Use max message size from Meta     │
   ├─────────────────────────────────────────┤
   │ 5. Update Cluster Client                │
   └─────────────────────────────────────────┘
   ```

4. **Subsequent Calls**
   - `EnsureInitializedAsync()` returns immediately (already initialized)
   - No performance penalty

### No More `GetAwaiter().GetResult()`!

The old pattern had to block:
```csharp
// ❌ Old: Blocking async call in constructor
var client = new WeaviateClient(config);
var meta = GetMetaAsync().GetAwaiter().GetResult(); // Deadlock risk!
```

The new pattern is fully async:
```csharp
// ✅ New: Lazy async initialization
var client = new WeaviateClient(options);
// Later, when needed...
await client.InitializeAsync(); // Or called automatically
```

---

## Migration Guide

### From Old Constructor Pattern

**Before:**
```csharp
var config = new ClientConfiguration
{
    RestAddress = "localhost",
    RestPort = 8080
};

var client = new WeaviateClient(config);
```

**After (DI):**
```csharp
builder.Services.AddWeaviate(options =>
{
    options.RestEndpoint = "localhost";
    options.RestPort = 8080;
});

// In your service
public MyService(WeaviateClient client) { ... }
```

**After (Non-DI):**
```csharp
var client = await Connect.Local();
// Or
var options = Options.Create(new WeaviateOptions { ... });
var client = new WeaviateClient(options);
await client.InitializeAsync();
```

### From `WeaviateClientBuilder`

**Before:**
```csharp
var client = await WeaviateClientBuilder
    .Local()
    .WithCredentials(Auth.ApiKey("key"))
    .BuildAsync();
```

**After (still works!):**
```csharp
// This pattern still works exactly as before
var client = await WeaviateClientBuilder
    .Local()
    .WithCredentials(Auth.ApiKey("key"))
    .BuildAsync();
```

**Or with DI:**
```csharp
builder.Services.AddWeaviate(options =>
{
    options.Credentials = Auth.ApiKey("key");
});
```

---

## Summary

✅ **Fully async** - No blocking calls in constructors
✅ **DI-friendly** - Works seamlessly with ASP.NET Core and other DI containers
✅ **Eager initialization** - Ready before first request (with IHostedService)
✅ **Lazy initialization** - Initialize on demand if needed
✅ **Backward compatible** - `Connect.Local()`, `Connect.Cloud()`, and `WeaviateClientBuilder` still work
✅ **Same initialization flow** - REST → Meta → gRPC with max message size
✅ **Thread-safe** - Uses `Lazy<Task>` pattern
✅ **Testable** - Easy to mock and test

For more information, see the [main README](README.md) and [examples](src/Example/).
