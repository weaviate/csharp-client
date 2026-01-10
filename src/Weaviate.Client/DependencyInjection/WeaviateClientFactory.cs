using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Weaviate.Client.DependencyInjection;

/// <summary>
/// Factory for creating and managing multiple named Weaviate clients.
/// Clients are created lazily and cached for the lifetime of the factory.
/// </summary>
internal class WeaviateClientFactory : IWeaviateClientFactory, IDisposable
{
    /// <summary>
    /// The options monitor
    /// </summary>
    private readonly IOptionsMonitor<WeaviateOptions> _optionsMonitor;

    /// <summary>
    /// The logger factory
    /// </summary>
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// The clients
    /// </summary>
    private readonly ConcurrentDictionary<string, Lazy<Task<WeaviateClient>>> _clients = new();

    /// <summary>
    /// The disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateClientFactory"/> class
    /// </summary>
    /// <param name="optionsMonitor">The options monitor</param>
    /// <param name="loggerFactory">The logger factory</param>
    public WeaviateClientFactory(
        IOptionsMonitor<WeaviateOptions> optionsMonitor,
        ILoggerFactory loggerFactory
    )
    {
        _optionsMonitor = optionsMonitor;
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Gets or creates a client synchronously.
    /// If the client hasn't been initialized yet, this will block until initialization completes.
    /// Consider using GetClientAsync() for better async behavior.
    /// </summary>
    public WeaviateClient GetClient(string name)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WeaviateClientFactory));

        var lazyClient = _clients.GetOrAdd(
            name,
            n => new Lazy<Task<WeaviateClient>>(() => CreateClientAsync(n))
        );

        // This will block if the client is still initializing
        // For non-blocking behavior, use GetClientAsync()
        return lazyClient.Value.GetAwaiter().GetResult();
    }

    /// <summary>
    /// Gets or creates a client asynchronously.
    /// Ensures the client is fully initialized before returning.
    /// </summary>
    public async Task<WeaviateClient> GetClientAsync(
        string name,
        CancellationToken cancellationToken = default
    )
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(WeaviateClientFactory));

        var lazyClient = _clients.GetOrAdd(
            name,
            n => new Lazy<Task<WeaviateClient>>(() => CreateClientAsync(n))
        );

        return await lazyClient.Value;
    }

    /// <summary>
    /// Creates the client using the specified name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The client</returns>
    private async Task<WeaviateClient> CreateClientAsync(string name)
    {
        var options = _optionsMonitor.Get(name);
        var logger = _loggerFactory.CreateLogger<WeaviateClient>();

        var clientOptions = Options.Create(options);
        var client = new WeaviateClient(clientOptions, logger);

        // Initialize the client
        await client.InitializeAsync();

        return client;
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        foreach (var lazyClient in _clients.Values)
        {
            if (lazyClient.IsValueCreated && lazyClient.Value.IsCompletedSuccessfully)
            {
                lazyClient.Value.Result.Dispose();
            }
        }

        _clients.Clear();
        _disposed = true;
    }
}
