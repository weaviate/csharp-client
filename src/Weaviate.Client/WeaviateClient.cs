using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Weaviate.Client.Tests")]

namespace Weaviate.Client;

public interface ICredentials { }

public static class Auth
{
    public sealed record ApiKeyCredentials(string Value) : ICredentials;

    public sealed record BearerTokenCredentials(
        string AccessToken,
        int ExpiresIn = 60,
        string RefreshToken = ""
    ) : ICredentials;

    public sealed record ClientCredentialsFlow(string ClientSecret, params string?[] Scope)
        : ICredentials;

    public sealed record ClientPasswordFlow(
        string Username,
        string Password,
        params string?[] Scope
    ) : ICredentials;

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
    ICredentials? Credentials = null,
    bool AddEmbeddingHeader = false
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

public class WeaviateClient : IDisposable
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
            GrpcMaxMessageSize = meta?.GrpcMaxMessageSize ?? 0,
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

    private System.Version? _weaviateVersion;
    private readonly SemaphoreSlim _versionSemaphore = new(1, 1);

    public async Task<System.Version> GetWeaviateVersionAsync()
    {
        if (_weaviateVersion != null)
            return _weaviateVersion;

        await _versionSemaphore.WaitAsync();
        try
        {
            if (_weaviateVersion == null)
            {
                var meta = await GetMeta();
                _weaviateVersion = meta.Version;
            }
        }
        finally
        {
            _versionSemaphore.Release();
        }

        return _weaviateVersion;
    }

    public System.Version WeaviateVersion => GetWeaviateVersionAsync().GetAwaiter().GetResult();

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

    public NodesClient Nodes { get; }

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

        if (Configuration.Credentials is Auth.ApiKeyCredentials apiKey)
        {
            _tokenService = new ApiKeyTokenService(apiKey);
        }
        else if (Configuration.Credentials is Auth.ClientCredentialsFlow clientCreds)
        {
            var openIdConfig = OAuthTokenService
                .GetOpenIdConfig(Configuration.RestUri.ToString())
                .GetAwaiter()
                .GetResult();

            if (!openIdConfig.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to retrieve OpenID configuration");
            }
            else
            {
                var tokenEndpoint = openIdConfig.TokenEndpoint!;
                var clientId = openIdConfig.ClientID!;
                _tokenService = new OAuthTokenService(
                    new HttpClient(),
                    new OAuthConfig
                    {
                        TokenEndpoint = tokenEndpoint,
                        ClientId = clientId,
                        ClientSecret = clientCreds.ClientSecret,
                        GrantType = "client_credentials",
                        Scope = string.Join(
                            " ",
                            clientCreds.Scope.Where(s => !string.IsNullOrEmpty(s))
                        ),
                    }
                );
            }
        }
        else if (Configuration.Credentials is Auth.ClientPasswordFlow clientPass)
        {
            var openIdConfig = OAuthTokenService
                .GetOpenIdConfig(Configuration.RestUri.ToString())
                .GetAwaiter()
                .GetResult();

            if (!openIdConfig.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to retrieve OpenID configuration");
            }
            else
            {
                var tokenEndpoint = openIdConfig.TokenEndpoint!;
                var clientId = openIdConfig.ClientID!;

                _tokenService = new OAuthTokenService(
                    new HttpClient(),
                    new OAuthConfig
                    {
                        TokenEndpoint = tokenEndpoint,
                        ClientId = clientId,
                        GrantType = "password",
                        Username = clientPass.Username,
                        Password = clientPass.Password,
                        Scope = string.Join(
                            " ",
                            clientPass.Scope.Where(s => !string.IsNullOrEmpty(s))
                        ),
                    }
                );
            }
        }
        else if (Configuration.Credentials is Auth.BearerTokenCredentials bearerToken)
        {
            var openIdConfig = OAuthTokenService
                .GetOpenIdConfig(Configuration.RestUri.ToString())
                .GetAwaiter()
                .GetResult();

            if (!openIdConfig.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to retrieve OpenID configuration");
            }
            else
            {
                var tokenEndpoint = openIdConfig.TokenEndpoint!;

                _tokenService = new BearerTokenService(bearerToken, tokenEndpoint);
            }
        }

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

        var wcdHost =
            (Configuration.AddEmbeddingHeader && IsWeaviateDomain(Configuration.RestAddress))
                ? Configuration.RestAddress
                : null;

        if (wcdHost != null)
        {
            httpClient.DefaultRequestHeaders.Add(
                "X-Weaviate-Cluster-URL",
                Configuration.RestUri.ToString()
            );
        }

        RestClient = new WeaviateRestClient(Configuration.RestUri, httpClient);
        GrpcClient = new WeaviateGrpcClient(Configuration.GrpcUri, wcdHost, _tokenService);

        Nodes = new NodesClient(RestClient);
        Collections = new CollectionsClient(this);
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
