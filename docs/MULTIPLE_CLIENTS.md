# Multiple Weaviate Clients

When you need to connect to multiple Weaviate instances (e.g., production, staging, local dev, or different databases), you can use the **named client pattern** via `IWeaviateClientFactory`.

## Quick Start

### Register Multiple Clients

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register multiple named clients
builder.Services.AddWeaviateClient("production", options =>
{
    options.RestEndpoint = "prod.weaviate.cloud";
    options.GrpcEndpoint = "grpc-prod.weaviate.cloud";
    options.UseSsl = true;
    options.Credentials = Auth.ApiKey("prod-key");
});

builder.Services.AddWeaviateClient("staging", options =>
{
    options.RestEndpoint = "staging.weaviate.cloud";
    options.GrpcEndpoint = "grpc-staging.weaviate.cloud";
    options.UseSsl = true;
    options.Credentials = Auth.ApiKey("staging-key");
});

builder.Services.AddWeaviateClient("local", "localhost", 8080, 50051);

// Or use cloud helper
builder.Services.AddWeaviateCloudClient("analytics", "analytics.weaviate.cloud", "api-key");

var app = builder.Build();
```

### Use Multiple Clients in Your Service

```csharp
public class DataSyncService
{
    private readonly IWeaviateClientFactory _clientFactory;

    public DataSyncService(IWeaviateClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task SyncProductionToStagingAsync()
    {
        // Get clients by name
        var prodClient = await _clientFactory.GetClientAsync("production");
        var stagingClient = await _clientFactory.GetClientAsync("staging");

        // Fetch from production
        var prodCollection = prodClient.Collections.Use<Product>("Product");
        var products = await prodCollection.Query.FetchObjects(limit: 1000);

        // Insert into staging
        var stagingCollection = stagingClient.Collections.Use<Product>("Product");
        foreach (var product in products.Objects)
        {
            await stagingCollection.Data.Insert(product.As<Product>()!);
        }
    }
}
```

---

## Registration Patterns

### Pattern 1: Inline Configuration

```csharp
builder.Services.AddWeaviateClient("prod", options =>
{
    options.RestEndpoint = "prod.weaviate.cloud";
    options.Credentials = Auth.ApiKey("key");
    options.QueryTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddWeaviateClient("staging", options =>
{
    options.RestEndpoint = "staging.weaviate.cloud";
    options.Credentials = Auth.ApiKey("key");
});
```

### Pattern 2: Using Helper Methods

```csharp
// For local instances
builder.Services.AddWeaviateClient(
    name: "local",
    hostname: "localhost",
    restPort: 8080,
    grpcPort: 50051
);

// For cloud instances
builder.Services.AddWeaviateCloudClient(
    name: "prod",
    clusterEndpoint: "prod.weaviate.cloud",
    apiKey: "prod-key"
);
```

### Pattern 3: From Configuration

```json
// appsettings.json
{
  "Weaviate": {
    "Clients": {
      "Production": {
        "RestEndpoint": "prod.weaviate.cloud",
        "GrpcEndpoint": "grpc-prod.weaviate.cloud",
        "UseSsl": true,
        "RestPort": 443,
        "GrpcPort": 443
      },
      "Staging": {
        "RestEndpoint": "staging.weaviate.cloud",
        "GrpcEndpoint": "grpc-staging.weaviate.cloud",
        "UseSsl": true,
        "RestPort": 443,
        "GrpcPort": 443
      },
      "Local": {
        "RestEndpoint": "localhost",
        "GrpcEndpoint": "localhost",
        "RestPort": 8080,
        "GrpcPort": 50051
      }
    }
  }
}
```

```csharp
// Program.cs
var clientsConfig = builder.Configuration.GetSection("Weaviate:Clients");

foreach (var client in new[] { "Production", "Staging", "Local" })
{
    builder.Services.AddWeaviateClient(
        client.ToLower(),
        clientsConfig.GetSection(client).Get<WeaviateOptions>()!);
}
```

---

## Usage Patterns

### Pattern 1: Inject Factory

Most flexible - get clients on demand:

```csharp
public class MyService
{
    private readonly IWeaviateClientFactory _factory;

    public MyService(IWeaviateClientFactory factory)
    {
        _factory = factory;
    }

    public async Task ProcessAsync(string environment)
    {
        // Get client based on runtime logic
        var client = await _factory.GetClientAsync(environment);

        var collection = client.Collections.Use<Data>("Data");
        var results = await collection.Query.FetchObjects();

        // Process...
    }
}
```

### Pattern 2: Get Specific Clients

Clearer intent when you always use specific clients:

```csharp
public class MultiEnvironmentService
{
    private readonly WeaviateClient _prodClient;
    private readonly WeaviateClient _stagingClient;

    public MultiEnvironmentService(IWeaviateClientFactory factory)
    {
        // Get clients once in constructor
        // Note: Using synchronous GetClient() - will block during DI resolution
        _prodClient = factory.GetClient("production");
        _stagingClient = factory.GetClient("staging");
    }

    public async Task SyncAsync()
    {
        // Use pre-fetched clients
        var prodData = await _prodClient.Collections.Use<Data>("Data")
            .Query.FetchObjects();

        var stagingCollection = _stagingClient.Collections.Use<Data>("Data");
        // ...
    }
}
```

### Pattern 3: Lazy Client Resolution

Best for async initialization:

```csharp
public class LazyClientService
{
    private readonly IWeaviateClientFactory _factory;
    private WeaviateClient? _prodClient;
    private WeaviateClient? _stagingClient;

    public LazyClientService(IWeaviateClientFactory factory)
    {
        _factory = factory;
    }

    private async Task<WeaviateClient> GetProdClientAsync()
    {
        return _prodClient ??= await _factory.GetClientAsync("production");
    }

    private async Task<WeaviateClient> GetStagingClientAsync()
    {
        return _stagingClient ??= await _factory.GetClientAsync("staging");
    }

    public async Task ProcessAsync()
    {
        var prod = await GetProdClientAsync();
        var staging = await GetStagingClientAsync();

        // Use clients...
    }
}
```

---

## Common Scenarios

### Scenario 1: Multi-Region Data Sync

```csharp
public class MultiRegionSyncService
{
    private readonly IWeaviateClientFactory _factory;

    public MultiRegionSyncService(IWeaviateClientFactory factory)
    {
        _factory = factory;
    }

    public async Task SyncAcrossRegionsAsync()
    {
        var usClient = await _factory.GetClientAsync("us-east");
        var euClient = await _factory.GetClientAsync("eu-west");
        var apacClient = await _factory.GetClientAsync("apac");

        // Fetch from primary region
        var usCollection = usClient.Collections.Use<User>("User");
        var users = await usCollection.Query.FetchObjects(limit: 10000);

        // Replicate to other regions
        var euCollection = euClient.Collections.Use<User>("User");
        var apacCollection = apacClient.Collections.Use<User>("User");

        foreach (var user in users.Objects)
        {
            var userData = user.As<User>()!;
            await euCollection.Data.Insert(userData);
            await apacCollection.Data.Insert(userData);
        }
    }
}
```

### Scenario 2: Environment-Based Processing

```csharp
public class EnvironmentAwareService
{
    private readonly IWeaviateClientFactory _factory;
    private readonly IHostEnvironment _environment;

    public EnvironmentAwareService(
        IWeaviateClientFactory factory,
        IHostEnvironment environment)
    {
        _factory = factory;
        _environment = environment;
    }

    public async Task<WeaviateClient> GetCurrentEnvironmentClientAsync()
    {
        var clientName = _environment.IsDevelopment() ? "local" :
                        _environment.IsStaging() ? "staging" :
                        "production";

        return await _factory.GetClientAsync(clientName);
    }

    public async Task ProcessAsync()
    {
        var client = await GetCurrentEnvironmentClientAsync();
        var collection = client.Collections.Use<Data>("Data");

        // Process with appropriate environment client...
    }
}
```

### Scenario 3: Multi-Tenant Architecture

```csharp
public class MultiTenantService
{
    private readonly IWeaviateClientFactory _factory;

    public MultiTenantService(IWeaviateClientFactory factory)
    {
        _factory = factory;
    }

    // Register clients per tenant
    public static void ConfigureTenants(IServiceCollection services)
    {
        services.AddWeaviateClient("tenant-acme", options =>
        {
            options.RestEndpoint = "acme.weaviate.cloud";
            options.Credentials = Auth.ApiKey("acme-key");
        });

        services.AddWeaviateClient("tenant-globex", options =>
        {
            options.RestEndpoint = "globex.weaviate.cloud";
            options.Credentials = Auth.ApiKey("globex-key");
        });
    }

    public async Task<List<Product>> GetTenantProductsAsync(string tenantId)
    {
        var clientName = $"tenant-{tenantId}";
        var client = await _factory.GetClientAsync(clientName);

        var collection = client.Collections.Use<Product>("Product");
        var results = await collection.Query.FetchObjects(limit: 100);

        return results.Objects.Select(o => o.As<Product>()!).ToList();
    }
}
```

### Scenario 4: Testing vs Production

```csharp
public class TestableService
{
    private readonly IWeaviateClientFactory _factory;

    public TestableService(IWeaviateClientFactory factory)
    {
        _factory = factory;
    }

    public async Task ProcessDataAsync(bool useTestDatabase = false)
    {
        var clientName = useTestDatabase ? "test" : "production";
        var client = await _factory.GetClientAsync(clientName);

        var collection = client.Collections.Use<Data>("Data");
        var results = await collection.Query.FetchObjects();

        // Process...
    }
}

// In tests
public class TestableServiceTests
{
    [Fact]
    public async Task TestWithTestDatabase()
    {
        var services = new ServiceCollection();

        // Register test database
        services.AddWeaviateClient("test", "localhost", 8080, 50051);

        var provider = services.BuildServiceProvider();
        var service = new TestableService(
            provider.GetRequiredService<IWeaviateClientFactory>());

        await service.ProcessDataAsync(useTestDatabase: true);
    }
}
```

---

## Client Lifecycle

- **Creation**: Clients are created lazily on first access via the factory
- **Caching**: Once created, clients are cached for the lifetime of the factory
- **Initialization**: Each client initializes asynchronously when created
- **Disposal**: All clients are disposed when the factory is disposed

---

## Mixing Single and Multiple Client Patterns

You can use both patterns in the same application:

```csharp
// Register a default single client
builder.Services.AddWeaviate(options =>
{
    options.RestEndpoint = "localhost";
});

// Also register named clients
builder.Services.AddWeaviateClient("analytics", "analytics.weaviate.cloud");
builder.Services.AddWeaviateClient("backup", "backup.weaviate.cloud");

// Usage
public class MixedService
{
    private readonly WeaviateClient _defaultClient;
    private readonly IWeaviateClientFactory _factory;

    public MixedService(
        WeaviateClient defaultClient,
        IWeaviateClientFactory factory)
    {
        _defaultClient = defaultClient; // Default client
        _factory = factory; // For named clients
    }

    public async Task ProcessAsync()
    {
        // Use default client for main operations
        var mainData = await _defaultClient.Collections.Use<Data>("Data")
            .Query.FetchObjects();

        // Use analytics client for metrics
        var analyticsClient = await _factory.GetClientAsync("analytics");
        var metrics = await analyticsClient.Collections.Use<Metric>("Metric")
            .Query.FetchObjects();
    }
}
```

---

## Best Practices

1. **Use descriptive names**: `"production"`, `"staging"`, `"analytics"` instead of `"client1"`, `"client2"`

2. **Get clients asynchronously**: Use `GetClientAsync()` when possible to avoid blocking

3. **Cache clients in services**: If you always use specific clients, get them once

4. **Configuration over code**: Store connection details in `appsettings.json`

5. **Environment-based selection**: Use `IHostEnvironment` to select appropriate clients

6. **Don't create clients manually**: Always use the factory for proper lifecycle management

---

## Summary

✅ **Multiple clients** - Connect to multiple Weaviate instances
✅ **Named clients** - Identify clients by logical names
✅ **Lazy initialization** - Clients created on first use
✅ **Automatic caching** - Clients reused across the application
✅ **Thread-safe** - Factory handles concurrent access
✅ **Proper disposal** - All clients cleaned up with factory
✅ **Flexible usage** - Inject factory or specific clients
✅ **Configuration-friendly** - Works with `appsettings.json`

For single-client scenarios, see [DEPENDENCY_INJECTION.md](DEPENDENCY_INJECTION.md).
