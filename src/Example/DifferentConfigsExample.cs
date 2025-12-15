using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Weaviate.Client;
using Weaviate.Client.DependencyInjection;

namespace Example;

/// <summary>
/// Example showing how each named client can have completely different configuration.
/// Each client has its own: host, port, credentials, timeouts, SSL settings, etc.
/// </summary>
public class DifferentConfigsExample
{
    public static async Task Run()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices(
                (context, services) =>
                {
                    // Client 1: Production cloud with SSL and API key
                    services.AddWeaviateClient(
                        "production",
                        options =>
                        {
                            options.RestEndpoint = "prod.weaviate.cloud";
                            options.GrpcEndpoint = "grpc-prod.weaviate.cloud";
                            options.RestPort = 443;
                            options.GrpcPort = 443;
                            options.UseSsl = true;
                            options.Credentials = Auth.ApiKey("prod-api-key-here");
                            options.DefaultTimeout = TimeSpan.FromSeconds(60);
                            options.QueryTimeout = TimeSpan.FromSeconds(30);
                            options.Headers = new Dictionary<string, string>
                            {
                                ["X-Environment"] = "production",
                            };
                        }
                    );

                    // Client 2: Local development, no SSL, no auth, longer timeouts
                    services.AddWeaviateClient(
                        "local",
                        options =>
                        {
                            options.RestEndpoint = "localhost";
                            options.GrpcEndpoint = "localhost";
                            options.RestPort = 8080;
                            options.GrpcPort = 50051;
                            options.UseSsl = false;
                            options.Credentials = null; // No auth for local
                            options.DefaultTimeout = TimeSpan.FromSeconds(120); // Longer for debugging
                            options.QueryTimeout = TimeSpan.FromSeconds(300); // Very long for local testing
                        }
                    );

                    // Client 3: Staging with OAuth credentials
                    services.AddWeaviateClient(
                        "staging",
                        options =>
                        {
                            options.RestEndpoint = "staging.weaviate.cloud";
                            options.GrpcEndpoint = "grpc-staging.weaviate.cloud";
                            options.RestPort = 443;
                            options.GrpcPort = 443;
                            options.UseSsl = true;
                            options.Credentials = Auth.ClientCredentials(
                                "staging-client-secret",
                                "weaviate.read",
                                "weaviate.write"
                            );
                            options.DefaultTimeout = TimeSpan.FromSeconds(45);
                        }
                    );

                    // Client 4: Analytics cluster with custom ports and retry policy
                    services.AddWeaviateClient(
                        "analytics",
                        options =>
                        {
                            options.RestEndpoint = "analytics.internal.company.com";
                            options.GrpcEndpoint = "analytics.internal.company.com";
                            options.RestPort = 9090; // Custom port
                            options.GrpcPort = 9091; // Custom port
                            options.UseSsl = true;
                            options.Credentials = Auth.ApiKey("analytics-key");
                            options.QueryTimeout = TimeSpan.FromSeconds(120); // Slow analytics queries
                            options.RetryPolicy = new RetryPolicy
                            {
                                MaxRetries = 5, // More retries for unreliable network
                                InitialDelay = TimeSpan.FromSeconds(2),
                            };
                        }
                    );

                    // Client 5: Legacy system with password auth
                    services.AddWeaviateClient(
                        "legacy",
                        options =>
                        {
                            options.RestEndpoint = "legacy.oldserver.com";
                            options.GrpcEndpoint = "legacy.oldserver.com";
                            options.RestPort = 8081;
                            options.GrpcPort = 50052;
                            options.UseSsl = false; // Old server doesn't support SSL
                            options.Credentials = Auth.ClientPassword(
                                "legacy-username",
                                "legacy-password"
                            );
                            options.InitTimeout = TimeSpan.FromSeconds(10); // Slow to start
                        }
                    );

                    services.AddSingleton<MultiConfigService>();
                }
            )
            .Build();

        await host.StartAsync();

        var service = host.Services.GetRequiredService<MultiConfigService>();
        await service.ShowDifferentConfigsAsync();

        await host.StopAsync();
    }
}

public class MultiConfigService
{
    private readonly IWeaviateClientFactory _factory;

    public MultiConfigService(IWeaviateClientFactory factory)
    {
        _factory = factory;
    }

    public async Task ShowDifferentConfigsAsync()
    {
        Console.WriteLine("=== Different Client Configurations ===\n");

        // Get all clients - each with different configuration
        var prodClient = await _factory.GetClientAsync("production");
        var localClient = await _factory.GetClientAsync("local");
        var stagingClient = await _factory.GetClientAsync("staging");
        var analyticsClient = await _factory.GetClientAsync("analytics");
        var legacyClient = await _factory.GetClientAsync("legacy");

        // Show each client's configuration
        Console.WriteLine($"Production:");
        Console.WriteLine(
            $"  - Endpoint: {prodClient.Configuration.RestAddress}:{prodClient.Configuration.RestPort}"
        );
        Console.WriteLine($"  - SSL: {prodClient.Configuration.UseSsl}");
        Console.WriteLine($"  - Version: {prodClient.WeaviateVersion}");
        Console.WriteLine($"  - Query Timeout: {prodClient.Configuration.QueryTimeout}");
        Console.WriteLine();

        Console.WriteLine($"Local:");
        Console.WriteLine(
            $"  - Endpoint: {localClient.Configuration.RestAddress}:{localClient.Configuration.RestPort}"
        );
        Console.WriteLine($"  - SSL: {localClient.Configuration.UseSsl}");
        Console.WriteLine($"  - Version: {localClient.WeaviateVersion}");
        Console.WriteLine($"  - Query Timeout: {localClient.Configuration.QueryTimeout}");
        Console.WriteLine();

        Console.WriteLine($"Staging:");
        Console.WriteLine(
            $"  - Endpoint: {stagingClient.Configuration.RestAddress}:{stagingClient.Configuration.RestPort}"
        );
        Console.WriteLine($"  - SSL: {stagingClient.Configuration.UseSsl}");
        Console.WriteLine($"  - Version: {stagingClient.WeaviateVersion}");
        Console.WriteLine();

        Console.WriteLine($"Analytics:");
        Console.WriteLine(
            $"  - Endpoint: {analyticsClient.Configuration.RestAddress}:{analyticsClient.Configuration.RestPort}"
        );
        Console.WriteLine(
            $"  - Custom Ports: REST={analyticsClient.Configuration.RestPort}, gRPC={analyticsClient.Configuration.GrpcPort}"
        );
        Console.WriteLine($"  - Version: {analyticsClient.WeaviateVersion}");
        Console.WriteLine();

        Console.WriteLine($"Legacy:");
        Console.WriteLine(
            $"  - Endpoint: {legacyClient.Configuration.RestAddress}:{legacyClient.Configuration.RestPort}"
        );
        Console.WriteLine($"  - SSL: {legacyClient.Configuration.UseSsl}");
        Console.WriteLine($"  - Version: {legacyClient.WeaviateVersion}");

        // Now use them with completely different configurations
        await UseProductionClient(prodClient);
        await UseLocalClient(localClient);
        await UseAnalyticsClient(analyticsClient);
    }

    private async Task UseProductionClient(WeaviateClient client)
    {
        // Production has strict timeouts and requires auth
        var collection = client.Collections.Use<Product>("Product");
        var results = await collection.Query.FetchObjects(limit: 10);
        // This will use 30s query timeout configured above
    }

    private async Task UseLocalClient(WeaviateClient client)
    {
        // Local has no auth and longer timeouts for debugging
        var collection = client.Collections.Use<Product>("Product");
        var results = await collection.Query.FetchObjects(limit: 100);
        // This will use 300s query timeout - perfect for debugging
    }

    private async Task UseAnalyticsClient(WeaviateClient client)
    {
        // Analytics has custom ports and longer timeouts for slow queries
        var collection = client.Collections.Use<Metric>("Metric");
        var results = await collection.Query.FetchObjects(limit: 10000);
        // This will use 120s query timeout and retry 5 times if it fails
    }
}

public class Product
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
}

public class Metric
{
    public string? Name { get; set; }
    public double Value { get; set; }
}
