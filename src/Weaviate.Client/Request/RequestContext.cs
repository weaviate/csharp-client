using System;
using System.Collections.Generic;
using System.Threading;

namespace Weaviate.Client.Request;

/// <summary>
/// Contains a request and its execution context.
/// This is passed through the request pipeline and can be inspected/modified by interceptors.
/// </summary>
public class RequestContext
{
    /// <summary>
    /// The logical request being executed.
    /// </summary>
    public IWeaviateRequest Request { get; init; }

    /// <summary>
    /// Cancellation token for the request.
    /// </summary>
    public CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// Timeout for the request.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Additional metadata that can be attached to the request.
    /// Interceptors can read/write this data.
    /// </summary>
    public Dictionary<string, object> Metadata { get; init; }

    /// <summary>
    /// Collection name (if applicable).
    /// </summary>
    public string? Collection { get; init; }

    /// <summary>
    /// Tenant name (if applicable).
    /// </summary>
    public string? Tenant { get; init; }

    /// <summary>
    /// Consistency level (if applicable).
    /// </summary>
    public string? ConsistencyLevel { get; init; }

    public RequestContext(
        IWeaviateRequest request,
        CancellationToken cancellationToken = default,
        TimeSpan? timeout = null,
        Dictionary<string, object>? metadata = null,
        string? collection = null,
        string? tenant = null,
        string? consistencyLevel = null)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        CancellationToken = cancellationToken;
        Timeout = timeout;
        Metadata = metadata ?? new Dictionary<string, object>();
        Collection = collection;
        Tenant = tenant;
        ConsistencyLevel = consistencyLevel;
    }

    /// <summary>
    /// Creates a new context with modified properties.
    /// </summary>
    public RequestContext With(
        IWeaviateRequest? request = null,
        CancellationToken? cancellationToken = null,
        TimeSpan? timeout = null,
        Dictionary<string, object>? metadata = null,
        string? collection = null,
        string? tenant = null,
        string? consistencyLevel = null)
    {
        return new RequestContext(
            request ?? Request,
            cancellationToken ?? CancellationToken,
            timeout ?? Timeout,
            metadata ?? Metadata,
            collection ?? Collection,
            tenant ?? Tenant,
            consistencyLevel ?? ConsistencyLevel
        );
    }
}
