using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The query client class
/// </summary>
public partial class QueryClient
{
    /// <summary>
    /// The collection client
    /// </summary>
    private readonly CollectionClient _collectionClient;

    /// <summary>
    /// Gets the value of the  collectionname
    /// </summary>
    private string _collectionName => _collectionClient.Name;

    /// <summary>
    /// Gets the value of the  client
    /// </summary>
    private WeaviateClient _client => _collectionClient.Client;

    /// <summary>
    /// Gets the value of the  grpc
    /// </summary>
    private Grpc.WeaviateGrpcClient _grpc => _client.GrpcClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryClient"/> class
    /// </summary>
    /// <param name="collectionClient">The collection client</param>
    public QueryClient(CollectionClient collectionClient)
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
