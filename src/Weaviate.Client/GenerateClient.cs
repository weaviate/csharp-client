using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public GenerateClient Generate => new(this);
}

public class GenerateClient
{
    private readonly CollectionClient _collectionClient;
    private string _collectionName => _collectionClient.Name;

    private WeaviateClient _client => _collectionClient.Client;

    public GenerateClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    #region Objects
    public async Task<GenerativeGroupByResult> FetchObjects(
        Models.GroupByRequest groupBy,
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        await _client.GrpcClient.FetchObjects(
            _collectionName,
            limit: limit,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            filters: filters,
            sort: sort,
            groupBy: groupBy,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );

    public async Task<GenerativeWeaviateResult?> FetchObjects(
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    ) =>
        await _client.GrpcClient.FetchObjects(
            _collectionName,
            limit: limit,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            filters: filters,
            sort: sort,
            tenant: tenant ?? _collectionClient.Tenant,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );

    public async Task<GenerativeWeaviateResult?> FetchObjectByID(
        Guid id,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.FetchObjects(
            _collectionName,
            returnProperties: returnProperties,
            filters: Filter.WithID(id),
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            tenant: tenant ?? _collectionClient.Tenant,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt
        );
        return result;
    }

    public async Task<GenerativeWeaviateResult> FetchObjectsByIDs(
        HashSet<Guid> ids,
        uint? limit = null,
        string? tenant = null,
        Rerank? rerank = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        if (ids.Count == 0)
            return GenerativeWeaviateResult.Empty;

        Filter idFilter = ids.Count == 1 ? Filter.WithID(ids.First()) : Filter.WithIDs(ids);

        if (filters is not null)
            idFilter = filters & idFilter;

        return await _client.GrpcClient.FetchObjects(
            _collectionName,
            limit: limit,
            filters: idFilter,
            sort: sort,
            tenant: tenant ?? _collectionClient.Tenant,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
    }
    #endregion

    #region Search

    public async Task<GenerativeWeaviateResult> NearText(
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
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            text.ToArray(),
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return result;
    }

    public async Task<GenerativeGroupByResult> NearText(
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
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            text.ToArray(),
            groupBy: groupBy,
            distance: distance,
            certainty: certainty,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoCut: autoCut,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return result;
    }

    public async Task<GenerativeWeaviateResult> NearVector(
        Vectors vector,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearVector(
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return result;
    }

    public async Task<GenerativeGroupByResult> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearVector(
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return result;
    }

    public async Task<GenerativeGroupByResult> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        var result = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoCut: autoCut,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            after: after,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            returnProperties: returnProperties
        );
        return result;
    }

    public async Task<GenerativeWeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filters = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        var result = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filters: filters,
            autoCut: autoCut,
            limit: limit,
            offset: offset,
            groupBy: null,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            after: after,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: consistencyLevel ?? _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            returnProperties: returnProperties
        );

        return result;
    }

    public async Task<GenerativeWeaviateResult> Hybrid(
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
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeGroupByResult> Hybrid(
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
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeWeaviateResult> Hybrid(
        string? query,
        IHybridVectorInput vectors,
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
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeGroupByResult> Hybrid(
        string? query,
        Models.GroupByRequest groupBy,
        float? alpha = null,
        IHybridVectorInput? vectors = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchHybrid(
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeWeaviateResult> NearObject(
        Guid nearObject,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeGroupByResult> NearObject(
        Guid nearObject,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeWeaviateResult> NearImage(
        byte[] nearImage,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
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
            prompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeGroupByResult> NearImage(
        byte[] nearImage,
        GroupByRequest groupBy,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
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
            prompt: prompt,
            groupedPrompt: groupedPrompt,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeWeaviateResult> NearMedia(
        byte[] media,
        NearMediaType mediaType,
        double? certainty = null,
        double? distance = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            tenant: tenant ?? _collectionClient.Tenant,
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    public async Task<GenerativeGroupByResult> NearMedia(
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
        SinglePrompt? prompt = null,
        GroupedPrompt? groupedPrompt = null,
        TargetVectors? targetVector = null,
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
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            tenant: tenant ?? _collectionClient.Tenant,
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return result;
    }

    #endregion
}
