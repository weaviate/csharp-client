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
    public MockHttpMessageHandler AddResponse(MockHttpResponse response)
    {
        _responses.Enqueue(response);
        return this;
    }

    /// <summary>
    /// Enqueues a JSON response with status 200 OK.
    /// </summary>
    public MockHttpMessageHandler AddJsonResponse<T>(
        T data,
        HttpStatusCode statusCode = HttpStatusCode.OK
    )
    {
        var json = JsonSerializer.Serialize(
            data,
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System
                    .Text
                    .Json
                    .Serialization
                    .JsonIgnoreCondition
                    .WhenWritingNull,
            }
        );

        return AddResponse(
            new MockHttpResponse
            {
                StatusCode = statusCode,
                Content = json,
                ContentType = "application/json",
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
        // Store the request for later assertions
        _requests.Add(request);

        // If a custom handler is set, use it
        if (_requestHandler != null)
        {
            return await _requestHandler(request);
        }

        // Otherwise, use queued responses
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException(
                "No mock response available. Use AddResponse() or SetHandler() to configure responses."
            );
        }

        var mockResponse = _responses.Dequeue();
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
}
