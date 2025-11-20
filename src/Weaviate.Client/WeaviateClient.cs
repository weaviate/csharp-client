using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Weaviate.Client.Tests")]

namespace Weaviate.Client;

/// <summary>
/// Global default settings for Weaviate clients.
/// </summary>
public static class WeaviateDefaults
{
    /// <summary>
    /// Default timeout for all requests. Default is 30 seconds.
    /// This can be overridden per client via ClientConfiguration.
    /// </summary>
    public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default timeout for initialization operations (GetMeta, Live, IsReady). Default is 2 seconds.
    /// This can be overridden per client via ClientConfiguration.WithInitTimeout().
    /// </summary>
    public static TimeSpan InitTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Default timeout for data operations (Insert, Delete, Update, Reference management). Default is 120 seconds.
    /// This can be overridden per client via ClientConfiguration.WithDataTimeout().
    /// </summary>
    public static TimeSpan DataTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Default timeout for query/search operations (FetchObjects, NearText, BM25, Hybrid, etc.). Default is 60 seconds.
    /// This can be overridden per client via ClientConfiguration.WithQueryTimeout().
    /// </summary>
    public static TimeSpan QueryTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Default retry policy applied when a client does not specify one explicitly.
    /// </summary>
    public static RetryPolicy DefaultRetryPolicy { get; set; } = RetryPolicy.Default;
}

public interface ICredentials
{
    internal string GetScopes();
}

public static class Auth
{
    public sealed record ApiKeyCredentials(string Value) : ICredentials
    {
        string ICredentials.GetScopes() => "";

        public static implicit operator ApiKeyCredentials(string value) => new(value);
    }

    public sealed record BearerTokenCredentials(
        string AccessToken,
        int ExpiresIn = 60,
        string RefreshToken = ""
    ) : ICredentials
    {
        string ICredentials.GetScopes() => "";
    }

    public sealed record ClientCredentialsFlow(string ClientSecret, params string?[] Scope)
        : ICredentials
    {
        public string GetScopes() => string.Join(" ", Scope.Where(s => !string.IsNullOrEmpty(s)));
    }

    public sealed record ClientPasswordFlow(
        string Username,
        string Password,
        params string?[] Scope
    ) : ICredentials
    {
        public string GetScopes() => string.Join(" ", Scope.Where(s => !string.IsNullOrEmpty(s)));
    }

    public static ApiKeyCredentials ApiKey(string value) => new(value);

    public static BearerTokenCredentials BearerToken(
        string accessToken,
        int expiresIn = 60,
        string refreshToken = ""
    ) => new(accessToken, expiresIn, refreshToken);

    public static ClientCredentialsFlow ClientCredentials(
        string clientSecret,
        params string?[] scope
    ) => new(clientSecret, scope);

    public static ClientPasswordFlow ClientPassword(
        string username,
        string password,
        params string?[] scope
    ) => new(username, password, scope);
}

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
    DelegatingHandler[]? CustomHandlers = null
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

        // Initialize token service asynchronously
        var tokenService = await InitializeTokenService(this);

        // Create REST client
        var restClient = WeaviateClient.CreateRestClient(
            this,
            messageHandler,
            tokenService,
            logger
        );

        // Fetch metadata from REST client to get gRPC max message size
        ulong? maxMessageSize = null;
        try
        {
            var metaDto = await restClient.GetMeta(CancellationToken.None);
            if (metaDto?.GrpcMaxMessageSize is not null)
            {
                maxMessageSize = Convert.ToUInt64(metaDto.GrpcMaxMessageSize);
            }
        }
        catch
        {
            // If metadata fetch fails, use defaults
        }

        // Create gRPC client with metadata
        var grpcClient = WeaviateClient.CreateGrpcClient(this, tokenService, maxMessageSize);

        // Create and return the client with pre-built services
        return new WeaviateClient(this, restClient, grpcClient, logger);
    }

    /// <summary>
    /// Asynchronous token service initialization.
    /// </summary>
    private static async Task<ITokenService?> InitializeTokenService(
        ClientConfiguration configuration
    )
    {
        if (configuration.Credentials is null)
        {
            return null;
        }

        if (configuration.Credentials is Auth.ApiKeyCredentials apiKey)
        {
            return new ApiKeyTokenService(apiKey);
        }

        var openIdConfig = await OAuthTokenService.GetOpenIdConfig(
            configuration.RestUri.ToString()
        );

        if (!openIdConfig.IsSuccessStatusCode)
        {
            return null;
        }

        var tokenEndpoint = openIdConfig.TokenEndpoint!;
        var clientId = openIdConfig.ClientID!;

        OAuthConfig oauthConfig = new()
        {
            TokenEndpoint = tokenEndpoint,
            ClientId = clientId,
            GrantType = configuration.Credentials switch
            {
                Auth.ClientCredentialsFlow => "client_credentials",
                Auth.ClientPasswordFlow => "password",
                Auth.BearerTokenCredentials => "bearer",
                _ => throw new NotSupportedException("Unsupported credentials type"),
            },
            Scope = configuration.Credentials?.GetScopes() ?? "",
        };

        var httpClient = new HttpClient();

        if (configuration.Credentials is Auth.BearerTokenCredentials bearerToken)
        {
            return new OAuthTokenService(httpClient, oauthConfig)
            {
                CurrentToken = new(
                    bearerToken.AccessToken,
                    bearerToken.ExpiresIn,
                    bearerToken.RefreshToken
                ),
            };
        }

        if (configuration.Credentials is Auth.ClientCredentialsFlow clientCreds)
        {
            oauthConfig = oauthConfig with { ClientSecret = clientCreds.ClientSecret };
        }
        else if (configuration.Credentials is Auth.ClientPasswordFlow clientPass)
        {
            oauthConfig = oauthConfig with
            {
                Username = clientPass.Username,
                Password = clientPass.Password,
            };
        }

        return new OAuthTokenService(httpClient, oauthConfig);
    }

    /// <summary>
    /// Synchronous wrapper for InitializeTokenService for use in the builder pattern.
    /// </summary>
    internal static ITokenService? InitializeTokenServiceSync(ClientConfiguration configuration)
    {
        if (configuration.Credentials is null)
        {
            return null;
        }

        if (configuration.Credentials is Auth.ApiKeyCredentials apiKey)
        {
            return new ApiKeyTokenService(apiKey);
        }

        // For OAuth credentials, we need to fetch the OpenID config synchronously
        // This is acceptable here since it's only during initial construction
        try
        {
            var openIdConfig = OAuthTokenService
                .GetOpenIdConfig(configuration.RestUri.ToString())
                .GetAwaiter()
                .GetResult();

            if (!openIdConfig.IsSuccessStatusCode)
            {
                return null;
            }

            var tokenEndpoint = openIdConfig.TokenEndpoint!;
            var clientId = openIdConfig.ClientID!;

            OAuthConfig oauthConfig = new()
            {
                TokenEndpoint = tokenEndpoint,
                ClientId = clientId,
                GrantType = configuration.Credentials switch
                {
                    Auth.ClientCredentialsFlow => "client_credentials",
                    Auth.ClientPasswordFlow => "password",
                    Auth.BearerTokenCredentials => "bearer",
                    _ => throw new NotSupportedException("Unsupported credentials type"),
                },
                Scope = configuration.Credentials?.GetScopes() ?? "",
            };

            var httpClient = new HttpClient();

            if (configuration.Credentials is Auth.BearerTokenCredentials bearerToken)
            {
                return new OAuthTokenService(httpClient, oauthConfig)
                {
                    CurrentToken = new(
                        bearerToken.AccessToken,
                        bearerToken.ExpiresIn,
                        bearerToken.RefreshToken
                    ),
                };
            }

            if (configuration.Credentials is Auth.ClientCredentialsFlow clientCreds)
            {
                oauthConfig = oauthConfig with { ClientSecret = clientCreds.ClientSecret };
            }
            else if (configuration.Credentials is Auth.ClientPasswordFlow clientPass)
            {
                oauthConfig = oauthConfig with
                {
                    Username = clientPass.Username,
                    Password = clientPass.Password,
                };
            }

            return new OAuthTokenService(httpClient, oauthConfig);
        }
        catch
        {
            return null;
        }
    }
};

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

    public async Task<Models.MetaInfo> GetMeta(CancellationToken cancellationToken = default)
    {
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

    private async Task<Models.MetaInfo?> GetMetaCached(
        CancellationToken cancellationToken = default
    )
    {
        if (_metaCache != null)
            return _metaCache.Value;

        await _metaCacheSemaphore.WaitAsync();
        try
        {
            if (_metaCache == null)
            {
                var meta = await GetMeta(CreateInitCancellationToken(cancellationToken));
                _metaCache = meta;
            }
        }
        finally
        {
            _metaCacheSemaphore.Release();
        }

        return _metaCache.Value;
    }

    private readonly SemaphoreSlim _metaCacheSemaphore = new(1, 1);
    public Models.MetaInfo? Meta => GetMetaCached().GetAwaiter().GetResult(); // Synchronous accessor unchanged; advanced usage should call GetMeta directly with a token.
    public System.Version? WeaviateVersion => Meta?.Version;

    /// <summary>
    /// Returns true if the Weaviate process is live.
    /// </summary>
    public Task<bool> Live(CancellationToken cancellationToken = default)
    {
        return RestClient.LiveAsync(CreateInitCancellationToken(cancellationToken));
    }

    /// <summary>
    /// Returns true if the Weaviate instance is ready to accept requests.
    /// </summary>
    public Task<bool> IsReady(CancellationToken cancellationToken = default)
    {
        return RestClient.ReadyAsync(CreateInitCancellationToken(cancellationToken));
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

    public TimeSpan? DefaultTimeout => Configuration.DefaultTimeout;
    public TimeSpan? InitTimeout => Configuration.InitTimeout;
    public TimeSpan? DataTimeout => Configuration.DataTimeout;
    public TimeSpan? QueryTimeout => Configuration.QueryTimeout;

    /// <summary>
    /// Creates a WeaviateClient with the given configuration and services.
    /// Internal method used by the builder pattern.
    /// </summary>
    internal WeaviateClient(
        ClientConfiguration configuration,
        WeaviateRestClient restClient,
        WeaviateGrpcClient grpcClient,
        ILogger<WeaviateClient>? logger = null
    )
    {
        _logger = logger ?? _logger;
        Configuration = configuration;
        RestClient = restClient;
        GrpcClient = grpcClient;

        Cluster = new ClusterClient(RestClient);
        Collections = new CollectionsClient(this);
        Alias = new AliasClient(this);
        Users = new UsersClient(this);
        Roles = new RolesClient(this);
        Groups = new GroupsClient(this);
    }

    /// <summary>
    /// Creates a WeaviateClient from configuration and optional HTTP message handler.
    /// For backward compatibility with existing code that creates clients directly.
    /// </summary>
    public WeaviateClient(
        ClientConfiguration? configuration = null,
        HttpMessageHandler? httpMessageHandler = null,
        ILogger<WeaviateClient>? logger = null
    )
    {
        var config = configuration ?? DefaultOptions;
        var loggerInstance =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        var restClient = CreateRestClientForPublic(config, httpMessageHandler, loggerInstance);
        var grpcClientInstance = CreateGrpcClientForPublic(config);

        // Initialize like the internal constructor
        _logger = loggerInstance;
        Configuration = config;
        RestClient = restClient;
        GrpcClient = grpcClientInstance;

        Cluster = new ClusterClient(RestClient);
        Collections = new CollectionsClient(this);
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
        ILogger<WeaviateClient>? logger = null,
        WeaviateGrpcClient? grpcClient = null
    )
    {
        var config = configuration ?? DefaultOptions;
        var loggerInstance =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        var restClient = CreateRestClientForPublic(config, httpMessageHandler, loggerInstance);
        var grpcClientInstance = grpcClient ?? CreateGrpcClientForPublic(config);

        // Initialize like the internal constructor
        _logger = loggerInstance;
        Configuration = config;
        RestClient = restClient;
        GrpcClient = grpcClientInstance;

        Cluster = new ClusterClient(RestClient);
        Collections = new CollectionsClient(this);
        Alias = new AliasClient(this);
        Users = new UsersClient(this);
        Roles = new RolesClient(this);
        Groups = new GroupsClient(this);
    }

    /// <summary>
    /// Helper to create REST client for the public constructor.
    /// </summary>
    private static WeaviateRestClient CreateRestClientForPublic(
        ClientConfiguration config,
        HttpMessageHandler? httpMessageHandler,
        ILogger<WeaviateClient>? logger
    )
    {
        var loggerInstance =
            logger
            ?? LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<WeaviateClient>();

        var tokenService = ClientConfiguration.InitializeTokenServiceSync(config);
        return CreateRestClient(config, httpMessageHandler, tokenService, loggerInstance);
    }

    /// <summary>
    /// Helper to create gRPC client for the public constructor.
    /// </summary>
    private static WeaviateGrpcClient CreateGrpcClientForPublic(ClientConfiguration config)
    {
        var tokenService = ClientConfiguration.InitializeTokenServiceSync(config);
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
