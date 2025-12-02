using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client.DependencyInjection;

/// <summary>
/// Background service that eagerly initializes the Weaviate client during application startup.
/// This ensures the client is ready to use when injected into other services.
/// </summary>
internal class WeaviateInitializationService : IHostedService
{
    private readonly WeaviateClient _client;
    private readonly ILogger<WeaviateInitializationService> _logger;

    public WeaviateInitializationService(
        WeaviateClient client,
        ILogger<WeaviateInitializationService> logger
    )
    {
        _client = client;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing Weaviate client...");

        try
        {
            // Trigger async initialization
            await _client.InitializeAsync(cancellationToken);

            var version = _client.WeaviateVersion;
            _logger.LogInformation(
                "Weaviate client initialized successfully. Server version: {Version}",
                version
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Weaviate client");
            throw; // Fail startup if client can't connect
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Weaviate client initialization service");
        return Task.CompletedTask;
    }
}
