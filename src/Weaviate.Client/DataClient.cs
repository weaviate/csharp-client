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
            ID = data.ID ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = data.Data,
            Vector = data.Vector?.Count == 0 ? null : data.Vector,
            Vectors = data.Vectors?.Count == 0 ? null : data.Vectors,
            Additional = data.Additional,
            CreationTimeUnix = data.CreationTime.HasValue ? new DateTimeOffset(data.CreationTime.Value).ToUnixTimeMilliseconds() : null,
            LastUpdateTimeUnix = data.LastUpdateTime.HasValue ? new DateTimeOffset(data.LastUpdateTime.Value).ToUnixTimeMilliseconds() : null,
            Tenant = data.Tenant
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        return response.ID!.Value;
    }

    public async Task Delete(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id);
    }

}