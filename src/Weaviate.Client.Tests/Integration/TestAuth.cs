namespace Weaviate.Client.Tests.Integration;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Weaviate.Client;
using Xunit;

public class TestAuth : IntegrationTests
{
    const int ANON_PORT = 8080;
    const int OKTA_PORT_CC = 8082;
    const int OKTA_PORT_USERS = 8083;

    private static async Task<(
        bool IsSuccessStatusCode,
        string? TokenEndpoint,
        string? ClientID
    )> GetOpenIdConfig(string url)
    {
        var response = await OAuthTokenService.GetOpenIdConfig(url);

        return response;
    }

    private static async Task<(
        bool IsSuccessStatusCode,
        string? AccessToken,
        int? ExpiresIn,
        string? RefreshToken,
        string? ClientID
    )> GetAccessToken(string url, string user, string password)
    {
        var response = await OAuthTokenService.GetOpenIdConfig($"http://{url}/v1/");

        var tokenSvc = new OAuthTokenService(
            new HttpClient(),
            new()
            {
                ClientId = response.ClientID!,
                GrantType = "password",
                Password = password,
                Username = user,
                TokenEndpoint = response.TokenEndpoint!,
                Scope = "",
            }
        );

        var _ = await tokenSvc.GetAccessTokenAsync();

        if (tokenSvc.CurrentToken == null)
        {
            throw new InvalidOperationException("Failed to retrieve access token");
        }

        var currentTokenResponse = tokenSvc.CurrentToken;

        return (
            response.IsSuccessStatusCode,
            currentTokenResponse?.AccessToken ?? "",
            currentTokenResponse?.ExpiresIn ?? 0,
            currentTokenResponse?.RefreshToken ?? "",
            response.ClientID
        );
    }

    private static async Task<bool> IsAuthEnabled(string url)
    {
        return (await GetOpenIdConfig($"http://{url}/v1/")).IsSuccessStatusCode;
    }

    [Fact]
    public async Task TestNoAuthProvided()
    {
        Assert.True(await IsAuthEnabled($"localhost:{OKTA_PORT_CC}"));

        var client = Connect.Local(hostname: "localhost", restPort: OKTA_PORT_CC).Build();

        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
        {
            await client.Collections.List().ToListAsync(TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task TestAuthenticationClientCredentials_Okta()
    {
        string clientSecret = Environment.GetEnvironmentVariable("OKTA_CLIENT_SECRET")!;
        if (string.IsNullOrEmpty(clientSecret))
        {
            Assert.Skip("OKTA_CLIENT_SECRET is not set");
        }

        Assert.True(await IsAuthEnabled($"localhost:{OKTA_PORT_CC}"));

        var client = Connect
            .Local(
                hostname: "localhost",
                restPort: OKTA_PORT_CC,
                credentials: Auth.ClientCredentials(clientSecret, "some_scope"),
                httpMessageHandler: _httpMessageHandler
            )
            .Build();

        await client.Collections.List().ToListAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestAuthenticationUserPw_Okta()
    {
        string pw = Environment.GetEnvironmentVariable("OKTA_DUMMY_CI_PW")!;
        if (string.IsNullOrEmpty(pw))
        {
            Assert.Skip("OKTA_DUMMY_CI_PW is not set");
        }

        Assert.True(await IsAuthEnabled($"localhost:{OKTA_PORT_USERS}"));

        var client = Connect
            .Local(
                hostname: "localhost",
                restPort: OKTA_PORT_USERS,
                credentials: Auth.ClientPassword(
                    username: "test@test.de",
                    password: pw,
                    scope: "some_scope offline_access"
                ),
                httpMessageHandler: _httpMessageHandler
            )
            .Build();

        await client.Collections.List().ToListAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task TestClientWithAuthenticationWithAnonWeaviate()
    {
        Assert.False(await IsAuthEnabled($"localhost:{ANON_PORT}"));

        var client = Connect.Local(
            hostname: "localhost",
            restPort: ANON_PORT,
            credentials: Auth.ClientPassword(
                username: "someUser",
                password: "SomePw",
                scope: "some_scope"
            ),
            httpMessageHandler: _httpMessageHandler
        );

        // TODO Needs a finalized way to inject a logger and check that no warnings were logged
    }

    [Fact]
    public async Task TestAuthenticationWithBearerToken_Okta()
    {
        string url = $"localhost:{OKTA_PORT_USERS}";
        Assert.True(await IsAuthEnabled(url));
        string pw = Environment.GetEnvironmentVariable("OKTA_DUMMY_CI_PW")!;
        if (string.IsNullOrEmpty(pw))
        {
            Assert.Skip("OKTA_DUMMY_CI_PW is not set");
        }

        var token = await GetAccessToken(url, "test@test.de", pw);
        var auth = Auth.BearerToken(
            accessToken: token.AccessToken ?? "",
            expiresIn: token.ExpiresIn ?? 0,
            refreshToken: token.RefreshToken ?? ""
        );

        var client = Connect
            .Local(
                hostname: "localhost",
                restPort: OKTA_PORT_USERS,
                credentials: auth,
                httpMessageHandler: _httpMessageHandler
            )
            .Build();

        await client.Collections.Exists("something");
    }

    [Fact]
    public async Task TestAuthenticationWithBearerTokenNoRefresh()
    {
        string url = $"localhost:{OKTA_PORT_USERS}";
        Assert.True(await IsAuthEnabled(url));
        string pw = Environment.GetEnvironmentVariable("OKTA_DUMMY_CI_PW")!;
        if (string.IsNullOrEmpty(pw))
        {
            Assert.Skip("OKTA_DUMMY_CI_PW is not set");
        }

        var token = await GetAccessToken(url, "test@test.de", pw);
        var auth = Auth.BearerToken(
            accessToken: token.AccessToken ?? "",
            expiresIn: token.ExpiresIn ?? 0,
            refreshToken: "" // No refresh token provided
        );

        var client = Connect
            .Local(
                hostname: "localhost",
                restPort: OKTA_PORT_USERS,
                credentials: auth,
                httpMessageHandler: _httpMessageHandler
            )
            .Build();

        await client.Collections.List().ToListAsync(TestContext.Current.CancellationToken);

        // TODO Needs a finalized way to inject a logger and check that no warnings were logged
    }
}
