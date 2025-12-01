using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Weaviate.Client.Request.Transport;

/// <summary>
/// Represents an HTTP request that will be sent.
/// This captures the request before it goes over the wire, allowing inspection and testing.
/// </summary>
public class HttpRequestDetails
{
    /// <summary>
    /// The HTTP method (GET, POST, PUT, DELETE, etc.).
    /// </summary>
    public HttpMethod Method { get; init; } = HttpMethod.Get;

    /// <summary>
    /// The request URI (relative or absolute).
    /// </summary>
    public string Uri { get; init; } = string.Empty;

    /// <summary>
    /// The request headers.
    /// </summary>
    public HttpRequestHeaders Headers { get; init; } = new();

    /// <summary>
    /// The request body (if any).
    /// </summary>
    public HttpContent? Content { get; init; }

    /// <summary>
    /// The logical request that generated this HTTP request.
    /// </summary>
    public IWeaviateRequest? LogicalRequest { get; init; }
}

/// <summary>
/// Simplified representation of HTTP request headers for inspection.
/// </summary>
public class HttpRequestHeaders
{
    private readonly Dictionary<string, List<string>> _headers = new();

    public void Add(string name, string value)
    {
        if (!_headers.ContainsKey(name))
            _headers[name] = new List<string>();
        _headers[name].Add(value);
    }

    public void Add(string name, IEnumerable<string> values)
    {
        if (!_headers.ContainsKey(name))
            _headers[name] = new List<string>();
        _headers[name].AddRange(values);
    }

    public bool TryGetValues(string name, out IEnumerable<string>? values)
    {
        if (_headers.TryGetValue(name, out var list))
        {
            values = list;
            return true;
        }
        values = null;
        return false;
    }

    public IEnumerable<KeyValuePair<string, IEnumerable<string>>> GetAll()
    {
        return _headers.Select(kvp => new KeyValuePair<string, IEnumerable<string>>(kvp.Key, kvp.Value));
    }
}

/// <summary>
/// Interface for REST transport layer.
/// Implementations can be real (HTTP) or mock (for testing).
/// </summary>
public interface IRestTransport
{
    /// <summary>
    /// Sends an HTTP request and returns the response.
    /// </summary>
    /// <param name="request">The HTTP request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The HTTP response</returns>
    Task<HttpResponseMessage> SendAsync(HttpRequestDetails request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an HTTP request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="request">The HTTP request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The deserialized response</returns>
    Task<TResponse?> SendAsync<TResponse>(HttpRequestDetails request, CancellationToken cancellationToken = default);
}
