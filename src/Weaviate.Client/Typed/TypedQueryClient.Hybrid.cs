using Weaviate.Client.Internal;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

/// <summary>
/// The typed query client class
/// </summary>
public partial class TypedQueryClient<T>
{
    /// <summary>
    /// Performs a hybrid search using keyword search only.
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
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public Task<WeaviateResult<WeaviateObject<T>>> Hybrid(
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
    /// Performs a hybrid search combining keyword and vector search.
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
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task<WeaviateResult<WeaviateObject<T>>> Hybrid(
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
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Performs a hybrid search with group-by aggregation using keyword search only.
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
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public Task<GroupByResult<T>> Hybrid(
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
    /// Performs a hybrid search with group-by aggregation.
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
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task<GroupByResult<T>> Hybrid(
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
        var result = await _queryClient.Hybrid(
            query: query,
            vectors: vectors,
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
        return result.ToTyped<T>();
    }
}

/// <summary>
/// Extension methods for TypedQueryClient Hybrid search with lambda vector builders.
/// </summary>
public static class TypedQueryClientHybridExtensions
{
    /// <summary>
    /// Performs a hybrid search (keyword + vector search) using a lambda to build HybridVectorInput.
    /// Supports NearVector, NearText, or direct vector input via .Vectors().
    /// </summary>
    /// <example>
    /// // Using NearVector with target vectors:
    /// await collection.Query.AsTyped&lt;MyClass&gt;().Hybrid(
    ///     "test",
    ///     v => v.NearVector().ManualWeights(
    ///         ("title", 1.2, new[] { 1f, 2f }),
    ///         ("description", 0.8, new[] { 3f, 4f })
    ///     )
    /// );
    ///
    /// // Using direct vectors (no combination method):
    /// await collection.Query.AsTyped&lt;MyClass&gt;().Hybrid(
    ///     "test",
    ///     v => v.Vectors(
    ///         ("title", new[] { 1f, 2f }),
    ///         ("description", new[] { 3f, 4f })
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
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public static async Task<WeaviateResult<WeaviateObject<T>>> Hybrid<T>(
        this TypedQueryClient<T> client,
        string query,
        HybridVectorInput.FactoryFn vectors,
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
        where T : class, new() =>
        await client.Hybrid(
            query: query,
            vectors: vectors(VectorInputBuilderFactories.CreateHybridBuilder()),
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
    /// Performs a hybrid search (keyword + vector search) with grouping using a lambda to build HybridVectorInput.
    /// Supports NearVector, NearText, or direct vector input via .Vectors().
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
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public static async Task<GroupByResult<T>> Hybrid<T>(
        this TypedQueryClient<T> client,
        string query,
        HybridVectorInput.FactoryFn vectors,
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
        where T : class, new() =>
        await client.Hybrid(
            query: query,
            vectors: vectors(VectorInputBuilderFactories.CreateHybridBuilder()),
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
