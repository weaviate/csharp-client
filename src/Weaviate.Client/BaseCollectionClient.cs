namespace Weaviate.Client;

/// <summary>
/// Base class for all clients that operate within the context of a collection.
/// Provides common access to the collection client, Weaviate client, collection name, and tenant information.
/// </summary>
public abstract class BaseCollectionClient
{
    protected readonly CollectionClient _collectionClient;

    /// <summary>
    /// Gets the underlying WeaviateClient instance.
    /// </summary>
    protected WeaviateClient Client => _collectionClient.Client;

    /// <summary>
    /// Gets the collection name.
    /// </summary>
    protected string CollectionName => _collectionClient.Name;

    /// <summary>
    /// Gets the tenant associated with this collection client, if any.
    /// </summary>
    protected string? Tenant => _collectionClient.Tenant;

    /// <summary>
    /// Gets the consistency level associated with this collection client, if any.
    /// </summary>
    protected ConsistencyLevels? ConsistencyLevel => _collectionClient.ConsistencyLevel;

    /// <summary>
    /// Gets the REST client for making REST API calls.
    /// </summary>
    internal Rest.WeaviateRestClient Rest => Client.RestClient;

    /// <summary>
    /// Gets the gRPC client for making gRPC calls.
    /// </summary>
    internal Grpc.WeaviateGrpcClient Grpc => Client.GrpcClient;

    /// <summary>
    /// Initializes a new instance of the BaseCollectionClient class.
    /// </summary>
    /// <param name="collectionClient">The collection client instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when collectionClient is null.</exception>
    protected BaseCollectionClient(CollectionClient collectionClient)
    {
        _collectionClient =
            collectionClient ?? throw new ArgumentNullException(nameof(collectionClient));
    }
}
