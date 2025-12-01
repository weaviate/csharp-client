using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace Weaviate.Client.Request.Transport;

/// <summary>
/// Represents a gRPC request that will be sent.
/// This captures the request before it goes over the wire, allowing inspection and testing.
/// </summary>
/// <typeparam name="TRequest">The gRPC request type</typeparam>
public class GrpcRequestDetails<TRequest>
{
    /// <summary>
    /// The gRPC method being called (e.g., "/weaviate.v1.Weaviate/Search").
    /// </summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// The gRPC request message.
    /// </summary>
    public TRequest? Request { get; init; }

    /// <summary>
    /// The call options (headers, deadline, etc.).
    /// </summary>
    public CallOptions Options { get; init; }

    /// <summary>
    /// The logical request that generated this gRPC request.
    /// </summary>
    public IWeaviateRequest? LogicalRequest { get; init; }
}

/// <summary>
/// Interface for gRPC transport layer.
/// Implementations can be real (gRPC channel) or mock (for testing).
/// </summary>
public interface IGrpcTransport
{
    /// <summary>
    /// Sends a unary gRPC call and returns the response.
    /// </summary>
    /// <typeparam name="TRequest">The gRPC request type</typeparam>
    /// <typeparam name="TResponse">The gRPC response type</typeparam>
    /// <param name="request">The gRPC request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The gRPC response</returns>
    Task<TResponse> UnaryCallAsync<TRequest, TResponse>(
        GrpcRequestDetails<TRequest> request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if the transport is connected and healthy.
    /// </summary>
    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
