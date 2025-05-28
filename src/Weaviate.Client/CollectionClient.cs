using Weaviate.Client.Models;

namespace Weaviate.Client;

public class CollectionClient<TData>
{
    private readonly WeaviateClient _client;
    private DataClient<TData> _dataClient;
    private QueryClient<TData> _queryClient;
    private readonly string _collectionName;

    private Collection? _backingCollection;

    public Collection? Collection => _backingCollection;

    public string Name => Collection?.Name ?? _collectionName;

    public WeaviateClient Client => _client;
    public DataClient<TData> Data => _dataClient;
    public QueryClient<TData> Query => _queryClient;

    internal CollectionClient(WeaviateClient client, Collection collection)
        : this(client, collection.Name)
    {
        _backingCollection = collection;
    }

    internal CollectionClient(WeaviateClient client, string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _client = client;
        _collectionName = _backingCollection?.Name ?? name;
        _backingCollection = _backingCollection ?? new Collection { Name = _collectionName };
        _dataClient = new DataClient<TData>(this);
        _queryClient = new QueryClient<TData>(this);
    }

    public async Task<Collection?> Get()
    {
        var response = await _client.RestClient.CollectionGet(_collectionName);

        if (response is null)
        {
            return null;
        }

        _backingCollection = response.ToModel();

        return _backingCollection;
    }

    public async Task Delete()
    {
        await _client.RestClient.CollectionDelete(_collectionName);

        _backingCollection = null;
    }

    // TODO Move to a Config scope
    internal async Task AddReference(ReferenceProperty referenceProperty)
    {
        var p = (Property)referenceProperty;

        var dto = new Rest.Dto.Property() { Name = p.Name, DataType = [.. p.DataType] };

        await _client.RestClient.CollectionAddProperty(_collectionName, dto);
    }
}
