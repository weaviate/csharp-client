using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    /// <summary>
    /// Performs a hybrid search (keyword + vector search).
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<WeaviateResult> Hybrid(
        string query,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        Hybrid(
            query: query,
            vectors: (HybridVectorInput?)null,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a hybrid search (keyword + vector search).
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task<WeaviateResult> Hybrid(
        string? query,
        HybridVectorInput? vectors,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        Diversity? diversitySelection = null,
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
        if (query is null && vectors is null)
        {
            throw new ArgumentException(
                "At least one of 'query' or 'vectors' must be provided for hybrid search."
            );
        }

        return await _grpc.SearchHybrid(
            _collectionClient.Name,
            query: query,
            alpha: alpha,
            vectors: vectors,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// Performs a hybrid search (keyword + vector search) with grouping.
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<GroupByResult> Hybrid(
        string query,
        GroupByRequest groupBy,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        Hybrid(
            query: query,
            vectors: (HybridVectorInput?)null,
            groupBy: groupBy,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a hybrid search (keyword + vector search) with grouping.
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task<GroupByResult> Hybrid(
        string? query,
        HybridVectorInput? vectors,
        GroupByRequest groupBy,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        Diversity? diversitySelection = null,
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
        if (query is null && vectors is null)
        {
            throw new ArgumentException(
                "At least one of 'query' or 'vectors' must be provided for hybrid search."
            );
        }

        return await _grpc.SearchHybrid(
            _collectionClient.Name,
            query: query,
            vectors: vectors,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }
}

/// <summary>
/// Extension methods for QueryClient Hybrid search with lambda vector builders.
/// </summary>
public static class QueryClientHybridExtensions
{
    /// <summary>
    /// Performs a hybrid search (keyword + vector search) using a lambda to build HybridVectorInput.
    /// This allows chaining NearVector or NearText configuration with target vectors.
    /// </summary>
    /// <example>
    /// await collection.Query.Hybrid(
    ///     "test",
    ///     v => v.NearVector().ManualWeights(
    ///         ("title", 1.2, new[] { 1f, 2f }),
    ///         ("description", 0.8, new[] { 3f, 4f })
    ///     )
    /// );
    /// </example>
    /// <param name="client">The client</param>
    /// <param name="query">The query</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public static async Task<WeaviateResult> Hybrid(
        this QueryClient client,
        string? query = null,
        HybridVectorInput.FactoryFn? vectors = null,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        Diversity? diversitySelection = null,
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
        var vectorsLocal = vectors?.Invoke(VectorInputBuilderFactories.CreateHybridBuilder());

        return await client.Hybrid(
            query: query,
            vectors: vectorsLocal,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Performs a hybrid search (keyword + vector search) with grouping using a lambda to build HybridVectorInput.
    /// This allows chaining NearVector or NearText configuration with target vectors.
    /// </summary>
    /// <param name="client">The client</param>
    /// <param name="query">The query</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public static async Task<GroupByResult> Hybrid(
        this QueryClient client,
        string? query,
        HybridVectorInput.FactoryFn? vectors,
        GroupByRequest groupBy,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        Diversity? diversitySelection = null,
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
        var vectorsLocal = vectors?.Invoke(VectorInputBuilderFactories.CreateHybridBuilder());

        return await client.Hybrid(
            query: query,
            vectors: vectorsLocal,
            groupBy: groupBy,
            alpha: alpha,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
    }
}
