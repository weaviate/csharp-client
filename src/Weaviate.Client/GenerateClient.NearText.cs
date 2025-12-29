using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class GenerateClient
{
    /// <summary>
    /// Search near text with generative AI capabilities.
    /// </summary>
    /// <param name="text">Text to search near</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="targets">Target vectors</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult> NearText(
        AutoArray<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        TargetVectors? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            text.ToArray(),
            distance: distance,
            certainty: certainty,
            limit: limit,
            moveTo: moveTo,
            moveAway: moveAway,
            offset: offset,
            autoLimit: autoLimit,
            targetVector: targets,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
        return result;
    }

    /// <summary>
    /// Search near text with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="text">Text to search near</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="moveTo">Move towards concept</param>
    /// <param name="moveAway">Move away from concept</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="targets">Target vectors</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative group-by result</returns>
    public async Task<GenerativeGroupByResult> NearText(
        AutoArray<string> text,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        TargetVectors? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            text.ToArray(),
            groupBy: groupBy,
            distance: distance,
            certainty: certainty,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            targetVector: targets,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
        return result;
    }
}

/// <summary>
/// Extension methods for GenerateClient NearText search with lambda target vector builders.
/// </summary>
public static class GenerateClientNearTextExtensions
{
    /// <summary>
    /// Search near text with generative AI capabilities using a lambda to build target vectors.
    /// </summary>
    public static async Task<GenerativeWeaviateResult> NearText(
        this GenerateClient client,
        AutoArray<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await client.NearText(
            text: text,
            certainty: certainty,
            distance: distance,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            targets: targets?.Invoke(new TargetVectors.Builder()),
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Search near text with generative AI capabilities and grouping using a lambda to build target vectors.
    /// </summary>
    public static async Task<GenerativeGroupByResult> NearText(
        this GenerateClient client,
        AutoArray<string> text,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await client.NearText(
            text: text,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            targets: targets?.Invoke(new TargetVectors.Builder()),
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
}
