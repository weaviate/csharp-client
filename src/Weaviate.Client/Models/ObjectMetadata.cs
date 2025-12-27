namespace Weaviate.Client.Models;

/// <summary>
/// Contains metadata about a Weaviate object, including timestamps, relevance scores, and consistency information.
/// </summary>
/// <remarks>
/// Metadata is returned with query results when requested via <see cref="MetadataQuery"/>.
/// Different metadata fields are populated depending on the type of query and what was requested.
/// </remarks>
public record Metadata
{
    /// <summary>
    /// Gets or sets the timestamp when the object was created.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="MetadataOptions.CreationTime"/> is requested in the query.
    /// </remarks>
    public DateTime? CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the object was last updated.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="MetadataOptions.LastUpdateTime"/> is requested in the query.
    /// </remarks>
    public DateTime? LastUpdateTime { get; set; }

    /// <summary>
    /// Gets the distance between the query vector and this object's vector.
    /// </summary>
    /// <remarks>
    /// Only populated for vector searches when <see cref="MetadataOptions.Distance"/> is requested.
    /// Lower values indicate closer similarity. The metric used depends on the collection's distance configuration (cosine, dot, euclidean, etc.).
    /// </remarks>
    public double? Distance { get; init; }

    /// <summary>
    /// Gets the certainty score (normalized distance between 0.0 and 1.0).
    /// </summary>
    /// <remarks>
    /// Deprecated in favor of <see cref="Distance"/>.
    /// Only populated when <see cref="MetadataOptions.Certainty"/> is requested.
    /// Higher values indicate higher similarity.
    /// </remarks>
    public double? Certainty { get; init; }

    /// <summary>
    /// Gets the relevance score for this result.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="MetadataOptions.Score"/> is requested.
    /// The scoring mechanism depends on the query type (vector, hybrid, BM25, etc.).
    /// </remarks>
    public double? Score { get; init; }

    /// <summary>
    /// Gets a detailed explanation of how the score was calculated.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="MetadataOptions.ExplainScore"/> is requested.
    /// Provides insights into the scoring algorithm's decision-making process.
    /// </remarks>
    public string? ExplainScore { get; init; }

    /// <summary>
    /// Gets a value indicating whether this object is consistent across all replicas.
    /// </summary>
    /// <remarks>
    /// Only populated when <see cref="MetadataOptions.IsConsistent"/> is requested.
    /// Relevant for eventual consistency scenarios in distributed deployments.
    /// </remarks>
    public bool? IsConsistent { get; init; }

    /// <summary>
    /// Gets the reranking score applied after initial retrieval.
    /// </summary>
    /// <remarks>
    /// Populated when a reranker module is used to refine search results.
    /// Represents the relevance score assigned by the reranking model.
    /// </remarks>
    public double? RerankScore { get; init; }
}
