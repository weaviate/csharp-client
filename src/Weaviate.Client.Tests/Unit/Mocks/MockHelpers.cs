using System.Net;
using System.Text;
using System.Text.Json;

namespace Weaviate.Client.Tests.Unit.Mocks;

/// <summary>
/// Helper class for creating WeaviateClient instances with mocked HTTP handlers for unit testing.
/// </summary>
public static class MockWeaviateClient
{
    /// <summary>
    /// Creates a WeaviateClient with a mock HTTP handler.
    /// Automatically includes a MetaInfo handler for the /v1/meta endpoint.
    /// </summary>
    public static (WeaviateClient Client, MockHttpMessageHandler Handler) CreateWithMockHandler()
    {
        var handler = new MockHttpMessageHandler();

        // Set a handler that returns MetaInfo for /v1/meta, otherwise uses the queue
        handler.SetHandler(req =>
        {
            if (req.RequestUri?.PathAndQuery.Contains("/v1/meta") == true)
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

            // For all other requests, let SendAsync use the queued responses
            return null!;

            throw new InvalidOperationException(
                $"No mock response configured for {req.Method} {req.RequestUri?.PathAndQuery}. "
                    + "Use handler.AddResponse() or handler.SetHandler() to configure responses."
            );
        });

        // Create a no-op gRPC channel to avoid connecting to the gRPC port
        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);

        var client = new WeaviateClient(httpMessageHandler: handler, grpcClient: grpcClient);

        return (client, handler);
    }

    /// <summary>
    /// Creates a WeaviateClient with a pre-configured handler function.
    /// </summary>
    public static WeaviateClient CreateWithHandler(
        Func<HttpRequestMessage, HttpResponseMessage> handlerFunc
    )
    {
        var handler = new MockHttpMessageHandler();
        handler.SetHandler(handlerFunc);

        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);

        return new WeaviateClient(httpMessageHandler: handler, grpcClient: grpcClient);
    }

    /// <summary>
    /// Creates a WeaviateClient with a pre-configured async handler function.
    /// </summary>
    public static WeaviateClient CreateWithHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handlerFunc
    )
    {
        var handler = new MockHttpMessageHandler();
        handler.SetHandler(handlerFunc);

        var noOpChannel = NoOpGrpcChannel.Create();
        var grpcClient = new Weaviate.Client.Grpc.WeaviateGrpcClient(noOpChannel);

        return new WeaviateClient(httpMessageHandler: handler, grpcClient: grpcClient);
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
            Modules = new { },
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
