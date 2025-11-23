using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Weaviate.Client;
using Weaviate.Client.DependencyInjection;

namespace Example;

/// <summary>
/// Example demonstrating how to use Weaviate with dependency injection.
/// </summary>
public class DependencyInjectionExample
{
    public static async Task Main(string[] args)
    {
        // Build host with dependency injection
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register Weaviate client
                services.AddWeaviateLocal(
                    hostname: "localhost",
                    restPort: 8080,
                    grpcPort: 50051,
                    eagerInitialization: true // Client initializes on startup
                );

                // Register your services that use Weaviate
                services.AddSingleton<CatService>();
            })
            .Build();

        // Run the host - this triggers eager initialization
        await host.StartAsync();

        // Get the service and use it
        var catService = host.Services.GetRequiredService<CatService>();

        Console.WriteLine("=== Dependency Injection Example ===\n");

        // The client is already initialized and ready to use!
        await catService.DemonstrateUsageAsync();

        await host.StopAsync();
    }
}

/// <summary>
/// Example service that uses Weaviate via dependency injection.
/// </summary>
public class CatService
{
    private readonly WeaviateClient _weaviate;
    private readonly ILogger<CatService> _logger;

    public CatService(WeaviateClient weaviate, ILogger<CatService> logger)
    {
        _weaviate = weaviate;
        _logger = logger;

        // Client is already initialized!
        _logger.LogInformation(
            "CatService created. Weaviate version: {Version}",
            _weaviate.WeaviateVersion);
    }

    public async Task DemonstrateUsageAsync()
    {
        // Check if client is initialized
        _logger.LogInformation("Client initialized: {IsInitialized}", _weaviate.IsInitialized);

        // Create or get collection
        var collection = _weaviate.Collections.Use<Cat>("Cat");

        try
        {
            // Check if collection exists
            var config = await collection.Config.Get();
            _logger.LogInformation("Collection 'Cat' already exists");
        }
        catch
        {
            // Create collection
            _logger.LogInformation("Creating Cat collection...");
            await _weaviate.Collections.Create<Cat>(new Weaviate.Client.Models.CollectionConfig
            {
                Name = "Cat",
                Description = "Example cat collection for DI demo",
                Properties = Weaviate.Client.Models.Property.FromClass<Cat>(),
                VectorConfig = new Weaviate.Client.Models.VectorConfig(
                    "default",
                    new Weaviate.Client.Models.Vectorizer.Text2VecWeaviate())
            });
        }

        // Insert a cat
        _logger.LogInformation("Inserting a cat...");
        var catId = await collection.Data.Insert(new Cat
        {
            Name = "Fluffy",
            Breed = "Persian",
            Color = "white",
            Counter = 1
        });

        _logger.LogInformation("Inserted cat with ID: {Id}", catId);

        // Query cats
        _logger.LogInformation("Querying cats...");
        var results = await collection.Query.FetchObjects(limit: 10);

        _logger.LogInformation("Found {Count} cats", results.Objects.Count());

        foreach (var obj in results.Objects)
        {
            var cat = obj.As<Cat>();
            _logger.LogInformation("  - {Name} ({Breed}, {Color})", cat?.Name, cat?.Breed, cat?.Color);
        }

        // Cleanup
        _logger.LogInformation("Cleaning up...");
        await collection.Delete();
    }
}

/// <summary>
/// Alternative example using configuration from appsettings.json
/// </summary>
public class ConfigurationExample
{
    public static async Task RunAsync()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register from configuration section
                services.AddWeaviate(
                    context.Configuration.GetSection("Weaviate"),
                    eagerInitialization: true
                );

                services.AddSingleton<CatService>();
            })
            .Build();

        await host.StartAsync();

        var catService = host.Services.GetRequiredService<CatService>();
        await catService.DemonstrateUsageAsync();

        await host.StopAsync();
    }
}

/// <summary>
/// Example using lazy initialization
/// </summary>
public class LazyInitializationExample
{
    public static async Task RunAsync()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Lazy initialization - client initializes on first use
                services.AddWeaviateLocal(eagerInitialization: false);
            })
            .Build();

        await host.StartAsync();

        var client = host.Services.GetRequiredService<WeaviateClient>();

        Console.WriteLine($"Is initialized: {client.IsInitialized}"); // False

        // Manually trigger initialization
        await client.InitializeAsync();

        Console.WriteLine($"Is initialized: {client.IsInitialized}"); // True
        Console.WriteLine($"Weaviate version: {client.WeaviateVersion}");

        await host.StopAsync();
    }
}

/// <summary>
/// Example using Connect helpers (backward compatible)
/// </summary>
public class ConnectHelperExample
{
    public static async Task RunAsync()
    {
        // These still work! Fully async, no blocking
        var client = await Connect.Local();

        Console.WriteLine($"Connected to Weaviate {client.WeaviateVersion}");

        var collection = client.Collections.Use<Cat>("Cat");

        // Use the client...
        var results = await collection.Query.FetchObjects(limit: 10);

        Console.WriteLine($"Found {results.Objects.Count()} cats");

        client.Dispose();
    }
}
