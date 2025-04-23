using Weaviate.Client.Grpc;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public class DataClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;
    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

    public DataClient(CollectionClient<TData> collectionClient)
    {
        _collectionClient = collectionClient;
    }

    public async Task<Guid> Insert(WeaviateObject<TData> data)
    {
        var dto = new Rest.Dto.WeaviateObject()
        {
            Id = Guid.NewGuid(),
            Class = data.CollectionName,
            Properties = data.Data,
            Vector = data.Vector.Count == 0 ? null : data.Vector,
            Vectors = data.Vectors.Count == 0 ? null : data.Vectors,
            Additional = data.Additional,
            CreationTimeUnix = data.CreationTime.HasValue ? new DateTimeOffset(data.CreationTime.Value).ToUnixTimeMilliseconds() : null,
            LastUpdateTimeUnix = data.LastUpdateTime.HasValue ? new DateTimeOffset(data.LastUpdateTime.Value).ToUnixTimeMilliseconds() : null,
            Tenant = data.Tenant
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        return response.Id!.Value;
    }

    public async Task Delete(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id);
    }

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

        return data.BuildWeaviateObject<TData>();
    }

    public async IAsyncEnumerable<WeaviateObject<TData>> FetchObjectsByIDs(ISet<Guid> ids, uint? limit = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit, filter: Filter.WithIDs(ids));

        foreach (var data in list.ToObjects<TData>())
        {
            yield return data;
        }
    }
}