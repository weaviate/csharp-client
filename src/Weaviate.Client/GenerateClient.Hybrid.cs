using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class GenerateClient
{
    /// <summary>
    /// Hybrid search with generative AI capabilities.
    /// </summary>
    public Task<GenerativeWeaviateResult> Hybrid(
        string query,
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
    public async Task<GenerativeWeaviateResult> Hybrid(
        string? query,
        HybridVectorInput? vectors,
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
        if (query is null && vectors is null)
        {
            throw new ArgumentException(
                "At least one of 'query' or 'vectors' must be provided for hybrid search."
            );
        }

        var result = await _client.GrpcClient.SearchHybrid(
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
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

        return result;
    }

    /// <summary>
    /// Hybrid search with generative AI capabilities and grouping.
    /// </summary>
    public Task<GenerativeGroupByResult> Hybrid(
        string query,
        Models.GroupByRequest groupBy,
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
    public async Task<GenerativeGroupByResult> Hybrid(
        string? query,
        HybridVectorInput? vectors,
        Models.GroupByRequest groupBy,
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
        if (query is null && vectors is null)
        {
            throw new ArgumentException(
                "At least one of 'query' or 'vectors' must be provided for hybrid search."
            );
        }

        var result = await _client.GrpcClient.SearchHybrid(
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
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

        return result;
    }
}

/// <summary>
/// Extension methods for GenerateClient Hybrid search with lambda vector builders.
/// </summary>
public static class GenerateClientHybridExtensions
{
    /// <summary>
    /// Hybrid search with generative AI capabilities using a lambda to build HybridVectorInput.
    /// This allows chaining NearVector or NearText configuration with target vectors.
    /// </summary>
    /// <example>
    /// await collection.Generate.Hybrid(
    ///     "test",
    ///     v => v.NearVector().ManualWeights(
    ///         ("title", 1.2, new[] { 1f, 2f }),
    ///         ("description", 0.8, new[] { 3f, 4f })
    ///     ),
    ///     singlePrompt: "Describe this item"
    /// );
    /// </example>
    public static async Task<GenerativeWeaviateResult> Hybrid(
        this GenerateClient client,
        string query,
        HybridVectorInput.FactoryFn vectors,
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
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
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
    public static async Task<GenerativeGroupByResult> Hybrid(
        this GenerateClient client,
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
