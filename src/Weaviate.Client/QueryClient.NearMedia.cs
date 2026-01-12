using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
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
    /// <param name="query">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearMedia(
        NearMediaInput.FactoryFn query,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = query(new NearMediaBuilder());
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
    /// <param name="query">Lambda builder for creating NearMediaInput with media data and target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearMedia(
        NearMediaInput.FactoryFn query,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var input = query(new NearMediaBuilder());
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

    /// <summary>
    /// Performs a near-media search using raw media bytes and media type.
    /// This is a legacy overload - prefer using the lambda builder overload with NearMediaInput.FactoryFn.
    /// </summary>
    /// <param name="media">The media content as a byte array.</param>
    /// <param name="mediaType">The type of media (Image, Video, Audio, etc.).</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targets">Target vectors factory function for multi-vector collections.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    public async Task<WeaviateResult> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearMedia(
            _collectionClient.Name,
            media: media,
            mediaType: mediaType,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: null,
            rerank: rerank,
            singlePrompt: null,
            groupedTask: null,
            tenant: _collectionClient.Tenant,
            targetVector: targets?.Invoke(new TargetVectors.Builder()),
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Performs a near-media search with group-by aggregation using raw media bytes.
    /// This is a legacy overload - prefer using the lambda builder overload with NearMediaInput.FactoryFn.
    /// </summary>
    /// <param name="media">The media content as a byte array.</param>
    /// <param name="mediaType">The type of media (Image, Video, Audio, etc.).</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="certainty">Minimum certainty threshold (0-1).</param>
    /// <param name="distance">Maximum distance threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="targets">Target vectors factory function for multi-vector collections.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Grouped search results.</returns>
    public async Task<GroupByResult> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearMedia(
            _collectionClient.Name,
            media: media,
            mediaType: mediaType,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: null,
            groupedTask: null,
            tenant: _collectionClient.Tenant,
            targetVector: targets?.Invoke(new TargetVectors.Builder()),
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
}
