using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

/// <summary>
/// The typed query client class
/// </summary>
public partial class TypedQueryClient<T>
{
    /// <summary>
    /// Performs a near-media search using media embeddings.
    /// </summary>
    /// <example>
    /// // Simple image search
    /// await typedQuery.NearMedia(m => m.Image(imageBytes));
    ///
    /// // With target vectors
    /// await typedQuery.NearMedia(m => m.Video(videoBytes, certainty: 0.8f).Sum("v1", "v2"));
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearMedia(
        NearMediaInput.FactoryFn media,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearMedia(
            query: media,
            filters: filters,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-media search with group-by aggregation.
    /// </summary>
    /// <example>
    /// // Image search with grouping
    /// await typedQuery.NearMedia(
    ///     m => m.Image(imageBytes).Sum("visual", "semantic"),
    ///     groupBy: new GroupByRequest("category", objectsPerGroup: 5)
    /// );
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearMedia(
        NearMediaInput.FactoryFn media,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearMedia(
            query: media,
            groupBy: groupBy,
            filters: filters,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }
}
