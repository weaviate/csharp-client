using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class GenerateClient
{
    /// <summary>
    /// BM25 search with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="searchFields">Fields to search in</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="after">Cursor for pagination</param>
    /// <param name="consistencyLevel">Consistency level</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative group-by result</returns>
    public async Task<GenerativeGroupByResult> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        Guid? after = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: prompt,
            groupedTask: groupedTask,
            after: after,
            tenant: _collectionClient.Tenant,
            consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            returnProperties: returnProperties,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
        return result;
    }

    /// <summary>
    /// BM25 search with generative AI capabilities.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="searchFields">Fields to search in</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="prompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="after">Cursor for pagination</param>
    /// <param name="consistencyLevel">Consistency level</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedTask? groupedTask = null,
        Guid? after = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: null,
            rerank: rerank,
            singlePrompt: prompt,
            groupedTask: groupedTask,
            after: after,
            tenant: _collectionClient.Tenant,
            consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            returnProperties: returnProperties,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

        return result;
    }
}
