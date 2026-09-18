namespace Weaviate.Client;

/// <summary>
/// Connection helpers for creating WeaviateClient instances with common configurations.
/// All methods are now async to ensure proper initialization without blocking.
/// </summary>
public static class Connect
{
    /// <summary>
    /// Creates a WeaviateClient connecting to a local Weaviate instance.
    /// </summary>
    public static Task<WeaviateClient> Local(
        Auth.ApiKeyCredentials credentials,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? insertTimeout = null,
        TimeSpan? queryTimeout = null
    ) =>
        WeaviateClientBuilder
            .Local(credentials, hostname, restPort, grpcPort, useSsl, headers, httpMessageHandler)
            .ApplyTimeouts(defaultTimeout, initTimeout, insertTimeout, queryTimeout)
            .BuildAsync();

    /// <summary>
    /// Creates a WeaviateClient connecting to a local Weaviate instance.
    /// </summary>
    public static Task<WeaviateClient> Local(
        ICredentials? credentials = null,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? insertTimeout = null,
        TimeSpan? queryTimeout = null
    ) =>
        WeaviateClientBuilder
            .Local(credentials, hostname, restPort, grpcPort, useSsl, headers, httpMessageHandler)
            .ApplyTimeouts(defaultTimeout, initTimeout, insertTimeout, queryTimeout)
            .BuildAsync();

    /// <summary>
    /// Creates a WeaviateClient connecting to Weaviate Cloud.
    /// </summary>
    /// <param name="restEndpoint">The cluster URL (e.g. <c>https://my-cluster.weaviate.cloud</c>) or its
    /// bare hostname (e.g. <c>my-cluster.weaviate.cloud</c>). Only the host is used: the scheme, including
    /// <c>http://</c>, and any path are ignored, because Weaviate Cloud always uses TLS on port 443.</param>
    /// <param name="apiKey">API key for authentication.</param>
    /// <param name="headers">Additional HTTP headers to include in requests.</param>
    /// <param name="httpMessageHandler">Optional HTTP message handler for REST requests.</param>
    /// <param name="defaultTimeout">Default timeout for all operations.</param>
    /// <param name="initTimeout">Timeout for initialization operations.</param>
    /// <param name="insertTimeout">Timeout for data operations.</param>
    /// <param name="queryTimeout">Timeout for query operations.</param>
    /// <exception cref="ArgumentException"><paramref name="restEndpoint"/> is empty, uses a scheme other
    /// than http or https, contains user credentials, specifies a port other than 443, or does not have a
    /// valid DNS hostname.</exception>
    public static Task<WeaviateClient> Cloud(
        string restEndpoint,
        string? apiKey = null,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null,
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? insertTimeout = null,
        TimeSpan? queryTimeout = null
    ) =>
        WeaviateClientBuilder
            .Cloud(restEndpoint, apiKey, headers, httpMessageHandler)
            .ApplyTimeouts(defaultTimeout, initTimeout, insertTimeout, queryTimeout)
            .BuildAsync();

    /// <summary>
    /// Creates a WeaviateClient from environment variables.
    /// Supports environment variables prefixed with WEAVIATE_ (or custom prefix).
    /// </summary>
    public static Task<WeaviateClient> FromEnvironment(
        string prefix = "WEAVIATE_",
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? insertTimeout = null,
        TimeSpan? queryTimeout = null
    )
    {
        var restEndpoint = Environment.GetEnvironmentVariable($"{prefix}REST_ENDPOINT");
        var grpcEndpoint = Environment.GetEnvironmentVariable($"{prefix}GRPC_ENDPOINT");
        var restPort = Environment.GetEnvironmentVariable($"{prefix}REST_PORT") ?? "8080";
        var grpcPort = Environment.GetEnvironmentVariable($"{prefix}GRPC_PORT") ?? "50051";
        var useSsl = Environment.GetEnvironmentVariable($"{prefix}USE_SSL")?.ToLower() == "true";
        var apiKey = Environment.GetEnvironmentVariable($"{prefix}API_KEY");
        var openaiKey = Environment.GetEnvironmentVariable($"{prefix}OPENAI_API_KEY");

        if (restEndpoint is null && grpcEndpoint is null)
        {
            throw new InvalidOperationException("No REST or GRPC endpoint provided.");
        }
        else if (restEndpoint is not null && grpcEndpoint is null)
        {
            grpcEndpoint = restEndpoint;
        }
        else if (restEndpoint is null && grpcEndpoint is not null)
        {
            restEndpoint = grpcEndpoint;
        }

        var builder = WeaviateClientBuilder.Custom(
            restEndpoint: restEndpoint!,
            grpcEndpoint: grpcEndpoint!,
            restPort: restPort,
            grpcPort: grpcPort,
            useSsl: useSsl,
            credentials: string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey)
        );

        if (openaiKey is not null && !string.IsNullOrEmpty(openaiKey))
        {
            builder.WithOpenAI(openaiKey);
        }

        return builder
            .ApplyTimeouts(defaultTimeout, initTimeout, insertTimeout, queryTimeout)
            .BuildAsync();
    }
}
