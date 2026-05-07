namespace Weaviate.Client;

/// <summary>
/// The default token service factory class
/// </summary>
/// <seealso cref="ITokenServiceFactory"/>
public class DefaultTokenServiceFactory : ITokenServiceFactory
{
    /// <summary>
    /// Creates the configuration
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <exception cref="NotSupportedException">Unsupported credentials type</exception>
    /// <returns>A task containing the token service</returns>
    public async Task<ITokenService?> CreateAsync(ClientConfiguration configuration)
    {
        if (configuration.Credentials is null)
            return null;

        if (configuration.Credentials is Auth.ApiKeyCredentials apiKey)
            return new ApiKeyTokenService(apiKey);

        var openIdConfig = await OAuthTokenService.GetOpenIdConfig(
            configuration.RestUri.ToString()
        );
        if (!openIdConfig.IsSuccessStatusCode)
            return null;

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

        // Guard against HttpClient leak if an exception is thrown between creation and handoff.
        var httpClient = new HttpClient();
        try
        {
            if (configuration.Credentials is Auth.BearerTokenCredentials bearerToken)
            {
                return new OAuthTokenService(httpClient, oauthConfig, configuration.LoggerFactory)
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

            return new OAuthTokenService(httpClient, oauthConfig, configuration.LoggerFactory);
        }
        catch
        {
            httpClient.Dispose();
            throw;
        }
    }

    /// <summary>
    /// Creates a token service synchronously. Performs a blocking network call to discover
    /// the OIDC endpoint; prefer <see cref="CreateAsync"/> where possible.
    /// </summary>
    /// <param name="configuration">The configuration</param>
    /// <exception cref="NotSupportedException">Unsupported credentials type</exception>
    /// <returns>The token service, or <c>null</c> if no credentials are configured or OIDC discovery fails.</returns>
    [Obsolete(
        "CreateSync() blocks the calling thread and can deadlock in ASP.NET or any single-threaded SynchronizationContext. Use CreateAsync() instead.",
        error: false
    )]
    public ITokenService? CreateSync(ClientConfiguration configuration)
    {
        if (configuration.Credentials is null)
            return null;

        if (configuration.Credentials is Auth.ApiKeyCredentials apiKey)
            return new ApiKeyTokenService(apiKey);

        // Task.Run escapes any captured SynchronizationContext so the await
        // inside GetOpenIdConfig can schedule continuations on the thread pool.
        // GetOpenIdConfig swallows network exceptions and signals failure via IsSuccessStatusCode.
        var openIdConfig = Task.Run(() =>
                OAuthTokenService.GetOpenIdConfig(configuration.RestUri.ToString())
            )
            .GetAwaiter()
            .GetResult();

        if (!openIdConfig.IsSuccessStatusCode)
            return null;

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
        try
        {
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

            return new OAuthTokenService(httpClient, oauthConfig, configuration.LoggerFactory);
        }
        catch
        {
            httpClient.Dispose();
            throw;
        }
    }
}
