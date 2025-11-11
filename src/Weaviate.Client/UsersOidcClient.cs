using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing OIDC user role assignments.
/// Accessed via <see cref="UsersClient.Oidc"/>.
/// </summary>
public class UsersOidcClient
{
    private readonly WeaviateClient _client;
    private const RbacUserType UserType = RbacUserType.Oidc;

    internal UsersOidcClient(WeaviateClient client) => _client = client;

    /// <summary>
    /// Assigns roles to an OIDC user.
    /// </summary>
    public Task<bool> AssignRoles(string userId, IEnumerable<string> roles) =>
        _client.RestClient.UserAssignRoles(userId, UserType.ToEnumMemberString(), roles);

    /// <summary>
    /// Revokes roles from an OIDC user.
    /// </summary>
    public Task<bool> RevokeRoles(string userId, IEnumerable<string> roles) =>
        _client.RestClient.UserRevokeRoles(userId, UserType.ToEnumMemberString(), roles);

    /// <summary>
    /// Gets all roles assigned to an OIDC user.
    /// </summary>
    public async Task<IEnumerable<RoleInfo>> GetRoles(string userId, bool? includeFullRoles = null)
    {
        var roles = await _client.RestClient.UserRolesGet(
            userId,
            UserType.ToEnumMemberString(),
            includeFullRoles
        );
        return roles.Select(r => new RoleInfo(
            r.Name ?? string.Empty,
            (r.Permissions ?? []).Select(p => new PermissionInfo(
                p.Action.ToEnumMemberString() ?? string.Empty
            ))
        ));
    }
}
