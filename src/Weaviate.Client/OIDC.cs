using System.Net;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text.Json;
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
    private Auth.ApiKeyCredentials credentialsAPIKey;

    public ApiKeyTokenService(Auth.ApiKeyCredentials credentialsAPIKey)
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
        return Task.FromResult(true);
    }
}

internal class OAuthTokenService : ITokenService
{
    internal record OAuthTokenResponse(string? AccessToken, int? ExpiresIn, string? RefreshToken)
    {
        public bool IsError { get; internal set; }
    }

    private readonly HttpClient _httpClient;
    private readonly OAuthConfig _config;
    private readonly ILogger<OAuthTokenService> _logger;
    private readonly SemaphoreSlim _refreshSemaphore = new(1, 1);

    internal OAuthTokenResponse? CurrentToken { get; set; }

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
        if (CurrentToken?.AccessToken == null || IsTokenExpired())
        {
            await AuthenticateAsync();
        }

        return CurrentToken?.AccessToken;
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

    public bool IsAuthenticated()
    {
        return CurrentToken?.AccessToken != null && !IsTokenExpired();
    }

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
