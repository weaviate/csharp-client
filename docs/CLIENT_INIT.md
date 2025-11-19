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

The simplest way to create a client is to use the `WeaviateClientBuilder`:

```csharp
using Weaviate.Client;

// Create a client connecting to a local Weaviate instance
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .Build();
```

This creates a client with default settings:

- REST endpoint: `http://localhost:8080`
- gRPC endpoint: `localhost:50051`
- Default timeout: 30 seconds

### Quick Connection Helpers

For the most common connection scenarios, the `Connect` helper class provides convenient shortcuts:

**Local Development:**

```csharp
// Connect to local Weaviate with defaults
var client = Connect.Local();

// Connect to local with custom credentials
var client = Connect.Local(
    credentials: Auth.ApiKey("your-api-key"),
    hostname: "localhost",
    restPort: 8080,
    grpcPort: 50051,
    useSsl: false
);
```

**Weaviate Cloud:**

```csharp
var client = Connect.Cloud(
    restEndpoint: "your-cluster.weaviate.cloud",
    apiKey: "your-api-key"
);
```

**From Environment Variables:**

```csharp
// Load configuration from environment variables (WEAVIATE_REST_ENDPOINT, WEAVIATE_GRPC_ENDPOINT, etc.)
var client = Connect.FromEnvironment();

// Use a custom prefix for environment variables
var client = Connect.FromEnvironment(prefix: "MY_WEAVIATE_");
```

The `Connect` class wraps the builder pattern, making it ideal for quick prototyping and simple applications. For more advanced configuration needs, use `WeaviateClientBuilder` directly.

## Connection Configuration

### Local Development

For connecting to a local Weaviate instance:

```csharp
var client = new WeaviateClientBuilder()
    .Local(
        hostname: "localhost",
        restPort: 8080,
        grpcPort: 50051,
        useSsl: false
    )
    .Build();
```

### Custom Configuration

For more control over connection parameters:

```csharp
var client = new WeaviateClientBuilder()
    .Custom(
        restEndpoint: "192.168.1.100",
        restPort: "8080",
        grpcEndpoint: "192.168.1.100",
        grpcPort: "50051",
        useSsl: false
    )
    .Build();
```

### Weaviate Cloud

For connecting to a Weaviate Cloud instance:

```csharp
var client = new WeaviateClientBuilder()
    .Cloud(
        restEndpoint: "your-cluster-id.weaviate.cloud",
        apiKey: "your-api-key"
    )
    .Build();
```

## Authentication

### API Key Authentication

For Weaviate Cloud or self-hosted instances with API key authentication:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("your-cluster.weaviate.cloud")
    .WithCredentials(Auth.ApiKey("your-api-key"))
    .Build();
```

### Bearer Token Authentication

For OAuth 2.0 bearer token authentication:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithCredentials(Auth.BearerToken(
        accessToken: "your-access-token",
        expiresIn: 3600,
        refreshToken: "your-refresh-token"
    ))
    .Build();
```

### OAuth 2.0 Client Credentials Flow

For service-to-service authentication:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithCredentials(Auth.ClientCredentials(
        clientSecret: "your-client-secret",
        scope: "openid profile email"
    ))
    .Build();
```

### OAuth 2.0 Resource Owner Password Flow

For user credential-based authentication:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithCredentials(Auth.ClientPassword(
        username: "username",
        password: "password",
        scope: "openid profile email"
    ))
    .Build();
```

## Timeout Management

Timeouts are critical for preventing indefinite waits on network requests. The Weaviate client supports differentiated timeouts for different operation groups.

### Timeout Types

The client supports four timeout categories:

1. **DefaultTimeout**: Applied to all operations (fallback timeout) - **Default: 30 seconds**
2. **InitTimeout**: Applied to initialization operations (GetMeta, Live, IsReady) - **Default: 2 seconds**
3. **DataTimeout**: Applied to data operations (Insert, Delete, Update, Reference management) - **Default: 120 seconds**
4. **QueryTimeout**: Applied to query/search operations (FetchObjects, NearText, BM25, Hybrid, Aggregate) and generative operations (Generate.*) - **Default: 60 seconds**

### Basic Timeout Configuration

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))  // Fallback for all operations
    .Build();
```

### Differentiated Timeout Strategy

For optimal performance, configure different timeouts based on operation characteristics:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))   // Fallback timeout
    .WithInitTimeout(TimeSpan.FromSeconds(5))       // Fast init timeout
    .WithDataTimeout(TimeSpan.FromSeconds(60))      // Longer for writes
    .WithQueryTimeout(TimeSpan.FromSeconds(120))    // Longest for searches
    .Build();
```

### Timeout Strategy Rationale

- **InitTimeout (2s)**: Initialization operations are quick health checks. A very short timeout quickly detects network connectivity issues during client startup without blocking for long.

- **DataTimeout (120s)**: Write operations (inserts, deletes, updates, references) may take longer due to indexing and consistency guarantees. The 120-second default allows time for moderate-to-large batch operations.

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

var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithHttpMessageHandler(httpHandler)
    .Build();
```

### Custom Headers

Add custom headers to all requests:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithHeader("X-Custom-Header", "custom-value")
    .WithHeaders(new Dictionary<string, string>
    {
        { "X-Request-ID", "request-123" },
        { "X-Custom-Header", "custom-value" }
    })
    .Build();
```

### Retry Policy

Configure automatic retry behavior for transient failures:

```csharp
var retryPolicy = new RetryPolicy(
    maxRetries: 3,
    initialBackoff: TimeSpan.FromMilliseconds(100),
    maxBackoff: TimeSpan.FromSeconds(5)
);

var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithRetryPolicy(retryPolicy)
    .Build();
```

Disable retries:

```csharp
var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .WithoutRetries()
    .Build();
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

var client = new WeaviateClientBuilder()
    .WithRestEndpoint("localhost")
    .AddHandler(new LoggingHandler())
    .Build();
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
        var client = new WeaviateClientBuilder()
            .Cloud(
                restEndpoint: "my-cluster.weaviate.cloud",
                apiKey: Environment.GetEnvironmentVariable("WEAVIATE_API_KEY")
            )
            .WithDefaultTimeout(TimeSpan.FromSeconds(30))
            .WithInitTimeout(TimeSpan.FromSeconds(5))
            .WithDataTimeout(TimeSpan.FromSeconds(60))
            .WithQueryTimeout(TimeSpan.FromSeconds(120))
            .WithRetryPolicy(new RetryPolicy(maxRetries: 3))
            .Build();

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
var client = new WeaviateClientBuilder()
    .Local(hostname: "localhost")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))
    .WithInitTimeout(TimeSpan.FromSeconds(10))
    .WithDataTimeout(TimeSpan.FromSeconds(60))
    .WithQueryTimeout(TimeSpan.FromSeconds(120))
    .Build();
```

### Example: Custom Configuration with Proxy

```csharp
var httpHandler = new HttpClientHandler
{
    Proxy = new WebProxy("http://corporate-proxy:8080"),
    UseProxy = true
};

var client = new WeaviateClientBuilder()
    .Custom(
        restEndpoint: "weaviate.example.com",
        restPort: "443"
    )
    .UseSsl(true)
    .WithHttpMessageHandler(httpHandler)
    .WithHeader("X-API-Key", "secret-key")
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))
    .WithInitTimeout(TimeSpan.FromSeconds(5))
    .WithDataTimeout(TimeSpan.FromSeconds(90))
    .WithQueryTimeout(TimeSpan.FromSeconds(180))
    .Build();
```

### Example: Per-Operation Timeout Override

```csharp
var client = new WeaviateClientBuilder()
    .Local()
    .WithDefaultTimeout(TimeSpan.FromSeconds(30))
    .WithQueryTimeout(TimeSpan.FromSeconds(120))
    .Build();

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
