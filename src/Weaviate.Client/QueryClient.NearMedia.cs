using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
    /// <summary>
    /// Performs a near-media search using media embeddings.
    /// </summary>
    /// <example>
    /// // Simple image search
    /// await collection.Query.NearMedia(m => m.Image(imageBytes));
    ///
    /// // With certainty and target vectors
    /// await collection.Query.NearMedia(m => m.Image(imageBytes, certainty: 0.8f).Sum("v1", "v2"));
    ///
    /// // All media types supported
    /// await collection.Query.NearMedia(m => m.Video(videoBytes));
    /// await collection.Query.NearMedia(m => m.Audio(audioBytes));
    /// await collection.Query.NearMedia(m => m.Thermal(thermalBytes));
    /// await collection.Query.NearMedia(m => m.Depth(depthBytes));
    /// await collection.Query.NearMedia(m => m.IMU(imuBytes));
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearMedia(
        NearMediaInput.FactoryFn media,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = media(VectorInputBuilderFactories.CreateNearMediaBuilder());
        return await _grpc.SearchNearMedia(
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
            singlePrompt: null,
            groupedTask: null,
            tenant: _collectionClient.Tenant,
            targetVector: input.TargetVectors,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }

    /// <summary>
    /// Performs a near-media search with group-by aggregation.
    /// </summary>
    /// <example>
    /// // Image search with grouping
    /// await collection.Query.NearMedia(
    ///     m => m.Image(imageBytes).Sum("visual", "semantic"),
    ///     groupBy: new GroupByRequest("category", objectsPerGroup: 5)
    /// );
    /// </example>
    /// <param name="media">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearMedia(
        NearMediaInput.FactoryFn media,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = media(VectorInputBuilderFactories.CreateNearMediaBuilder());
        return await _grpc.SearchNearMedia(
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
            singlePrompt: null,
            groupedTask: null,
            tenant: _collectionClient.Tenant,
            targetVector: input.TargetVectors,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }
}
