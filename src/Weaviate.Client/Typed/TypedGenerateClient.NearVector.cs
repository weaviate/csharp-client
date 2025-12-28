using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedGenerateClient<T>
{
    /// <summary>
    /// Search near vector with generative AI capabilities.
    /// </summary>
    /// <param name="vectors">Vector to search near</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative result</returns>
    public async Task<GenerativeWeaviateResult<T>> NearVector(
        VectorSearchInput vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
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
        var result = await _generateClient.NearVector(
            vectors: vectors,
            filters: filters,
            certainty: certainty,
            distance: distance,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
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
    /// Search near vector with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="vectors">Vector to search near</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative group-by result</returns>
    public async Task<GenerativeGroupByResult<T>> NearVector(
        VectorSearchInput vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
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
        var result = await _generateClient.NearVector(
            vectors: vectors,
            groupBy: groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
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
