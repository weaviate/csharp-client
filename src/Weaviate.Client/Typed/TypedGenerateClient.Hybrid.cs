using Weaviate.Client.Internal;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

/// <summary>
/// The typed generate client class
/// </summary>
public partial class TypedGenerateClient<T>
{
    /// <summary>
    /// Hybrid search with generative AI capabilities (query-only, no vectors).
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="fusionType">The fusion type</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="offset">The offset</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">The boost for soft-ranking results. Preview: requires Weaviate 1.38+ (older servers silently ignore it)</param>
    /// <param name="singlePrompt">The single prompt</param>
    /// <param name="groupedTask">The grouped task</param>
    /// <param name="provider">The provider</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<GenerativeWeaviateResult<T>> Hybrid(
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
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
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Hybrid search with generative AI capabilities.
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
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">The boost for soft-ranking results. Preview: requires Weaviate 1.38+ (older servers silently ignore it)</param>
    /// <param name="singlePrompt">The single prompt</param>
    /// <param name="groupedTask">The grouped task</param>
    /// <param name="provider">The provider</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task<GenerativeWeaviateResult<T>> Hybrid(
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.Hybrid(
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
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Hybrid search with generative AI capabilities and grouping (query-only, no vectors).
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
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">The boost for soft-ranking results. Preview: requires Weaviate 1.38+ (older servers silently ignore it)</param>
    /// <param name="singlePrompt">The single prompt</param>
    /// <param name="groupedTask">The grouped task</param>
    /// <param name="provider">The provider</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public Task<GenerativeGroupByResult<T>> Hybrid(
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
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
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Hybrid search with generative AI capabilities and grouping.
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
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">The boost for soft-ranking results. Preview: requires Weaviate 1.38+ (older servers silently ignore it)</param>
    /// <param name="singlePrompt">The single prompt</param>
    /// <param name="groupedTask">The grouped task</param>
    /// <param name="provider">The provider</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task<GenerativeGroupByResult<T>> Hybrid(
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.Hybrid(
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
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
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
/// Extension methods for TypedGenerateClient Hybrid search with lambda vector builders.
/// </summary>
public static class TypedGenerateClientHybridExtensions
{
    /// <summary>
    /// Hybrid search with generative AI capabilities using a lambda to build HybridVectorInput.
    /// This allows chaining NearVector or NearText configuration with target vectors.
    /// </summary>
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
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">The boost for soft-ranking results. Preview: requires Weaviate 1.38+ (older servers silently ignore it)</param>
    /// <param name="singlePrompt">The single prompt</param>
    /// <param name="groupedTask">The grouped task</param>
    /// <param name="provider">The provider</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public static async Task<GenerativeWeaviateResult<T>> Hybrid<T>(
        this TypedGenerateClient<T> client,
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
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
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Hybrid search with generative AI capabilities and grouping using a lambda to build HybridVectorInput.
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
    /// <param name="autoLimit">The auto limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="rerank">The rerank</param>
    /// <param name="boost">The boost for soft-ranking results. Preview: requires Weaviate 1.38+ (older servers silently ignore it)</param>
    /// <param name="singlePrompt">The single prompt</param>
    /// <param name="groupedTask">The grouped task</param>
    /// <param name="provider">The provider</param>
    /// <param name="returnProperties">The return properties</param>
    /// <param name="returnReferences">The return references</param>
    /// <param name="returnMetadata">The return metadata</param>
    /// <param name="includeVectors">The include vectors</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public static async Task<GenerativeGroupByResult<T>> Hybrid<T>(
        this TypedGenerateClient<T> client,
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
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
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
}
