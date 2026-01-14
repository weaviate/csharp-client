using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    // Simple overload accepting VectorSearchInput (with implicit conversions support)
    /// <summary>
    /// Nears the vector using the specified vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <param name="filters">The filters</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the weaviate result</returns>
    public async Task<WeaviateResult> NearVector(
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
    ) =>
        await _grpc.SearchNearVector(
            _collectionClient.Name,
            vectors,
            distance: distance,
            certainty: certainty,
            offset: offset,
            autoLimit: autoLimit,
            limit: limit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    // Simple overload with GroupBy
    /// <summary>
    /// Nears the vector using the specified vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="filters">The filters</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the group by result</returns>
    public async Task<GroupByResult> NearVector(
        VectorSearchInput vectors,
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
        await _grpc.SearchNearVector(
            _collectionClient.Name,
            vectors,
            groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            offset: offset,
            autoLimit: autoLimit,
            limit: limit,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    // Lambda syntax overload
    /// <summary>
    /// Nears the vector using the specified vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <param name="filters">The filters</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the weaviate result</returns>
    public async Task<WeaviateResult> NearVector(
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
        await NearVector(
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

    // Lambda syntax overload with GroupBy
    /// <summary>
    /// Nears the vector using the specified vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="filters">The filters</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the group by result</returns>
    public async Task<GroupByResult> NearVector(
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
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            groupBy,
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

    // NearVectorInput overload
    /// <summary>
    /// Performs a near-vector search using a NearVectorInput record.
    /// </summary>
    /// <param name="query">Near-vector input containing vector, certainty, and distance.</param>
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
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearVector(
        NearVectorInput query,
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
        await NearVector(
            vectors: query.Vector,
            filters: filters,
            certainty: query.Certainty,
            distance: query.Distance,
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

    // NearVectorInput overload with GroupBy
    /// <summary>
    /// Performs a near-vector search with group-by using a NearVectorInput record.
    /// </summary>
    /// <param name="query">Near-vector input containing vector, certainty, and distance.</param>
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
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearVector(
        NearVectorInput query,
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
        await NearVector(
            vectors: query.Vector,
            groupBy: groupBy,
            filters: filters,
            certainty: query.Certainty,
            distance: query.Distance,
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

    // NearVectorInputBuilder lambda overload
    /// <summary>
    /// Performs a near-vector search using a lambda builder for NearVectorInput.
    /// </summary>
    /// <example>
    /// v => v(certainty: 0.8).ManualWeights(
    ///     ("title", 1.2f, new[] { 1f, 2f }),
    ///     ("description", 0.8f, new[] { 3f, 4f })
    /// )
    /// </example>
    /// <param name="vectors"></param>
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
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearVector(
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
        await NearVector(
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

    // NearVectorInputBuilder lambda overload with GroupBy
    /// <summary>
    /// Performs a near-vector search with group-by using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="vectors"></param>
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
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearVector(
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
        await NearVector(
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
