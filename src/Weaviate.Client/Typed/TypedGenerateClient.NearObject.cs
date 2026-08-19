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
    /// Search near object with generative AI capabilities.
    /// </summary>
    /// <param name="nearObject">Object ID to search near</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="targetVectors">Target vectors to search</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative result</returns>
    public async Task<GenerativeWeaviateResult<T>> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        TargetVectors.FactoryFn? targetVectors = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.NearObject(
            nearObject: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            boost: boost,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            targetVectors: targetVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Search near object with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="nearObject">Object ID to search near</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="diversitySelection">The diversity selection</param>
    /// <param name="autoLimit">Auto-limit threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="targetVectors">Target vectors to search</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative group-by result</returns>
    public async Task<GenerativeGroupByResult<T>> NearObject(
        Guid nearObject,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        TargetVectors.FactoryFn? targetVectors = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.NearObject(
            nearObject: nearObject,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            boost: boost,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            targetVectors: targetVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }
}
