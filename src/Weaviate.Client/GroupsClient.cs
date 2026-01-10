namespace Weaviate.Client;

/// <summary>
/// Client for managing groups.
/// Provides access to OIDC group operations via <see cref="Oidc"/>.
/// </summary>
public class GroupsClient
{
    /// <summary>
    /// The client
    /// </summary>
    private readonly WeaviateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="GroupsClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
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
