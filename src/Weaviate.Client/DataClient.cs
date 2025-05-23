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
        return
        [
            .. guids.Select(uuid => new Dictionary<string, string>
            {
                { "beacon", $"weaviate://localhost/{uuid}" },
            }),
        ];
    }

    internal static IDictionary<string, object?> BuildDynamicObject(object? data)
    {
        var obj = new ExpandoObject();
        var propDict = obj as IDictionary<string, object?>;

        if (data is null)
        {
            return propDict;
        }

        foreach (var propertyInfo in data.GetType().GetProperties())
        {
            if (!propertyInfo.CanRead)
                continue; // skip non-readable properties

            var value = propertyInfo.GetValue(data);

            if (value is null)
            {
                continue;
            }
            else if (propertyInfo.PropertyType.IsNativeType())
            {
                propDict[propertyInfo.Name] = value;
            }
            else
            {
                propDict[propertyInfo.Name] = BuildDynamicObject(value); // recursive call
            }
        }

        return obj;
    }

    public async Task<Guid> Insert(
        TData data,
        Guid? id = null,
        NamedVectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        string? tenant = null
    )
    {
        var propDict = BuildDynamicObject(data);

        foreach (var kvp in references ?? [])
        {
            propDict[kvp.Name] = MakeBeacons(kvp.TargetID);
        }

        var dtoVectors =
            vectors?.Count == 0 ? null : Vectors.FromJson(JsonSerializer.Serialize(vectors));

        var dto = new Rest.Dto.Object()
        {
            Id = id ?? Guid.NewGuid(),
            Class = _collectionName,
            Properties = propDict,
            Vectors = dtoVectors,
            Tenant = tenant,
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
