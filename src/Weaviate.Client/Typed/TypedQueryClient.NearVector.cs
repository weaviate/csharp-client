using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedQueryClient<T>
{
    /// <summary>
    /// Performs a near-vector search using vector embeddings.
    /// </summary>
    /// <param name="vectors">The vector to search near.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
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
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearVector(
        VectorSearchInput vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
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
        var result = await _queryClient.NearVector(
            vectors: vectors,
            filters: filters,
            certainty: certainty,
            distance: distance,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
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
    /// Performs a near-vector search with group-by aggregation.
    /// </summary>
    /// <param name="vectors">The vector to search near.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
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
    public async Task<GroupByResult<T>> NearVector(
        VectorSearchInput vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
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
        var result = await _queryClient.NearVector(
            vectors: vectors,
            groupBy: groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
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
    /// Performs a near-vector search using a lambda to build vectors.
    /// </summary>
    /// <param name="vectors">Lambda function to build the vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
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
    public Task<Models.WeaviateResult<WeaviateObject<T>>> NearVector(
        VectorSearchInput.FactoryFn vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors(new VectorSearchInput.Builder()),
            filters,
            certainty,
            distance,
            autoLimit,
            limit,
            offset,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Performs a near-vector search with group-by aggregation using a lambda to build vectors.
    /// </summary>
    /// <param name="vectors">Lambda function to build the vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
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
    public Task<GroupByResult<T>> NearVector(
        VectorSearchInput.FactoryFn vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors(new VectorSearchInput.Builder()),
            groupBy,
            filters,
            distance,
            certainty,
            autoLimit,
            limit,
            offset,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Performs a near-vector search using a NearVectorInput record.
    /// </summary>
    /// <param name="input">Near-vector input containing vector, certainty, and distance.</param>
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
    public Task<Models.WeaviateResult<WeaviateObject<T>>> NearVector(
        NearVectorInput input,
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
    ) =>
        NearVector(
            vectors: input.Vector,
            filters: filters,
            certainty: input.Certainty,
            distance: input.Distance,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a near-vector search with group-by using a NearVectorInput record.
    /// </summary>
    /// <param name="input">Near-vector input containing vector, certainty, and distance.</param>
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
    public Task<GroupByResult<T>> NearVector(
        NearVectorInput input,
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
    ) =>
        NearVector(
            vectors: input.Vector,
            groupBy: groupBy,
            filters: filters,
            certainty: input.Certainty,
            distance: input.Distance,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a near-vector search using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearVectorInput with target vectors.</param>
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
    public Task<Models.WeaviateResult<WeaviateObject<T>>> NearVector(
        NearVectorInput.FactoryFn vectors,
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
    ) =>
        NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            filters,
            autoLimit,
            limit,
            offset,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Performs a near-vector search with group-by using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearVectorInput with target vectors.</param>
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
    public Task<GroupByResult<T>> NearVector(
        NearVectorInput.FactoryFn vectors,
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
    ) =>
        NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            groupBy,
            filters,
            autoLimit,
            limit,
            offset,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );
}
