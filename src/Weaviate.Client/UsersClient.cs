using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing users (database and OIDC).
/// Provides access to database user operations via <see cref="Db"/> and OIDC user role assignments via <see cref="Oidc"/>.
/// </summary>
public class UsersClient
{
    private readonly WeaviateClient _client;

    internal UsersClient(WeaviateClient client)
    {
        _client = client;
        Db = new UsersDatabaseClient(client);
        Oidc = new UsersOidcClient(client);
    }

    /// <summary>
    /// Database users client - for CRUD operations, lifecycle management, and role assignments.
    /// </summary>
    public UsersDatabaseClient Db { get; }

    /// <summary>
    /// OIDC users client - for role assignments to OIDC users.
    /// </summary>
    public UsersOidcClient Oidc { get; }

    /// <summary>
    /// Gets information about the current authenticated user.
    /// </summary>
    public async Task<CurrentUserInfo?> OwnInfo()
    {
        var dto = await _client.RestClient.UserOwnInfoGet();
        if (dto is null)
            return null;
        return new CurrentUserInfo(
            dto.Username ?? string.Empty,
            (dto.Roles ?? []).Select(r => new RoleInfo(
                r.Name ?? string.Empty,
                (r.Permissions ?? []).Select(p => new PermissionInfo(
                    p.Action.ToEnumMemberString() ?? string.Empty
                ))
            )),
            dto.Groups
        );
    }
}
