using Weaviate.Client.Models;

namespace Weaviate.Client;

public class QueryClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;
    private string _collectionName => _collectionClient.Name;

    private WeaviateClient _client => _collectionClient.Client;

    public QueryClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    #region Objects
    public async Task<GroupByResult> FetchObjects(
        Models.GroupByRequest groupBy,
        uint? limit = null,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: returnProperties,
                limit: limit,
                sort: sort,
                filter: filter,
                groupBy: groupBy,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).group;

    public async Task<WeaviateResult> FetchObjects(
        uint? limit = null,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: returnProperties,
                limit: limit,
                sort: sort,
                filter: filter,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant
            )
        ).result;

    public async Task<WeaviateObject?> FetchObjectByID(
        Guid id,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: returnProperties,
                filter: Filter.WithID(id),
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant
            )
        ).result.SingleOrDefault();

    public async Task<WeaviateResult> FetchObjectsByIDs(
        ISet<Guid> ids,
        uint? limit = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: returnProperties,
                limit: limit,
                filter: Filter.WithIDs(ids),
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant
            )
        ).result;
    #endregion

    #region Search

    public async Task<WeaviateResult> NearText(
        string text,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearText(
                _collectionClient.Name,
                text,
                distance: distance,
                certainty: certainty,
                limit: limit,
                moveTo: moveTo,
                moveAway: moveAway,
                fields: returnProperties,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).result;

    public async Task<GroupByResult> NearText(
        string text,
        Models.GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearText(
                _collectionClient.Name,
                text,
                groupBy: groupBy,
                distance: distance,
                certainty: certainty,
                limit: limit,
                fields: returnProperties,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).group;

    public async Task<WeaviateResult> NearVector(
        Vectors vector,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                distance: distance,
                certainty: certainty,
                limit: limit,
                targetVector: targetVector,
                fields: returnProperties,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).result;

    public async Task<GroupByResult> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                groupBy,
                distance: distance,
                certainty: certainty,
                limit: limit,
                targetVector: targetVector,
                fields: returnProperties,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).group;

    public async Task<GroupByResult> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                fields: returnProperties,
                groupBy: groupBy,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).group;

    public async Task<WeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                fields: returnProperties,
                reference: returnReferences,
                metadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).result;

    public async Task<WeaviateResult> Hybrid(
        string? query,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        return (
            await _client.GrpcClient.SearchHybrid(
                _collectionClient.Name,
                query: query,
                alpha: alpha,
                queryProperties: queryProperties,
                fusionType: fusionType,
                maxVectorDistance: maxVectorDistance,
                limit: limit,
                offset: offset,
                bm25Operator: bm25Operator,
                autoLimit: autoLimit,
                filters: filters,
                rerank: rerank,
                targetVector: targetVector,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).result;
    }

    public async Task<WeaviateResult> Hybrid<TVector>(
        string? query,
        TVector vectors,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
        where TVector : class, IHybridVectorInput
    {
        return (
            await _client.GrpcClient.SearchHybrid(
                _collectionClient.Name,
                query: query,
                alpha: alpha,
                vector: vectors as Vectors,
                nearVector: vectors as HybridNearVector,
                nearText: vectors as HybridNearText,
                queryProperties: queryProperties,
                fusionType: fusionType,
                maxVectorDistance: maxVectorDistance,
                limit: limit,
                offset: offset,
                bm25Operator: bm25Operator,
                autoLimit: autoLimit,
                filters: filters,
                rerank: rerank,
                targetVector: targetVector,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).result;
    }

    public async Task<GroupByResult> Hybrid<TVector>(
        string? query,
        Models.GroupByRequest groupBy,
        float? alpha = null,
        TVector? vectors = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
        where TVector : class, IHybridVectorInput
    {
        return (
            await _client.GrpcClient.SearchHybrid(
                _collectionClient.Name,
                query: query,
                alpha: alpha,
                vector: vectors as Vectors,
                nearVector: vectors as HybridNearVector,
                nearText: vectors as HybridNearText,
                queryProperties: queryProperties,
                fusionType: fusionType,
                maxVectorDistance: maxVectorDistance,
                limit: limit,
                offset: offset,
                bm25Operator: bm25Operator,
                autoLimit: autoLimit,
                filters: filters,
                groupBy: groupBy,
                rerank: rerank,
                targetVector: targetVector,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).group;
    }

    public async Task<GroupByResult> Hybrid(
        string? query,
        Models.GroupByRequest groupBy,
        float? alpha = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        return (
            await _client.GrpcClient.SearchHybrid(
                _collectionClient.Name,
                query: query,
                alpha: alpha,
                queryProperties: queryProperties,
                fusionType: fusionType,
                maxVectorDistance: maxVectorDistance,
                limit: limit,
                offset: offset,
                bm25Operator: bm25Operator,
                autoLimit: autoLimit,
                filters: filters,
                groupBy: groupBy,
                rerank: rerank,
                targetVector: targetVector,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel
            )
        ).group;
    }

    public async Task<WeaviateResult> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearObject(
            _collectionClient.Name,
            objectID: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: null,
            rerank: rerank,
            targetVector: targetVector,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel
        );

        return result.result;
    }

    public async Task<GroupByResult> NearObject(
        Guid nearObject,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearObject(
            _collectionClient.Name,
            objectID: nearObject,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            targetVector: targetVector,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel
        );

        return result.group;
    }

    public async Task<WeaviateResult> NearImage(
        byte[] nearImage,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await NearMedia(
            media: nearImage,
            mediaType: NearMediaType.Image,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targetVector: targetVector,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            tenant: tenant ?? _collectionClient.Tenant
        );

        return result;
    }

    public async Task<GroupByResult> NearImage(
        byte[] nearImage,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await NearMedia(
            media: nearImage,
            mediaType: NearMediaType.Image,
            certainty: certainty,
            distance: distance,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            groupBy: groupBy,
            rerank: rerank,
            targetVector: targetVector,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            tenant: tenant ?? _collectionClient.Tenant
        );

        return result;
    }

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
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearMedia(
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
            tenant: tenant ?? _collectionClient.Tenant,
            targetVector: targetVector,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            consistencyLevel: _collectionClient.ConsistencyLevel
        );

        return result.result;
    }

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
        string[]? targetVector = null,
        string? tenant = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearMedia(
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
            tenant: tenant ?? _collectionClient.Tenant,
            targetVector: targetVector,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            consistencyLevel: _collectionClient.ConsistencyLevel
        );

        return result.group;
    }

    #endregion
}
