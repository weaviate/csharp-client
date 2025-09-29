using Weaviate.Client.Models;

namespace Weaviate.Client;

public record GenerativeDebug(string? FullPrompt);

public record GenerativeReply(
    string Result,
    GenerativeDebug? Debug = null,
    object? Metadata = null // TODO: GenerativeMetadata?
);

public record GenerativeResult(IReadOnlyList<GenerativeReply> Values);

public record GenerativeGroupByResult : GroupByResult
{
    internal GenerativeGroupByResult(GroupByResult result, GenerativeResult generative)
        : base(result.Objects, result.Groups)
    {
        Generative = generative;
    }

    public GenerativeResult Generative { get; init; }
};

public record GenerativeWeaviateResult : WeaviateResult
{
    internal GenerativeWeaviateResult(WeaviateResult result, GenerativeResult generative)
    {
        base.Objects = result.Objects;
        Generative = generative;
    }

    public GenerativeResult Generative { get; init; }
}

public class GenerateClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;
    private string _collectionName => _collectionClient.Name;

    private WeaviateClient _client => _collectionClient.Client;

    public GenerateClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    #region Objects
    public async Task<GenerativeGroupByResult> FetchObjects(
        Models.GroupByRequest groupBy,
        uint? limit = null,
        Filter? filter = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
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
            limit: limit,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            filter: filter,
            sort: sort,
            groupBy: groupBy,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return new GenerativeGroupByResult(result.Group, result.Generative!);
    }

    public async Task<GenerativeWeaviateResult> FetchObjects(
        uint? limit = null,
        Filter? filter = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
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
            limit: limit,
            rerank: rerank,
            singlePrompt: prompt,
            groupedPrompt: groupedPrompt,
            filter: filter,
            sort: sort,
            tenant: tenant ?? _collectionClient.Tenant,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return new GenerativeWeaviateResult(result.Result, result.Generative!);
    }

    public async Task<GenerativeWeaviateResult?> FetchObjectByID(
        Guid id,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.FetchObjects(
            _collectionName,
            returnProperties: returnProperties,
            filter: Filter.WithID(id),
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            tenant: tenant ?? _collectionClient.Tenant
        );
        return new GenerativeWeaviateResult(result.Result, result.Generative!);
    }

    public async Task<GenerativeWeaviateResult> FetchObjectsByIDs(
        ISet<Guid> ids,
        uint? limit = null,
        string? tenant = null,
        Rerank? rerank = null,
        Filter? filter = null,
        OneOrManyOf<Sort>? sort = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.FetchObjects(
            _collectionName,
            limit: limit,
            filter: filter != null ? Filter.WithIDs(ids) & filter : Filter.WithIDs(ids),
            sort: sort,
            tenant: tenant ?? _collectionClient.Tenant,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return new GenerativeWeaviateResult(result.Result, result.Generative!);
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
        Filter? filter = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
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
            filters: filter,
            tenant: tenant ?? _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return new GenerativeWeaviateResult(result.Result, result.Generative!);
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
        Filter? filter = null,
        Rerank? rerank = null,
        string[]? targetVector = null,
        string? tenant = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null
    )
    {
        var result = await _client.GrpcClient.SearchNearText(
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
            filters: filter,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return new GenerativeGroupByResult(result.Group, result.Generative!);
    }

    public async Task<GenerativeWeaviateResult> NearVector(
        Vectors vector,
        Filter? filter = null,
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
            filters: filter,
            tenant: tenant ?? _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata
        );
        return new GenerativeWeaviateResult(result.Result, result.Generative!);
    }

    public async Task<GenerativeGroupByResult> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        Filter? filter = null,
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
    )
    {
        var result = await _client.GrpcClient.SearchNearVector(
            _collectionClient.Name,
            vector,
            groupBy,
            filters: filter,
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
        );
        return new GenerativeGroupByResult(result.Group, result.Generative!);
    }

    public async Task<GenerativeGroupByResult> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        Filter? filter = null,
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
        var result = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filter: filter,
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
        );
        return new GenerativeGroupByResult(result.Group, result.Generative!);
    }

    public async Task<GenerativeWeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        Filter? filter = null,
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
        var result = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            filter: filter,
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
        );

        return new GenerativeWeaviateResult(result.Result, result.Generative!);
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
        string[]? targetVector = null,
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
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeWeaviateResult(result.Result, result.Generative!);
    }

    public async Task<GenerativeWeaviateResult> Hybrid<TVector>(
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
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeWeaviateResult(result.Result, result.Generative!);
    }

    public async Task<GenerativeGroupByResult> Hybrid<TVector>(
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
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeGroupByResult(result.Group, result.Generative!);
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
        string[]? targetVector = null,
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
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeGroupByResult(result.Group, result.Generative!);
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
            singlePrompt: null,
            groupedPrompt: null,
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeWeaviateResult(result.Result, result.Generative!);
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
            singlePrompt: null, // TODO
            groupedPrompt: null, // TODO
            targetVector: targetVector,
            tenant: tenant ?? _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeGroupByResult(result.Group, result.Generative!);
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
            singlePrompt: null, // TODO
            groupedPrompt: null, // TODO
            tenant: tenant ?? _collectionClient.Tenant,
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeWeaviateResult(result.Result, result.Generative!);
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
            singlePrompt: null, // TODO
            groupedPrompt: null, // TODO
            tenant: tenant ?? _collectionClient.Tenant,
            targetVector: targetVector,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnMetadata: returnMetadata,
            returnProperties: returnProperties,
            returnReferences: returnReferences
        );

        return new GenerativeGroupByResult(result.Group, result.Generative!);
    }

    #endregion
}
