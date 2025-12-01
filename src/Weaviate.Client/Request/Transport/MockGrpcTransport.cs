using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Weaviate.Client.Request.Transport;

/// <summary>
/// Mock gRPC transport for testing.
/// Captures requests and allows returning pre-configured responses.
/// </summary>
public class MockGrpcTransport : IGrpcTransport
{
    private readonly List<CapturedGrpcRequest> _capturedRequests = new();
    private readonly List<GrpcResponseRule> _responseRules = new();
    private Func<object, object>? _defaultResponseFactory;
    private bool _isHealthy = true;

    /// <summary>
    /// All requests that have been captured.
    /// </summary>
    public IReadOnlyList<CapturedGrpcRequest> CapturedRequests => _capturedRequests.AsReadOnly();

    /// <summary>
    /// Controls whether health checks return success.
    /// </summary>
    public bool IsHealthy
    {
        get => _isHealthy;
        set => _isHealthy = value;
    }

    /// <summary>
    /// Clears all captured requests.
    /// </summary>
    public void ClearCapturedRequests()
    {
        _capturedRequests.Clear();
    }

    /// <summary>
    /// Sets a default response factory for requests that don't match any rules.
    /// </summary>
    public void SetDefaultResponse<TRequest, TResponse>(Func<TRequest, TResponse> factory)
    {
        _defaultResponseFactory = req => factory((TRequest)req)!;
    }

    /// <summary>
    /// Adds a response rule that matches requests and returns a configured response.
    /// </summary>
    public void AddResponseRule<TRequest, TResponse>(
        Func<GrpcRequestDetails<TRequest>, bool> matcher,
        Func<GrpcRequestDetails<TRequest>, TResponse> responseFactory)
    {
        _responseRules.Add(new GrpcResponseRule
        {
            Matcher = req =>
            {
                if (req is GrpcRequestDetails<TRequest> typedReq)
                    return matcher(typedReq);
                return false;
            },
            ResponseFactory = req =>
            {
                if (req is GrpcRequestDetails<TRequest> typedReq)
                    return responseFactory(typedReq)!;
                throw new InvalidOperationException("Request type mismatch");
            }
        });
    }

    /// <summary>
    /// Adds a response rule for a specific gRPC method.
    /// </summary>
    public void AddResponseRuleForMethod<TRequest, TResponse>(
        string method,
        TResponse response)
    {
        AddResponseRule<TRequest, TResponse>(
            req => req.Method == method,
            _ => response
        );
    }

    /// <summary>
    /// Adds a response rule for a specific operation name.
    /// </summary>
    public void AddResponseRuleForOperation<TRequest, TResponse>(
        string operationName,
        TResponse response)
    {
        AddResponseRule<TRequest, TResponse>(
            req => req.LogicalRequest?.OperationName == operationName,
            _ => response
        );
    }

    public Task<TResponse> UnaryCallAsync<TRequest, TResponse>(
        GrpcRequestDetails<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        // Capture the request
        _capturedRequests.Add(new CapturedGrpcRequest
        {
            Method = request.Method,
            Request = request.Request,
            LogicalRequest = request.LogicalRequest,
            Timestamp = DateTime.UtcNow
        });

        // Find matching response rule
        var rule = _responseRules.FirstOrDefault(r => r.Matcher(request));
        if (rule != null)
        {
            var response = rule.ResponseFactory(request);
            return Task.FromResult((TResponse)response);
        }

        // Use default response factory if set
        if (_defaultResponseFactory != null && request.Request != null)
        {
            var response = _defaultResponseFactory(request.Request);
            return Task.FromResult((TResponse)response);
        }

        // No response configured
        throw new RpcException(new Status(StatusCode.Unimplemented,
            $"No mock response configured for method {request.Method}"));
    }

    public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_isHealthy);
    }

    /// <summary>
    /// Gets requests matching a predicate.
    /// </summary>
    public IEnumerable<CapturedGrpcRequest> GetRequests(Func<CapturedGrpcRequest, bool> predicate)
    {
        return _capturedRequests.Where(predicate);
    }

    /// <summary>
    /// Gets requests for a specific gRPC method.
    /// </summary>
    public IEnumerable<CapturedGrpcRequest> GetRequestsForMethod(string method)
    {
        return GetRequests(req => req.Method == method);
    }

    /// <summary>
    /// Gets requests for a specific operation.
    /// </summary>
    public IEnumerable<CapturedGrpcRequest> GetRequestsForOperation(string operationName)
    {
        return GetRequests(req => req.LogicalRequest?.OperationName == operationName);
    }

    /// <summary>
    /// Gets the last captured request of a specific type.
    /// </summary>
    public TRequest? GetLastRequest<TRequest>() where TRequest : class
    {
        return _capturedRequests
            .Where(r => r.Request is TRequest)
            .Select(r => r.Request as TRequest)
            .LastOrDefault();
    }

    private class GrpcResponseRule
    {
        public Func<object, bool> Matcher { get; init; } = null!;
        public Func<object, object> ResponseFactory { get; init; } = null!;
    }
}

/// <summary>
/// Represents a captured gRPC request.
/// </summary>
public class CapturedGrpcRequest
{
    public string Method { get; init; } = string.Empty;
    public object? Request { get; init; }
    public IWeaviateRequest? LogicalRequest { get; init; }
    public DateTime Timestamp { get; init; }
}
