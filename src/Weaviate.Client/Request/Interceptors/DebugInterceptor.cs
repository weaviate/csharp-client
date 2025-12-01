using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Weaviate.Client.Request.Interceptors;

/// <summary>
/// Interceptor that captures detailed debug information about requests.
/// Useful for development and troubleshooting.
/// </summary>
public class DebugInterceptor : RequestInterceptorBase
{
    /// <summary>
    /// Maximum depth for JSON serialization when capturing debug information.
    /// </summary>
    private const int MaxSerializationDepth = 5;

    private readonly Dictionary<IWeaviateRequest, RequestDebugInfo> _debugInfo = new();
    private readonly Action<RequestDebugInfo>? _onRequestComplete;

    /// <summary>
    /// Creates a debug interceptor.
    /// </summary>
    /// <param name="onRequestComplete">Optional callback invoked when a request completes</param>
    public DebugInterceptor(Action<RequestDebugInfo>? onRequestComplete = null)
    {
        _onRequestComplete = onRequestComplete;
    }

    /// <summary>
    /// Gets all captured debug information.
    /// </summary>
    public IReadOnlyDictionary<IWeaviateRequest, RequestDebugInfo> CapturedRequests => _debugInfo;

    /// <summary>
    /// Clears all captured debug information.
    /// </summary>
    public void Clear()
    {
        _debugInfo.Clear();
    }

    public override Task<RequestContext> OnBeforeSendAsync(RequestContext context)
    {
        var info = new RequestDebugInfo
        {
            Request = context.Request,
            OperationName = context.Request.OperationName,
            RequestType = context.Request.Type,
            Protocol = context.Request.PreferredProtocol,
            Collection = context.Collection,
            Tenant = context.Tenant,
            Timeout = context.Timeout,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>(context.Metadata)
        };

        // Capture request details
        try
        {
            info.RequestDetails = JsonSerializer.Serialize(context.Request, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception ex)
        {
            info.RequestDetails = $"(Could not serialize: {ex.Message})";
        }

        info.Stopwatch.Start();
        _debugInfo[context.Request] = info;

        return base.OnBeforeSendAsync(context);
    }

    public override Task<TResponse> OnAfterReceiveAsync<TResponse>(RequestContext context, TResponse response)
    {
        if (_debugInfo.TryGetValue(context.Request, out var info))
        {
            info.Stopwatch.Stop();
            info.Duration = info.Stopwatch.Elapsed;
            info.Success = true;

            // Capture response details
            try
            {
                info.ResponseDetails = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    MaxDepth = MaxSerializationDepth
                });
            }
            catch (Exception ex)
            {
                info.ResponseDetails = $"(Could not serialize: {ex.Message})";
            }

            _onRequestComplete?.Invoke(info);
        }

        return base.OnAfterReceiveAsync(context, response);
    }

    public override Task OnErrorAsync(RequestContext context, Exception exception)
    {
        if (_debugInfo.TryGetValue(context.Request, out var info))
        {
            info.Stopwatch.Stop();
            info.Duration = info.Stopwatch.Elapsed;
            info.Success = false;
            info.Error = exception;
            info.ErrorMessage = exception.ToString();

            _onRequestComplete?.Invoke(info);
        }

        return base.OnErrorAsync(context, exception);
    }
}

/// <summary>
/// Debug information captured for a request.
/// </summary>
public class RequestDebugInfo
{
    public IWeaviateRequest? Request { get; init; }
    public string OperationName { get; init; } = string.Empty;
    public RequestType RequestType { get; init; }
    public TransportProtocol Protocol { get; init; }
    public string? Collection { get; init; }
    public string? Tenant { get; init; }
    public TimeSpan? Timeout { get; init; }
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();

    public string RequestDetails { get; set; } = string.Empty;
    public string? ResponseDetails { get; set; }

    public Stopwatch Stopwatch { get; } = new();
    public TimeSpan Duration { get; set; }
    public bool Success { get; set; }
    public Exception? Error { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString()
    {
        var status = Success ? "Success" : "Failed";
        var result = $"[{status}] {OperationName} ({Protocol}) - {Duration.TotalMilliseconds:F2}ms";
        if (Collection != null)
            result += $" | Collection: {Collection}";
        if (!Success && ErrorMessage != null)
            result += $"\n  Error: {ErrorMessage}";
        return result;
    }
}
