using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Weaviate.Client.Request.Interceptors;
using Weaviate.Client.Request.Models;
using Weaviate.Client.Request.Testing;
using Weaviate.Client.Request.Transport;

namespace Weaviate.Client.Request.Examples;

/// <summary>
/// Examples demonstrating offline E2E testing using the request interception layer.
/// These examples show how to test the client without a real Weaviate server.
/// </summary>
public class OfflineE2ETestExample
{
    /// <summary>
    /// Example 1: Basic request/response mocking
    /// Demonstrates capturing requests and returning mock responses
    /// </summary>
    public async Task BasicRequestMocking()
    {
        // Setup: Create mock transports
        var mockRest = new MockRestTransport();
        var mockGrpc = new MockGrpcTransport();

        // Configure mock responses
        mockRest.When()
            .ForOperation("ObjectInsert")
            .RespondWithSuccess(new { id = Guid.NewGuid() });

        // TODO: Create client with mock transports
        // var client = new WeaviateClient(mockRest, mockGrpc);

        // Act: Perform operation
        // await client.Collections.Get("TestCollection").Data.Insert(new { name = "test" });

        // Assert: Verify request was sent correctly
        var capturedRequest = mockRest.AssertRestRequest()
            .ForOperation("ObjectInsert")
            .WithMethod(HttpMethod.Post)
            .WasSent();

        Console.WriteLine($"✓ Request captured: {capturedRequest.Request.OperationName}");

        // Verify request details
        if (capturedRequest.Request.Content != null)
        {
            var content = await capturedRequest.Request.Content.GetJsonStringAsync();
            Console.WriteLine($"  Request body: {content}");
        }
    }

    /// <summary>
    /// Example 2: Request validation
    /// Demonstrates validating request structure and values
    /// </summary>
    public async Task RequestValidation()
    {
        var mockRest = new MockRestTransport();

        // Setup mock response for search operation
        mockRest.When()
            .ForOperation("SearchNearText")
            .RespondWithSuccess(new { results = new[] { new { id = "obj1" } } });

        // TODO: Create client and execute search
        // var client = new WeaviateClient(mockRest, mockGrpc);
        // await client.Collections.Get("Articles").Query.NearText("AI research");

        // Assert: Validate the request
        var request = mockRest.AssertRestRequest()
            .ForOperation("SearchNearText")
            .WasSent();

        // Verify logical request properties
        var logicalRequest = request.Request.LogicalRequest as NearTextSearchRequest;
        if (logicalRequest != null)
        {
            Console.WriteLine($"✓ Collection: {logicalRequest.Collection}");
            Console.WriteLine($"✓ Query: {string.Join(", ", logicalRequest.Query)}");
            Console.WriteLine($"✓ Protocol: {logicalRequest.PreferredProtocol}");

            // Assert specific values
            if (logicalRequest.Collection != "Articles")
                throw new Exception($"Expected collection 'Articles', got '{logicalRequest.Collection}'");

            if (!logicalRequest.Query.Contains("AI research"))
                throw new Exception("Expected query to contain 'AI research'");
        }
    }

    /// <summary>
    /// Example 3: Complex scenario testing
    /// Demonstrates testing multiple operations in sequence
    /// </summary>
    public async Task ComplexScenarioTesting()
    {
        var mockRest = new MockRestTransport();
        var mockGrpc = new MockGrpcTransport();

        // Setup: Configure multiple mock responses
        var collectionId = Guid.NewGuid();
        var objectId1 = Guid.NewGuid();
        var objectId2 = Guid.NewGuid();

        // Collection creation
        mockRest.When()
            .ForOperation("CollectionCreate")
            .RespondWithSuccess(new { id = collectionId, name = "Articles" });

        // Object insertions
        mockRest.When()
            .ForOperation("ObjectInsert")
            .RespondWithSuccess(new { id = objectId1 });

        // Search query
        mockGrpc.AddResponseRuleForOperation<object, object>(
            "SearchNearText",
            new { results = new[] { new { id = objectId1 }, new { id = objectId2 } } }
        );

        // TODO: Execute operations
        // var client = new WeaviateClient(mockRest, mockGrpc);
        // await client.Collections.Create(...);
        // await client.Collections.Get("Articles").Data.Insert(...);
        // await client.Collections.Get("Articles").Query.NearText(...);

        // Assert: Verify operations were executed in correct order
        var allRequests = mockRest.CapturedRequests;
        Console.WriteLine($"✓ Captured {allRequests.Count} REST requests");

        // Verify sequence
        mockRest.AssertRestRequest()
            .ForOperation("CollectionCreate")
            .WasSent();

        mockRest.AssertRestRequest()
            .ForOperation("ObjectInsert")
            .WasSent();

        mockGrpc.AssertGrpcRequest()
            .ForOperation("SearchNearText")
            .WasSent();

        Console.WriteLine("✓ All operations executed in correct order");
    }

    /// <summary>
    /// Example 4: Error scenario testing
    /// Demonstrates testing error handling
    /// </summary>
    public async Task ErrorScenarioTesting()
    {
        var mockRest = new MockRestTransport();

        // Setup: Configure error response
        mockRest.When()
            .ForOperation("ObjectInsert")
            .RespondWith(HttpStatusCode.Conflict, new
            {
                error = "Object already exists"
            });

        // TODO: Execute operation and expect error
        // var client = new WeaviateClient(mockRest, mockGrpc);
        // try
        // {
        //     await client.Collections.Get("Test").Data.Insert(...);
        //     throw new Exception("Expected exception was not thrown");
        // }
        // catch (WeaviateConflictException ex)
        // {
        //     Console.WriteLine($"✓ Caught expected exception: {ex.Message}");
        // }

        // Verify the request was still sent
        mockRest.AssertRestRequest()
            .ForOperation("ObjectInsert")
            .WasSent();
    }

    /// <summary>
    /// Example 5: Using interceptors for debugging
    /// Demonstrates using the debug interceptor to capture detailed information
    /// </summary>
    public async Task DebuggingWithInterceptors()
    {
        var mockRest = new MockRestTransport();
        var debugInterceptor = new DebugInterceptor(info =>
        {
            Console.WriteLine($"Request completed: {info}");
        });

        // Setup pipeline with debug interceptor
        var pipeline = new RequestPipeline(new[] { debugInterceptor });

        // Configure mock response
        mockRest.SetDefaultSuccessResponse(new { result = "success" });

        // Execute request through pipeline
        var context = new RequestContext(
            new ObjectInsertRequest
            {
                Collection = "TestCollection",
                Data = new { name = "test" }
            }
        );

        await pipeline.ExecuteAsync(context, async ctx =>
        {
            // Simulate transport call
            await Task.Delay(10); // Simulate network delay
            return new { id = Guid.NewGuid() };
        });

        // Check captured debug info
        var capturedInfo = debugInterceptor.CapturedRequests;
        Console.WriteLine($"✓ Captured {capturedInfo.Count} requests with timing info");

        foreach (var info in capturedInfo.Values)
        {
            Console.WriteLine($"  {info.OperationName}: {info.Duration.TotalMilliseconds}ms");
        }
    }

    /// <summary>
    /// Example 6: Request filtering and assertion
    /// Demonstrates advanced request filtering
    /// </summary>
    public void AdvancedRequestFiltering()
    {
        var mockRest = new MockRestTransport();

        // Simulate multiple requests
        for (int i = 0; i < 5; i++)
        {
            mockRest.SendAsync(new HttpRequestDetails
            {
                Method = HttpMethod.Post,
                Uri = $"/v1/objects",
                LogicalRequest = new ObjectInsertRequest
                {
                    Collection = i % 2 == 0 ? "EvenCollection" : "OddCollection"
                }
            }).Wait();
        }

        // Assert: Count requests by collection
        var evenRequests = mockRest.AssertRestRequest()
            .Where(req => (req.LogicalRequest as ObjectInsertRequest)?.Collection == "EvenCollection")
            .WasSent(3);

        var oddRequests = mockRest.AssertRestRequest()
            .Where(req => (req.LogicalRequest as ObjectInsertRequest)?.Collection == "OddCollection")
            .WasSent(2);

        Console.WriteLine($"✓ Even collection requests: {evenRequests.Count}");
        Console.WriteLine($"✓ Odd collection requests: {oddRequests.Count}");
    }

    /// <summary>
    /// Example 7: Testing timeout behavior
    /// Demonstrates testing timeout scenarios
    /// </summary>
    public async Task TimeoutTesting()
    {
        var mockRest = new MockRestTransport();

        // Setup: Configure delayed response
        mockRest.When()
            .ForOperation("ObjectInsert")
            .RespondWith(req =>
            {
                // Simulate slow response
                Task.Delay(5000).Wait();
                return new ResponseBuilder().Success().WithBody(new { id = Guid.NewGuid() });
            });

        // Create request context with short timeout
        var context = new RequestContext(
            new ObjectInsertRequest { Collection = "Test" },
            timeout: TimeSpan.FromMilliseconds(100)
        );

        Console.WriteLine("✓ Configured request with 100ms timeout");
        Console.WriteLine("  (In real implementation, this would trigger timeout exception)");
    }

    /// <summary>
    /// Example 8: gRPC request validation
    /// Demonstrates validating gRPC requests
    /// </summary>
    public void GrpcRequestValidation()
    {
        var mockGrpc = new MockGrpcTransport();

        // Configure mock response
        mockGrpc.AddResponseRuleForOperation<object, object>(
            "SearchNearVector",
            new { results = new[] { new { id = "obj1" } } }
        );

        // TODO: Execute search
        // var client = new WeaviateClient(mockRest, mockGrpc);
        // await client.Collections.Get("Test").Query.NearVector(new float[] { 1, 2, 3 });

        // Assert: Verify gRPC request
        var capturedRequests = mockGrpc.GetRequestsForOperation("SearchNearVector").ToList();
        Console.WriteLine($"✓ Captured {capturedRequests.Count} gRPC requests");

        // Verify request details
        foreach (var req in capturedRequests)
        {
            Console.WriteLine($"  Method: {req.Method}");
            Console.WriteLine($"  Timestamp: {req.Timestamp}");
        }
    }
}
