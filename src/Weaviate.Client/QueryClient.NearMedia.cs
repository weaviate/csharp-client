using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
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
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
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
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

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
        TargetVectors? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
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
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
}
