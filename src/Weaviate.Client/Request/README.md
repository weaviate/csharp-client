# Request Interception Layer

This directory contains the request interception and transport abstraction layer for the Weaviate C# client. This architecture provides a predictable, testable, and extensible way to handle requests.

## Architecture Overview

The request flow follows this layered approach:

```
User Code
    ↓
Client API
    ↓
Logical Request (IWeaviateRequest)
    ↓
Request Pipeline (Interceptors)
    ↓
Transport Layer (IRestTransport / IGrpcTransport)
    ↓
HTTP / gRPC
    ↓
Weaviate Server
```

## Key Components

### 1. Logical Requests (`IWeaviateRequest`)

Logical requests represent operations independent of the transport protocol. They capture **what** you want to do, not **how** it will be sent.

**Example:**
```csharp
var request = new ObjectInsertRequest
{
    Collection = "Articles",
    Data = new { title = "AI Research", content = "..." },
    Id = Guid.NewGuid()
};
```

**Available Request Types:**
- **Data Operations**: `ObjectInsertRequest`, `ObjectReplaceRequest`, `ObjectDeleteRequest`, `BatchInsertRequest`
- **Query Operations**: `NearTextSearchRequest`, `NearVectorSearchRequest`, `BM25SearchRequest`, `HybridSearchRequest`, `FetchObjectsRequest`
- **Schema Operations**: `CollectionCreateRequest`, `CollectionGetRequest`, `CollectionUpdateRequest`, `CollectionDeleteRequest`
- **Meta Operations**: `GetMetaRequest`, `LivenessRequest`, `ReadinessRequest`

### 2. Request Context

`RequestContext` wraps a logical request with execution metadata:

```csharp
var context = new RequestContext(
    request: myRequest,
    cancellationToken: cancellationToken,
    timeout: TimeSpan.FromSeconds(30),
    collection: "Articles",
    tenant: "tenant1"
);
```

### 3. Request Interceptors

Interceptors allow you to hook into the request lifecycle:

```csharp
public interface IRequestInterceptor
{
    Task<RequestContext> OnBeforeSendAsync(RequestContext context);
    Task<TResponse> OnAfterReceiveAsync<TResponse>(RequestContext context, TResponse response);
    Task OnErrorAsync(RequestContext context, Exception exception);
}
```

**Built-in Interceptors:**

#### Logging Interceptor
```csharp
var loggingInterceptor = new LoggingInterceptor(
    logger: myLogger,
    logLevel: LogLevel.Information,
    logRequestDetails: true,
    logResponseDetails: false
);
```

#### Debug Interceptor
```csharp
var debugInterceptor = new DebugInterceptor(info =>
{
    Console.WriteLine($"Request: {info.OperationName}");
    Console.WriteLine($"Duration: {info.Duration.TotalMilliseconds}ms");
    Console.WriteLine($"Success: {info.Success}");
});
```

### 4. Request Pipeline

The pipeline orchestrates interceptor execution:

```csharp
var pipeline = new RequestPipeline(new[]
{
    new LoggingInterceptor(logger),
    new DebugInterceptor()
});

var result = await pipeline.ExecuteAsync(context, async ctx =>
{
    // Actual execution logic
    return await transport.SendAsync(request);
});
```

### 5. Transport Layer

Transport abstractions separate logical requests from protocol-specific implementation:

#### REST Transport
```csharp
public interface IRestTransport
{
    Task<HttpResponseMessage> SendAsync(HttpRequestDetails request, CancellationToken cancellationToken = default);
    Task<TResponse?> SendAsync<TResponse>(HttpRequestDetails request, CancellationToken cancellationToken = default);
}
```

#### gRPC Transport
```csharp
public interface IGrpcTransport
{
    Task<TResponse> UnaryCallAsync<TRequest, TResponse>(
        GrpcRequestDetails<TRequest> request,
        CancellationToken cancellationToken = default);
}
```

## Benefits for Testing

### 1. Offline E2E Testing

Mock transports allow testing without a real Weaviate server:

```csharp
// Setup
var mockRest = new MockRestTransport();
var mockGrpc = new MockGrpcTransport();

// Configure responses
mockRest.When()
    .ForOperation("ObjectInsert")
    .RespondWithSuccess(new { id = Guid.NewGuid() });

// Create client with mock transports
var client = new WeaviateClient(mockRest, mockGrpc);

// Execute operation
await client.Collections.Get("Test").Data.Insert(myObject);

// Assert request structure
var capturedRequest = mockRest.AssertRestRequest()
    .ForOperation("ObjectInsert")
    .WithMethod(HttpMethod.Post)
    .WasSent();
```

### 2. Request Validation

Validate that requests have the correct structure and values:

```csharp
// Assert specific request properties
var request = mockRest.AssertRestRequest()
    .ForOperation("SearchNearText")
    .WasSent();

var logicalRequest = request.Request.LogicalRequest as NearTextSearchRequest;
Assert.Equal("Articles", logicalRequest.Collection);
Assert.Contains("AI research", logicalRequest.Query);
```

### 3. Scenario Testing

Test complex multi-step scenarios:

```csharp
// Setup multiple responses
mockRest.When().ForOperation("CollectionCreate").RespondWithSuccess();
mockRest.When().ForOperation("ObjectInsert").RespondWithSuccess();
mockGrpc.AddResponseRuleForOperation("SearchNearText", searchResults);

// Execute operations
await client.Collections.Create(...);
await client.Collections.Get("Test").Data.Insert(...);
var results = await client.Collections.Get("Test").Query.NearText(...);

// Verify sequence
mockRest.AssertRestRequest().ForOperation("CollectionCreate").WasSent();
mockRest.AssertRestRequest().ForOperation("ObjectInsert").WasSent();
mockGrpc.AssertGrpcRequest().ForOperation("SearchNearText").WasSent();
```

### 4. Error Testing

Test error handling without triggering real errors:

```csharp
mockRest.When()
    .ForOperation("ObjectInsert")
    .RespondWith(HttpStatusCode.Conflict, new { error = "Object exists" });

await Assert.ThrowsAsync<WeaviateConflictException>(() =>
    client.Collections.Get("Test").Data.Insert(myObject)
);
```

## Testing Utilities

### Request Assertions

Fluent API for asserting requests:

```csharp
// REST assertions
mockRest.AssertRestRequest()
    .ForOperation("ObjectInsert")
    .WithMethod(HttpMethod.Post)
    .WithUri("/v1/objects")
    .WasSent();

// gRPC assertions
mockGrpc.AssertGrpcRequest()
    .ForOperation("SearchNearText")
    .OfType(RequestType.Query)
    .WasSent();

// Count assertions
mockRest.AssertRestRequest()
    .ForOperation("ObjectInsert")
    .WasSent(expectedCount: 5);

// Negative assertions
mockRest.AssertRestRequest()
    .ForOperation("ObjectDelete")
    .WasNotSent();
```

### Response Building

Fluent API for building mock responses:

```csharp
mockRest.When()
    .ForOperation("SearchNearText")
    .RespondWith(builder => builder
        .Success()
        .WithBody(new { results = searchResults })
        .WithHeader("X-Request-Id", "123")
    );
```

### Debug Information

Capture detailed timing and execution information:

```csharp
var debugInterceptor = new DebugInterceptor();

// ... execute requests ...

foreach (var info in debugInterceptor.CapturedRequests.Values)
{
    Console.WriteLine($"{info.OperationName}:");
    Console.WriteLine($"  Duration: {info.Duration.TotalMilliseconds}ms");
    Console.WriteLine($"  Success: {info.Success}");
    Console.WriteLine($"  Request: {info.RequestDetails}");
    Console.WriteLine($"  Response: {info.ResponseDetails}");
}
```

## Integration with Existing Client

The request layer integrates with the existing client architecture:

```
WeaviateClient
    ├── RequestPipeline (with interceptors)
    ├── IRestTransport (HttpRestTransport or MockRestTransport)
    └── IGrpcTransport (GrpcTransport or MockGrpcTransport)
```

### Configuration

```csharp
var client = new WeaviateClientBuilder()
    .WithEndpoint("http://localhost:8080")
    .WithInterceptor(new LoggingInterceptor(logger))
    .WithInterceptor(new DebugInterceptor())
    .Build();
```

### Custom Transports

You can inject custom transports for testing or special scenarios:

```csharp
var mockRest = new MockRestTransport();
var mockGrpc = new MockGrpcTransport();

var client = new WeaviateClient(mockRest, mockGrpc);
```

## Examples

See `Examples/OfflineE2ETestExample.cs` for comprehensive examples demonstrating:

1. Basic request/response mocking
2. Request validation
3. Complex scenario testing
4. Error scenario testing
5. Using interceptors for debugging
6. Advanced request filtering
7. Timeout testing
8. gRPC request validation

## Future Extensions

This architecture enables future enhancements:

1. **Request batching**: Automatically batch multiple operations
2. **Caching**: Cache responses at the request level
3. **Retry policies**: Operation-specific retry logic
4. **Metrics collection**: Track request performance
5. **Request transformation**: Modify requests based on rules
6. **Response validation**: Validate responses against schemas
7. **Load balancing**: Route requests to different servers
8. **Circuit breakers**: Prevent cascading failures

## Summary

The request interception layer provides:

✅ **Predictable flow**: User → Client → Logical Request → Pipeline → Transport → Wire

✅ **Testability**: Mock transports enable offline E2E testing

✅ **Inspectability**: Capture and validate requests before they're sent

✅ **Extensibility**: Interceptors allow custom behavior at any stage

✅ **Protocol-agnostic**: Separate what you want to do from how it's sent

This architecture makes the client easier to test, maintain, and extend.
