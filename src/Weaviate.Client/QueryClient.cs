using Weaviate.Client.Grpc;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;

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
        uint? limit = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        return await _client.GrpcClient.FetchObjects(
            _collectionName,
            limit: limit,
            reference: references,
            metadata: metadata
        );
    }

    public async Task<WeaviateResult> FetchObjectByID(
        Guid id,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        return await _client.GrpcClient.FetchObjects(
            _collectionName,
            filter: Filter.WithID(id),
            reference: references,
            metadata: metadata
        );
    }

    public async Task<WeaviateResult> FetchObjectsByIDs(
        ISet<Guid> ids,
        uint? limit = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        return await _client.GrpcClient.FetchObjects(
            _collectionName,
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
        float[] vector,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        MetadataQuery? metadata = null
    )
    {
        var results = await _client.GrpcClient.SearchNearVector(
            _collectionClient.Name,
            vector,
            distance: distance,
            certainty: certainty,
            limit: limit,
            reference: references,
            metadata: metadata
        );

        return results;
    }

    public async Task<GroupByResult> NearVector(
        float[] vector,
        GroupByRequest groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        string[]? fields = null,
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

    #endregion
}
