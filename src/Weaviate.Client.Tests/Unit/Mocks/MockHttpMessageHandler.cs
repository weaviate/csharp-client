using System.Net;
using System.Text;
using System.Text.Json;

namespace Weaviate.Client.Tests.Unit.Mocks;

/// <summary>
/// Mock HTTP message handler for testing REST client requests without hitting a real server.
/// Allows intercepting requests for assertions and providing mock responses.
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<MockHttpResponse> _responses = new();
    private readonly List<HttpRequestMessage> _requests = new();
    private Func<HttpRequestMessage, Task<HttpResponseMessage>>? _requestHandler;
    private Func<
        HttpRequestMessage,
        CancellationToken,
        Task<HttpResponseMessage>
    >? _requestHandlerWithToken;

    /// <summary>
    /// Gets all intercepted requests.
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

    /// <summary>
    /// Gets the last request that was made.
    /// </summary>
    public HttpRequestMessage? LastRequest => _requests.LastOrDefault();

    /// <summary>
    /// Enqueues a response to be returned for the next request.
    /// Multiple responses can be queued for sequential requests.
    /// </summary>
    /// <param name="response">The mock response to return.</param>
    /// <param name="expectedEndpoint">Optional endpoint path that must match the incoming request (e.g., "/v1/users/db"). If provided, the handler will validate the request path before returning this response.</param>
    public MockHttpMessageHandler AddResponse(
        MockHttpResponse response,
        string? expectedEndpoint = null
    )
    {
        response.ExpectedEndpoint = expectedEndpoint;
        _responses.Enqueue(response);
        return this;
    }

    /// <summary>
    /// Enqueues a JSON response with status 200 OK.
    /// </summary>
    /// <param name="data">The object to serialize as JSON.</param>
    /// <param name="expectedEndpoint">Optional endpoint path that must match the incoming request (e.g., "/v1/users/db"). If provided, the handler will validate the request path before returning this response.</param>
    /// <param name="statusCode">HTTP status code (default: 200 OK).</param>
    public MockHttpMessageHandler AddJsonResponse<T>(
        T data,
        string? expectedEndpoint = null,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        var json = JsonSerializer.Serialize(
            data,
            Rest.WeaviateRestClient.RestJsonSerializerOptions
        );

        return AddResponse(
            new MockHttpResponse
            {
                StatusCode = statusCode,
                Content = json,
                ContentType = "application/json",
                ExpectedEndpoint = expectedEndpoint,
            }
        );
    }

    /// <summary>
    /// Sets a custom handler function that will be called for each request.
    /// This allows dynamic response generation based on the request.
    /// </summary>
    public MockHttpMessageHandler SetHandler(
        Func<HttpRequestMessage, Task<HttpResponseMessage>> handler
    )
    {
        _requestHandler = handler;
        return this;
    }

    /// <summary>
    /// Sets a custom handler function (synchronous version).
    /// </summary>
    public MockHttpMessageHandler SetHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _requestHandler = req => Task.FromResult(handler(req));
        return this;
    }

    /// <summary>
    /// Sets a custom handler function that receives the cancellation token from SendAsync.
    /// Use this when the mock needs to simulate delays that respect cancellation.
    /// </summary>
    public MockHttpMessageHandler SetHandler(
        Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler
    )
    {
        _requestHandlerWithToken = handler;
        return this;
    }

    /// <summary>
    /// Clears all queued responses and recorded requests.
    /// </summary>
    public void Reset()
    {
        _responses.Clear();
        _requests.Clear();
        _requestHandler = null;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // Exclude /v1/meta requests from being added to _requests
        if (request.RequestUri?.PathAndQuery.Contains("/v1/meta") != true)
        {
            _requests.Add(request);
        }

        // Handler with cancellation token support has priority
        if (_requestHandlerWithToken != null)
        {
            var tokenResponse = await _requestHandlerWithToken(request, cancellationToken);
            if (tokenResponse != null)
            {
                return tokenResponse;
            }
        }

        // If a custom handler is set, use it
        if (_requestHandler != null)
        {
            var handlerResponse = await _requestHandler(request);
            if (handlerResponse != null)
            {
                return handlerResponse;
            }
            // If handler returns null, fall back to queued responses
        }

        // Otherwise, use queued responses
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException(
                "No mock response available. Use AddResponse() or SetHandler() to configure responses."
            );
        }

        var mockResponse = _responses.Dequeue();

        // Validate expected endpoint if specified
        if (
            mockResponse.ExpectedEndpoint != null
            && !request.RequestUri!.PathAndQuery.Contains(mockResponse.ExpectedEndpoint)
        )
        {
            throw new InvalidOperationException(
                $"Expected request to '{mockResponse.ExpectedEndpoint}' but got '{request.RequestUri?.PathAndQuery}'"
            );
        }

        var response = new HttpResponseMessage(mockResponse.StatusCode)
        {
            Content = new StringContent(
                mockResponse.Content ?? string.Empty,
                Encoding.UTF8,
                mockResponse.ContentType ?? "application/json"
            ),
        };

        foreach (var header in mockResponse.Headers)
        {
            response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return response;
    }
}

/// <summary>
/// Represents a mock HTTP response.
/// </summary>
public class MockHttpResponse
{
    public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
    public string? Content { get; set; }
    public string? ContentType { get; set; } = "application/json";
    public Dictionary<string, string> Headers { get; set; } = new();
    public string? ExpectedEndpoint { get; set; }
}
