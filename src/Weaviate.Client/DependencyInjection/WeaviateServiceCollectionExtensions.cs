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
    /// Adds Weaviate client services for a local Weaviate instance.
    /// Similar to Connect.Local() but for dependency injection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="hostname">Hostname for local Weaviate instance. Default is "localhost".</param>
    /// <param name="restPort">REST port. Default is 8080.</param>
    /// <param name="grpcPort">gRPC port. Default is 50051.</param>
    /// <param name="useSsl">Whether to use SSL/TLS. Default is false.</param>
    /// <param name="credentials">Authentication credentials.</param>
    /// <param name="headers">Additional HTTP headers to include in requests.</param>
    /// <param name="defaultTimeout">Default timeout for all operations.</param>
    /// <param name="initTimeout">Timeout for initialization operations.</param>
    /// <param name="dataTimeout">Timeout for data operations.</param>
    /// <param name="queryTimeout">Timeout for query operations.</param>
    /// <param name="eagerInitialization">Whether to initialize the client eagerly on application startup. Default is true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateLocal(
        this IServiceCollection services,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        ICredentials? credentials = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? dataTimeout = null,
        TimeSpan? queryTimeout = null,
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
            options.Headers = headers;
            options.DefaultTimeout = defaultTimeout;
            options.InitTimeout = initTimeout;
            options.DataTimeout = dataTimeout;
            options.QueryTimeout = queryTimeout;
        }, eagerInitialization);

        return services;
    }

    /// <summary>
    /// Adds Weaviate client services configured for Weaviate Cloud.
    /// Similar to Connect.Cloud() but for dependency injection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="clusterEndpoint">The Weaviate Cloud cluster endpoint (e.g., "my-cluster.weaviate.cloud").</param>
    /// <param name="apiKey">API key for authentication.</param>
    /// <param name="headers">Additional HTTP headers to include in requests.</param>
    /// <param name="defaultTimeout">Default timeout for all operations.</param>
    /// <param name="initTimeout">Timeout for initialization operations.</param>
    /// <param name="dataTimeout">Timeout for data operations.</param>
    /// <param name="queryTimeout">Timeout for query operations.</param>
    /// <param name="eagerInitialization">Whether to initialize the client eagerly on application startup. Default is true.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateCloud(
        this IServiceCollection services,
        string clusterEndpoint,
        string? apiKey = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? dataTimeout = null,
        TimeSpan? queryTimeout = null,
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
            options.Headers = headers;
            options.DefaultTimeout = defaultTimeout;
            options.InitTimeout = initTimeout;
            options.DataTimeout = dataTimeout;
            options.QueryTimeout = queryTimeout;
        }, eagerInitialization);

        return services;
    }

    // Named client helper methods

    /// <summary>
    /// Adds a named Weaviate client for a local instance with default configuration.
    /// Uses localhost:8080 for REST and localhost:50051 for gRPC.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateLocal(
        this IServiceCollection services,
        string name)
    {
        return services.AddWeaviateClient(name, options =>
        {
            options.RestEndpoint = "localhost";
            options.GrpcEndpoint = "localhost";
            options.RestPort = 8080;
            options.GrpcPort = 50051;
            options.UseSsl = false;
        });
    }

    /// <summary>
    /// Adds a named Weaviate client for a local instance with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="configureOptions">Action to configure Weaviate options for this client.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateLocal(
        this IServiceCollection services,
        string name,
        Action<WeaviateOptions> configureOptions)
    {
        return services.AddWeaviateClient(name, configureOptions);
    }

    /// <summary>
    /// Adds a named Weaviate client for a local instance with specific parameters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="hostname">Hostname for local Weaviate instance. Default is "localhost".</param>
    /// <param name="restPort">REST port. Default is 8080.</param>
    /// <param name="grpcPort">gRPC port. Default is 50051.</param>
    /// <param name="useSsl">Whether to use SSL/TLS. Default is false.</param>
    /// <param name="credentials">Authentication credentials.</param>
    /// <param name="headers">Additional HTTP headers to include in requests.</param>
    /// <param name="defaultTimeout">Default timeout for all operations.</param>
    /// <param name="initTimeout">Timeout for initialization operations.</param>
    /// <param name="dataTimeout">Timeout for data operations.</param>
    /// <param name="queryTimeout">Timeout for query operations.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateLocal(
        this IServiceCollection services,
        string name,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        ICredentials? credentials = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? dataTimeout = null,
        TimeSpan? queryTimeout = null)
    {
        return services.AddWeaviateClient(name, options =>
        {
            options.RestEndpoint = hostname;
            options.GrpcEndpoint = hostname;
            options.RestPort = restPort;
            options.GrpcPort = grpcPort;
            options.UseSsl = useSsl;
            options.Credentials = credentials;
            options.Headers = headers;
            options.DefaultTimeout = defaultTimeout;
            options.InitTimeout = initTimeout;
            options.DataTimeout = dataTimeout;
            options.QueryTimeout = queryTimeout;
        });
    }

    /// <summary>
    /// Adds a named Weaviate client for Weaviate Cloud with custom configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="configureOptions">Action to configure Weaviate options for this client.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateCloud(
        this IServiceCollection services,
        string name,
        Action<WeaviateOptions> configureOptions)
    {
        return services.AddWeaviateClient(name, configureOptions);
    }

    /// <summary>
    /// Adds a named Weaviate client for Weaviate Cloud with specific parameters.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="name">The logical name of the client.</param>
    /// <param name="clusterEndpoint">The Weaviate Cloud cluster endpoint (e.g., "my-cluster.weaviate.cloud").</param>
    /// <param name="apiKey">API key for authentication.</param>
    /// <param name="headers">Additional HTTP headers to include in requests.</param>
    /// <param name="defaultTimeout">Default timeout for all operations.</param>
    /// <param name="initTimeout">Timeout for initialization operations.</param>
    /// <param name="dataTimeout">Timeout for data operations.</param>
    /// <param name="queryTimeout">Timeout for query operations.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWeaviateCloud(
        this IServiceCollection services,
        string name,
        string clusterEndpoint,
        string? apiKey = null,
        Dictionary<string, string>? headers = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? dataTimeout = null,
        TimeSpan? queryTimeout = null)
    {
        return services.AddWeaviateClient(name, options =>
        {
            options.RestEndpoint = clusterEndpoint;
            options.GrpcEndpoint = $"grpc-{clusterEndpoint}";
            options.RestPort = 443;
            options.GrpcPort = 443;
            options.UseSsl = true;
            options.Credentials = string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey);
            options.Headers = headers;
            options.DefaultTimeout = defaultTimeout;
            options.InitTimeout = initTimeout;
            options.DataTimeout = dataTimeout;
            options.QueryTimeout = queryTimeout;
        });
    }

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
    [Obsolete("Use AddWeaviateLocal(name, hostname, ...) instead for better API consistency.")]
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
    [Obsolete("Use AddWeaviateCloud(name, clusterEndpoint, apiKey) instead for better API consistency.")]
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
