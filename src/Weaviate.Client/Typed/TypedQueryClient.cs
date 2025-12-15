using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

/// <summary>
/// Strongly-typed wrapper for QueryClient that returns typed results.
/// All query methods return WeaviateObject&lt;T&gt; instead of untyped WeaviateObject.
/// </summary>
/// <typeparam name="T">The C# type to deserialize object properties into.</typeparam>
public class TypedQueryClient<T>
    where T : class, new()
{
    private readonly QueryClient _queryClient;

    /// <summary>
    /// Initializes a new instance of the TypedQueryClient class.
    /// </summary>
    /// <param name="queryClient">The underlying QueryClient to wrap.</param>
    public TypedQueryClient(QueryClient queryClient)
    {
        ArgumentNullException.ThrowIfNull(queryClient);

        _queryClient = queryClient;
    }

    #region Objects

    /// <summary>
    /// Fetches objects with group-by aggregation.
    /// </summary>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="filters">Filters to apply to the query.</param>
    /// <param name="sort">Sorting configuration.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> FetchObjects(
        GroupByRequest groupBy,
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjects(
            groupBy: groupBy,
            limit: limit,
            filters: filters,
            sort: sort,
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
    /// Fetches objects from the collection.
    /// </summary>
    /// <param name="after">Cursor for pagination.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="filters">Filters to apply to the query.</param>
    /// <param name="sort">Sorting configuration.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the fetched objects.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> FetchObjects(
        Guid? after = null,
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjects(
            limit: limit,
            filters: filters,
            sort: sort,
            rerank: rerank,
            after: after,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Fetches a single object by its ID.
    /// </summary>
    /// <param name="uuid">The UUID of the object to fetch.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed object, or null if not found.</returns>
    public async Task<WeaviateObject<T>?> FetchObjectByID(
        Guid uuid,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjectByID(
            id: uuid,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result?.ToTyped<T>();
    }

    /// <summary>
    /// Fetches multiple objects by their IDs.
    /// </summary>
    /// <param name="uuids">The UUIDs of the objects to fetch.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="filters">Additional filters to apply.</param>
    /// <param name="sort">Sorting configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the fetched objects.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> FetchObjectsByIDs(
        HashSet<Guid> uuids,
        uint? limit = null,
        Rerank? rerank = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjectsByIDs(
            ids: uuids,
            limit: limit,
            rerank: rerank,
            filters: filters,
            sort: sort,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    #endregion

    #region Search

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
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearText(
        OneOrManyOf<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
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
            targetVector: targetVector,
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
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearText(
        OneOrManyOf<string> text,
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
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
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
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-vector search using vector embeddings.
    /// </summary>
    /// <param name="vector">The vector to search near.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearVector(
        Vectors vector,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        Rerank? rerank = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearVector(
            vector: vector,
            filters: filters,
            certainty: certainty,
            distance: distance,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            targetVector: targetVector,
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
    /// <param name="vector">The vector to search near.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        Rerank? rerank = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearVector(
            vector: vector,
            groupBy: groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            targetVector: targetVector,
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
    /// Performs a BM25 keyword search with group-by aggregation.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="searchFields">Fields to search in.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="consistencyLevel">Consistency level for the query.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.BM25(
            query: query,
            groupBy: groupBy,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            consistencyLevel: consistencyLevel,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a BM25 keyword search.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="searchFields">Fields to search in.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="consistencyLevel">Consistency level for the query.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.BM25(
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            consistencyLevel: consistencyLevel,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a hybrid search combining keyword and vector search.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="vectors">The vectors to search near.</param>
    /// <param name="alpha">Balance between keyword and vector search (0=BM25, 1=vector).</param>
    /// <param name="queryProperties">Properties to search in.</param>
    /// <param name="fusionType">Fusion algorithm for combining results.</param>
    /// <param name="maxVectorDistance">Maximum vector distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="bm25Operator">Operator for BM25 search terms.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> Hybrid(
        string? query,
        Vectors vectors,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.Hybrid(
            query: query,
            vectors: vectors,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a hybrid search combining keyword and vector search.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="vectors">The vector input (can be Vectors, HybridNearVector, or HybridNearText).</param>
    /// <param name="alpha">Balance between keyword and vector search (0=BM25, 1=vector).</param>
    /// <param name="queryProperties">Properties to search in.</param>
    /// <param name="fusionType">Fusion algorithm for combining results.</param>
    /// <param name="maxVectorDistance">Maximum vector distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="bm25Operator">Operator for BM25 search terms.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> Hybrid(
        string? query,
        IHybridVectorInput? vectors = null,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.Hybrid(
            query: query,
            vectors: vectors,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a hybrid search with group-by aggregation.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="vectors">The vectors to search near.</param>
    /// <param name="alpha">Balance between keyword and vector search (0=BM25, 1=vector).</param>
    /// <param name="queryProperties">Properties to search in.</param>
    /// <param name="fusionType">Fusion algorithm for combining results.</param>
    /// <param name="maxVectorDistance">Maximum vector distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="bm25Operator">Operator for BM25 search terms.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> Hybrid(
        string? query,
        GroupByRequest groupBy,
        Vectors vectors,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.Hybrid(
            query: query,
            groupBy: groupBy,
            vectors: vectors,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a hybrid search with group-by aggregation.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="vectors">The vector input (can be Vectors, HybridNearVector, or HybridNearText).</param>
    /// <param name="alpha">Balance between keyword and vector search (0=BM25, 1=vector).</param>
    /// <param name="queryProperties">Properties to search in.</param>
    /// <param name="fusionType">Fusion algorithm for combining results.</param>
    /// <param name="maxVectorDistance">Maximum vector distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="bm25Operator">Operator for BM25 search terms.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> Hybrid(
        string? query,
        GroupByRequest groupBy,
        IHybridVectorInput? vectors = null,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.Hybrid(
            query: query,
            groupBy: groupBy,
            vectors: vectors,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-object search using another object as reference.
    /// </summary>
    /// <param name="nearObject">The ID of the object to search near.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearObject(
            nearObject: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-object search with group-by aggregation.
    /// </summary>
    /// <param name="nearObject">The ID of the object to search near.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearObject(
        Guid nearObject,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearObject(
            nearObject: nearObject,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-image search using image embeddings.
    /// </summary>
    /// <param name="nearImage">The image data as a byte array.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearImage(
        byte[] nearImage,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearImage(
            nearImage: nearImage,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-image search with group-by aggregation.
    /// </summary>
    /// <param name="nearImage">The image data as a byte array.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearImage(
        byte[] nearImage,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearImage(
            nearImage: nearImage,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a near-media search using media embeddings (image, video, audio, etc.).
    /// </summary>
    /// <param name="media">The media data as a byte array.</param>
    /// <param name="mediaType">The type of media (image, video, audio, etc.).</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the search results.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearMedia(
            media: media,
            mediaType: mediaType,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
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
    /// <param name="media">The media data as a byte array.</param>
    /// <param name="mediaType">The type of media (image, video, audio, etc.).</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result limit threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targetVector">Target vector configuration for named vectors.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.NearMedia(
            media: media,
            mediaType: mediaType,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    #endregion
}
