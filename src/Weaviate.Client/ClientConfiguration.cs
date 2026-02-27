using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

/// <summary>
/// Configuration settings for connecting to a Weaviate instance.
/// </summary>
/// <param name="RestAddress">The hostname or IP address for REST API connections. Defaults to "localhost".</param>
/// <param name="RestPath">The base path for REST API endpoints. Defaults to "v1/".</param>
/// <param name="GrpcAddress">The hostname or IP address for gRPC connections. Defaults to "localhost".</param>
/// <param name="GrpcPath">The base path for gRPC endpoints. Defaults to empty string.</param>
/// <param name="RestPort">The port number for REST API connections. Defaults to 8080.</param>
/// <param name="GrpcPort">The port number for gRPC connections. Defaults to 50051.</param>
/// <param name="UseSsl">Whether to use SSL/TLS for secure connections. Defaults to false.</param>
/// <param name="Headers">Optional custom HTTP headers to include with requests.</param>
/// <param name="Credentials">Optional credentials for authentication.</param>
/// <param name="DefaultTimeout">Default timeout for all operations if not otherwise specified.</param>
/// <param name="InitTimeout">Timeout specifically for initialization operations.</param>
/// <param name="InsertTimeout">Timeout specifically for data insertion operations.</param>
/// <param name="QueryTimeout">Timeout specifically for query operations.</param>
/// <param name="RetryPolicy">Optional retry policy for handling transient failures.</param>
/// <param name="CustomHandlers">Optional custom HTTP message handlers for request/response processing.</param>
/// <param name="HttpMessageHandler">Optional custom HTTP message handler for low-level HTTP operations.</param>
/// <param name="LoggerFactory">Optional logger factory used to create loggers for all internal components. When null, NullLoggerFactory.Instance is used (silent, no console output).</param>
/// <param name="LogRequests">When true, HTTP requests/responses and gRPC calls are logged. Requires LoggerFactory to be set. Defaults to false.</param>
/// <param name="RequestLoggingLevel">The log level used for request/response log entries. Defaults to Debug.</param>
public sealed record ClientConfiguration(
    string RestAddress = "localhost",
    string RestPath = "v1/",
    string GrpcAddress = "localhost",
    string GrpcPath = "",
    ushort RestPort = 8080,
    ushort GrpcPort = 50051,
    bool UseSsl = false,
    Dictionary<string, string>? Headers = null,
    ICredentials? Credentials = null,
    TimeSpan? DefaultTimeout = null,
    TimeSpan? InitTimeout = null,
    TimeSpan? InsertTimeout = null,
    TimeSpan? QueryTimeout = null,
    RetryPolicy? RetryPolicy = null,
    DelegatingHandler[]? CustomHandlers = null,
    HttpMessageHandler? HttpMessageHandler = null,
    ILoggerFactory? LoggerFactory = null,
    bool LogRequests = false,
    LogLevel RequestLoggingLevel = LogLevel.Debug
)
{
    /// <summary>
    /// Gets the complete URI for REST API connections, constructed from the REST configuration properties.
    /// </summary>
    public Uri RestUri =>
        new UriBuilder()
        {
            Host = RestAddress,
            Scheme = UseSsl ? "https" : "http",
            Port = RestPort,
            Path = RestPath,
        }.Uri;

    /// <summary>
    /// Gets the complete URI for gRPC connections, constructed from the gRPC configuration properties.
    /// </summary>
    public Uri GrpcUri =>
        new UriBuilder()
        {
            Host = GrpcAddress,
            Scheme = UseSsl ? "https" : "http",
            Port = GrpcPort,
            Path = GrpcPath,
        }.Uri;

    /// <summary>
    /// Builds a WeaviateClient asynchronously, initializing all services in the correct order.
    /// This is the recommended way to create clients.
    /// </summary>
    internal async Task<WeaviateClient> BuildAsync()
    {
        var factory =
            LoggerFactory ?? Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
        var logger = factory.CreateLogger<WeaviateClient>();

        // Create client - it will initialize itself via PerformInitializationAsync
        var client = new WeaviateClient(this, logger);

        // Wait for initialization to complete
        await client.InitializeAsync();

        return client;
    }
};
