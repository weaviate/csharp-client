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
    public async Task<GroupByResult> List(
        Models.GroupByRequest groupBy,
        string[]? properties = null,
        uint? limit = null,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: properties,
                limit: limit,
                sort: sort,
                filter: filter,
                groupBy: groupBy,
                reference: references,
                metadata: metadata
            )
        ).group;

    public async Task<WeaviateResult> List(
        string[]? properties = null,
        uint? limit = null,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: properties,
                limit: limit,
                sort: sort,
                filter: filter,
                reference: references,
                metadata: metadata
            )
        ).result;

    public async Task<WeaviateObject?> FetchObjectByID(
        Guid id,
        string[]? properties = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: properties,
                filter: Filter.WithID(id),
                reference: references,
                metadata: metadata
            )
        ).result.SingleOrDefault();

    public async Task<WeaviateResult> FetchObjectsByIDs(
        ISet<Guid> ids,
        string[]? properties = null,
        uint? limit = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: properties,
                limit: limit,
                filter: Filter.WithIDs(ids),
                reference: references,
                metadata: metadata
            )
        ).result;
    #endregion

    #region Search

    public async Task<WeaviateResult> NearText(
        string text,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null,
        Move? moveTo = null,
        Move? moveAway = null
    ) =>
        (
            await _client.GrpcClient.SearchNearText(
                _collectionClient.Name,
                text,
                distance: distance,
                certainty: certainty,
                limit: limit,
                reference: references,
                fields: fields,
                metadata: metadata,
                moveTo: moveTo,
                moveAway: moveAway
            )
        ).result;

    public async Task<GroupByResult> NearText(
        string text,
        Models.GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearText(
                _collectionClient.Name,
                text,
                groupBy: groupBy,
                distance: distance,
                certainty: certainty,
                limit: limit,
                fields: fields,
                reference: references,
                metadata: metadata
            )
        ).group;

    public async Task<WeaviateResult> NearVector(
        Vectors vector,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null,
        string[]? targetVector = null
    ) =>
        (
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                distance: distance,
                certainty: certainty,
                limit: limit,
                fields: fields,
                reference: references,
                metadata: metadata,
                targetVector: targetVector
            )
        ).result;

    public async Task<GroupByResult> NearVector(
        Vectors vector,
        GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        string[]? targetVector = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                groupBy,
                distance: distance,
                certainty: certainty,
                limit: limit,
                fields: fields,
                targetVector: targetVector,
                reference: references,
                metadata: metadata
            )
        ).group;

    public async Task<GroupByResult> BM25(
        string query,
        GroupByRequest groupBy,
        string[]? searchFields = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                fields: fields,
                groupBy: groupBy,
                reference: references,
                metadata: metadata
            )
        ).group;

    public async Task<WeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    ) =>
        (
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                fields: fields,
                reference: references,
                metadata: metadata
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
        object? rerank = null,
        string[]? targetVector = null,
        MetadataQuery? metadata = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null
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
                metadata: metadata,
                fields: returnProperties,
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
        object? rerank = null,
        string[]? targetVector = null,
        MetadataQuery? metadata = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null
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
                metadata: metadata,
                fields: returnProperties,
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
        object? rerank = null,
        string[]? targetVector = null,
        MetadataQuery? metadata = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null
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
                metadata: metadata,
                fields: returnProperties,
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
        object? rerank = null,
        string[]? targetVector = null,
        MetadataQuery? metadata = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null
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
                metadata: metadata,
                fields: returnProperties,
                returnReferences: returnReferences
            )
        ).group;
    }
    #endregion
}
