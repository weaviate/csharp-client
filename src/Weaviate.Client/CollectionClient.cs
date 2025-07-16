using System.Runtime.CompilerServices;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient<TData>
{
    public const uint ITERATOR_CACHE_SIZE = 100;

    public System.Version WeaviateVersion => _client.WeaviateVersion;

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
            return _backingCollection = null;
        }

        _backingCollection = response.ToModel();

        return _backingCollection;
    }

    public CollectionUpdateBuilder<TData> Config =>
        new CollectionUpdateBuilder<TData>(_client, _collectionName);

    public async Task Delete()
    {
        await _client.RestClient.CollectionDelete(_collectionName);

        _backingCollection = null;
    }

    public async IAsyncEnumerable<WeaviateObject> Iterator(
        Guid? after = null,
        uint cacheSize = ITERATOR_CACHE_SIZE,
        MetadataQuery? metadata = null,
        string[]? fields = null,
        IList<QueryReference>? references = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Guid? cursor = after;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var page = await _client.GrpcClient.FetchObjects(
                _collectionName,
                limit: cacheSize,
                metadata: metadata,
                fields: fields,
                reference: references,
                after: cursor
            );

            if (!page.Objects.Any())
            {
                yield break;
            }

            foreach (var c in page.Objects)
            {
                cursor = c.ID;
                yield return c;
            }
        }
    }
}
