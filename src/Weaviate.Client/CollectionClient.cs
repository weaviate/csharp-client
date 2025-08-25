using System.Runtime.CompilerServices;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public const uint ITERATOR_CACHE_SIZE = 100;

    public System.Version WeaviateVersion => _client.WeaviateVersion;

    private readonly WeaviateClient _client;

    private readonly string _collectionName;

    private readonly string? _pinnedTenant = null;
    private readonly ConsistencyLevels? _pinnedConsistencyLevel = null;

    public string? Tenant => _pinnedTenant ?? string.Empty;
    public ConsistencyLevels? ConsistencyLevel =>
        _pinnedConsistencyLevel ?? ConsistencyLevels.Unspecified;

    public WeaviateClient Client => _client;

    public string Name => _collectionName;

    internal CollectionClient(
        WeaviateClient client,
        Collection collection,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null
    )
        : this(client, collection.Name, tenant, consistencyLevel)
    {
        ArgumentNullException.ThrowIfNull(collection);
    }

    internal CollectionClient(
        WeaviateClient client,
        string name,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _client = client;
        _collectionName = name;
        _pinnedTenant = tenant;
        _pinnedConsistencyLevel = consistencyLevel;
    }

    public async Task<Collection?> Get()
    {
        var response = await _client.RestClient.CollectionGet(Name);

        return response?.ToModel();
    }

    public async Task Delete()
    {
        await _client.RestClient.CollectionDelete(Name);
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

            var page = (
                await _client.GrpcClient.FetchObjects(
                    Name,
                    limit: cacheSize,
                    metadata: metadata,
                    fields: fields,
                    reference: references,
                    after: cursor
                )
            ).result;

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

    public async Task<ulong> Count()
    {
        var result = await Aggregate.OverAll(totalCount: true);
        return Convert.ToUInt64(result.TotalCount);
    }

    public CollectionClient WithTenant(string tenant)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenant);

        return new CollectionClient(_client, _collectionName, tenant, ConsistencyLevel);
    }

    public CollectionClient WithConsistencyLevel(ConsistencyLevels consistencyLevel)
    {
        ArgumentException.ThrowIfNullOrEmpty(
            consistencyLevel == Weaviate.Client.ConsistencyLevels.Unspecified
                ? null
                : consistencyLevel.ToString()
        );

        return new CollectionClient(_client, _collectionName, Tenant, consistencyLevel);
    }
}

public partial class CollectionClient<TData> : CollectionClient
{
    private DataClient<TData> _dataClient;
    private QueryClient<TData> _queryClient;
    public DataClient<TData> Data => _dataClient;
    public QueryClient<TData> Query => _queryClient;

    internal CollectionClient(
        WeaviateClient client,
        Collection collection,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null
    )
        : this(client, collection.Name, tenant, consistencyLevel) { }

    internal CollectionClient(
        WeaviateClient client,
        string name,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null
    )
        : base(client, name, tenant, consistencyLevel)
    {
        _dataClient = new DataClient<TData>(this);
        _queryClient = new QueryClient<TData>(this);
    }

    public CollectionUpdateBuilder<TData> Config => new(Client, Name);

    public new CollectionClient<TData> WithTenant(string tenant)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenant);

        return new CollectionClient<TData>(Client, Name, tenant, ConsistencyLevel);
    }

    public new CollectionClient WithConsistencyLevel(ConsistencyLevels consistencyLevel)
    {
        ArgumentException.ThrowIfNullOrEmpty(
            consistencyLevel == Weaviate.Client.ConsistencyLevels.Unspecified
                ? null
                : consistencyLevel.ToString()
        );

        return new CollectionClient(Client, Name, Tenant, consistencyLevel);
    }
}
