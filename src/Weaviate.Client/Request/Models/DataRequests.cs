using System;
using System.Collections.Generic;

namespace Weaviate.Client.Request.Models;

/// <summary>
/// Request to insert an object into a collection.
/// </summary>
public record ObjectInsertRequest : IWeaviateRequest
{
    public string OperationName => "ObjectInsert";
    public RequestType Type => RequestType.Data;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The collection to insert into.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// The object data to insert.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Optional object ID. If not specified, one will be generated.
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// Optional vector to use for this object.
    /// </summary>
    public float[]? Vector { get; init; }

    /// <summary>
    /// Optional named vectors (for multi-vector collections).
    /// </summary>
    public Dictionary<string, float[]>? Vectors { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }
}

/// <summary>
/// Request to update/replace an object.
/// </summary>
public record ObjectReplaceRequest : IWeaviateRequest
{
    public string OperationName => "ObjectReplace";
    public RequestType Type => RequestType.Data;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The collection containing the object.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the object to replace.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The new object data.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Optional vector to use for this object.
    /// </summary>
    public float[]? Vector { get; init; }

    /// <summary>
    /// Optional named vectors (for multi-vector collections).
    /// </summary>
    public Dictionary<string, float[]>? Vectors { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }
}

/// <summary>
/// Request to delete an object.
/// </summary>
public record ObjectDeleteRequest : IWeaviateRequest
{
    public string OperationName => "ObjectDelete";
    public RequestType Type => RequestType.Data;
    public TransportProtocol PreferredProtocol => TransportProtocol.Rest;

    /// <summary>
    /// The collection containing the object.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the object to delete.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }
}

/// <summary>
/// Request to fetch an object by ID.
/// </summary>
public record ObjectGetRequest : IWeaviateRequest
{
    public string OperationName => "ObjectGet";
    public RequestType Type => RequestType.Data;
    public TransportProtocol PreferredProtocol => TransportProtocol.Grpc;

    /// <summary>
    /// The collection containing the object.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// The ID of the object to fetch.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }

    /// <summary>
    /// Consistency level for the read.
    /// </summary>
    public string? ConsistencyLevel { get; init; }
}

/// <summary>
/// Request to insert multiple objects in a batch.
/// </summary>
public record BatchInsertRequest : IWeaviateRequest
{
    public string OperationName => "BatchInsert";
    public RequestType Type => RequestType.Batch;
    public TransportProtocol PreferredProtocol => TransportProtocol.Grpc;

    /// <summary>
    /// The collection to insert into.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// The objects to insert.
    /// </summary>
    public List<object> Objects { get; init; } = new();

    /// <summary>
    /// Optional object IDs.
    /// </summary>
    public List<Guid>? Ids { get; init; }

    /// <summary>
    /// Optional vectors for the objects.
    /// </summary>
    public List<float[]>? Vectors { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }
}
