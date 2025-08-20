using System.Runtime.CompilerServices;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public const uint ITERATOR_CACHE_SIZE = 100;

    public System.Version WeaviateVersion => _client.WeaviateVersion;

    private readonly WeaviateClient _client;

    private readonly string _collectionName;

    protected readonly string? _fixedTenant;

    internal string? Tenant => _fixedTenant ?? string.Empty;

    public WeaviateClient Client => _client;
    public TenantsClient Tenants { get; }

    public string Name => _collectionName;

    internal CollectionClient(WeaviateClient client, Collection collection, string? tenant = null)
        : this(client, collection.Name, tenant)
    {
        ArgumentNullException.ThrowIfNull(collection);
    }

    internal CollectionClient(WeaviateClient client, string name, string? tenant = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        _client = client;
        _collectionName = name;
        _fixedTenant = tenant;

        Tenants = new TenantsClient(this);
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

        if (_fixedTenant is not null)
        {
            throw new InvalidOperationException(
                "This collection client is already bound to a specific tenant."
            );
        }

        return new CollectionClient(_client, _collectionName, tenant);
    }
}

public partial class CollectionClient<TData> : CollectionClient
{
    private DataClient<TData> _dataClient;
    private QueryClient<TData> _queryClient;
    public DataClient<TData> Data => _dataClient;
    public QueryClient<TData> Query => _queryClient;

    internal CollectionClient(WeaviateClient client, Collection collection, string? tenant = null)
        : this(client, collection.Name, tenant) { }

    internal CollectionClient(WeaviateClient client, string name, string? tenant = null)
        : base(client, name, tenant)
    {
        _dataClient = new DataClient<TData>(this);
        _queryClient = new QueryClient<TData>(this);
    }

    public CollectionUpdateBuilder<TData> Config =>
        new CollectionUpdateBuilder<TData>(Client, Name);

    public new CollectionClient<TData> WithTenant(string tenant)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenant);

        if (_fixedTenant is not null)
        {
            throw new InvalidOperationException(
                "This collection client is already bound to a specific tenant."
            );
        }

        return new CollectionClient<TData>(Client, Name, tenant);
    }
}
