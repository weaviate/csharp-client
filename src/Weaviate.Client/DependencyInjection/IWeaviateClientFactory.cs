namespace Weaviate.Client.DependencyInjection;

/// <summary>
/// Factory for creating and managing multiple named Weaviate clients.
/// </summary>
public interface IWeaviateClientFactory
{
    /// <summary>
    /// Gets or creates a Weaviate client with the specified name.
    /// </summary>
    /// <param name="name">The logical name of the client to create.</param>
    /// <returns>A WeaviateClient instance.</returns>
    WeaviateClient GetClient(string name);

    /// <summary>
    /// Gets or creates a Weaviate client asynchronously with the specified name.
    /// Ensures the client is fully initialized before returning.
    /// </summary>
    /// <param name="name">The logical name of the client to create.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A fully initialized WeaviateClient instance.</returns>
    Task<WeaviateClient> GetClientAsync(string name, CancellationToken cancellationToken = default);
}
