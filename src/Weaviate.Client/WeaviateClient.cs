using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;
using Weaviate.Client.Rest.Dto;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Weaviate.Client.Tests")]

namespace Weaviate.Client;

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
    ICredentials? Credentials = null
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

    public WeaviateClient Client(HttpMessageHandler? messageHandler = null) =>
        new(this, httpMessageHandler: messageHandler);
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
    private readonly ITokenService? _tokenService;

    public async Task<Models.MetaInfo> GetMeta()
    {
        var meta = await RestClient.GetMeta();

        return new Models.MetaInfo
        {
            GrpcMaxMessageSize = meta?.GrpcMaxMessageSize is not null
                ? Convert.ToUInt64(meta?.GrpcMaxMessageSize)
                : null,
            Hostname = meta?.Hostname ?? string.Empty,
            Version =
                Models.MetaInfo.ParseWeaviateVersion(meta?.Version ?? string.Empty)
                ?? new System.Version(0, 0),
            Modules =
                (meta?.Modules as JsonElement?)
                    ?.EnumerateObject()
                    .ToDictionary(k => k.Name, k => (object)k.Value) ?? [],
        };
    }

    private Models.MetaInfo? _metaCache;

    private async Task<Models.MetaInfo?> GetMetaCached()
    {
        if (_metaCache != null)
            return _metaCache.Value;

        await _metaCacheSemaphore.WaitAsync();
        try
        {
            if (_metaCache == null)
            {
                var meta = await GetMeta();
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
    public Models.MetaInfo? Meta => GetMetaCached().GetAwaiter().GetResult();
    public System.Version? WeaviateVersion => Meta?.Version;

    /// <summary>
    /// Returns true if the Weaviate process is live.
    /// </summary>
    public Task<bool> Live() => RestClient.LiveAsync();

    /// <summary>
    /// Returns true if the Weaviate instance is ready to accept requests.
    /// </summary>
    public Task<bool> IsReady() => RestClient.ReadyAsync();

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
            if (await IsReady())
            {
                return true;
            }
            await Task.Delay(interval, cancellationToken);
        }
        return await IsReady();
    }

    public static ClientConfiguration DefaultOptions => _defaultOptions.Value;

    private bool _isDisposed = false;
    private readonly ILogger<WeaviateClient> _logger = LoggerFactory
        .Create(builder => builder.AddConsole())
        .CreateLogger<WeaviateClient>();

    [NotNull]
    internal WeaviateRestClient RestClient { get; init; }

    [NotNull]
    internal WeaviateGrpcClient GrpcClient { get; init; }

    public ClientConfiguration Configuration { get; }

    public CollectionsClient Collections { get; }
    public ClusterClient Cluster { get; }

    public AliasClient Alias { get; }

    static bool IsWeaviateDomain(string url)
    {
        return url.ToLower().Contains("weaviate.io")
            || url.ToLower().Contains("semi.technology")
            || url.ToLower().Contains("weaviate.cloud");
    }

    public WeaviateClient(
        ClientConfiguration? configuration = null,
        HttpMessageHandler? httpMessageHandler = null,
        ILogger<WeaviateClient>? logger = null
    )
    {
        _logger = logger ?? _logger;

        Configuration = configuration ?? DefaultOptions;

        _tokenService = InitializeTokenService().GetAwaiter().GetResult();

        httpMessageHandler ??= new HttpClientHandler();

        if (_tokenService != null)
        {
            // Add a delegating handler to inject the access token into each request
            httpMessageHandler = new AuthenticatedHttpHandler(_tokenService)
            {
                InnerHandler = httpMessageHandler,
            };
        }

        var httpClient = new HttpClient(httpMessageHandler);

        var wcdHost = IsWeaviateDomain(Configuration.RestAddress)
            ? Configuration.RestAddress
            : null;

        if (wcdHost != null)
        {
            httpClient.DefaultRequestHeaders.Add(
                "X-Weaviate-Cluster-URL",
                Configuration.RestUri.ToString()
            );
        }

        if (Configuration.Headers != null)
        {
            foreach (var header in Configuration.Headers)
            {
                httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        RestClient = new WeaviateRestClient(Configuration.RestUri, httpClient);

        GrpcClient = new WeaviateGrpcClient(
            Configuration.GrpcUri,
            wcdHost,
            _tokenService,
            Configuration.Headers,
            null,
            Meta?.GrpcMaxMessageSize ?? null
        );

        Cluster = new ClusterClient(RestClient);
        Collections = new CollectionsClient(this);
        Alias = new AliasClient(this);
    }

    private async Task<ITokenService?> InitializeTokenService()
    {
        if (Configuration.Credentials is null)
        {
            return null;
        }

        if (Configuration.Credentials is Auth.ApiKeyCredentials apiKey)
        {
            return new ApiKeyTokenService(apiKey);
        }

        var openIdConfig = await OAuthTokenService.GetOpenIdConfig(
            Configuration.RestUri.ToString()
        );

        if (!openIdConfig.IsSuccessStatusCode)
        {
            _logger?.LogWarning("Failed to retrieve OpenID configuration");
            return null;
        }

        var tokenEndpoint = openIdConfig.TokenEndpoint!;
        var clientId = openIdConfig.ClientID!;

        OAuthConfig oauthConfig = new()
        {
            TokenEndpoint = tokenEndpoint,
            ClientId = clientId,
            GrantType = Configuration.Credentials switch
            {
                Auth.ClientCredentialsFlow => "client_credentials",
                Auth.ClientPasswordFlow => "password",
                Auth.BearerTokenCredentials => "bearer",
                _ => throw new NotSupportedException("Unsupported credentials type"),
            },
            Scope = Configuration.Credentials?.GetScopes() ?? "",
        };

        var httpClient = new HttpClient();

        if (Configuration.Credentials is Auth.BearerTokenCredentials bearerToken)
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

        if (Configuration.Credentials is Auth.ClientCredentialsFlow clientCreds)
        {
            oauthConfig = oauthConfig with { ClientSecret = clientCreds.ClientSecret };
        }
        else if (Configuration.Credentials is Auth.ClientPasswordFlow clientPass)
        {
            oauthConfig = oauthConfig with
            {
                Username = clientPass.Username,
                Password = clientPass.Password,
            };
        }

        return new OAuthTokenService(httpClient, oauthConfig);
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
