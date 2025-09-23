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
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                limit: limit,
                rerank: rerank,
                filters: filters,
                sort: sort,
                groupBy: groupBy,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).group;

    public async Task<WeaviateResult> FetchObjects(
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                limit: limit,
                rerank: rerank,
                filters: filters,
                sort: sort,
                tenant: tenant ?? _collectionClient.Tenant,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).result;

    public async Task<WeaviateObject?> FetchObjectByID(
        Guid id,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                returnProperties: returnProperties,
                filters: Filter.WithID(id),
                returnReferences: returnReferences,
                returnMetadata: returnMetadata,
                tenant: tenant ?? _collectionClient.Tenant
            )
        ).result.SingleOrDefault();

    public async Task<WeaviateResult> FetchObjectsByIDs(
        ISet<Guid> ids,
        uint? limit = null,
        string? tenant = null,
        Rerank? rerank = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                limit: limit,
                filters: filters != null ? Filter.WithIDs(ids) & filters : Filter.WithIDs(ids),
                sort: sort,
                tenant: tenant ?? _collectionClient.Tenant,
                rerank: rerank,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).result;
    #endregion

    #region Search

    public async Task<WeaviateResult> NearText(
        OneOrManyOf<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoCut = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string? tenant = null,
        string[]? targetVector = null,
        OneOrManyOf<string>? returnProperties = null,
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
                offset: offset,
                autoCut: autoCut,
                targetVector: targetVector,
                filters: filters,
                tenant: tenant ?? _collectionClient.Tenant,
                rerank: rerank,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).result;

    public async Task<GroupByResult> NearText(
        OneOrManyOf<string> text,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoCut = null,
        Filter? filters = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
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
                moveTo: moveTo,
                moveAway: moveAway,
                limit: limit,
                offset: offset,
                autoCut: autoCut,
                filters: filters,
                rerank: rerank,
                targetVector: targetVector,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).group;

    public async Task<WeaviateResult> NearVector(
        Vectors vector,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        string[]? targetVector = null,
        string? tenant = null,
        Rerank? rerank = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                distance: distance,
                certainty: certainty,
                offset: offset,
                autoCut: autoCut,
                limit: limit,
                targetVector: targetVector,
                filters: filters,
                tenant: tenant ?? _collectionClient.Tenant,
                rerank: rerank,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).result;

    public async Task<GroupByResult> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        string[]? targetVector = null,
        string? tenant = null,
        Rerank? rerank = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                groupBy,
                filters: filters,
                distance: distance,
                certainty: certainty,
                offset: offset,
                autoCut: autoCut,
                limit: limit,
                targetVector: targetVector,
                tenant: tenant ?? _collectionClient.Tenant,
                rerank: rerank,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                returnMetadata: returnMetadata
            )
        ).group;

    public async Task<GroupByResult> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        return (
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                filters: filters,
                autoCut: autoCut,
                limit: limit,
                offset: offset,
                groupBy: groupBy,
                rerank: rerank,
                after: after,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
                returnMetadata: returnMetadata,
                returnReferences: returnReferences,
                returnProperties: returnProperties
            )
        ).group;
    }

    public async Task<WeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        return (
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                filters: filters,
                autoCut: autoCut,
                limit: limit,
                offset: offset,
                groupBy: null,
                rerank: rerank,
                after: after,
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
                returnMetadata: returnMetadata,
                returnReferences: returnReferences,
                returnProperties: returnProperties
            )
        ).result;
    }

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
        OneOrManyOf<string>? returnProperties = null,
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
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
                tenant: tenant ?? _collectionClient.Tenant,
                consistencyLevel: _collectionClient.ConsistencyLevel,
                returnMetadata: returnMetadata,
                returnProperties: returnProperties,
                returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
            tenant: tenant ?? _collectionClient.Tenant,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
            tenant: tenant ?? _collectionClient.Tenant,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
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
        OneOrManyOf<string>? returnProperties = null,
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
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result.group;
    }

    #endregion
}
