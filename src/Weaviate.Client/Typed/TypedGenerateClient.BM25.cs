using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedGenerateClient<T>
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
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="consistencyLevel">Consistency level</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative group-by result</returns>
    public async Task<GenerativeGroupByResult<T>> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        ConsistencyLevels? consistencyLevel = null,
        AutoArray<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.BM25(
            query: query,
            groupBy: groupBy,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
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
    /// BM25 search with generative AI capabilities.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="searchFields">Fields to search in</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="consistencyLevel">Consistency level</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative result</returns>
    public async Task<GenerativeWeaviateResult<T>> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        ConsistencyLevels? consistencyLevel = null,
        AutoArray<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.BM25(
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            consistencyLevel: consistencyLevel,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnReferences: returnReferences,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }
}
