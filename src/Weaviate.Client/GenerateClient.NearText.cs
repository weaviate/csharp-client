using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class GenerateClient
{
    /// <summary>
    /// Search near text with generative AI capabilities.
    /// </summary>
    /// <param name="query">Text to search near</param>
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
        AutoArray<string> query,
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
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            query.ToArray(),
            distance: distance,
            certainty: certainty,
            limit: limit,
            moveTo: moveTo,
            moveAway: moveAway,
            offset: offset,
            autoLimit: autoLimit,
            targetVector: null,
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
    /// <param name="query">Text to search near</param>
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
        AutoArray<string> query,
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
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            query.ToArray(),
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
            targetVector: null,
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
    /// Search near text with generative AI capabilities using a NearTextInput record.
    /// </summary>
    /// <param name="input">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generative search results.</returns>
    public async Task<GenerativeWeaviateResult> NearText(
        NearTextInput input,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
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
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            input.Query.ToArray(),
            distance: input.Distance,
            certainty: input.Certainty,
            limit: limit,
            moveTo: input.MoveTo,
            moveAway: input.MoveAway,
            offset: offset,
            autoLimit: autoLimit,
            targetVector: input.TargetVectors,
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
    /// Search near text with generative AI capabilities and grouping using a NearTextInput record.
    /// </summary>
    /// <param name="input">Near-text input containing query text, target vectors, certainty, distance, and move parameters.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generative grouped search results.</returns>
    public async Task<GenerativeGroupByResult> NearText(
        NearTextInput input,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
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
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            input.Query.ToArray(),
            groupBy: groupBy,
            distance: input.Distance,
            certainty: input.Certainty,
            moveTo: input.MoveTo,
            moveAway: input.MoveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            targetVector: input.TargetVectors,
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
    /// Performs a near-text search with generative AI using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generative search results.</returns>
    public async Task<GenerativeWeaviateResult> NearText(
        NearTextInput.FactoryFn query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
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
        await NearText(
            query(VectorInputBuilderFactories.CreateNearTextBuilder()),
            filters,
            limit,
            offset,
            autoLimit,
            rerank,
            singlePrompt,
            groupedTask,
            provider,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Performs a near-text search with generative AI and grouping using a lambda builder for NearTextInput.
    /// </summary>
    /// <param name="inputBuilder">Lambda builder for creating NearTextInput with target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Generative grouped search results.</returns>
    public async Task<GenerativeGroupByResult> NearText(
        NearTextInput.FactoryFn query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
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
        await NearText(
            query(VectorInputBuilderFactories.CreateNearTextBuilder()),
            groupBy,
            filters,
            limit,
            offset,
            autoLimit,
            rerank,
            singlePrompt,
            groupedTask,
            provider,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );
}
