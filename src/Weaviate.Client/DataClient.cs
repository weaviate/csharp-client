using System.Dynamic;
using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client;

public class DataClient<TData>
{
    private readonly CollectionClient<TData> _collectionClient;
    private WeaviateClient _client => _collectionClient.Client;
    private string _collectionName => _collectionClient.Name;

    internal DataClient(CollectionClient<TData> collectionClient)
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

        var additional = Rest.Dto.AdditionalProperties.FromJson(JsonSerializer.Serialize(data.Additional));

        var vector = (C11yVector?)(data.Vector?.Count == 0 ? null : data.Vector);

        var vectors = data.Vectors?.Count == 0 ? null : Rest.Dto.Vectors.FromJson(JsonSerializer.Serialize(data.Vectors));

        var dto = new Rest.Dto.Object()
        {
            Id = data.ID ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = obj,
            Vector = vector,
            Vectors = vectors,
            Additional = additional,
            CreationTimeUnix = data.Metadata.CreationTime.HasValue ? new DateTimeOffset(data.Metadata.CreationTime.Value).ToUnixTimeMilliseconds() : null,
            LastUpdateTimeUnix = data.Metadata.LastUpdateTime.HasValue ? new DateTimeOffset(data.Metadata.LastUpdateTime.Value).ToUnixTimeMilliseconds() : null,
            Tenant = data.Tenant
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        return response.Id!.Value;
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