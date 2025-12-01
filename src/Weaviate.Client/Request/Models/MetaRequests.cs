namespace Weaviate.Client.Request.Models;

/// <summary>
/// Request to get server metadata.
/// </summary>
public record GetMetaRequest : IWeaviateRequest
{
    public string OperationName => "GetMeta";
    public RequestType Type => RequestType.Meta;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;
}

/// <summary>
/// Request to check if server is live.
/// </summary>
public record LivenessRequest : IWeaviateRequest
{
    public string OperationName => "Liveness";
    public RequestType Type => RequestType.Meta;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;
}

/// <summary>
/// Request to check if server is ready.
/// </summary>
public record ReadinessRequest : IWeaviateRequest
{
    public string OperationName => "Readiness";
    public RequestType Type => RequestType.Meta;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;
}
