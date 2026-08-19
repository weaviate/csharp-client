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
    /// <param name="query">Text query for the keyword (BM25) half of the search.</param>
    /// <param name="alpha">Balance between the keyword and the vector half of the search: 0.0 is pure keyword (BM25), 1.0 is pure vector. If not specified, the server default of 0.75 is used, or 1.0 when no query text is given.</param>
    /// <param name="queryProperties">Properties the keyword (BM25) half of the search runs against. If not specified, all text properties are searched.</param>
    /// <param name="fusionType">How the keyword and vector result sets are fused: Ranked adds inverted ranks, RelativeScore adds normalized scores. If not specified, the server default (RelativeScore) is used.</param>
    /// <param name="maxVectorDistance">Maximum distance allowed for the vector half of the search. If not specified, no distance threshold is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="bm25Operator">Operator for the keyword (BM25) half of the search, setting how many query tokens a property must match. If not specified, the server default (Or) is used.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
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
        Boost? boost = null,
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
            boost: boost,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a hybrid search (keyword + vector search).
    /// </summary>
    /// <param name="query">Text query for the keyword (BM25) half of the search.</param>
    /// <param name="vectors">Vector input for the vector half of the search. If not specified, the query text is vectorized and used.</param>
    /// <param name="alpha">Balance between the keyword and the vector half of the search: 0.0 is pure keyword (BM25), 1.0 is pure vector. If not specified, the server default of 0.75 is used, or 1.0 when no query text is given.</param>
    /// <param name="queryProperties">Properties the keyword (BM25) half of the search runs against. If not specified, all text properties are searched.</param>
    /// <param name="fusionType">How the keyword and vector result sets are fused: Ranked adds inverted ranks, RelativeScore adds normalized scores. If not specified, the server default (RelativeScore) is used.</param>
    /// <param name="maxVectorDistance">Maximum distance allowed for the vector half of the search. If not specified, no distance threshold is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="bm25Operator">Operator for the keyword (BM25) half of the search, setting how many query tokens a property must match. If not specified, the server default (Or) is used.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
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
        Boost? boost = null,
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
            boost: boost,
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
    /// <param name="query">Text query for the keyword (BM25) half of the search.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="alpha">Balance between the keyword and the vector half of the search: 0.0 is pure keyword (BM25), 1.0 is pure vector. If not specified, the server default of 0.75 is used, or 1.0 when no query text is given.</param>
    /// <param name="queryProperties">Properties the keyword (BM25) half of the search runs against. If not specified, all text properties are searched.</param>
    /// <param name="fusionType">How the keyword and vector result sets are fused: Ranked adds inverted ranks, RelativeScore adds normalized scores. If not specified, the server default (RelativeScore) is used.</param>
    /// <param name="maxVectorDistance">Maximum distance allowed for the vector half of the search. If not specified, no distance threshold is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="bm25Operator">Operator for the keyword (BM25) half of the search, setting how many query tokens a property must match. If not specified, the server default (Or) is used.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
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
        Boost? boost = null,
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
            boost: boost,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a hybrid search (keyword + vector search) with grouping.
    /// </summary>
    /// <param name="query">Text query for the keyword (BM25) half of the search.</param>
    /// <param name="vectors">Vector input for the vector half of the search. If not specified, the query text is vectorized and used.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="alpha">Balance between the keyword and the vector half of the search: 0.0 is pure keyword (BM25), 1.0 is pure vector. If not specified, the server default of 0.75 is used, or 1.0 when no query text is given.</param>
    /// <param name="queryProperties">Properties the keyword (BM25) half of the search runs against. If not specified, all text properties are searched.</param>
    /// <param name="fusionType">How the keyword and vector result sets are fused: Ranked adds inverted ranks, RelativeScore adds normalized scores. If not specified, the server default (RelativeScore) is used.</param>
    /// <param name="maxVectorDistance">Maximum distance allowed for the vector half of the search. If not specified, no distance threshold is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="bm25Operator">Operator for the keyword (BM25) half of the search, setting how many query tokens a property must match. If not specified, the server default (Or) is used.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
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
        Boost? boost = null,
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
            boost: boost,
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
    /// <param name="client">The client to run the search on.</param>
    /// <param name="query">Text query for the keyword (BM25) half of the search.</param>
    /// <param name="vectors">Lambda builder for the vector input used by the vector half of the search.</param>
    /// <param name="alpha">Balance between the keyword and the vector half of the search: 0.0 is pure keyword (BM25), 1.0 is pure vector. If not specified, the server default of 0.75 is used, or 1.0 when no query text is given.</param>
    /// <param name="queryProperties">Properties the keyword (BM25) half of the search runs against. If not specified, all text properties are searched.</param>
    /// <param name="fusionType">How the keyword and vector result sets are fused: Ranked adds inverted ranks, RelativeScore adds normalized scores. If not specified, the server default (RelativeScore) is used.</param>
    /// <param name="maxVectorDistance">Maximum distance allowed for the vector half of the search. If not specified, no distance threshold is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="bm25Operator">Operator for the keyword (BM25) half of the search, setting how many query tokens a property must match. If not specified, the server default (Or) is used.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
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
        Boost? boost = null,
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
            boost: boost,
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
    /// <param name="client">The client to run the search on.</param>
    /// <param name="query">Text query for the keyword (BM25) half of the search.</param>
    /// <param name="vectors">Lambda builder for the vector input used by the vector half of the search.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="alpha">Balance between the keyword and the vector half of the search: 0.0 is pure keyword (BM25), 1.0 is pure vector. If not specified, the server default of 0.75 is used, or 1.0 when no query text is given.</param>
    /// <param name="queryProperties">Properties the keyword (BM25) half of the search runs against. If not specified, all text properties are searched.</param>
    /// <param name="fusionType">How the keyword and vector result sets are fused: Ranked adds inverted ranks, RelativeScore adds normalized scores. If not specified, the server default (RelativeScore) is used.</param>
    /// <param name="maxVectorDistance">Maximum distance allowed for the vector half of the search. If not specified, no distance threshold is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="bm25Operator">Operator for the keyword (BM25) half of the search, setting how many query tokens a property must match. If not specified, the server default (Or) is used.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
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
        Boost? boost = null,
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
            boost: boost,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
    }
}
