using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The generate client class
/// </summary>
public partial class GenerateClient
{
    /// <summary>
    /// Performs a near-media search with generative AI capabilities.
    /// </summary>
    /// <example>
    /// // Simple image search with generation
    /// await collection.Generate.NearMedia(
    ///     m => m.Image(imageBytes),
    ///     singlePrompt: "Describe this image"
    /// );
    ///
    /// // With target vectors and generation
    /// await collection.Generate.NearMedia(
    ///     m => m.Video(videoBytes, certainty: 0.8f).Sum("v1", "v2"),
    ///     singlePrompt: "Summarize this video"
    /// );
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
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
    /// <returns>Generative search result.</returns>
    public async Task<GenerativeWeaviateResult> NearMedia(
        NearMediaInput.FactoryFn media,
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
        var input = media(new NearMediaBuilder());
        var result = await _client.GrpcClient.SearchNearMedia(
            _collectionClient.Name,
            media: input.Media,
            mediaType: input.Type,
            certainty: input.Certainty,
            distance: input.Distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: null,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            tenant: _collectionClient.Tenant,
            targetVector: input.TargetVectors,
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
    /// Performs a near-media search with generative AI capabilities and group-by aggregation.
    /// </summary>
    /// <example>
    /// // Image search with grouping and generation
    /// await collection.Generate.NearMedia(
    ///     m => m.Image(imageBytes).Sum("visual", "semantic"),
    ///     groupBy: new GroupByRequest("category", objectsPerGroup: 5),
    ///     groupedTask: "Summarize each group"
    /// );
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
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
    /// <returns>Generative grouped search result.</returns>
    public async Task<GenerativeGroupByResult> NearMedia(
        NearMediaInput.FactoryFn media,
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
        var input = media(new NearMediaBuilder());
        var result = await _client.GrpcClient.SearchNearMedia(
            _collectionClient.Name,
            media: input.Media,
            mediaType: input.Type,
            certainty: input.Certainty,
            distance: input.Distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            tenant: _collectionClient.Tenant,
            targetVector: input.TargetVectors,
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
