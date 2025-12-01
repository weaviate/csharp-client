using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

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
    TimeSpan? DataTimeout = null,
    TimeSpan? QueryTimeout = null,
    RetryPolicy? RetryPolicy = null,
    DelegatingHandler[]? CustomHandlers = null,
    HttpMessageHandler? HttpMessageHandler = null
)
{
    public Uri RestUri =>
        new UriBuilder()
        {
            Host = RestAddress,
            Scheme = UseSsl ? "https" : "http",
            Port = RestPort,
            Path = RestPath,
        }.Uri;

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
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<WeaviateClient>();

        // Create client - it will initialize itself via PerformInitializationAsync
        var client = new WeaviateClient(this, logger);

        // Wait for initialization to complete
        await client.InitializeAsync();

        return client;
    }
};
