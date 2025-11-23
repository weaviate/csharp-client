using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Weaviate.Client.DependencyInjection;

/// <summary>
/// Extension methods for registering Weaviate services with dependency injection.
/// </summary>
public static class WeaviateServiceCollectionExtensions
{
    /// <summary>
    /// Adds Weaviate client factory for managing multiple named clients.
    /// Use this when you need to connect to multiple Weaviate instances.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateClientFactory(this IServiceCollection services)
    {
        services.AddSingleton<IWeaviateClientFactory, WeaviateClientFactory>();
        return services;
    }

    /// <summary>
    /// Adds a named Weaviate client to the factory.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="configureOptions">Action to configure Weaviate options for this client.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateClient(
        this IServiceCollection services,
        string name,
        Action<WeaviateOptions> configureOptions)
    {
        // Ensure factory is registered
        if (!services.Any(x => x.ServiceType == typeof(IWeaviateClientFactory)))
        {
            services.AddWeaviateClientFactory();
        }

        // Configure options for this named client
        services.Configure(name, configureOptions);

        return services;
    }

    /// <summary>
    /// Adds Weaviate client services to the dependency injection container.
    /// This registers a single default client.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure Weaviate options.</param>
    /// <param name="eagerInitialization">Whether to initialize the client eagerly on application startup. Default is true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviate(
        this IServiceCollection services,
        Action<WeaviateOptions> configureOptions,
        bool eagerInitialization = true)
    {
        services.Configure(configureOptions);
        services.AddSingleton<WeaviateClient>();

        if (eagerInitialization)
        {
            services.AddHostedService<WeaviateInitializationService>();
        }

        return services;
    }

    /// <summary>
    /// Adds Weaviate client services to the dependency injection container using configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration section containing Weaviate settings.</param>
    /// <param name="eagerInitialization">Whether to initialize the client eagerly on application startup. Default is true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviate(
        this IServiceCollection services,
        IConfiguration configuration,
        bool eagerInitialization = true)
    {
        services.Configure<WeaviateOptions>(configuration);
        services.AddSingleton<WeaviateClient>();

        if (eagerInitialization)
        {
            services.AddHostedService<WeaviateInitializationService>();
        }

        return services;
    }

    /// <summary>
    /// Adds Weaviate client services with connection helpers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="hostname">Hostname for local Weaviate instance. Default is "localhost".</param>
    /// <param name="restPort">REST port. Default is 8080.</param>
    /// <param name="grpcPort">gRPC port. Default is 50051.</param>
    /// <param name="useSsl">Whether to use SSL/TLS. Default is false.</param>
    /// <param name="credentials">Authentication credentials.</param>
    /// <param name="eagerInitialization">Whether to initialize the client eagerly on application startup. Default is true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateLocal(
        this IServiceCollection services,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        ICredentials? credentials = null,
        bool eagerInitialization = true)
    {
        services.AddWeaviate(options =>
        {
            options.RestEndpoint = hostname;
            options.GrpcEndpoint = hostname;
            options.RestPort = restPort;
            options.GrpcPort = grpcPort;
            options.UseSsl = useSsl;
            options.Credentials = credentials;
        }, eagerInitialization);

        return services;
    }

    /// <summary>
    /// Adds Weaviate client services configured for Weaviate Cloud.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="clusterEndpoint">The Weaviate Cloud cluster endpoint (e.g., "my-cluster.weaviate.cloud").</param>
    /// <param name="apiKey">API key for authentication.</param>
    /// <param name="eagerInitialization">Whether to initialize the client eagerly on application startup. Default is true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateCloud(
        this IServiceCollection services,
        string clusterEndpoint,
        string? apiKey = null,
        bool eagerInitialization = true)
    {
        services.AddWeaviate(options =>
        {
            options.RestEndpoint = clusterEndpoint;
            options.GrpcEndpoint = $"grpc-{clusterEndpoint}";
            options.RestPort = 443;
            options.GrpcPort = 443;
            options.UseSsl = true;
            options.Credentials = string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey);
        }, eagerInitialization);

        return services;
    }

    // Named client helper methods

    /// <summary>
    /// Adds a named Weaviate client configured for a local instance.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="hostname">Hostname for local Weaviate instance. Default is "localhost".</param>
    /// <param name="restPort">REST port. Default is 8080.</param>
    /// <param name="grpcPort">gRPC port. Default is 50051.</param>
    /// <param name="useSsl">Whether to use SSL/TLS. Default is false.</param>
    /// <param name="credentials">Authentication credentials.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateClient(
        this IServiceCollection services,
        string name,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        ICredentials? credentials = null)
    {
        return services.AddWeaviateClient(name, options =>
        {
            options.RestEndpoint = hostname;
            options.GrpcEndpoint = hostname;
            options.RestPort = restPort;
            options.GrpcPort = grpcPort;
            options.UseSsl = useSsl;
            options.Credentials = credentials;
        });
    }

    /// <summary>
    /// Adds a named Weaviate client configured for Weaviate Cloud.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="clusterEndpoint">The Weaviate Cloud cluster endpoint (e.g., "my-cluster.weaviate.cloud").</param>
    /// <param name="apiKey">API key for authentication.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateCloudClient(
        this IServiceCollection services,
        string name,
        string clusterEndpoint,
        string? apiKey = null)
    {
        return services.AddWeaviateClient(name, options =>
        {
            options.RestEndpoint = clusterEndpoint;
            options.GrpcEndpoint = $"grpc-{clusterEndpoint}";
            options.RestPort = 443;
            options.GrpcPort = 443;
            options.UseSsl = true;
            options.Credentials = string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey);
        });
    }
}
