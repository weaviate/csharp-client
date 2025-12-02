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
