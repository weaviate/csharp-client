# Weaviate Client Initialization

This guide covers how to initialize and configure a Weaviate client in C#, including connection settings, authentication, and timeout management.

## Table of Contents

1. [Basic Client Creation](#basic-client-creation)
2. [Connection Configuration](#connection-configuration)
3. [Authentication](#authentication)
4. [Timeout Management](#timeout-management)
5. [Advanced Configuration](#advanced-configuration)
6. [Examples](#examples)

## Basic Client Creation

The simplest way to create a client is to use the `WeaviateClientBuilder` with the async `BuildAsync()` method:

```csharp
using Weaviate.Client;

// Create a client connecting to a local Weaviate instance
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .BuildAsync();
```

This creates a client with default settings:

- REST endpoint: `http://localhost:8080`
- gRPC endpoint: `localhost:50051`
- Default timeout: 30 seconds

**Important:** Client initialization is asynchronous. Metadata is fetched during initialization to properly configure the gRPC client and validate authentication. Always use `await` with `BuildAsync()`.

### Quick Connection Helpers

For the most common connection scenarios, the `Connect` helper class provides convenient shortcuts:

**Local Development:**

```csharp
// Connect to local Weaviate with defaults
var client = await Connect.Local();

// Connect to local with custom credentials
var client = await Connect.Local(
    credentials: Auth.ApiKey("your-api-key"),
    hostname: "localhost",
    restPort: 8080,
    grpcPort: 50051,
    useSsl: false
);

// Connect with custom timeouts
var client = await Connect.Local(
    credentials: Auth.ApiKey("your-api-key"),
    hostname: "localhost",
    defaultTimeout: TimeSpan.FromSeconds(30),
    initTimeout: TimeSpan.FromSeconds(5),
    insertTimeout: TimeSpan.FromSeconds(60),
    queryTimeout: TimeSpan.FromSeconds(120)
);
```

**Weaviate Cloud:**

```csharp
var client = await Connect.Cloud(
    restEndpoint: "your-cluster.weaviate.cloud",
    apiKey: "your-api-key"
);

// With custom timeouts
var client = await Connect.Cloud(
    restEndpoint: "your-cluster.weaviate.cloud",
    apiKey: "your-api-key",
    defaultTimeout: TimeSpan.FromSeconds(30),
    queryTimeout: TimeSpan.FromSeconds(120)
);
```

**From Environment Variables:**

```csharp
// Load configuration from environment variables (WEAVIATE_REST_ENDPOINT, WEAVIATE_GRPC_ENDPOINT, etc.)
var client = await Connect.FromEnvironment();

// Use a custom prefix for environment variables
var client = await Connect.FromEnvironment(prefix: "MY_WEAVIATE_");
```

The `Connect` class wraps the builder pattern, making it ideal for quick prototyping and simple applications. For more advanced configuration needs, use `WeaviateClientBuilder` directly.

### Why Async Initialization?

Client initialization is asynchronous because:

1. **Metadata Fetching**: Server metadata (including gRPC max message size) is fetched during initialization
2. **Authentication Validation**: Credentials are validated immediately, catching auth errors early
3. **Thread Safety**: Async operations don't block thread pool threads, improving application responsiveness
4. **Error Handling**: Connection and authentication issues surface immediately during client creation, not later during operations

## Initialization Lifecycle

### Builder Pattern (Recommended)

When using `WeaviateClientBuilder` or `Connect` helpers, initialization happens automatically:

```csharp
// Client is fully initialized before being returned
var client = await WeaviateClientBuilder.Local().BuildAsync();
// ✅ RestClient and GrpcClient are ready to use
var results = await client.Collections.Get("Article").Query.FetchObjects();
```

**Key Points:**
- `BuildAsync()` calls `InitializeAsync()` internally before returning
- The client is **always fully initialized** when you receive it
- No manual initialization needed

### Dependency Injection Pattern

When using dependency injection, there are two modes:

#### Eager Initialization (Default - Recommended)

```csharp
services.AddWeaviateLocal(
    hostname: "localhost",
    eagerInitialization: true  // Default
);
```

**How it works:**
- A hosted service (`WeaviateInitializationService`) runs on application startup
- Calls `InitializeAsync()` automatically before your app serves requests
- The client is **always initialized** when injected into services

```csharp
public class MyService
{
    private readonly WeaviateClient _client;

    public MyService(WeaviateClient client)
    {
        // ✅ Client is already initialized by the hosted service
        _client = client;
    }

    public async Task DoWork()
    {
        // ✅ Safe to use immediately
        var results = await _client.Collections.Get("Article").Query.FetchObjects();
    }
}
```

#### Lazy Initialization (Manual Control)

```csharp
services.AddWeaviateLocal(
    hostname: "localhost",
    eagerInitialization: false  // Opt-in to lazy initialization
);
```

**How it works:**
- Client is created but NOT initialized
- You must call `InitializeAsync()` before first use
- Useful for scenarios where you want to control when initialization happens

```csharp
public class MyService
{
    private readonly WeaviateClient _client;

    public MyService(WeaviateClient client)
    {
        // ⚠️ Client is NOT yet initialized
        _client = client;
    }

    public async Task Initialize()
    {
        // ✅ Manually trigger initialization
        await _client.InitializeAsync();
    }

    public async Task DoWork()
    {
        // Check if initialized
        if (!_client.IsInitialized)
        {
            await _client.InitializeAsync();
        }

        var results = await _client.Collections.Get("Article").Query.FetchObjects();
    }
}
```

### Checking Initialization Status

Use the `IsInitialized` property to check if the client is ready:

```csharp
if (client.IsInitialized)
{
    // Safe to use RestClient and GrpcClient
    var results = await client.Collections.Get("Article").Query.FetchObjects();
}
else
{
    // Must call InitializeAsync() first
    await client.InitializeAsync();
}
```

**What happens during initialization:**
1. Token service is created (for authentication)
2. REST client is configured
3. Server metadata is fetched (validates auth, gets gRPC settings)
4. gRPC client is configured with server metadata
5. `RestClient` and `GrpcClient` properties are populated

### Important: When is the Client Ready?

| Pattern | When Ready | RestClient/GrpcClient Available |
|---------|-----------|--------------------------------|
| `await BuildAsync()` | Immediately after return | ✅ Yes |
| DI Eager (default) | Before app starts serving | ✅ Yes |
| DI Lazy | After calling `InitializeAsync()` | ⚠️ Only after init |

**⚠️ Using uninitialized client causes `NullReferenceException`:**

```csharp
// ❌ BAD: Lazy DI without calling InitializeAsync()
var client = serviceProvider.GetService<WeaviateClient>();
var results = await client.Collections.Get("Article").Query.FetchObjects();  // NullReferenceException!

// ✅ GOOD: Check and initialize if needed
var client = serviceProvider.GetService<WeaviateClient>();
if (!client.IsInitialized)
{
    await client.InitializeAsync();
}
var results = await client.Collections.Get("Article").Query.FetchObjects();
```

### Automatic Initialization Guards

**✨ Safety Feature:** All public async methods in the Weaviate client automatically ensure initialization before executing. This provides a safety net against accidental use of uninitialized clients.

**How it works:**

When you call any async method on the client (like `Collections.Create()`, `Cluster.Replicate()`, `Alias.Get()`, etc.), the client automatically calls `EnsureInitializedAsync()` internally before performing the operation. If the client isn't initialized yet:

- **With eager initialization (default)**: The guard passes immediately since the client is already initialized
- **With lazy initialization**: The guard triggers initialization automatically on first use

```csharp
// Lazy DI - initialization happens automatically on first call
services.AddWeaviateLocal(hostname: "localhost", eagerInitialization: false);

// Later in your code...
var client = serviceProvider.GetService<WeaviateClient>();

// ✅ This works! The client auto-initializes on first use
var collections = await client.Collections.List();  // Initialization happens here automatically
```

**Performance Impact:**

- **Eager initialization (default)**: No overhead - guards pass through immediately
- **Lazy initialization**: Minimal overhead - first call triggers initialization, subsequent calls pass through
- Guards use `Lazy<Task>` internally, ensuring initialization happens exactly once even with concurrent calls

**When guards help:**

- Forgetting to call `InitializeAsync()` in lazy initialization scenarios
- Race conditions where multiple threads access an uninitialized client
- Unit tests that don't properly set up client initialization

**What's protected:**

- ✅ `client.Collections.*` - All collection operations
- ✅ `client.Cluster.*` - All cluster operations
- ✅ `client.Alias.*` - All alias operations
- ✅ `client.Users.*`, `client.Roles.*`, `client.Groups.*` - All auth operations
- ✅ Collection-level operations like `collection.Delete()`, `collection.Iterator()`
- ✅ All async methods that access REST or gRPC clients

**What's not protected:**

- ❌ Synchronous property accessors (design limitation - properties can't be async)
- ❌ Direct access to internal clients (not part of public API)

## Connection Configuration

### Local Development

For connecting to a local Weaviate instance:

```csharp
var client = await new WeaviateClientBuilder()
    .Local(
        hostname: "localhost",
        restPort: 8080,
        grpcPort: 50051,
        useSsl: false
    )
    .BuildAsync();
```

### Custom Configuration

For more control over connection parameters:

```csharp
var client = await new WeaviateClientBuilder()
    .Custom(
        restEndpoint: "192.168.1.100",
        restPort: "8080",
        grpcEndpoint: "192.168.1.100",
        grpcPort: "50051",
        useSsl: false
    )
    .BuildAsync();
```

### Weaviate Cloud

For connecting to a Weaviate Cloud instance:

```csharp
var client = await new WeaviateClientBuilder()
    .Cloud(
        restEndpoint: "your-cluster-id.weaviate.cloud",
        apiKey: "your-api-key"
    )
    .BuildAsync();
```

## Authentication

### API Key Authentication

For Weaviate Cloud or self-hosted instances with API key authentication:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("your-cluster.weaviate.cloud")
    .WithCredentials(Auth.ApiKey("your-api-key"))
    .BuildAsync();
```

### Bearer Token Authentication

For OAuth 2.0 bearer token authentication:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithCredentials(Auth.BearerToken(
        accessToken: "your-access-token",
        expiresIn: 3600,
        refreshToken: "your-refresh-token"
    ))
    .BuildAsync();
```

### OAuth 2.0 Client Credentials Flow

For service-to-service authentication:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithCredentials(Auth.ClientCredentials(
        clientSecret: "your-client-secret",
        scope: "openid profile email"
    ))
    .BuildAsync();
```

### OAuth 2.0 Resource Owner Password Flow

For user credential-based authentication:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithCredentials(Auth.ClientPassword(
        username: "username",
        password: "password",
        scope: "openid profile email"
    ))
    .BuildAsync();
```

## Timeout Management

Timeouts are critical for preventing indefinite waits on network requests. The Weaviate client supports differentiated timeouts for different operation groups.

### Timeout Types

The client supports four timeout categories:

1. **DefaultTimeout**: Applied to all operations (fallback timeout) - **Default: 30 seconds**
2. **InitTimeout**: Applied to initialization operations (GetMeta, Live, IsReady) - **Default: 2 seconds**
3. **InsertTimeout**: Applied to data operations (Insert, Delete, Update, Reference management) - **Default: 120 seconds**
4. **QueryTimeout**: Applied to query/search operations (FetchObjects, NearText, BM25, Hybrid, Aggregate) and generative operations (Generate.*) - **Default: 60 seconds**

### Basic Timeout Configuration

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))  // Fallback for all operations
    .BuildAsync();
```

### Differentiated Timeout Strategy

For optimal performance, configure different timeouts based on operation characteristics:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))   // Fallback timeout
    .WithInitTimeout(TimeSpan.FromSeconds(5))       // Fast init timeout
    .WithInsertTimeout(TimeSpan.FromSeconds(60))      // Longer for writes
    .WithQueryTimeout(TimeSpan.FromSeconds(120))    // Longest for searches
    .BuildAsync();
```

### Timeout Strategy Rationale

- **InitTimeout (2s)**: Initialization operations are quick health checks. A very short timeout quickly detects network connectivity issues during client startup without blocking for long.

- **InsertTimeout (120s)**: Write operations (inserts, deletes, updates, references) may take longer due to indexing and consistency guarantees. The 120-second default allows time for moderate-to-large batch operations.

- **QueryTimeout (60s)**: Search and retrieval operations (queries, aggregations, generative operations) can be computationally intensive, especially with complex filters, reranking, large result sets, or LLM processing. The 60-second default balances responsiveness with allowing complex queries and generative operations to complete.

- **DefaultTimeout (30s)**: Used as a fallback for any operations that don't have a specific timeout configured. Appropriate for most standard operations.

### Per-Operation Timeout Override

You can provide a custom cancellation token to override the configured timeouts for specific operations:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

try
{
    var result = await collection.Query.NearText(
        "search query",
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Query timed out after 10 seconds");
}
```

## Advanced Configuration

### Custom HTTP Message Handler

For advanced networking scenarios (proxies, custom certificates, etc.):

```csharp
var httpHandler = new HttpClientHandler
{
    Proxy = new WebProxy("http://proxy.example.com:8080"),
    UseProxy = true
};

var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithHttpMessageHandler(httpHandler)
    .BuildAsync();
```

### Custom Headers

Add custom headers to all requests:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithHeader("X-Custom-Header", "custom-value")
    .WithHeaders(new Dictionary<string, string>
    {
        { "X-Request-ID", "request-123" },
        { "X-Custom-Header", "custom-value" }
    })
    .BuildAsync();
```

### Retry Policy

Configure automatic retry behavior for transient failures:

```csharp
var retryPolicy = new RetryPolicy(
    maxRetries: 3,
    initialBackoff: TimeSpan.FromMilliseconds(100),
    maxBackoff: TimeSpan.FromSeconds(5)
);

var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithRetryPolicy(retryPolicy)
    .BuildAsync();
```

Disable retries:

```csharp
var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithoutRetries()
    .BuildAsync();
```

### Custom Delegating Handlers

Add custom message handlers for logging, monitoring, or custom processing:

```csharp
public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Sending request: {request.Method} {request.RequestUri}");
        var response = await base.SendAsync(request, cancellationToken);
        Console.WriteLine($"Response status: {response.StatusCode}");
        return response;
    }
}

var client = await new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .AddHandler(new LoggingHandler())
    .BuildAsync();
```

## Examples

### Complete Example: Cloud Setup with Authentication and Timeouts

```csharp
using Weaviate.Client;
using System;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        // Create a configured client
        var client = await new WeaviateClientBuilder()
            .Cloud(
                restEndpoint: "my-cluster.weaviate.cloud",
                apiKey: Environment.GetEnvironmentVariable("WEAVIATE_API_KEY")
            )
            .WithDefaultTimeout(TimeSpan.FromSeconds(30))
            .WithInitTimeout(TimeSpan.FromSeconds(5))
            .WithInsertTimeout(TimeSpan.FromSeconds(60))
            .WithQueryTimeout(TimeSpan.FromSeconds(120))
            .WithRetryPolicy(new RetryPolicy(maxRetries: 3))
            .BuildAsync();

        try
        {
            // Check if the instance is ready
            bool isReady = await client.IsReady();
            Console.WriteLine($"Weaviate ready: {isReady}");

            // Get meta information
            var meta = await client.GetMeta();
            Console.WriteLine($"Weaviate version: {meta.Version}");

            // Work with collections
            var articleCollection = client.Collections.Get("Article");

            // Insert data
            var article = new { title = "Hello World", content = "..." };
            var id = await articleCollection.Data.Insert(article);
            Console.WriteLine($"Inserted article with ID: {id}");

            // Query data
            var results = await articleCollection.Query
                .NearText("science");

            foreach (var obj in results.Objects)
            {
                Console.WriteLine($"Found: {obj.Properties}");
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            client.Dispose();
        }
    }
}
```

### Example: Local Development Setup

```csharp
var client = await new WeaviateClientBuilder()
    .Local(hostname: "localhost")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))
    .WithInitTimeout(TimeSpan.FromSeconds(10))
    .WithInsertTimeout(TimeSpan.FromSeconds(60))
    .WithQueryTimeout(TimeSpan.FromSeconds(120))
    .BuildAsync();
```

### Example: Custom Configuration with Proxy

```csharp
var httpHandler = new HttpClientHandler
{
    Proxy = new WebProxy("http://corporate-proxy:8080"),
    UseProxy = true
};

var client = await new WeaviateClientBuilder()
    .Custom(
        restEndpoint: "weaviate.example.com",
        restPort: "443"
    )
    .UseSsl(true)
    .WithHttpMessageHandler(httpHandler)
    .WithHeader("X-API-Key", "secret-key")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))
    .WithInitTimeout(TimeSpan.FromSeconds(5))
    .WithInsertTimeout(TimeSpan.FromSeconds(90))
    .WithQueryTimeout(TimeSpan.FromSeconds(180))
    .BuildAsync();
```

### Example: Per-Operation Timeout Override

```csharp
var client = await new WeaviateClientBuilder()
    .Local()
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))
    .WithQueryTimeout(TimeSpan.FromSeconds(120))
    .BuildAsync();

var articleCollection = client.Collections.Get("Article");

// Use the configured QueryTimeout (120 seconds)
var normalResults = await articleCollection.Query.NearText("science");

// Override with a shorter timeout for this specific operation
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
try
{
    var quickResults = await articleCollection.Query.NearText(
        "science",
        cancellationToken: cts.Token
    );
}
catch (OperationCanceledException)
{
    Console.WriteLine("Quick query timed out as expected");
}
```

## Best Practices

1. **Always dispose the client**: Use `using` statements or call `Dispose()` when finished.

2. **Choose appropriate timeouts**: Consider your network latency and operation complexity when setting timeouts.

3. **Use retry policies**: Enable retries to handle transient network failures gracefully.

4. **Monitor initialization**: Test `IsReady()` and `Live()` during startup to detect connectivity issues early.

5. **Handle cancellation**: Always catch `OperationCanceledException` when using custom timeouts.

6. **Reuse the client**: Create the client once and reuse it across your application. The client is thread-safe.

7. **Secure credentials**: Never hardcode API keys. Use environment variables or secret management systems.

## Troubleshooting

### Connection Timeouts

If you're experiencing timeout errors:

1. Check network connectivity to the Weaviate instance
2. Verify firewall rules allow traffic on the REST (8080) and gRPC (50051) ports
3. Increase timeout values if the server is slow
4. Enable retry policies to handle transient failures

### Authentication Failures

If authentication fails:

1. Verify your credentials (API key, username, password) are correct
2. Check that the authentication method matches your Weaviate configuration
3. Ensure tokens haven't expired
4. Verify HTTPS/SSL settings match your server configuration

### Slow Initialization

If client initialization is slow:

1. Check that `InitTimeout` is sufficient for your network latency
2. Verify the Weaviate server is running and healthy
3. Use `IsReady()` to check server status before performing operations
