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
        if (factory == null) throw new ArgumentNullException(nameof(factory));
        _defaultResponseFactory = req => factory((TRequest)req)!;
    }

    /// <summary>
    /// Adds a response rule that matches requests and returns a configured response.
    /// </summary>
    public void AddResponseRule<TRequest, TResponse>(
        Func<GrpcRequestDetails<TRequest>, bool> matcher,
        Func<GrpcRequestDetails<TRequest>, TResponse> responseFactory)
    {
        if (matcher == null) throw new ArgumentNullException(nameof(matcher));
        if (responseFactory == null) throw new ArgumentNullException(nameof(responseFactory));

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
                throw new InvalidOperationException($"Request type mismatch. Expected {typeof(TRequest).Name} but got {req?.GetType().Name ?? "null"}");
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
        if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("Method name cannot be null or whitespace.", nameof(method));

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
        if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentException("Operation name cannot be null or whitespace.", nameof(operationName));

        AddResponseRule<TRequest, TResponse>(
            req => req.LogicalRequest?.OperationName == operationName,
            _ => response
        );
    }

    public Task<TResponse> UnaryCallAsync<TRequest, TResponse>(
        GrpcRequestDetails<TRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

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

        // No response configured - provide helpful error message
        throw new RpcException(new Status(StatusCode.Unimplemented,
            $"No mock response configured for method {request.Method}. " +
            $"Use AddResponseRule, AddResponseRuleForMethod, or SetDefaultResponse to configure a response."));
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
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        return _capturedRequests.Where(predicate);
    }

    /// <summary>
    /// Gets requests for a specific gRPC method.
    /// </summary>
    public IEnumerable<CapturedGrpcRequest> GetRequestsForMethod(string method)
    {
        if (string.IsNullOrWhiteSpace(method)) throw new ArgumentException("Method name cannot be null or whitespace.", nameof(method));
        return GetRequests(req => req.Method == method);
    }

    /// <summary>
    /// Gets requests for a specific operation.
    /// </summary>
    public IEnumerable<CapturedGrpcRequest> GetRequestsForOperation(string operationName)
    {
        if (string.IsNullOrWhiteSpace(operationName)) throw new ArgumentException("Operation name cannot be null or whitespace.", nameof(operationName));
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
public class CapturedGrpcRequest : CapturedRequestBase
{
    /// <summary>
    /// The gRPC method name.
    /// </summary>
    public required string Method { get; init; }

    /// <summary>
    /// The captured request object.
    /// </summary>
    public object? Request { get; init; }
}
