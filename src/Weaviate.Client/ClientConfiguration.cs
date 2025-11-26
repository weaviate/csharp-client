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
    ITokenServiceFactory? TokenServiceFactory = null
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
    internal async Task<WeaviateClient> BuildAsync(HttpMessageHandler? messageHandler = null)
    {
        var logger = LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<WeaviateClient>();

        // Use factory to create token service
        var tokenService = await (
            TokenServiceFactory ?? new DefaultTokenServiceFactory()
        ).CreateAsync(this);

        // Create REST client
        var restClient = WeaviateClient.CreateRestClient(
            this,
            messageHandler,
            tokenService,
            logger
        );

        // Fetch metadata eagerly with init timeout - this will throw if authentication fails
        var initTimeout = InitTimeout ?? DefaultTimeout ?? WeaviateDefaults.DefaultTimeout;
        var metaCts = new CancellationTokenSource(initTimeout);
        var metaDto = await restClient.GetMeta(metaCts.Token);
        var meta = new Models.MetaInfo
        {
            GrpcMaxMessageSize = metaDto?.GrpcMaxMessageSize is not null
                ? Convert.ToUInt64(metaDto.GrpcMaxMessageSize)
                : null,
            Hostname = metaDto?.Hostname ?? string.Empty,
            Version =
                Models.MetaInfo.ParseWeaviateVersion(metaDto?.Version ?? string.Empty)
                ?? new System.Version(0, 0),
            Modules = metaDto?.Modules?.ToDictionary() ?? [],
        };

        var maxMessageSize = meta.GrpcMaxMessageSize;

        // Create gRPC client with metadata
        var grpcClient = WeaviateClient.CreateGrpcClient(this, tokenService, maxMessageSize);

        // Create and return the client with pre-built services
        return new WeaviateClient(this, restClient, grpcClient, logger, meta);
    }
};
