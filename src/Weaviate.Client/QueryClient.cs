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
    public async IAsyncEnumerable<WeaviateObject<TData>> List(uint? limit = null, IList<QueryReference>? references = null, MetadataQuery? metadata = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit, reference: references, metadata: metadata);

        foreach (var data in list.ToObjects<TData>())
        {
            yield return data;
        }
    }

    public async Task<WeaviateObject<TData>?> FetchObjectByID(Guid id, IList<QueryReference>? references = null, MetadataQuery? metadata = null)
    {
        var reply = await _client.GrpcClient.FetchObjects(_collectionName, filter: Filter.WithID(id), reference: references, metadata: metadata);

        var data = reply.FirstOrDefault();

        if (data is null)
        {
            return null;
        }

        return data.ToWeaviateObject<TData>();
    }

    public async IAsyncEnumerable<WeaviateObject<TData>> FetchObjectsByIDs(ISet<Guid> ids, uint? limit = null, IList<QueryReference>? references = null, MetadataQuery? metadata = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit, filter: Filter.WithIDs(ids), reference: references, metadata: metadata);

        foreach (var r in list.Select(x => x.ToWeaviateObject<TData>()))
        {
            yield return r;
        }
    }
    #endregion

    #region Search

    public async IAsyncEnumerable<WeaviateObject<TData>> NearText(string text,
                                                                  float? distance = null,
                                                                  float? certainty = null,
                                                                  uint? limit = null,
                                                                  string[]? fields = null,
                                                                  IList<QueryReference>? references = null,
                                                                  MetadataQuery? metadata = null,
                                                                  Move? moveTo = null,
                                                                  Move? moveAway = null)
    {
        var results =
            await _client.GrpcClient.SearchNearText(
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

        foreach (var r in results.Select(x => x.ToWeaviateObject<TData>()))
        {
            yield return r;
        }
    }

    public async Task<GroupByResult> NearText(string text,
                                              Models.GroupByConstraint groupBy,
                                              float? distance = null,
                                              float? certainty = null,
                                              uint? limit = null,
                                              string[]? fields = null,
                                              IList<QueryReference>? references = null,
                                              MetadataQuery? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearText(
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

    public async IAsyncEnumerable<WeaviateObject<TData>> NearVector(float[] vector,
                                                                    float? distance = null,
                                                                    float? certainty = null,
                                                                    uint? limit = null,
                                                                    string[]? fields = null,
                                                                    IList<QueryReference>? references = null,
                                                                    MetadataQuery? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                distance: distance,
                certainty: certainty,
                limit: limit,
                reference: references,
                metadata: metadata
            );

        foreach (var r in results)
        {
            yield return r.ToWeaviateObject<TData>();
        }
    }

    public async Task<GroupByResult> NearVector(float[] vector,
                                                GroupByConstraint groupBy,
                                                float? distance = null,
                                                float? certainty = null,
                                                uint? limit = null,
                                                string[]? fields = null,
                                                IList<QueryReference>? references = null,
                                                MetadataQuery? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearVector(
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

    public async Task<IEnumerable<WeaviateObject<TData>>> BM25(string query,
                                                               string[]? searchFields = null,
                                                               string[]? fields = null,
                                                               IList<QueryReference>? references = null,
                                                               MetadataQuery? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchBM25(
                _collectionClient.Name,
                query: query,
                searchFields: searchFields,
                fields: fields,
                reference: references,
                metadata: metadata
            );

        return results.Select(r => r.ToWeaviateObject<TData>());
    }

    #endregion
}