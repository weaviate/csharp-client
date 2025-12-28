using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
    // Simple overload accepting VectorSearchInput (with implicit conversions support)
    public async Task<WeaviateResult> NearVector(
        VectorSearchInput vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearVector(
            _collectionClient.Name,
            vectors,
            distance: distance,
            certainty: certainty,
            offset: offset,
            autoLimit: autoLimit,
            limit: limit,
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

    // Simple overload with GroupBy
    public async Task<GroupByResult> NearVector(
        VectorSearchInput vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearVector(
            _collectionClient.Name,
            vectors,
            groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            offset: offset,
            autoLimit: autoLimit,
            limit: limit,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    // Lambda syntax overload
    public async Task<WeaviateResult> NearVector(
        Func<VectorSearchInput.Builder, VectorSearchInput> vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            filters,
            certainty,
            distance,
            autoLimit,
            limit,
            offset,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    // Lambda syntax overload with GroupBy
    public async Task<GroupByResult> NearVector(
        Func<VectorSearchInput.Builder, VectorSearchInput> vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            groupBy,
            filters,
            certainty,
            distance,
            autoLimit,
            limit,
            offset,
            rerank,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );
}
