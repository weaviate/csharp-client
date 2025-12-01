using System.Collections.Generic;

namespace Weaviate.Client.Request.Models;

/// <summary>
/// Base class for search/query requests.
/// </summary>
public abstract record SearchRequestBase : IWeaviateRequest
{
    public abstract string OperationName { get; }
    public RequestType Type => RequestType.Query;
    public TransportProtocol PreferredProtocol => TransportProtocol.Grpc;

    /// <summary>
    /// The collection to search in.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// Filter to apply to the search.
    /// </summary>
    public object? Filter { get; init; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int? Limit { get; init; }

    /// <summary>
    /// Number of results to skip (for pagination).
    /// </summary>
    public int? Offset { get; init; }

    /// <summary>
    /// Minimum certainty/distance threshold.
    /// </summary>
    public float? Certainty { get; init; }

    /// <summary>
    /// Maximum distance threshold.
    /// </summary>
    public float? Distance { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }

    /// <summary>
    /// Consistency level for the query.
    /// </summary>
    public string? ConsistencyLevel { get; init; }

    /// <summary>
    /// Fields to include in the result.
    /// </summary>
    public List<string>? ReturnProperties { get; init; }

    /// <summary>
    /// Whether to include vector in the result.
    /// </summary>
    public bool IncludeVector { get; init; }

    /// <summary>
    /// Grouping configuration.
    /// </summary>
    public object? GroupBy { get; init; }
}

/// <summary>
/// Request for near-text semantic search.
/// </summary>
public record NearTextSearchRequest : SearchRequestBase
{
    public override string OperationName => "SearchNearText";

    /// <summary>
    /// The text to search for.
    /// </summary>
    public List<string> Query { get; init; } = new();

    /// <summary>
    /// Optional target vectors (for multi-vector collections).
    /// </summary>
    public string? TargetVector { get; init; }

    /// <summary>
    /// Move towards concepts.
    /// </summary>
    public object? MoveTo { get; init; }

    /// <summary>
    /// Move away from concepts.
    /// </summary>
    public object? MoveAway { get; init; }
}

/// <summary>
/// Request for near-vector search.
/// </summary>
public record NearVectorSearchRequest : SearchRequestBase
{
    public override string OperationName => "SearchNearVector";

    /// <summary>
    /// The vector to search for.
    /// </summary>
    public float[] Vector { get; init; } = System.Array.Empty<float>();

    /// <summary>
    /// Optional target vectors (for multi-vector collections).
    /// </summary>
    public string? TargetVector { get; init; }
}

/// <summary>
/// Request for BM25 keyword search.
/// </summary>
public record BM25SearchRequest : SearchRequestBase
{
    public override string OperationName => "SearchBM25";

    /// <summary>
    /// The query text for BM25 search.
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Properties to search in.
    /// </summary>
    public List<string>? Properties { get; init; }
}

/// <summary>
/// Request for hybrid search (combining vector and keyword).
/// </summary>
public record HybridSearchRequest : SearchRequestBase
{
    public override string OperationName => "SearchHybrid";

    /// <summary>
    /// The query text.
    /// </summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>
    /// Optional vector to combine with keyword search.
    /// </summary>
    public float[]? Vector { get; init; }

    /// <summary>
    /// Alpha value for balancing vector vs keyword (0=pure BM25, 1=pure vector).
    /// </summary>
    public float? Alpha { get; init; }

    /// <summary>
    /// Properties to search in.
    /// </summary>
    public List<string>? Properties { get; init; }

    /// <summary>
    /// Fusion type for combining results.
    /// </summary>
    public string? FusionType { get; init; }

    /// <summary>
    /// Optional target vectors (for multi-vector collections).
    /// </summary>
    public string? TargetVector { get; init; }
}

/// <summary>
/// Request for fetching objects by IDs.
/// </summary>
public record FetchObjectsRequest : SearchRequestBase
{
    public override string OperationName => "FetchObjects";

    /// <summary>
    /// Maximum number of objects to fetch.
    /// </summary>
    public int Limit { get; init; } = 10;

    /// <summary>
    /// After ID for cursor-based pagination.
    /// </summary>
    public string? After { get; init; }

    /// <summary>
    /// Sort configuration.
    /// </summary>
    public object? Sort { get; init; }
}

/// <summary>
/// Request for aggregation queries.
/// </summary>
public record AggregateRequest : IWeaviateRequest
{
    public string OperationName => "Aggregate";
    public RequestType Type => RequestType.Query;
    public TransportProtocol PreferredProtocol => TransportProtocol.Grpc;

    /// <summary>
    /// The collection to aggregate.
    /// </summary>
    public string Collection { get; init; } = string.Empty;

    /// <summary>
    /// Filter to apply before aggregation.
    /// </summary>
    public object? Filter { get; init; }

    /// <summary>
    /// Fields to aggregate.
    /// </summary>
    public List<string>? Fields { get; init; }

    /// <summary>
    /// Group by configuration.
    /// </summary>
    public object? GroupBy { get; init; }

    /// <summary>
    /// Tenant name (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; init; }

    /// <summary>
    /// Consistency level for the query.
    /// </summary>
    public string? ConsistencyLevel { get; init; }
}
