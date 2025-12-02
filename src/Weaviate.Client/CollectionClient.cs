using System.Runtime.CompilerServices;
using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class CollectionClient
{
    public const uint ITERATOR_CACHE_SIZE = 100;

    public System.Version? WeaviateVersion => _client.WeaviateVersion;

    private readonly WeaviateClient _client;
    private readonly string _collectionName;
    private readonly string? _pinnedTenant = null;
    private readonly ConsistencyLevels? _pinnedConsistencyLevel = null;

    private DataClient _dataClient;
    private QueryClient _queryClient;

    public string? Tenant => _pinnedTenant ?? string.Empty;
    public ConsistencyLevels? ConsistencyLevel => _pinnedConsistencyLevel;

    public WeaviateClient Client => _client;
    public string Name => _collectionName;

    public DataClient Data => _dataClient;
    public QueryClient Query => _queryClient;

    internal CollectionClient(
        WeaviateClient client,
        CollectionConfig collection,
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

        _dataClient = new DataClient(this);
        _queryClient = new QueryClient(this);
    }

    /// <summary>
    /// Deletes this collection from Weaviate.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    public async Task Delete(CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        await _client.RestClient.CollectionDelete(Name, cancellationToken);
    }

    public async IAsyncEnumerable<WeaviateObject> Iterator(
        Guid? after = null,
        uint cacheSize = ITERATOR_CACHE_SIZE,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        Guid? cursor = after;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            WeaviateResult page = await _client.GrpcClient.FetchObjects(
                Name,
                limit: cacheSize,
                after: cursor,
                returnMetadata: returnMetadata,
                includeVectors: includeVectors,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                cancellationToken: cancellationToken,
                tenant: Tenant
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

    /// <summary>
    /// Returns the total count of objects in this collection.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The total number of objects in the collection.</returns>
    public async Task<ulong> Count(CancellationToken cancellationToken = default)
    {
        var result = await Aggregate.OverAll(
            totalCount: true,
            cancellationToken: cancellationToken
        );
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

    public CollectionConfigClient Config => new(Client, Name);
}
