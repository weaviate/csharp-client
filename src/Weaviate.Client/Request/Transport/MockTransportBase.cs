using System;
using System.Collections.Generic;
using System.Linq;

namespace Weaviate.Client.Request.Transport;

/// <summary>
/// Base class for mock transport implementations.
/// Provides common functionality for capturing requests and managing response rules.
/// </summary>
/// <typeparam name="TRequest">The request details type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
/// <typeparam name="TCaptured">The captured request type</typeparam>
public abstract class MockTransportBase<TRequest, TResponse, TCaptured>
    where TCaptured : CapturedRequestBase
{
    private readonly List<TCaptured> _capturedRequests = new();
    private readonly List<ResponseRuleBase<TRequest, TResponse>> _responseRules = new();

    /// <summary>
    /// All requests that have been captured.
    /// </summary>
    public IReadOnlyList<TCaptured> CapturedRequests => _capturedRequests.AsReadOnly();

    /// <summary>
    /// Clears all captured requests.
    /// </summary>
    public void ClearCapturedRequests()
    {
        _capturedRequests.Clear();
    }

    /// <summary>
    /// Captures a request for later inspection.
    /// </summary>
    protected void CaptureRequest(TCaptured capturedRequest)
    {
        if (capturedRequest == null)
            throw new ArgumentNullException(nameof(capturedRequest));

        _capturedRequests.Add(capturedRequest);
    }

    /// <summary>
    /// Adds a response rule that matches requests and returns a configured response.
    /// </summary>
    protected void AddRule(ResponseRuleBase<TRequest, TResponse> rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));

        _responseRules.Add(rule);
    }

    /// <summary>
    /// Finds the first response rule matching the given request.
    /// </summary>
    protected TResponse? FindMatchingResponse(TRequest request)
    {
        if (request == null)
            return default;

        var rule = _responseRules.FirstOrDefault(r => r.Matcher(request));
        return rule != null ? rule.ResponseFactory(request) : default;
    }

    /// <summary>
    /// Checks if any response rule matches the given request.
    /// </summary>
    protected bool HasMatchingRule(TRequest request)
    {
        return _responseRules.Any(r => r.Matcher(request));
    }

    /// <summary>
    /// Gets requests matching a predicate.
    /// </summary>
    protected IEnumerable<TCaptured> GetCapturedRequests(Func<TCaptured, bool> predicate)
    {
        return _capturedRequests.Where(predicate);
    }
}

/// <summary>
/// Base class for captured request information.
/// </summary>
public abstract class CapturedRequestBase
{
    /// <summary>
    /// The timestamp when the request was captured.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// The logical request associated with this captured request, if any.
    /// </summary>
    public IWeaviateRequest? LogicalRequest { get; init; }
}

/// <summary>
/// Base class for response rules.
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class ResponseRuleBase<TRequest, TResponse>
{
    /// <summary>
    /// Predicate that determines if this rule matches a request.
    /// </summary>
    public required Func<TRequest, bool> Matcher { get; init; }

    /// <summary>
    /// Factory function that creates a response for a matched request.
    /// </summary>
    public required Func<TRequest, TResponse> ResponseFactory { get; init; }
}
