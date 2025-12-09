using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class GenerateClient
{
    /// <summary>
    /// Hybrid search with generative AI capabilities.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="fusionType">Fusion type</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult> Hybrid(
        string? query,
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
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
            _collectionClient.Name,
            query: query,
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
            singlePrompt: prompt,
            groupedTask: groupedTask,
            targetVector: targetVector,
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
    /// <param name="query">Search query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="fusionType">Fusion type</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative group-by result</returns>
    public async Task<GenerativeGroupByResult> Hybrid(
        string? query,
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
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
            _collectionClient.Name,
            query: query,
            alpha: alpha,
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
            singlePrompt: prompt,
            groupedTask: groupedTask,
            targetVector: targetVector,
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
    /// Hybrid search with generative AI capabilities using vectors.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="vectors">Vectors for search</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="fusionType">Fusion type</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult> Hybrid(
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
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await Hybrid(
            query,
            vectors: vectors,
            alpha,
            queryProperties,
            fusionType,
            maxVectorDistance,
            limit,
            offset,
            bm25Operator,
            autoLimit,
            filters,
            rerank,
            prompt,
            groupedTask,
            targetVector,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Hybrid search with generative AI capabilities using hybrid vector input.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="vectors">Hybrid vector input</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="fusionType">Fusion type</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult> Hybrid(
        string? query,
        IHybridVectorInput vectors,
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
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
            _collectionClient.Name,
            query: query,
            alpha: alpha,
            vector: vectors as Vectors,
            nearVector: vectors as HybridNearVector,
            nearText: vectors as HybridNearText,
            queryProperties: queryProperties,
            fusionType: fusionType,
            maxVectorDistance: maxVectorDistance,
            limit: limit,
            offset: offset,
            bm25Operator: bm25Operator,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            singlePrompt: prompt,
            groupedTask: groupedTask,
            targetVector: targetVector,
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
    /// Hybrid search with generative AI capabilities, grouping, and hybrid vector input.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="alpha">Alpha value for hybrid search</param>
    /// <param name="vectors">Hybrid vector input</param>
    /// <param name="queryProperties">Properties to query</param>
    /// <param name="fusionType">Fusion type</param>
    /// <param name="maxVectorDistance">Maximum vector distance</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="bm25Operator">BM25 operator</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative group-by result</returns>
    public async Task<GenerativeGroupByResult> Hybrid(
        string? query,
        Models.GroupByRequest groupBy,
        float? alpha = null,
        IHybridVectorInput? vectors = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
            _collectionClient.Name,
            query: query,
            alpha: alpha,
            vector: vectors as Vectors,
            nearVector: vectors as HybridNearVector,
            nearText: vectors as HybridNearText,
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
            singlePrompt: prompt,
            groupedTask: groupedTask,
            targetVector: targetVector,
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
