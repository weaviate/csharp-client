using System.Runtime.CompilerServices;
using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Provides client operations for interacting with a specific Weaviate collection.
/// </summary>
public partial class CollectionClient
{
    /// <summary>
    /// The default cache size for collection iterators.
    /// </summary>
    public const uint ITERATOR_CACHE_SIZE = 100;

    /// <summary>
    /// Gets the Weaviate server version.
    /// </summary>
    public Version? WeaviateVersion => _client.WeaviateVersion;

    /// <summary>
    /// The client
    /// </summary>
    private readonly WeaviateClient _client;

    /// <summary>
    /// The collection name
    /// </summary>
    private readonly string _collectionName;

    /// <summary>
    /// The pinned tenant
    /// </summary>
    private readonly string? _pinnedTenant = null;

    /// <summary>
    /// The pinned consistency level
    /// </summary>
    private readonly ConsistencyLevels? _pinnedConsistencyLevel = null;

    /// <summary>
    /// The data client
    /// </summary>
    private DataClient _dataClient;

    /// <summary>
    /// The query client
    /// </summary>
    private QueryClient _queryClient;

    /// <summary>
    /// Gets the tenant name for this collection client instance, or empty string for no tenant.
    /// </summary>
    public string? Tenant => _pinnedTenant ?? string.Empty;

    /// <summary>
    /// Gets the consistency level for this collection client instance.
    /// </summary>
    public ConsistencyLevels? ConsistencyLevel => _pinnedConsistencyLevel;

    /// <summary>
    /// Gets the parent WeaviateClient instance.
    /// </summary>
    public WeaviateClient Client => _client;

    /// <summary>
    /// Gets the name of the collection.
    /// </summary>
    public string Name => _collectionName;

    /// <summary>
    /// Gets the data client for performing data operations on this collection.
    /// </summary>
    public DataClient Data => _dataClient;

    /// <summary>
    /// Gets the query client for performing query operations on this collection.
    /// </summary>
    public QueryClient Query => _queryClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
    /// <param name="collection">The collection</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="consistencyLevel">The consistency level</param>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
    /// <param name="name">The name</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="consistencyLevel">The consistency level</param>
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
    /// Iterates over all objects in the collection asynchronously.
    /// </summary>
    /// <param name="after">UUID to start iteration after (for pagination).</param>
    /// <param name="cacheSize">Maximum number of objects to fetch per request.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="filter">Filter to apply to the objects.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An async enumerable of WeaviateObject instances.</returns>
    public async IAsyncEnumerable<WeaviateObject> Iterator(
        Guid? after = null,
        Filter? filter = null,
        uint cacheSize = ITERATOR_CACHE_SIZE,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        Guid? cursor = after;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var reply = await _client.GrpcClient.FetchObjects(
                Name,
                limit: cacheSize,
                after: cursor,
                filters: filter,
                returnMetadata: returnMetadata,
                includeVectors: includeVectors,
                returnProperties: returnProperties,
                returnReferences: returnReferences,
                cancellationToken: cancellationToken,
                tenant: Tenant
            );

            WeaviateResult page = reply;

            Guid? nextUuid =
                reply.IteratorNextUuid.Length > 0
                    ? ObjectHelper.GuidFromByteString(reply.IteratorNextUuid)
                    : null;

            if (filter is null && !page.Objects.Any())
            {
                yield break;
            }

            foreach (var c in page.Objects)
            {
                cursor = c.UUID;
                yield return c;
            }

            if (filter is not null && nextUuid is not null)
            {
                if (nextUuid != Guid.Empty)
                {
                    cursor = nextUuid;
                }
                else
                {
                    yield break;
                }
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

    /// <summary>
    /// Creates a new CollectionClient instance with the specified tenant.
    /// </summary>
    /// <param name="tenant">The tenant name.</param>
    /// <returns>A new CollectionClient instance configured for the specified tenant.</returns>
    public CollectionClient WithTenant(string tenant)
    {
        ArgumentException.ThrowIfNullOrEmpty(tenant);

        return new CollectionClient(_client, _collectionName, tenant, ConsistencyLevel);
    }

    /// <summary>
    /// Creates a new CollectionClient instance with the specified consistency level.
    /// </summary>
    /// <param name="consistencyLevel">The consistency level to use.</param>
    /// <returns>A new CollectionClient instance configured with the specified consistency level.</returns>
    public CollectionClient WithConsistencyLevel(ConsistencyLevels consistencyLevel)
    {
        ArgumentException.ThrowIfNullOrEmpty(
            consistencyLevel == Weaviate.Client.ConsistencyLevels.Unspecified
                ? null
                : consistencyLevel.ToString()
        );

        return new CollectionClient(_client, _collectionName, Tenant, consistencyLevel);
    }

    /// <summary>
    /// Gets the configuration client for managing collection configuration.
    /// </summary>
    public CollectionConfigClient Config => new(Client, Name);
}
