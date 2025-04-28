using Weaviate.Client.Grpc;
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

        foreach (var data in list.ToObjects<TData>())
        {
            yield return data;
        }
    }
    #endregion

    #region Search

    public async Task<object> NearText(string text, int? limit = null)
    {
        return await Task.FromResult(new object { });
    }

    public async IAsyncEnumerable<Rest.Dto.WeaviateObject> NearVector(float[] vector, float? distance = null, float? certainty = null, uint? limit = null, string[]? fields = null, string[]? metadata = null)
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
            yield return r;
        }
    }

    #endregion
}