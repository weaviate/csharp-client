namespace Weaviate.Client;

/// <summary>
/// Interface for authentication credentials in Weaviate client.
/// </summary>
public interface ICredentials
{
    /// <summary>
    /// Gets the scopes
    /// </summary>
    /// <returns>The string</returns>
    internal string GetScopes();
}

/// <summary>
/// Provides factory methods and credential types for Weaviate authentication.
/// </summary>
public static class Auth
{
    /// <summary>
    /// Represents API key authentication credentials.
    /// </summary>
    /// <param name="Value">The API key value.</param>
    public sealed record ApiKeyCredentials(string Value) : ICredentials
    {
        /// <summary>
        /// Gets the scopes
        /// </summary>
        /// <returns>The string</returns>
        string ICredentials.GetScopes() => "";

        /// <summary>
        /// Implicitly converts a string to ApiKeyCredentials.
        /// </summary>
        public static implicit operator ApiKeyCredentials(string value) => new(value);
    }

    /// <summary>
    /// Represents Bearer token authentication credentials.
    /// </summary>
    /// <param name="AccessToken">The access token.</param>
    /// <param name="ExpiresIn">Token expiration time in seconds.</param>
    /// <param name="RefreshToken">The refresh token.</param>
    public sealed record BearerTokenCredentials(
        string AccessToken,
        int ExpiresIn = 60,
        string RefreshToken = ""
    ) : ICredentials
    {
        /// <summary>
        /// Gets the scopes
        /// </summary>
        /// <returns>The string</returns>
        string ICredentials.GetScopes() => "";
    }

    /// <summary>
    /// Represents OAuth2 client credentials flow authentication.
    /// </summary>
    /// <param name="ClientSecret">The client secret.</param>
    /// <param name="Scope">The OAuth scopes.</param>
    public sealed record ClientCredentialsFlow(string ClientSecret, params string?[] Scope)
        : ICredentials
    {
        /// <summary>
        /// Gets the space-separated OAuth scopes.
        /// </summary>
        public string GetScopes() => string.Join(" ", Scope.Where(s => !string.IsNullOrEmpty(s)));
    }

    /// <summary>
    /// Represents OAuth2 resource owner password credentials flow authentication.
    /// </summary>
    /// <param name="Username">The username.</param>
    /// <param name="Password">The password.</param>
    /// <param name="Scope">The OAuth scopes.</param>
    public sealed record ClientPasswordFlow(
        string Username,
        string Password,
        params string?[] Scope
    ) : ICredentials
    {
        /// <summary>
        /// Gets the space-separated OAuth scopes.
        /// </summary>
        public string GetScopes() => string.Join(" ", Scope.Where(s => !string.IsNullOrEmpty(s)));
    }

    /// <summary>
    /// Creates API key authentication credentials.
    /// </summary>
    /// <param name="value">The API key value.</param>
    /// <returns>ApiKeyCredentials instance.</returns>
    public static ApiKeyCredentials ApiKey(string value) => new(value);

    /// <summary>
    /// Creates Bearer token authentication credentials.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="expiresIn">Token expiration time in seconds.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>BearerTokenCredentials instance.</returns>
    public static BearerTokenCredentials BearerToken(
        string accessToken,
        int expiresIn = 60,
        string refreshToken = ""
    ) => new(accessToken, expiresIn, refreshToken);

    /// <summary>
    /// Creates OAuth2 client credentials flow authentication.
    /// </summary>
    /// <param name="clientSecret">The client secret.</param>
    /// <param name="scope">The OAuth scopes.</param>
    /// <returns>ClientCredentialsFlow instance.</returns>
    public static ClientCredentialsFlow ClientCredentials(
        string clientSecret,
        params string?[] scope
    ) => new(clientSecret, scope);

    /// <summary>
    /// Creates OAuth2 resource owner password credentials flow authentication.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="scope">The OAuth scopes.</param>
    /// <returns>ClientPasswordFlow instance.</returns>
    public static ClientPasswordFlow ClientPassword(
        string username,
        string password,
        params string?[] scope
    ) => new(username, password, scope);
}
