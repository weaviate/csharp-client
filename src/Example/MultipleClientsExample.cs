using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weaviate.Client;
using Weaviate.Client.DependencyInjection;

namespace Example;

/// <summary>
/// Example demonstrating how to use multiple Weaviate clients via dependency injection.
/// </summary>
public class MultipleClientsExample
{
    /// <summary>
    /// Runs
    /// </summary>
    public static async Task Run()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (context, services) =>
                {
                    // Register multiple named Weaviate clients
                    services.AddWeaviateClient(
                        "production",
                        options =>
                        {
                            options.RestEndpoint = "prod.weaviate.cloud";
                            options.GrpcEndpoint = "grpc-prod.weaviate.cloud";
                            options.RestPort = 443;
                            options.GrpcPort = 443;
                            options.UseSsl = true;
                            options.Credentials = Auth.ApiKey("prod-api-key");
                        }
                    );

                    services.AddWeaviateClient(
                        "staging",
                        options =>
                        {
                            options.RestEndpoint = "staging.weaviate.cloud";
                            options.GrpcEndpoint = "grpc-staging.weaviate.cloud";
                            options.RestPort = 443;
                            options.GrpcPort = 443;
                            options.UseSsl = true;
                            options.Credentials = Auth.ApiKey("staging-api-key");
                        }
                    );

                    services.AddWeaviateLocal("local", "localhost", 8080, 50051);

                    // Or use helper methods
                    services.AddWeaviateCloud(
                        "analytics",
                        "analytics.weaviate.cloud",
                        "analytics-key"
                    );

                    // Register services that use multiple clients
                    services.AddSingleton<MultiDatabaseService>();
                }
            )
            .Build();

        await host.StartAsync();

        var service = host.Services.GetRequiredService<MultiDatabaseService>();
        await service.DemonstrateMultipleClientsAsync();

        await host.StopAsync();
    }
}

/// <summary>
/// Service that uses multiple Weaviate clients simultaneously.
/// </summary>
public class MultiDatabaseService
{
    /// <summary>
    /// The client factory
    /// </summary>
    private readonly IWeaviateClientFactory _clientFactory;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<MultiDatabaseService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiDatabaseService"/> class
    /// </summary>
    /// <param name="clientFactory">The client factory</param>
    /// <param name="logger">The logger</param>
    public MultiDatabaseService(
        IWeaviateClientFactory clientFactory,
        ILogger<MultiDatabaseService> logger
    )
    {
        _clientFactory = clientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Demonstrates the multiple clients
    /// </summary>
    public async Task DemonstrateMultipleClientsAsync()
    {
        _logger.LogInformation("=== Multiple Weaviate Clients Example ===\n");

        // Get different clients by name
        var prodClient = await _clientFactory.GetClientAsync("production");
        var stagingClient = await _clientFactory.GetClientAsync("staging");
        var localClient = await _clientFactory.GetClientAsync("local");

        _logger.LogInformation("Production client version: {Version}", prodClient.WeaviateVersion);
        _logger.LogInformation("Staging client version: {Version}", stagingClient.WeaviateVersion);
        _logger.LogInformation("Local client version: {Version}", localClient.WeaviateVersion);

        // Use different clients for different purposes
        await SyncDataBetweenEnvironmentsAsync(prodClient, stagingClient);
        await TestLocallyAsync(localClient);
    }

    /// <summary>
    /// Syncs the data between environments using the specified prod client
    /// </summary>
    /// <param name="prodClient">The prod client</param>
    /// <param name="stagingClient">The staging client</param>
    private async Task SyncDataBetweenEnvironmentsAsync(
        WeaviateClient prodClient,
        WeaviateClient stagingClient
    )
    {
        _logger.LogInformation("\nSyncing data from production to staging...");

        var prodCollection = await prodClient.Collections.Use<Cat>("Cat").ValidateTypeOrThrow();
        var stagingCollection = await stagingClient
            .Collections.Use<Cat>("Cat")
            .ValidateTypeOrThrow();

        // Fetch from production
        var prodResults = await prodCollection.Query.FetchObjects(limit: 100);
        _logger.LogInformation("Found {Count} cats in production", prodResults.Objects.Count());

        // Insert into staging
        var cats = prodResults.Objects.Select(o => o.Object!);
        foreach (var cat in cats)
        {
            await stagingCollection.Data.Insert(cat);
        }

        _logger.LogInformation("Synced to staging environment");
    }

    /// <summary>
    /// Tests the locally using the specified local client
    /// </summary>
    /// <param name="localClient">The local client</param>
    private async Task TestLocallyAsync(WeaviateClient localClient)
    {
        _logger.LogInformation("\nTesting locally...");

        var localCollection = localClient.Collections.Use<Cat>("Cat");

        // Test queries locally before deploying to production
        var results = await localCollection.Query.FetchObjects(limit: 10);
        _logger.LogInformation("Local test completed: {Count} results", results.Objects.Count());
    }
}

/// <summary>
/// Alternative pattern: Inject factory and get clients on demand.
/// </summary>
public class OnDemandClientService
{
    /// <summary>
    /// The client factory
    /// </summary>
    private readonly IWeaviateClientFactory _clientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OnDemandClientService"/> class
    /// </summary>
    /// <param name="clientFactory">The client factory</param>
    public OnDemandClientService(IWeaviateClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    /// <summary>
    /// Processes the data from environment using the specified environment
    /// </summary>
    /// <param name="environment">The environment</param>
    public async Task ProcessDataFromEnvironmentAsync(string environment)
    {
        // Get the appropriate client based on runtime logic
        var client = await _clientFactory.GetClientAsync(environment);

        var collection = client.Collections.Use<Cat>("Cat");
        var results = await collection.Query.FetchObjects(limit: 100);

        // Process results...
    }
}

/// <summary>
/// Example with configuration from appsettings.json
/// </summary>
public class ConfigurationBasedMultiClientExample
{
    /// <summary>
    /// Runs
    /// </summary>
    public static async Task RunAsync()
    {
        /*
         * appsettings.json:
         * {
         *   "Weaviate": {
         *     "Production": {
         *       "RestEndpoint": "prod.weaviate.cloud",
         *       "ApiKey": "prod-key"
         *     },
         *     "Staging": {
         *       "RestEndpoint": "staging.weaviate.cloud",
         *       "ApiKey": "staging-key"
         *     }
         *   }
         * }
         */

        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (context, services) =>
                {
                    // Register clients from configuration
                    services.AddWeaviateClient(
                        "production",
                        options =>
                            context.Configuration.GetSection("Weaviate:Production").Bind(options)
                    );

                    services.AddWeaviateClient(
                        "staging",
                        options =>
                            context.Configuration.GetSection("Weaviate:Staging").Bind(options)
                    );
                }
            )
            .Build();

        await host.StartAsync();

        var factory = host.Services.GetRequiredService<IWeaviateClientFactory>();
        var prodClient = await factory.GetClientAsync("production");
        var stagingClient = await factory.GetClientAsync("staging");

        // Use clients...

        await host.StopAsync();
    }
}
