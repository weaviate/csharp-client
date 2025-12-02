using System.Net.Http;

namespace Weaviate.Client;

public class DefaultTokenServiceFactory : ITokenServiceFactory
{
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

    public ITokenService? CreateSync(ClientConfiguration configuration)
    {
        if (configuration.Credentials is null)
            return null;

        if (configuration.Credentials is Auth.ApiKeyCredentials apiKey)
            return new ApiKeyTokenService(apiKey);

        try
        {
            var openIdConfig = OAuthTokenService
                .GetOpenIdConfig(configuration.RestUri.ToString())
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
}
