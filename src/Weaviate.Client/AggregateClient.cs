namespace Weaviate.Client;

/// <summary>
/// The collection client class
/// </summary>
public partial class CollectionClient
{
    /// <summary>
    /// Gets the aggregate client for performing aggregation operations on this collection.
    /// </summary>
    public AggregateClient Aggregate => new(this);
}

/// <summary>
/// Provides aggregate query operations for a Weaviate collection.
/// </summary>
public partial class AggregateClient
{
    /// <summary>
    /// The collection client
    /// </summary>
    private readonly CollectionClient _collectionClient;

    /// <summary>
    /// Gets the value of the  client
    /// </summary>
    private WeaviateClient _client => _collectionClient.Client;

    /// <summary>
    /// Gets the value of the  collectionname
    /// </summary>
    private string _collectionName => _collectionClient.Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateClient"/> class
    /// </summary>
    /// <param name="collectionClient">The collection client</param>
    internal AggregateClient(CollectionClient collectionClient)
    {
        _collectionClient = collectionClient;
    }

    /// <summary>
    /// Creates a cancellation token with query-specific timeout configuration.
    /// Uses QueryTimeout if configured, falls back to DefaultTimeout, then to WeaviateDefaults.QueryTimeout.
    /// </summary>
    private CancellationToken CreateTimeoutCancellationToken(CancellationToken userToken = default)
    {
        var effectiveTimeout =
            _client.Configuration.QueryTimeout
            ?? _client.Configuration.DefaultTimeout
            ?? WeaviateDefaults.QueryTimeout;
        return TimeoutHelper.GetCancellationToken(effectiveTimeout, userToken);
    }
}
