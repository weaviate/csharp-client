namespace Weaviate.Client;

using System.Text.Json;
using Weaviate.Client.Grpc;
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

    private WeaviateObject<T> buildWeaviateObject<T>(Rest.Dto.WeaviateObject data)
    {
        return new WeaviateObject<T>(this)
        {
            Data = buildConcreteTypeObjectFromProperties<T>(data.Properties),
            Vector = data.Vector ?? WeaviateObject<T>.EmptyVector(),
            ID = data.Id,
            Additional = data.Additional,
            CreationTime = data.CreationTimeUnix.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.CreationTimeUnix.Value).DateTime : null,
            LastUpdateTime = data.LastUpdateTimeUnix.HasValue ? DateTimeOffset.FromUnixTimeMilliseconds(data.LastUpdateTimeUnix.Value).DateTime : null,
            Tenant = data.Tenant,
            VectorWeights = data.VectorWeights,
            Vectors = data.Vectors,
        };
    }


    private static T? buildConcreteTypeObjectFromProperties<T>(object? data)
    {
        T? props = default;

        switch (data)
        {
            case JsonElement properties:
                props = properties.Deserialize<T>(_defaultJsonSerializationOptions);
                break;
            case IDictionary<string, object?> dict:
                // TODO: Find a better way for this conversion
                props = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(dict), _defaultJsonSerializationOptions);
                break;
            case null:
                return props;
            default:
                throw new NotSupportedException($"Unsupported type for properties: {data?.GetType()}");
        }

        return props;
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
        var dto = new Rest.Dto.WeaviateObject()
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

    public async Task DeleteObject(Guid id)
    {
        await _client.RestClient.DeleteObject(_collectionName, id);
    }

    public async IAsyncEnumerable<WeaviateObject<T>> ListObjects<T>(uint? limit = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit);

        foreach (var data in list)
        {
            yield return buildWeaviateObject<T>(data);
        }
    }

    public async Task<WeaviateObject<T>?> FetchObjectByID<T>(Guid id)
    {
        var reply = await _client.GrpcClient.FetchObjects(_collectionName, Filter.WithID(id));

        var data = reply.FirstOrDefault();

        if (data is null)
        {
            return null;
        }

        return buildWeaviateObject<T>(data);
    }

    public async IAsyncEnumerable<WeaviateObject<T>> FetchObjectsByIDs<T>(ISet<Guid> ids, uint? limit = null)
    {
        var list = await _client.GrpcClient.FetchObjects(_collectionName, limit: limit, filter: Filter.WithIDs(ids));

        foreach (var data in list)
        {
            yield return buildWeaviateObject<T>(data);
        }
    }
}
