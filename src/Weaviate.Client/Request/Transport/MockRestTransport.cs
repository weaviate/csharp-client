using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Weaviate.Client.Request.Transport;

/// <summary>
/// Mock REST transport for testing.
/// Captures requests and allows returning pre-configured responses.
/// </summary>
public class MockRestTransport : MockTransportBase<HttpRequestDetails, HttpResponseMessage, CapturedRestRequest>, IRestTransport
{
    private Func<HttpRequestDetails, HttpResponseMessage>? _defaultResponseFactory;

    /// <summary>
    /// Sets a default response factory for requests that don't match any rules.
    /// </summary>
    public void SetDefaultResponse(Func<HttpRequestDetails, HttpResponseMessage> factory)
    {
        _defaultResponseFactory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Sets a default success response with optional JSON body.
    /// </summary>
    public void SetDefaultSuccessResponse(object? body = null)
    {
        _defaultResponseFactory = _ => CreateJsonResponse(HttpStatusCode.OK, body);
    }

    /// <summary>
    /// Adds a response rule that matches requests and returns a configured response.
    /// </summary>
    public void AddResponseRule(Func<HttpRequestDetails, bool> matcher, Func<HttpRequestDetails, HttpResponseMessage> responseFactory)
    {
        if (matcher == null) throw new ArgumentNullException(nameof(matcher));
        if (responseFactory == null) throw new ArgumentNullException(nameof(responseFactory));

        AddRule(new ResponseRuleBase<HttpRequestDetails, HttpResponseMessage>
        {
            Matcher = matcher,
            ResponseFactory = responseFactory
        });
    }

    /// <summary>
    /// Adds a response rule for a specific HTTP method and URI pattern.
    /// </summary>
    public void AddResponseRule(HttpMethod method, string uriPattern, HttpStatusCode statusCode, object? body = null)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (string.IsNullOrWhiteSpace(uriPattern)) throw new ArgumentException("URI pattern cannot be null or whitespace.", nameof(uriPattern));

        AddResponseRule(
            req => req.Method == method && req.Uri.Contains(uriPattern),
            _ => CreateJsonResponse(statusCode, body)
        );
    }

    /// <summary>
    /// Adds a response rule for a specific operation name.
    /// </summary>
    public void AddResponseRuleForOperation(string operationName, HttpStatusCode statusCode, object? body = null)
    {
        if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentException("Operation name cannot be null or whitespace.", nameof(operationName));

        AddResponseRule(
            req => req.LogicalRequest?.OperationName == operationName,
            _ => CreateJsonResponse(statusCode, body)
        );
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestDetails request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Capture the request
        CaptureRequest(new CapturedRestRequest
        {
            Request = request,
            Timestamp = DateTime.UtcNow,
            LogicalRequest = request.LogicalRequest
        });

        // Find matching response rule
        var response = FindMatchingResponse(request);
        if (response != null)
        {
            return Task.FromResult(response);
        }

        // Use default response factory if set
        if (_defaultResponseFactory != null)
        {
            return Task.FromResult(_defaultResponseFactory(request));
        }

        // Default: return 200 OK with empty body
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }

    public async Task<TResponse?> SendAsync<TResponse>(HttpRequestDetails request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var response = await SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}");
        }

        if (response.Content == null)
        {
            return default;
        }

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        return JsonSerializer.Deserialize<TResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Helper to create a JSON response.
    /// </summary>
    public static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, object? body = null)
    {
        var response = new HttpResponseMessage(statusCode);

        if (body != null)
        {
            var json = body is string str ? str : JsonSerializer.Serialize(body);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return response;
    }

    /// <summary>
    /// Gets requests matching a predicate.
    /// </summary>
    public IEnumerable<CapturedRestRequest> GetRequests(Func<HttpRequestDetails, bool> predicate)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return GetCapturedRequests(cr => predicate(cr.Request));
    }

    /// <summary>
    /// Gets requests for a specific operation.
    /// </summary>
    public IEnumerable<CapturedRestRequest> GetRequestsForOperation(string operationName)
    {
        if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentException("Operation name cannot be null or whitespace.", nameof(operationName));
        return GetRequests(req => req.LogicalRequest?.OperationName == operationName);
    }

    /// <summary>
    /// Gets requests for a specific HTTP method and URI pattern.
    /// </summary>
    public IEnumerable<CapturedRestRequest> GetRequests(HttpMethod method, string uriPattern)
    {
        if (method == null) throw new ArgumentNullException(nameof(method));
        if (string.IsNullOrWhiteSpace(uriPattern)) throw new ArgumentException("URI pattern cannot be null or whitespace.", nameof(uriPattern));
        return GetRequests(req => req.Method == method && req.Uri.Contains(uriPattern));
    }
}

/// <summary>
/// Represents a captured REST/HTTP request.
/// </summary>
public class CapturedRestRequest : CapturedRequestBase
{
    /// <summary>
    /// The captured HTTP request details.
    /// </summary>
    public required HttpRequestDetails Request { get; init; }
}
