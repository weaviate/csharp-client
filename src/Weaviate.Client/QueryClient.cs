using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
    private readonly CollectionClient _collectionClient;
    private string _collectionName => _collectionClient.Name;
    private WeaviateClient _client => _collectionClient.Client;
    private Grpc.WeaviateGrpcClient _grpc => _client.GrpcClient;

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
            _client.QueryTimeout ?? _client.DefaultTimeout ?? WeaviateDefaults.QueryTimeout;
        return TimeoutHelper.GetCancellationToken(effectiveTimeout, userToken);
    }
}
