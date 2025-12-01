using System;

namespace Weaviate.Client.Request;

/// <summary>
/// Base interface for all Weaviate request models.
/// Represents a logical request before it is converted to HTTP or gRPC.
/// </summary>
public interface IWeaviateRequest
{
    /// <summary>
    /// The name of the operation being performed (e.g., "ObjectInsert", "SearchNearText").
    /// </summary>
    string OperationName { get; }

    /// <summary>
    /// The type of request (Query, Data, Schema, etc.).
    /// </summary>
    RequestType Type { get; }

    /// <summary>
    /// The preferred transport protocol for this request.
    /// </summary>
    TransportProtocol PreferredProtocol { get; }
}

/// <summary>
/// Categorizes request types.
/// </summary>
public enum RequestType
{
    /// <summary>
    /// Query operations (search, aggregate, etc.)
    /// </summary>
    Query,

    /// <summary>
    /// Data manipulation operations (insert, update, delete)
    /// </summary>
    Data,

    /// <summary>
    /// Schema operations (collection create, update, delete)
    /// </summary>
    Schema,

    /// <summary>
    /// Cluster operations (status, nodes, etc.)
    /// </summary>
    Cluster,

    /// <summary>
    /// Backup operations
    /// </summary>
    Backup,

    /// <summary>
    /// Authorization operations (users, roles, groups)
    /// </summary>
    Authorization,

    /// <summary>
    /// Meta/health operations
    /// </summary>
    Meta,

    /// <summary>
    /// Batch operations
    /// </summary>
    Batch
}

/// <summary>
/// Specifies the preferred transport protocol.
/// </summary>
public enum TransportProtocol
{
    /// <summary>
    /// REST/HTTP protocol
    /// </summary>
    Rest,

    /// <summary>
    /// gRPC protocol
    /// </summary>
    Grpc,

    /// <summary>
    /// Either protocol can be used (client will choose)
    /// </summary>
    Either
}
