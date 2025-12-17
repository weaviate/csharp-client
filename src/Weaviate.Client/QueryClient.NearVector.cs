using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
    public async Task<WeaviateResult> NearVector(
        Vectors vector,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            (NearVectorInput)vector,
            filters,
            certainty,
            distance,
            autoLimit,
            limit,
            offset,
            targetVector,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    public async Task<WeaviateResult> NearVector(
        NearVectorInput vector,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearVector(
            _collectionClient.Name,
            vector,
            distance: distance,
            certainty: certainty,
            offset: offset,
            autoLimit: autoLimit,
            limit: limit,
            targetVector: targetVector,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    public async Task<GroupByResult> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            (NearVectorInput)vector,
            groupBy,
            filters,
            distance,
            certainty,
            autoLimit,
            limit,
            offset,
            targetVector,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    public async Task<GroupByResult> NearVector(
        NearVectorInput vector,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearVector(
            _collectionClient.Name,
            vector,
            groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            offset: offset,
            autoLimit: autoLimit,
            limit: limit,
            targetVector: targetVector,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
}
