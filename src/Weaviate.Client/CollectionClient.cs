namespace Weaviate.Client;

using System.Text.Json;
using Weaviate.Client.Models;

public class CollectionClient
{
    private readonly WeaviateClient _client;
    private readonly string _collectionName;
    private Collection? _collection;

    public string Name => _collection?.Name ?? _collectionName;

    private static readonly JsonSerializerOptions _defaultJsonSerializationOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true, // Case-insensitive property matching
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase, // Convert JSON names to PascalCase (C# convention)
        WriteIndented = true, // For readability
    };


    internal CollectionClient(WeaviateClient client, Collection collection)
        : this(client, collection.Name)
    {
        _collection = collection;
    }

    internal CollectionClient(WeaviateClient client, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _client = client;
        _collectionName = name;
    }

    public async Task Delete()
    {
        await _client.Collections.Delete(_collectionName);

        _collection = null;
    }

    public async Task<CollectionClient> Get()
    {
        var response = await _client.RestClient.CollectionGet(_collectionName);

        _collection = response.ToCollection();

        return new CollectionClient(_client, _collection);
    }

    // TODO Maybe move to an ObjectClient scope?
    public async Task<WeaviateObject<T>> Insert<T>(WeaviateObject<T> data)
    {
        var dto = new Rest.Models.WeaviateObject()
        {
            Id = Guid.NewGuid(),
            Class = data.CollectionName,
            Vector = data.Vector.Count == 0 ? null : data.Vector,
            Properties = data.Data
        };

        var response = await _client.RestClient.ObjectInsert(_collectionName, dto);

        T? props = default;

        if (response.Properties is JsonElement properties)
        {
            props = properties.Deserialize<T>(_defaultJsonSerializationOptions);
        }

        return new WeaviateObject<T>(this)
        {
            Data = props,
        };
    }

    public async IAsyncEnumerable<WeaviateObject<T>> GetObjects<T>(int? limit = null)
    {
        var response = await _client.RestClient.ObjectList(_collectionName, limit: limit);

        foreach (var item in response)
        {
            T? props = default;

            if (item.Properties is JsonElement properties)
            {
                props = properties.Deserialize<T>(_defaultJsonSerializationOptions);

                yield return new WeaviateObject<T>(this)
                {
                    Data = props,
                    Vector = item.Vector ?? WeaviateObject<T>.EmptyVector(),
                    Id = item.Id,
                    Additional = item.Additional,
                    CreationTime = item.CreationTimeUnix == null ? null : DateTimeOffset.FromUnixTimeMilliseconds(item.CreationTimeUnix.Value).DateTime,
                    LastUpdateTime = item.LastUpdateTimeUnix == null ? null : DateTimeOffset.FromUnixTimeMilliseconds(item.LastUpdateTimeUnix.Value).DateTime,
                    Tenant = item.Tenant,
                    VectorWeights = item.VectorWeights,
                    Vectors = item.Vectors,
                };
            }
        }
    }

    public async Task DeleteObject(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id);
    }
}
