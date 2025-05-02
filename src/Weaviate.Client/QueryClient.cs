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
    public async IAsyncEnumerable<WeaviateObject<TData>> List(uint? limit = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit);

        foreach (var data in list.ToObjects<TData>())
        {
            yield return data;
        }
    }

    public async Task<WeaviateObject<TData>?> FetchObjectByID(Guid id)
    {
        var reply = await _client.GrpcClient.FetchObjects(_collectionName, Filter.WithID(id));

        var data = reply.FirstOrDefault();

        if (data is null)
        {
            return null;
        }

        return data.ToWeaviateObject<TData>();
    }

    public async IAsyncEnumerable<WeaviateObject<TData>> FetchObjectsByIDs(ISet<Guid> ids, uint? limit = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit, filter: Filter.WithIDs(ids));

        foreach (var r in list.Select(x => x.ToWeaviateObject<TData>()))
        {
            yield return r;
        }
    }
    #endregion

    #region Search

    public async IAsyncEnumerable<WeaviateObject<TData>> NearText(string text, float? distance = null, float? certainty = null, uint? limit = null, string[]? fields = null,
                                   string[]? metadata = null, Move? moveTo = null, Move? moveAway = null)
    {
        var results =
            await _client.GrpcClient.SearchNearText(
                _collectionClient.Name,
                text,
                distance: distance,
                certainty: certainty,
                limit: limit,
                moveTo: moveTo,
                moveAway: moveAway
            );

        foreach (var r in results.Select(x => x.ToWeaviateObject<TData>()))
        {
            yield return r;
        }
    }

    public async Task<GroupByResult> NearText(string text, Models.GroupByConstraint groupBy, float? distance = null,
                                   float? certainty = null, uint? limit = null, string[]? fields = null,
                                   string[]? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearText(
                _collectionClient.Name,
                text,
                groupBy,
                distance: distance,
                certainty: certainty,
                limit: limit
            );

        return results;
    }

    public async IAsyncEnumerable<WeaviateObject<TData>> NearVector(float[] vector, float? distance = null, float? certainty = null, uint? limit = null, string[]? fields = null, string[]? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                distance: distance,
                certainty: certainty,
                limit: limit
            );

        foreach (var r in results)
        {
            yield return r.ToWeaviateObject<TData>();
        }
    }

    public async Task<GroupByResult> NearVector(float[] vector, GroupByConstraint groupBy, float? distance = null,
                                   float? certainty = null, uint? limit = null, string[]? fields = null,
                                   string[]? metadata = null)
    {
        var results =
            await _client.GrpcClient.SearchNearVector(
                _collectionClient.Name,
                vector,
                groupBy,
                distance: distance,
                certainty: certainty,
                limit: limit
            );

        return results;
    }

    #endregion
}