using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

/// <summary>
/// The token service interface
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Gets the access token
    /// </summary>
    /// <returns>A task containing the string</returns>
    Task<string?> GetAccessTokenAsync();

    /// <summary>
    /// Refreshes the token
    /// </summary>
    /// <returns>A task containing the bool</returns>
    Task<bool> RefreshTokenAsync();

    /// <summary>
    /// Ises the authenticated
    /// </summary>
    /// <returns>The bool</returns>
    bool IsAuthenticated();
}

/// <summary>
/// The auth config
/// </summary>
public record OAuthConfig
{
    /// <summary>
    /// Gets or inits the value of the token endpoint
    /// </summary>
    public required string TokenEndpoint { get; init; }

    /// <summary>
    /// Gets or inits the value of the client id
    /// </summary>
    public required string ClientId { get; init; }

    /// <summary>
    /// Gets or inits the value of the client secret
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <summary>
    /// Gets or inits the value of the grant type
    /// </summary>
    public required string GrantType { get; init; } // "client_credentials" or "password"

    /// <summary>
    /// Gets or inits the value of the scope
    /// </summary>
    public required string Scope { get; init; }

    // For password grant type
    /// <summary>
    /// Gets or inits the value of the username
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// Gets or inits the value of the password
    /// </summary>
    public string? Password { get; init; }
}

/// <summary>
/// The api key token service class
/// </summary>
/// <seealso cref="ITokenService"/>
internal class ApiKeyTokenService : ITokenService
{
    /// <summary>
    /// The credentials api key
    /// </summary>
    private Auth.ApiKeyCredentials credentialsAPIKey;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyTokenService"/> class
    /// </summary>
    /// <param name="credentialsAPIKey">The credentials api key</param>
    public ApiKeyTokenService(Auth.ApiKeyCredentials credentialsAPIKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(credentialsAPIKey?.Value, nameof(credentialsAPIKey));
        this.credentialsAPIKey = credentialsAPIKey;
    }

    /// <summary>
    /// Gets the access token
    /// </summary>
    /// <returns>A task containing the string</returns>
    public async Task<string?> GetAccessTokenAsync()
    {
        return await Task.FromResult(credentialsAPIKey?.Value);
    }

    /// <summary>
    /// Ises the authenticated
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsAuthenticated()
    {
        return credentialsAPIKey != null;
    }

    /// <summary>
    /// Refreshes the token
    /// </summary>
    /// <returns>A task containing the bool</returns>
    public Task<bool> RefreshTokenAsync()
    {
        return Task.FromResult(true);
    }
}

/// <summary>
/// The auth token service class
/// </summary>
/// <seealso cref="ITokenService"/>
internal class OAuthTokenService : ITokenService
{
    /// <summary>
    /// The auth token response
    /// </summary>
    internal record OAuthTokenResponse(string? AccessToken, int? ExpiresIn, string? RefreshToken)
    {
        /// <summary>
        /// Gets or sets the value of the is error
        /// </summary>
        public bool IsError { get; internal set; }
    }

    /// <summary>
    /// The http client
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The config
    /// </summary>
    private readonly OAuthConfig _config;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<OAuthTokenService> _logger;

    /// <summary>
    /// The refresh semaphore
    /// </summary>
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    /// <summary>
    /// Gets or sets the value of the current token
    /// </summary>
    internal OAuthTokenResponse? CurrentToken { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OAuthTokenService"/> class
    /// </summary>
    /// <param name="httpClient">The http client</param>
    /// <param name="config">The config</param>
    /// <param name="logger">The logger</param>
    public OAuthTokenService(
        HttpClient httpClient,
        OAuthConfig config,
        ILogger<OAuthTokenService>? logger = null
    )
    {
        _httpClient = httpClient;
        _config = config;
        _logger =
            logger
            ?? LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<OAuthTokenService>();
    }

    /// <summary>
    /// Gets the access token
    /// </summary>
    /// <returns>A task containing the string</returns>
    public async Task<string?> GetAccessTokenAsync()
    {
        if (CurrentToken?.AccessToken == null || IsTokenExpired())
        {
            await AuthenticateAsync();
        }

        return CurrentToken?.AccessToken;
    }

    /// <summary>
    /// Refreshes the token
    /// </summary>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> RefreshTokenAsync()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            // For client credentials, we just get a new token
            if (_config.GrantType == "client_credentials")
            {
                await AuthenticateAsync();

                return !(CurrentToken?.IsError ?? true);
            }

            // For password flow, try refresh token if available
            if (!string.IsNullOrEmpty(CurrentToken?.RefreshToken))
            {
                _logger.LogDebug("Attempting to refresh access token");
                var refreshTokenResponse = await _httpClient.RequestRefreshTokenAsync(
                    new RefreshTokenRequest
                    {
                        Address = _config.TokenEndpoint,
                        ClientId = _config.ClientId,
                        ClientSecret = _config.ClientSecret,
                        RefreshToken = CurrentToken.RefreshToken,
                        Scope = _config.Scope,
                    }
                );

                if (!refreshTokenResponse.IsError)
                {
                    _logger.LogDebug("Token refresh successful");
                    CurrentToken = new OAuthTokenResponse(
                        refreshTokenResponse.AccessToken,
                        refreshTokenResponse.ExpiresIn,
                        refreshTokenResponse.RefreshToken
                    );
                    return true;
                }
                else
                {
                    _logger.LogWarning(
                        "Token refresh failed: {Error} - {ErrorDescription}",
                        refreshTokenResponse.Error,
                        refreshTokenResponse.ErrorDescription
                    );
                }
            }

            // Fallback to full authentication
            await AuthenticateAsync();
            return !(CurrentToken?.IsError ?? true);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    /// <summary>
    /// Ises the authenticated
    /// </summary>
    /// <returns>The bool</returns>
    public bool IsAuthenticated()
    {
        return CurrentToken?.AccessToken != null && !IsTokenExpired();
    }

    /// <summary>
    /// Authenticates this instance
    /// </summary>
    /// <exception cref="NotSupportedException">Grant type '{_config.GrantType}' is not supported</exception>
    /// <exception cref="WeaviateAuthenticationException">OAuth authentication failed: {tokenResponse.Error}</exception>
    private async Task AuthenticateAsync()
    {
        _logger.LogDebug("Starting OAuth authentication with {GrantType}", _config.GrantType);

        var tokenResponse = _config.GrantType switch
        {
            "client_credentials" => await RequestClientCredentialsTokenAsync(),
            "password" => await RequestPasswordTokenAsync(),
            _ => throw new NotSupportedException(
                $"Grant type '{_config.GrantType}' is not supported"
            ),
        };

        if (tokenResponse.IsError)
        {
            _logger.LogError(
                "OAuth authentication failed: {Error} - {ErrorDescription}",
                tokenResponse.Error,
                tokenResponse.ErrorDescription
            );
            throw new WeaviateAuthenticationException(
                $"OAuth authentication failed: {tokenResponse.Error}"
            );
        }

        CurrentToken = new OAuthTokenResponse(
            tokenResponse.AccessToken,
            tokenResponse.ExpiresIn,
            tokenResponse.RefreshToken
        )
        {
            IsError = tokenResponse.IsError,
        };

        _logger.LogDebug("OAuth authentication successful");
    }

    /// <summary>
    /// Requests the client credentials token
    /// </summary>
    /// <returns>A task containing the token response</returns>
    private async Task<TokenResponse> RequestClientCredentialsTokenAsync()
    {
        return await _httpClient.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                Address = _config.TokenEndpoint,
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret,
                Scope = _config.Scope,
            }
        );
    }

    /// <summary>
    /// Requests the password token
    /// </summary>
    /// <exception cref="InvalidOperationException">Username and Password are required for password grant type</exception>
    /// <returns>A task containing the token response</returns>
    private async Task<TokenResponse> RequestPasswordTokenAsync()
    {
        if (string.IsNullOrEmpty(_config.Username) || string.IsNullOrEmpty(_config.Password))
        {
            throw new InvalidOperationException(
                "Username and Password are required for password grant type"
            );
        }

        return await _httpClient.RequestPasswordTokenAsync(
            new PasswordTokenRequest
            {
                Address = _config.TokenEndpoint,
                ClientId = _config.ClientId,
                ClientCredentialStyle = ClientCredentialStyle.PostBody,
                UserName = _config.Username,
                Password = _config.Password,
                Scope = _config.Scope,
            }
        );
    }

    /// <summary>
    /// Ises the token expired
    /// </summary>
    /// <returns>The is expired</returns>
    private bool IsTokenExpired()
    {
        if (CurrentToken?.ExpiresIn == null)
        {
            return true;
        }

        // Add a 1-minute buffer
        var expirationTime = DateTimeOffset
            .UtcNow.AddSeconds(CurrentToken?.ExpiresIn ?? 60)
            .AddMinutes(-1);
        var isExpired = DateTimeOffset.UtcNow >= expirationTime;

        if (isExpired)
        {
            _logger.LogDebug("Access token is expired or expiring soon");
        }

        return isExpired;
    }

    /// <summary>
    /// Gets the open id config using the specified url
    /// </summary>
    /// <param name="url">The url</param>
    /// <returns>A task containing the bool is success status code string token endpoint string client id</returns>
    public static async Task<(
        bool IsSuccessStatusCode,
        string? TokenEndpoint,
        string? ClientID
    )> GetOpenIdConfig(string url)
    {
        var isEndpointReachable = false;

        using var client = new HttpClient();
        try
        {
            var response = await client.GetAsync($"{url}.well-known/openid-configuration");

            isEndpointReachable = response.IsSuccessStatusCode;

            if (!response.IsSuccessStatusCode)
            {
                return (isEndpointReachable, null, null);
            }

            var oidcConfigJson = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var href = oidcConfigJson.RootElement.GetProperty("href").GetString();
            var clientId = oidcConfigJson.RootElement.GetProperty("clientId").GetString();

            var responseIdp = await client.GetAsync(href);

            isEndpointReachable &= responseIdp.IsSuccessStatusCode;

            var idpProviderConfigJson = JsonDocument.Parse(
                await responseIdp.Content.ReadAsStringAsync()
            );

            var tokenEndpoint = idpProviderConfigJson
                .RootElement.GetProperty("token_endpoint")
                .GetString();

            return (isEndpointReachable, tokenEndpoint, clientId);
        }
        catch
        {
            return (isEndpointReachable, null, null);
        }
    }
}

/// <summary>
/// The authenticated http handler class
/// </summary>
/// <seealso cref="DelegatingHandler"/>
public class AuthenticatedHttpHandler : DelegatingHandler
{
    /// <summary>
    /// The token service
    /// </summary>
    private readonly ITokenService _tokenService;

    /// <summary>
    /// The refresh interval minutes
    /// </summary>
    private readonly int? _refreshIntervalMinutes;

    /// <summary>
    /// The refresh timer
    /// </summary>
    private readonly Timer? _refreshTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticatedHttpHandler"/> class
    /// </summary>
    /// <param name="tokenService">The token service</param>
    /// <param name="refreshIntervalMinutes">The refresh interval minutes</param>
    public AuthenticatedHttpHandler(ITokenService tokenService, int? refreshIntervalMinutes = 5)
    {
        _tokenService = tokenService;
        _refreshIntervalMinutes = refreshIntervalMinutes;

        if (_refreshIntervalMinutes.HasValue)
        {
            _refreshTimer = new Timer(
                async _ => await PeriodicRefreshToken(),
                null,
                TimeSpan.FromMinutes(_refreshIntervalMinutes.Value),
                TimeSpan.FromMinutes(_refreshIntervalMinutes.Value)
            );
        }
    }

    /// <summary>
    /// Sends the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The response</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await _tokenService.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        // Handle 401 by refreshing token and retrying once
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            if (await _tokenService.RefreshTokenAsync())
            {
                var newToken = await _tokenService.GetAccessTokenAsync();
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, cancellationToken);
            }
        }

        return response;
    }

    /// <summary>
    /// Periodics the refresh token
    /// </summary>
    private async Task PeriodicRefreshToken()
    {
        await _tokenService.RefreshTokenAsync();
    }

    /// <summary>
    /// Disposes the disposing
    /// </summary>
    /// <param name="disposing">The disposing</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
