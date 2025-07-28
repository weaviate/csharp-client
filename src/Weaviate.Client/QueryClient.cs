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
    public async Task<WeaviateResult> List(
        string[]? properties = null,
        uint? limit = null,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        return await _client.GrpcClient.FetchObjects(
            _collectionName,
            fields: properties,
            limit: limit,
            sort: sort,
            filter: filter,
            reference: references,
            metadata: metadata
        );
    }

    public async Task<WeaviateObject?> FetchObjectByID(
        Guid id,
        string[]? properties = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        return (
            await _client.GrpcClient.FetchObjects(
                _collectionName,
                fields: properties,
                filter: Filter.WithID(id),
                reference: references,
                metadata: metadata
            )
        ).SingleOrDefault();
    }

    public async Task<WeaviateResult> FetchObjectsByIDs(
        ISet<Guid> ids,
        string[]? properties = null,
        uint? limit = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        return await _client.GrpcClient.FetchObjects(
            _collectionName,
            fields: properties,
            limit: limit,
            filter: Filter.WithIDs(ids),
            reference: references,
            metadata: metadata
        );
    }
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
    )
    {
        var results = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            text,
            distance: distance,
            certainty: certainty,
            limit: limit,
            reference: references,
            metadata: metadata,
            moveTo: moveTo,
            moveAway: moveAway
        );

        return results;
    }

    public async Task<GroupByResult> NearText(
        string text,
        Models.GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        var results = await _client.GrpcClient.SearchNearText(
            _collectionClient.Name,
            text,
            groupBy,
            distance: distance,
            certainty: certainty,
            limit: limit,
            reference: references,
            metadata: metadata
        );

        return results;
    }

    public async Task<WeaviateResult> NearVector(
        VectorContainer vector,
        float? certainty = null,
        float? distance = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null,
        string? targetVector = null
    )
    {
        var results = await _client.GrpcClient.SearchNearVector(
            _collectionClient.Name,
            vector,
            distance: distance,
            certainty: certainty,
            limit: limit,
            reference: references,
            metadata: metadata,
            targetVector: targetVector
        );

        return results;
    }

    public async Task<GroupByResult> NearVector(
        VectorContainer vector,
        GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        string? targetVector = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        var results = await _client.GrpcClient.SearchNearVector(
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
        );

        return results;
    }

    public async Task<WeaviateResult> BM25(
        string query,
        string[]? searchFields = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        var results = await _client.GrpcClient.SearchBM25(
            _collectionClient.Name,
            query: query,
            searchFields: searchFields,
            fields: fields,
            reference: references,
            metadata: metadata
        );

        return results;
    }

    public async Task<WeaviateResult> Hybrid(
        string? query,
        float? alpha = null,
        VectorContainer? vector = null,
        string[]? queryProperties = null,
        string? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        object? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        object? rerank = null,
        string? targetVector = null,
        bool includeVector = false,
        MetadataQuery? returnMetadata = null,
        string[]? returnProperties = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        var results = await _client.GrpcClient.SearchHybrid(
            _collectionClient.Name,
            query: query,
            alpha: alpha,
            vector: vector,
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
            returnReferences: returnReferences
        );

        return results;
    }

    #endregion
}
