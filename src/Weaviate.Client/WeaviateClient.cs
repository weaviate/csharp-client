using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

public partial class WeaviateClient : IDisposable
{
    private static readonly Lazy<ClientConfiguration> _defaultOptions = new(() =>
        new()
        {
            Credentials = null,
            RestPort = 8080,
            GrpcPort = 50051,
        }
    );

    // Async initialization support
    private readonly Lazy<Task>? _initializationTask;
    private readonly ClientConfiguration? _configForAsyncInit;

    public async Task<Models.MetaInfo> GetMeta(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        var meta = await RestClient.GetMeta(CreateInitCancellationToken(cancellationToken));

        return new Models.MetaInfo
        {
            GrpcMaxMessageSize = meta?.GrpcMaxMessageSize is not null
                ? Convert.ToUInt64(meta?.GrpcMaxMessageSize)
                : null,
            Hostname = meta?.Hostname ?? string.Empty,
            Version =
                Models.MetaInfo.ParseWeaviateVersion(meta?.Version ?? string.Empty)
                ?? new System.Version(0, 0),
            Modules = meta?.Modules?.ToDictionary() ?? [],
        };
    }

    private Models.MetaInfo? _metaCache;

    /// <summary>
    /// Gets the server metadata that was fetched during client initialization.
    /// This is always available and does not require network calls.
    /// </summary>
    public Models.MetaInfo? Meta => _metaCache;

    /// <summary>
    /// Gets the Weaviate server version from cached metadata.
    /// </summary>
    public System.Version? WeaviateVersion => Meta?.Version;

    /// <summary>
    /// Returns true if the Weaviate process is live.
    /// </summary>
    public async Task<bool> IsLive(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        return await RestClient.LiveAsync(CreateInitCancellationToken(cancellationToken));
    }

    /// <summary>
    /// Returns true if the Weaviate instance is ready to accept requests.
    /// </summary>
    public async Task<bool> IsReady(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync();
        return await RestClient.ReadyAsync(CreateInitCancellationToken(cancellationToken));
    }

    /// <summary>
    /// Waits until the instance becomes ready or the timeout/cancellation token triggers.
    /// </summary>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="cancellationToken">Cancellation token to abort waiting early.</param>
    /// <param name="pollInterval">Optional polling interval (defaults to 250ms).</param>
    /// <returns>true if ready was reached before timing out or cancellation; false otherwise.</returns>
    public async Task<bool> WaitUntilReady(
        TimeSpan timeout,
        CancellationToken cancellationToken,
        TimeSpan? pollInterval = null
    )
    {
        var interval = pollInterval ?? TimeSpan.FromMilliseconds(250);
        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await IsReady(cancellationToken))
            {
                return true;
            }
            await Task.Delay(interval, cancellationToken);
        }
        return await IsReady(cancellationToken);
    }

    public static ClientConfiguration DefaultOptions => _defaultOptions.Value;

    /// <summary>
    /// Creates a cancellation token with init-specific timeout configuration.
    /// Uses InitTimeout if configured, falls back to DefaultTimeout.
    /// </summary>
    private CancellationToken CreateInitCancellationToken(CancellationToken userToken = default)
    {
        return TimeoutHelper.GetCancellationToken(
            Configuration.InitTimeout,
            Configuration.DefaultTimeout,
            userToken
        );
    }

    private bool _isDisposed = false;
    private readonly ILogger<WeaviateClient> _logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<WeaviateClient>();

    internal WeaviateRestClient RestClient { get; private set; } = null!;

    internal WeaviateGrpcClient GrpcClient { get; private set; } = null!;

    public ClientConfiguration Configuration { get; }

    public CollectionsClient Collections { get; }
    public ClusterClient Cluster { get; }
    public AliasClient Alias { get; }
    public UsersClient Users { get; }
    public RolesClient Roles { get; }
    public GroupsClient Groups { get; }

    static bool IsWeaviateDomain(string url)
    {
        return url.ToLower().Contains("weaviate.io")
            || url.ToLower().Contains("semi.technology")
            || url.ToLower().Contains("weaviate.cloud");
    }

    /// <summary>
    /// Internal constructor for builder path with async initialization.
    /// </summary>
    internal WeaviateClient(ClientConfiguration configuration, ILogger<WeaviateClient>? logger)
    {
        Configuration = configuration;
        _logger =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        // Initialize Lazy task that will run initialization on first access
        _initializationTask = new Lazy<Task>(() => PerformInitializationAsync(configuration));

        // Initialize client facades
        Collections = new CollectionsClient(this);
        Cluster = new ClusterClient(this);
        Alias = new AliasClient(this);
        Users = new UsersClient(this);
        Roles = new RolesClient(this);
        Groups = new GroupsClient(this);
    }

    /// <summary>
    /// Internal constructor for testing with injected gRPC client.
    /// </summary>
    internal WeaviateClient(
        ClientConfiguration? configuration = null,
        HttpMessageHandler? httpMessageHandler = null,
        ITokenService? tokenService = null,
        ILogger<WeaviateClient>? logger = null,
        WeaviateGrpcClient? grpcClient = null
    )
    {
        var config = configuration ?? DefaultOptions;
        var loggerInstance =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        var restClient = CreateRestClientForPublic(
            config,
            httpMessageHandler,
            tokenService,
            loggerInstance
        );
        var grpcClientInstance = grpcClient ?? CreateGrpcClientForPublic(config, tokenService);

        // Initialize like the internal constructor
        _logger = loggerInstance;
        Configuration = config;
        RestClient = restClient;
        GrpcClient = grpcClientInstance;

        Cluster = new ClusterClient(this);
        Collections = new CollectionsClient(this);
        Alias = new AliasClient(this);
        Users = new UsersClient(this);
        Roles = new RolesClient(this);
        Groups = new GroupsClient(this);
    }

    /// <summary>
    /// Constructor for dependency injection scenarios.
    /// Uses async initialization pattern - call InitializeAsync() or ensure IHostedService runs.
    /// </summary>
    public WeaviateClient(IOptions<DependencyInjection.WeaviateOptions> options)
        : this(options, null) { }

    /// <summary>
    /// Constructor for dependency injection scenarios with logger.
    /// Uses async initialization pattern - call InitializeAsync() or ensure IHostedService runs.
    /// </summary>
    public WeaviateClient(
        IOptions<DependencyInjection.WeaviateOptions> options,
        ILogger<WeaviateClient>? logger
    )
    {
        var weaviateOptions = options.Value;
        _configForAsyncInit = weaviateOptions.ToClientConfiguration();
        Configuration = _configForAsyncInit;
        _logger =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        // Initialize Lazy task that will run initialization on first access
        _initializationTask = new Lazy<Task>(() => PerformInitializationAsync(_configForAsyncInit));

        Collections = new CollectionsClient(this);
        Cluster = new ClusterClient(this);
        Alias = new AliasClient(this);
        Users = new UsersClient(this);
        Roles = new RolesClient(this);
        Groups = new GroupsClient(this);
    }

    /// <summary>
    /// Performs the actual async initialization.
    /// This follows the same flow as ClientConfiguration.BuildAsync().
    /// </summary>
    private async Task PerformInitializationAsync(ClientConfiguration config)
    {
        _logger.LogDebug("Starting Weaviate client initialization...");

        // Initialize token service asynchronously - always use DefaultTokenServiceFactory
        var tokenService = await new DefaultTokenServiceFactory().CreateAsync(config);

        // Create REST client - get HttpMessageHandler from config
        RestClient = CreateRestClient(config, config.HttpMessageHandler, tokenService, _logger);

        // Fetch metadata eagerly with init timeout - this will throw if authentication fails
        var initTimeout =
            config.InitTimeout ?? config.DefaultTimeout ?? WeaviateDefaults.DefaultTimeout;
        using var metaCts = new CancellationTokenSource(initTimeout);
        var metaDto = await RestClient.GetMeta(metaCts.Token);
        _metaCache = new Models.MetaInfo
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

        // Log warning if connecting to a server older than 1.31.0
        var minSupportedVersion = new Version(1, 31, 0);
        if (_metaCache.HasValue && _metaCache.Value.Version < minSupportedVersion)
        {
            _logger.LogWarning(
                "Connected to Weaviate server version {ServerVersion}, which is earlier than the minimum supported version {MinVersion}. Some features may not work as expected.",
                _metaCache.Value.Version,
                minSupportedVersion
            );
        }

        var maxMessageSize = _metaCache?.GrpcMaxMessageSize;

        // Create gRPC client with metadata
        GrpcClient = CreateGrpcClient(config, tokenService, maxMessageSize);

        _logger.LogDebug("Weaviate client initialization completed");
    }

    /// <summary>
    /// Explicitly initializes the client asynchronously.
    /// This is called automatically by IHostedService when using DI with eager initialization.
    /// Can be called manually for lazy initialization scenarios.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the initialization.</returns>
    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_initializationTask == null)
        {
            // Client was created with non-DI constructor, already initialized
            return Task.CompletedTask;
        }

        // Lazy<T> ensures this only runs once even if called multiple times
        return _initializationTask.Value;
    }

    /// <summary>
    /// Checks if the client is fully initialized.
    /// </summary>
    public bool IsInitialized =>
        _initializationTask == null
        || // Non-DI constructor, always ready
        (_initializationTask.IsValueCreated && _initializationTask.Value.IsCompletedSuccessfully);

    /// <summary>
    /// Helper to ensure initialization before using the client.
    /// Throws if initialization failed.
    /// </summary>
    internal async Task EnsureInitializedAsync()
    {
        if (_initializationTask != null)
        {
            await _initializationTask.Value; // Will throw if initialization failed
        }
    }

    /// <summary>
    /// Helper to create REST client for the public constructor.
    /// </summary>
    private static WeaviateRestClient CreateRestClientForPublic(
        ClientConfiguration config,
        HttpMessageHandler? httpMessageHandler,
        ITokenService? tokenService,
        ILogger<WeaviateClient>? logger
    )
    {
        var loggerInstance =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        return CreateRestClient(config, httpMessageHandler, tokenService, loggerInstance);
    }

    /// <summary>
    /// Helper to create gRPC client for the public constructor.
    /// </summary>
    private static WeaviateGrpcClient CreateGrpcClientForPublic(
        ClientConfiguration config,
        ITokenService? tokenService
    )
    {
        return CreateGrpcClient(config, tokenService);
    }

    /// <summary>
    /// Creates a WeaviateRestClient from the given configuration.
    /// </summary>
    internal static WeaviateRestClient CreateRestClient(
        ClientConfiguration config,
        HttpMessageHandler? httpMessageHandler,
        ITokenService? tokenService,
        ILogger<WeaviateClient> logger
    )
    {
        // Base handler
        var effectiveHandler = httpMessageHandler ?? new HttpClientHandler();

        // Attach user-provided custom handlers (inner-most first so added order preserves intuitive wrapping)
        if (config.CustomHandlers is { Length: > 0 })
        {
            foreach (var handler in config.CustomHandlers)
            {
                if (handler.InnerHandler == null)
                {
                    handler.InnerHandler = effectiveHandler;
                }
                effectiveHandler = handler;
            }
        }

        // Auth handler (inner-most before base handler) so each retry re-runs auth logic
        if (tokenService != null)
        {
            effectiveHandler = new AuthenticatedHttpHandler(tokenService)
            {
                InnerHandler = effectiveHandler,
            };
        }

        // Retry handler (outer-most)
        var retryPolicy = config.RetryPolicy ?? WeaviateDefaults.DefaultRetryPolicy;
        if (retryPolicy is not null && retryPolicy.MaxRetries > 0)
        {
            effectiveHandler = new RetryHandler(retryPolicy, logger)
            {
                InnerHandler = effectiveHandler,
            };
        }

        var httpClient = new HttpClient(effectiveHandler);

        // Set default timeout for all requests
        var defaultTimeout = config.DefaultTimeout ?? WeaviateDefaults.DefaultTimeout;
        httpClient.Timeout = defaultTimeout;

        var wcdHost = IsWeaviateDomain(config.RestAddress) ? config.RestAddress : null;

        if (wcdHost != null)
        {
            httpClient.DefaultRequestHeaders.Add(
                "X-Weaviate-Cluster-URL",
                config.RestUri.ToString()
            );
        }

        if (config.Headers != null)
        {
            foreach (var header in config.Headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return new WeaviateRestClient(config.RestUri, httpClient);
    }

    /// <summary>
    /// Creates a WeaviateGrpcClient from the given configuration.
    /// </summary>
    internal static WeaviateGrpcClient CreateGrpcClient(
        ClientConfiguration config,
        ITokenService? tokenService,
        ulong? maxMessageSize = null
    )
    {
        var wcdHost = IsWeaviateDomain(config.RestAddress) ? config.RestAddress : null;

        var retryPolicy = config.RetryPolicy ?? WeaviateDefaults.DefaultRetryPolicy;
        var defaultTimeout = config.DefaultTimeout ?? WeaviateDefaults.DefaultTimeout;

        return WeaviateGrpcClient.Create(
            config.GrpcUri,
            wcdHost,
            tokenService,
            config.QueryTimeout ?? defaultTimeout,
            maxMessageSize,
            retryPolicy,
            config.Headers,
            null // Logger is not needed here, gRPC client creates its own
        );
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        GrpcClient?.Dispose();
        RestClient?.Dispose();

        _isDisposed = true;
    }
}
