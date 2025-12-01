namespace Weaviate.Client.Request.Models;

/// <summary>
/// Request to create a collection.
/// </summary>
public record CollectionCreateRequest : IWeaviateRequest
{
    public string OperationName => "CollectionCreate";
    public RequestType Type => RequestType.Schema;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The collection configuration.
    /// </summary>
    public object? CollectionConfig { get; init; }
}

/// <summary>
/// Request to get collection details.
/// </summary>
public record CollectionGetRequest : IWeaviateRequest
{
    public string OperationName => "CollectionGet";
    public RequestType Type => RequestType.Schema;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The name of the collection to get.
    /// </summary>
    public string Collection { get; init; } = string.Empty;
}

/// <summary>
/// Request to list all collections.
/// </summary>
public record CollectionListRequest : IWeaviateRequest
{
    public string OperationName => "CollectionList";
    public RequestType Type => RequestType.Schema;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;
}

/// <summary>
/// Request to update a collection.
/// </summary>
public record CollectionUpdateRequest : IWeaviateRequest
{
    public string OperationName => "CollectionUpdate";
    public RequestType Type => RequestType.Schema;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The name of the collection to update.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// The collection configuration updates.
    /// </summary>
    public object? CollectionConfig { get; init; }
}

/// <summary>
/// Request to delete a collection.
/// </summary>
public record CollectionDeleteRequest : IWeaviateRequest
{
    public string OperationName => "CollectionDelete";
    public RequestType Type => RequestType.Schema;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The name of the collection to delete.
    /// </summary>
    public string Collection { get; init; } = string.Empty;
}
