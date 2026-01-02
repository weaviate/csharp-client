using System.Net;
using System.Text;
using System.Text.Json;
using Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit.Mocks;

/// <summary>
/// Helper class for creating WeaviateClient instances with mocked HTTP handlers for unit testing.
/// </summary>
public static class MockWeaviateClient
{
    /// <summary>
    /// Unified mock client factory. Supports optional handler chain, meta auto-response, and providing a custom request handler
    /// (sync, async, or cancellation-aware). If no handler is provided and autoMeta=true, a default /v1/meta handler is installed.
    /// Order of precedence for handler selection: handlerWithToken -> asyncHandler -> syncHandler -> meta auto handler.
    /// If autoMeta=true and a custom handler is provided, meta responses are injected when the custom handler returns null for /v1/meta.
    /// </summary>
    public static (WeaviateClient Client, MockHttpMessageHandler Handler) CreateWithMockHandler(
        Func<MockHttpMessageHandler, HttpMessageHandler>? handlerChainFactory = null,
        bool autoMeta = true,
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? handlerWithToken =
            null,
        Func<HttpRequestMessage, Task<HttpResponseMessage>>? asyncHandler = null,
        Func<HttpRequestMessage, HttpResponseMessage>? syncHandler = null
    )
    {
        var leaf = new MockHttpMessageHandler();

        // Local helpers to reduce duplication.
        static bool IsMeta(HttpRequestMessage req) =>
            req.RequestUri?.PathAndQuery.Contains("/v1/meta") == true;
        static HttpResponseMessage BuildMeta()
        {
            var metaResponse = MockResponses.MetaInfo();
            return new HttpResponseMessage(metaResponse.StatusCode)
            {
                Content = new System.Net.Http.StringContent(
                    metaResponse.Content ?? string.Empty,
                    Encoding.UTF8,
                    metaResponse.ContentType ?? "application/json"
                ),
            };
        }

        // Install appropriate handler with meta fallback if requested.
        if (handlerWithToken is not null)
        {
            leaf.SetHandler(
                async (req, ct) =>
                {
                    if (autoMeta && IsMeta(req))
                    {
                        return BuildMeta();
                    }
                    return await handlerWithToken(req, ct);
                }
            );
        }
        else if (asyncHandler is not null)
        {
            leaf.SetHandler(async req =>
            {
                if (autoMeta && IsMeta(req))
                {
                    return BuildMeta();
                }
                return await asyncHandler(req);
            });
        }
        else if (syncHandler is not null)
        {
            leaf.SetHandler(req =>
            {
                if (autoMeta && IsMeta(req))
                {
                    return BuildMeta();
                }
                return syncHandler(req);
            });
        }
        else if (autoMeta)
        {
            leaf.SetHandler(req => IsMeta(req) ? BuildMeta() : null!);
        }

        var topHandler = handlerChainFactory != null ? handlerChainFactory(leaf) : leaf;
        var channel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(channel);
        var client = new WeaviateClient(httpMessageHandler: topHandler, grpcClient: grpcClient);
        return (client, leaf);
    }

    public static (WeaviateClient Client, MockHttpMessageHandler Handler) CreateWithMockHandler()
    {
        return CreateWithMockHandler(handlerChainFactory: null, autoMeta: true);
    }

    /// <summary>
    /// Convenience method for creating a client with a simple async handler.
    /// Returns only the client (not the handler) for backward compatibility.
    /// </summary>
    public static WeaviateClient CreateWithHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> asyncHandler
    )
    {
        var (client, _) = CreateWithMockHandler(asyncHandler: asyncHandler);
        return client;
    }

    /// <summary>
    /// Creates a WeaviateClient that captures gRPC Search requests.
    /// Delegates to MockGrpcClient for consistency.
    /// </summary>
    internal static (
        WeaviateClient Client,
        Func<Weaviate.Client.Grpc.Protobuf.V1.SearchRequest?> GetCapturedRequest
    ) CreateWithSearchCapture()
    {
        return MockGrpcClient.CreateWithSearchCapture();
    }

    /// <summary>
    /// Creates a WeaviateClient that captures gRPC Aggregate requests.
    /// Delegates to MockGrpcClient for consistency.
    /// </summary>
    internal static (
        WeaviateClient Client,
        Func<Weaviate.Client.Grpc.Protobuf.V1.AggregateRequest?> GetCapturedRequest
    ) CreateWithAggregateCapture()
    {
        return MockGrpcClient.CreateWithAggregateCapture();
    }

    // Legacy CreateWithHandler overloads consolidated into unified factory above.
}

/// <summary>
/// Helper class for creating WeaviateClient instances with gRPC request capture capabilities.
/// </summary>
internal static class MockGrpcClient
{
    /// <summary>
    /// Creates a WeaviateClient that captures gRPC requests for the specified path pattern.
    /// </summary>
    /// <typeparam name="TRequest">The protobuf request type to capture (e.g., SearchRequest, BatchObjectsRequest)</typeparam>
    /// <param name="pathPattern">The gRPC path pattern to match (e.g., "/weaviate.v1.Weaviate/Search")</param>
    /// <param name="responseFactory">Optional factory to create custom response messages. If null, returns empty SearchReply for Search operations.</param>
    /// <returns>A tuple containing the client and a function to retrieve the captured request</returns>
    public static (
        WeaviateClient Client,
        Func<TRequest?> GetCapturedRequest
    ) CreateWithRequestCapture<TRequest>(
        string pathPattern,
        Func<Google.Protobuf.IMessage>? responseFactory = null
    )
        where TRequest : class, Google.Protobuf.IMessage<TRequest>, new()
    {
        TRequest? capturedRequest = null;

        var channel = NoOpGrpcChannel.Create(
            customAsyncHandler: async (request, ct) =>
            {
                var path = request.RequestUri?.PathAndQuery ?? string.Empty;
                if (path.Contains(pathPattern))
                {
                    var content = await request.Content!.ReadAsByteArrayAsync(ct);
                    capturedRequest = DecodeGrpcRequest<TRequest>(content);

                    // Use factory if provided, otherwise default to empty SearchReply for Search operations
                    var replyMessage =
                        responseFactory?.Invoke()
                        ?? new SearchReply { Collection = "TestCollection" };
                    return Helpers.CreateGrpcResponse(replyMessage);
                }
                return null;
            }
        );

        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(channel);
        var client = new WeaviateClient(grpcClient: grpcClient);

        return (client, () => capturedRequest);
    }

    /// <summary>
    /// Creates a WeaviateClient that captures Search requests.
    /// Convenience method for the most common use case.
    /// </summary>
    public static (
        WeaviateClient Client,
        Func<SearchRequest?> GetCapturedRequest
    ) CreateWithSearchCapture()
    {
        return CreateWithRequestCapture<SearchRequest>("/weaviate.v1.Weaviate/Search");
    }

    /// <summary>
    /// Creates a WeaviateClient that captures Aggregate requests.
    /// </summary>
    public static (
        WeaviateClient Client,
        Func<Grpc.Protobuf.V1.AggregateRequest?> GetCapturedRequest
    ) CreateWithAggregateCapture()
    {
        return CreateWithRequestCapture<Grpc.Protobuf.V1.AggregateRequest>(
            "/weaviate.v1.Weaviate/Aggregate",
            () => new AggregateReply { Collection = "TestCollection" }
        );
    }

    private static T DecodeGrpcRequest<T>(byte[] content)
        where T : Google.Protobuf.IMessage<T>, new()
    {
        // gRPC wire format: 1 byte compressed flag + 4 bytes length + message bytes
        var messageBytes = content.Skip(5).ToArray();
        var parser = new Google.Protobuf.MessageParser<T>(() => new T());
        return parser.ParseFrom(messageBytes);
    }
}

/// <summary>
/// Extension methods for making assertions on HTTP requests.
/// </summary>
public static class HttpRequestAssertions
{
    /// <summary>
    /// Asserts that the request method matches the expected HTTP method.
    /// </summary>
    public static HttpRequestMessage ShouldHaveMethod(
        this HttpRequestMessage request,
        HttpMethod expectedMethod
    )
    {
        if (request.Method != expectedMethod)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected HTTP method {expectedMethod}, but got {request.Method}"
            );
        }
        return request;
    }

    /// <summary>
    /// Asserts that the request URI contains the expected path.
    /// </summary>
    public static HttpRequestMessage ShouldHavePath(
        this HttpRequestMessage request,
        string expectedPath
    )
    {
        var actualPath = request.RequestUri?.PathAndQuery ?? "";
        if (!actualPath.Contains(expectedPath))
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected request path to contain '{expectedPath}', but got '{actualPath}'"
            );
        }
        return request;
    }

    /// <summary>
    /// Asserts that the request URI matches the expected pattern using a predicate.
    /// </summary>
    public static HttpRequestMessage ShouldHaveUri(
        this HttpRequestMessage request,
        Func<Uri, bool> predicate,
        string? failureMessage = null
    )
    {
        if (request.RequestUri == null || !predicate(request.RequestUri))
        {
            throw new Xunit.Sdk.XunitException(
                failureMessage
                    ?? $"Request URI did not match expected pattern: {request.RequestUri}"
            );
        }
        return request;
    }

    /// <summary>
    /// Asserts that the request contains a specific header with the expected value.
    /// </summary>
    public static HttpRequestMessage ShouldHaveHeader(
        this HttpRequestMessage request,
        string headerName,
        string expectedValue
    )
    {
        if (!request.Headers.TryGetValues(headerName, out var values))
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected header '{headerName}' not found in request"
            );
        }

        var actualValue = string.Join(",", values);
        if (actualValue != expectedValue)
        {
            throw new Xunit.Sdk.XunitException(
                $"Expected header '{headerName}' to have value '{expectedValue}', but got '{actualValue}'"
            );
        }
        return request;
    }

    /// <summary>
    /// Deserializes the request body as JSON and returns the object.
    /// </summary>
    public static async Task<T?> GetJsonBody<T>(this HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return default;
        }

        var json = await request.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(
            json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }

    /// <summary>
    /// Gets the request body as a string.
    /// </summary>
    public static async Task<string> GetBodyAsString(this HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return string.Empty;
        }

        return await request.Content.ReadAsStringAsync();
    }

    /// <summary>
    /// Asserts that the request body contains the expected JSON structure.
    /// </summary>
    public static async Task<HttpRequestMessage> ShouldHaveJsonBody<T>(
        this HttpRequestMessage request,
        Action<T> assertions
    )
    {
        var body = await request.GetJsonBody<T>();
        if (body == null)
        {
            throw new Xunit.Sdk.XunitException(
                "Request body was null or could not be deserialized"
            );
        }

        assertions(body);
        return request;
    }
}

/// <summary>
/// Helper methods for creating common mock responses.
/// </summary>
public static class MockResponses
{
    /// <summary>
    /// Creates a mock response for a successful collection creation.
    /// </summary>
    public static MockHttpResponse CollectionCreated(string name, params string[] properties)
    {
        var response = new
        {
            @class = name,
            properties = properties
                .Select(p => new { name = p, dataType = new[] { "text" } })
                .ToArray(),
            vectorizer = "none",
            vectorIndexType = "hnsw",
            vectorIndexConfig = new { },
            multiTenancyConfig = new { enabled = false },
        };

        return new MockHttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ),
        };
    }

    /// <summary>
    /// Creates a mock response for getting meta information.
    /// </summary>
    public static MockHttpResponse MetaInfo(string version = "1.27.0")
    {
        var response = new Rest.Dto.Meta
        {
            Hostname = "localhost",
            Version = version,
            Modules = new Dictionary<string, object>(),
        };

        return new MockHttpResponse
        {
            StatusCode = HttpStatusCode.OK,
            Content = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ),
        };
    }

    /// <summary>
    /// Creates an error response.
    /// </summary>
    public static MockHttpResponse Error(
        HttpStatusCode statusCode,
        string message,
        string? code = null
    )
    {
        var response = new { error = new[] { new { message, code } } };

        return new MockHttpResponse
        {
            StatusCode = statusCode,
            Content = JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            ),
        };
    }

    /// <summary>
    /// Creates a 200 OK response with empty body.
    /// </summary>
    public static MockHttpResponse Ok() => new() { StatusCode = HttpStatusCode.OK, Content = "{}" };

    /// <summary>
    /// Creates a 204 No Content response.
    /// </summary>
    public static MockHttpResponse NoContent() =>
        new() { StatusCode = HttpStatusCode.NoContent, Content = "" };
}
