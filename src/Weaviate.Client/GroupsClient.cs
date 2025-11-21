namespace Weaviate.Client;

/// <summary>
/// Client for managing groups.
/// Provides access to OIDC group operations via <see cref="Oidc"/>.
/// </summary>
public class GroupsClient
{
    private readonly WeaviateClient _client;

    internal GroupsClient(WeaviateClient client)
    {
        _client = client;
        Oidc = new GroupsOidcClient(client);
    }

    /// <summary>
    /// OIDC groups client - for listing groups and managing role assignments.
    /// </summary>
    public GroupsOidcClient Oidc { get; }
}
