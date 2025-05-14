using System.Dynamic;
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

    public static IDictionary<string, string>[] MakeBeacons(params Guid[] guids)
    {
        return [
            .. guids.Select(uuid => new Dictionary<string, string> { { "beacon", $"weaviate://localhost/{uuid}" } })
        ];
    }

    public async Task<Guid> Insert(WeaviateObject<TData> data, Dictionary<string, Guid>? references = null)
    {
        ExpandoObject obj = new ExpandoObject();

        if (obj is IDictionary<string, object> propDict)
        {
            if (references is not null)
            {
                foreach (var kvp in references)
                {
                    propDict[kvp.Key] = MakeBeacons(kvp.Value);
                }
            }

            foreach (var property in data.Data?.GetType().GetProperties() ?? [])
            {
                if (!property.CanRead)
                {
                    continue;
                }

                object? value = property.GetValue(data.Data);

                if (value is null)
                {
                    continue;
                }

                propDict[property.Name] = value;
            }
        }

        var dto = new Rest.Dto.WeaviateObject()
        {
            ID = data.ID ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = obj,
            Vector = data.Vector?.Count == 0 ? null : data.Vector,
            Vectors = data.Vectors?.Count == 0 ? null : data.Vectors,
            Additional = data.Additional,
            CreationTimeUnix = data.Metadata.CreationTime.HasValue ? new DateTimeOffset(data.Metadata.CreationTime.Value).ToUnixTimeMilliseconds() : null,
            LastUpdateTimeUnix = data.Metadata.LastUpdateTime.HasValue ? new DateTimeOffset(data.Metadata.LastUpdateTime.Value).ToUnixTimeMilliseconds() : null,
            Tenant = data.Tenant
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        return response.ID!.Value;
    }

    public async Task Delete(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id);
    }

    public async Task ReferenceAdd(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceAdd(_collectionName, from, fromProperty, to);
    }

    public async Task ReferenceReplace(Guid from, string fromProperty, Guid[] to)
    {
        await _client.RestClient.ReferenceReplace(_collectionName, from, fromProperty, to);
    }

    public async Task ReferenceDelete(Guid from, string fromProperty, Guid to)
    {
        await _client.RestClient.ReferenceDelete(_collectionName, from, fromProperty, to);
    }
}