using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedGenerateClient<T>
{
    /// <summary>
    /// Performs a near-media search with generative AI capabilities.
    /// </summary>
    /// <example>
    /// // Simple image search with generation
    /// await typedGenerate.NearMedia(
    ///     m => m.Image(imageBytes),
    ///     singlePrompt: "Describe this image"
    /// );
    ///
    /// // With target vectors and generation
    /// await typedGenerate.NearMedia(
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
    /// <returns>A strongly-typed generative search result.</returns>
    public async Task<GenerativeWeaviateResult<T>> NearMedia(
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
        var result = await _generateClient.NearMedia(
            media: media,
            filters: filters,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
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
    /// Performs a near-media search with generative AI capabilities and group-by aggregation.
    /// </summary>
    /// <example>
    /// // Image search with grouping and generation
    /// await typedGenerate.NearMedia(
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
    /// <returns>A strongly-typed generative grouped search result.</returns>
    public async Task<GenerativeGroupByResult<T>> NearMedia(
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
        var result = await _generateClient.NearMedia(
            media: media,
            groupBy: groupBy,
            filters: filters,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
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
