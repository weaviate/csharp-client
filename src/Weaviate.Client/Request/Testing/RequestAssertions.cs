using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Weaviate.Client.Request.Transport;

namespace Weaviate.Client.Request.Testing;

/// <summary>
/// Fluent assertion helpers for validating captured requests in tests.
/// </summary>
public static class RequestAssertions
{
    /// <summary>
    /// Asserts that a REST request was sent.
    /// </summary>
    public static RestRequestAssertion AssertRestRequest(this MockRestTransport transport)
    {
        return new RestRequestAssertion(transport);
    }

    /// <summary>
    /// Asserts that a gRPC request was sent.
    /// </summary>
    public static GrpcRequestAssertion AssertGrpcRequest(this MockGrpcTransport transport)
    {
        return new GrpcRequestAssertion(transport);
    }
}

/// <summary>
/// Fluent assertions for REST requests.
/// </summary>
public class RestRequestAssertion
{
    private readonly MockRestTransport _transport;
    private readonly List<Func<HttpRequestDetails, bool>> _predicates = new();
    private string? _description;

    public RestRequestAssertion(MockRestTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Filters requests by HTTP method.
    /// </summary>
    public RestRequestAssertion WithMethod(HttpMethod method)
    {
        _predicates.Add(req => req.Method == method);
        _description = $"{_description} with method {method}";
        return this;
    }

    /// <summary>
    /// Filters requests by URI pattern.
    /// </summary>
    public RestRequestAssertion WithUri(string uriPattern)
    {
        _predicates.Add(req => req.Uri.Contains(uriPattern));
        _description = $"{_description} with URI containing '{uriPattern}'";
        return this;
    }

    /// <summary>
    /// Filters requests by operation name.
    /// </summary>
    public RestRequestAssertion ForOperation(string operationName)
    {
        _predicates.Add(req => req.LogicalRequest?.OperationName == operationName);
        _description = $"{_description} for operation '{operationName}'";
        return this;
    }

    /// <summary>
    /// Filters requests by request type.
    /// </summary>
    public RestRequestAssertion OfType(RequestType type)
    {
        _predicates.Add(req => req.LogicalRequest?.Type == type);
        _description = $"{_description} of type {type}";
        return this;
    }

    /// <summary>
    /// Filters requests by a custom predicate.
    /// </summary>
    public RestRequestAssertion Where(Func<HttpRequestDetails, bool> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Asserts that exactly one matching request was sent.
    /// </summary>
    public CapturedRequest WasSent()
    {
        var matches = GetMatches().ToList();

        if (matches.Count == 0)
        {
            throw new AssertionException($"Expected REST request{_description} was not sent. " +
                $"Total requests: {_transport.CapturedRequests.Count}");
        }

        if (matches.Count > 1)
        {
            throw new AssertionException($"Expected exactly 1 REST request{_description}, but {matches.Count} were sent.");
        }

        return matches[0];
    }

    /// <summary>
    /// Asserts that the request was sent a specific number of times.
    /// </summary>
    public IReadOnlyList<CapturedRequest> WasSent(int expectedCount)
    {
        var matches = GetMatches().ToList();

        if (matches.Count != expectedCount)
        {
            throw new AssertionException($"Expected {expectedCount} REST request(s){_description}, but {matches.Count} were sent.");
        }

        return matches;
    }

    /// <summary>
    /// Asserts that the request was not sent.
    /// </summary>
    public void WasNotSent()
    {
        var matches = GetMatches().ToList();

        if (matches.Count > 0)
        {
            throw new AssertionException($"Expected REST request{_description} not to be sent, but {matches.Count} were sent.");
        }
    }

    /// <summary>
    /// Gets all matching requests without asserting.
    /// </summary>
    public IEnumerable<CapturedRequest> GetMatches()
    {
        return _transport.CapturedRequests.Where(cr =>
            _predicates.All(predicate => predicate(cr.Request)));
    }
}

/// <summary>
/// Fluent assertions for gRPC requests.
/// </summary>
public class GrpcRequestAssertion
{
    private readonly MockGrpcTransport _transport;
    private readonly List<Func<CapturedGrpcRequest, bool>> _predicates = new();
    private string? _description;

    public GrpcRequestAssertion(MockGrpcTransport transport)
    {
        _transport = transport;
    }

    /// <summary>
    /// Filters requests by gRPC method.
    /// </summary>
    public GrpcRequestAssertion WithMethod(string method)
    {
        _predicates.Add(req => req.Method == method);
        _description = $"{_description} with method '{method}'";
        return this;
    }

    /// <summary>
    /// Filters requests by operation name.
    /// </summary>
    public GrpcRequestAssertion ForOperation(string operationName)
    {
        _predicates.Add(req => req.LogicalRequest?.OperationName == operationName);
        _description = $"{_description} for operation '{operationName}'";
        return this;
    }

    /// <summary>
    /// Filters requests by request type.
    /// </summary>
    public GrpcRequestAssertion OfType(RequestType type)
    {
        _predicates.Add(req => req.LogicalRequest?.Type == type);
        _description = $"{_description} of type {type}";
        return this;
    }

    /// <summary>
    /// Filters requests where the request message matches a predicate.
    /// </summary>
    public GrpcRequestAssertion WithRequest<TRequest>(Func<TRequest, bool> predicate) where TRequest : class
    {
        _predicates.Add(req => req.Request is TRequest typedReq && predicate(typedReq));
        return this;
    }

    /// <summary>
    /// Filters requests by a custom predicate.
    /// </summary>
    public GrpcRequestAssertion Where(Func<CapturedGrpcRequest, bool> predicate)
    {
        _predicates.Add(predicate);
        return this;
    }

    /// <summary>
    /// Asserts that exactly one matching request was sent.
    /// </summary>
    public CapturedGrpcRequest WasSent()
    {
        var matches = GetMatches().ToList();

        if (matches.Count == 0)
        {
            throw new AssertionException($"Expected gRPC request{_description} was not sent. " +
                $"Total requests: {_transport.CapturedRequests.Count}");
        }

        if (matches.Count > 1)
        {
            throw new AssertionException($"Expected exactly 1 gRPC request{_description}, but {matches.Count} were sent.");
        }

        return matches[0];
    }

    /// <summary>
    /// Asserts that the request was sent a specific number of times.
    /// </summary>
    public IReadOnlyList<CapturedGrpcRequest> WasSent(int expectedCount)
    {
        var matches = GetMatches().ToList();

        if (matches.Count != expectedCount)
        {
            throw new AssertionException($"Expected {expectedCount} gRPC request(s){_description}, but {matches.Count} were sent.");
        }

        return matches;
    }

    /// <summary>
    /// Asserts that the request was not sent.
    /// </summary>
    public void WasNotSent()
    {
        var matches = GetMatches().ToList();

        if (matches.Count > 0)
        {
            throw new AssertionException($"Expected gRPC request{_description} not to be sent, but {matches.Count} were sent.");
        }
    }

    /// <summary>
    /// Gets all matching requests without asserting.
    /// </summary>
    public IEnumerable<CapturedGrpcRequest> GetMatches()
    {
        return _transport.CapturedRequests.Where(req =>
            _predicates.All(predicate => predicate(req)));
    }

    /// <summary>
    /// Gets the last matching request and casts it to the expected type.
    /// </summary>
    public TRequest? GetLastRequest<TRequest>() where TRequest : class
    {
        var matches = GetMatches().Where(r => r.Request is TRequest).ToList();
        return matches.LastOrDefault()?.Request as TRequest;
    }
}

/// <summary>
/// Exception thrown when an assertion fails.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message)
    {
    }
}

/// <summary>
/// Extension methods for validating request content.
/// </summary>
public static class RequestContentAssertions
{
    /// <summary>
    /// Deserializes the HTTP content as JSON.
    /// </summary>
    public static async System.Threading.Tasks.Task<T?> GetJsonContentAsync<T>(this HttpContent content)
    {
        var json = await content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    /// <summary>
    /// Gets the JSON content as a string.
    /// </summary>
    public static async System.Threading.Tasks.Task<string> GetJsonStringAsync(this HttpContent content)
    {
        return await content.ReadAsStringAsync();
    }
}
