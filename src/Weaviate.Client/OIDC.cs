using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

public interface ITokenService
{
    Task<string?> GetAccessTokenAsync();
    Task<bool> RefreshTokenAsync();
    bool IsAuthenticated();
}

public record OAuthConfig
{
    public required string TokenEndpoint { get; init; }
    public required string ClientId { get; init; }
    public string? ClientSecret { get; init; }
    public required string GrantType { get; init; } // "client_credentials" or "password"
    public required string Scope { get; init; }

    // For password grant type
    public string? Username { get; init; }
    public string? Password { get; init; }
}

internal class ApiKeyTokenService : ITokenService
{
    private AuthApiKey? credentialsAPIKey;

    public ApiKeyTokenService(AuthApiKey? credentialsAPIKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(credentialsAPIKey?.Value, nameof(credentialsAPIKey));
        this.credentialsAPIKey = credentialsAPIKey;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await Task.FromResult(credentialsAPIKey?.Value);
    }

    public bool IsAuthenticated()
    {
        return credentialsAPIKey != null;
    }

    public Task<bool> RefreshTokenAsync()
    {
        throw new NotImplementedException();
    }
}

internal class BearerTokenService : ITokenService
{
    private readonly AuthBearerToken _credentialsBearerToken;
    private readonly HttpClient _httpClient;
    private readonly string _tokenEndpoint;
    private readonly ILogger<BearerTokenService> _logger;
    private TokenResponse? _currentTokenResponse = null;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public BearerTokenService(
        AuthBearerToken credentialsBearerToken,
        string tokenEndpoint,
        ILogger<BearerTokenService>? logger = null
    )
    {
        _tokenEndpoint = tokenEndpoint;
        _logger =
            logger
            ?? LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<BearerTokenService>();

        _credentialsBearerToken = credentialsBearerToken;

        _httpClient = new HttpClient();
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (IsTokenExpired())
        {
            await RefreshTokenAsync();
        }

        if (_currentTokenResponse is not null)
        {
            return _currentTokenResponse?.AccessToken;
        }

        return _credentialsBearerToken?.AccessToken;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            string token =
                _currentTokenResponse?.RefreshToken ?? _credentialsBearerToken.RefreshToken;

            if (!string.IsNullOrEmpty(token))
            {
                _logger.LogDebug("Attempting to refresh access token");
                var refreshTokenResponse = await _httpClient.RequestRefreshTokenAsync(
                    new RefreshTokenRequest { Address = _tokenEndpoint, RefreshToken = token }
                );

                if (!refreshTokenResponse.IsError)
                {
                    _logger.LogDebug("Token refresh successful");
                    _currentTokenResponse = refreshTokenResponse;
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

            return !(_currentTokenResponse?.IsError ?? true);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    private bool IsTokenExpired()
    {
        var expiresIn = _currentTokenResponse?.ExpiresIn ?? _credentialsBearerToken.ExpiresIn;

        // Add a 5-minute buffer
        var expirationTime = DateTimeOffset.UtcNow.AddSeconds(expiresIn).AddMinutes(-5);
        var isExpired = DateTimeOffset.UtcNow >= expirationTime;

        if (isExpired)
        {
            _logger.LogDebug("Access token is expired or expiring soon");
        }

        return isExpired;
    }

    public bool IsAuthenticated()
    {
        return !string.IsNullOrEmpty(
                _currentTokenResponse?.AccessToken ?? _credentialsBearerToken.AccessToken
            ) && !IsTokenExpired();
    }
}

internal class OAuthTokenService : ITokenService
{
    private readonly HttpClient _httpClient;
    private readonly OAuthConfig _config;
    private readonly ILogger<OAuthTokenService> _logger;
    private TokenResponse? _currentTokenResponse = null;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    public (string? AccessToken, int? ExpiresIn, string? RefreshToken)? CurrentToken =>
        (
            _currentTokenResponse?.AccessToken,
            _currentTokenResponse?.ExpiresIn,
            _currentTokenResponse?.RefreshToken
        );

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

    public async Task<string?> GetAccessTokenAsync()
    {
        if (_currentTokenResponse?.AccessToken == null || IsTokenExpired())
        {
            await AuthenticateAsync();
        }

        return _currentTokenResponse?.AccessToken;
    }

    public async Task<bool> RefreshTokenAsync()
    {
        await _refreshSemaphore.WaitAsync();
        try
        {
            // For client credentials, we just get a new token
            if (_config.GrantType == "client_credentials")
            {
                await AuthenticateAsync();

                return !(_currentTokenResponse?.IsError ?? true);
            }

            // For password flow, try refresh token if available
            if (!string.IsNullOrEmpty(_currentTokenResponse?.RefreshToken))
            {
                _logger.LogDebug("Attempting to refresh access token");
                var refreshTokenResponse = await _httpClient.RequestRefreshTokenAsync(
                    new RefreshTokenRequest
                    {
                        Address = _config.TokenEndpoint,
                        ClientId = _config.ClientId,
                        ClientSecret = _config.ClientSecret,
                        RefreshToken = _currentTokenResponse.RefreshToken,
                        Scope = _config.Scope,
                    }
                );

                if (!refreshTokenResponse.IsError)
                {
                    _logger.LogDebug("Token refresh successful");
                    _currentTokenResponse = refreshTokenResponse;
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
            return !(_currentTokenResponse?.IsError ?? true);
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    public bool IsAuthenticated()
    {
        return _currentTokenResponse?.AccessToken != null && !IsTokenExpired();
    }

    private async Task AuthenticateAsync()
    {
        _logger.LogDebug("Starting OAuth authentication with {GrantType}", _config.GrantType);

        _currentTokenResponse = _config.GrantType switch
        {
            "client_credentials" => await RequestClientCredentialsTokenAsync(),
            "password" => await RequestPasswordTokenAsync(),
            _ => throw new NotSupportedException(
                $"Grant type '{_config.GrantType}' is not supported"
            ),
        };

        if (_currentTokenResponse.IsError)
        {
            _logger.LogError(
                "OAuth authentication failed: {Error} - {ErrorDescription}",
                _currentTokenResponse.Error,
                _currentTokenResponse.ErrorDescription
            );
            throw new AuthenticationException(
                $"OAuth authentication failed: {_currentTokenResponse.Error}"
            );
        }

        _logger.LogDebug("OAuth authentication successful");
    }

    private async Task<TokenResponse> RequestClientCredentialsTokenAsync()
    {
        return await _httpClient.RequestClientCredentialsTokenAsync(
            new ClientCredentialsTokenRequest
            {
                Address = _config.TokenEndpoint,
                ClientId = _config.ClientId,
                ClientSecret = _config.ClientSecret,
                Scope = _config.Scope,
                Method = HttpMethod.Get,
            }
        );
    }

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

    private bool IsTokenExpired()
    {
        if (_currentTokenResponse?.ExpiresIn == null)
            return true;

        // Add a 5-minute buffer
        var expirationTime = DateTimeOffset
            .UtcNow.AddSeconds(_currentTokenResponse.ExpiresIn)
            .AddMinutes(-5);
        var isExpired = DateTimeOffset.UtcNow >= expirationTime;

        if (isExpired)
        {
            _logger.LogDebug("Access token is expired or expiring soon");
        }

        return isExpired;
    }

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

public class AuthenticatedHttpHandler : DelegatingHandler
{
    private readonly ITokenService _tokenService;
    private readonly int? _refreshIntervalMinutes;
    private readonly Timer? _refreshTimer;

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

    private async Task PeriodicRefreshToken()
    {
        await _tokenService.RefreshTokenAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
