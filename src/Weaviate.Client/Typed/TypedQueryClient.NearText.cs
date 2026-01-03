using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedQueryClient<T>
{
    /// <summary>
    /// Performs a near-text search using text embeddings.
    /// </summary>
    /// <param name="text">The text to search for (single or multiple strings).</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="moveTo">Move the query vector towards these concepts.</param>
    /// <param name="moveAway">Move the query vector away from these concepts.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targets">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearText(
        AutoArray<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearText(
            text: text,
            certainty: certainty,
            distance: distance,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
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
    /// Performs a near-text search with group-by aggregation.
    /// </summary>
    /// <param name="text">The text to search for (single or multiple strings).</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="moveTo">Move the query vector towards these concepts.</param>
    /// <param name="moveAway">Move the query vector away from these concepts.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targets">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearText(
        AutoArray<string> text,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearText(
            text: text,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
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
    /// Performs a near-text search using a NearTextInput record.
    /// </summary>
    /// <param name="input">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearText(
        NearTextInput input,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearText(
            _ => input,
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
    /// Performs a near-text search with group-by using a NearTextInput record.
    /// </summary>
    /// <param name="input">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearText(
        NearTextInput input,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearText(
            _ => input,
            groupBy,
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
    /// Performs a near-text search using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public Task<Models.WeaviateResult<WeaviateObject<T>>> NearText(
        NearTextInput.FactoryFn query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearText(
            query(VectorInputBuilderFactories.CreateNearTextBuilder()),
            filters,
            limit,
            offset,
            autoLimit,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Performs a near-text search with group-by using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public Task<GroupByResult<T>> NearText(
        NearTextInput.FactoryFn query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearText(
            query(VectorInputBuilderFactories.CreateNearTextBuilder()),
            groupBy,
            filters,
            limit,
            offset,
            autoLimit,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );
}
