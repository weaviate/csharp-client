using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing OIDC groups - listing and role assignments.
/// Accessed via <see cref="GroupsClient.Oidc"/>.
/// </summary>
public class GroupsOidcClient
{
    private readonly WeaviateClient _client;
    private const string GroupType = "oidc";

    internal GroupsOidcClient(WeaviateClient client) => _client = client;

    /// <summary>
    /// Lists all OIDC groups.
    /// </summary>
    public Task<IEnumerable<string>> List() => _client.RestClient.GroupsList(GroupType);

    /// <summary>
    /// Assigns roles to an OIDC group.
    /// </summary>
    public Task<bool> AssignRoles(string groupId, IEnumerable<string> roles) =>
        _client.RestClient.GroupAssignRoles(groupId, GroupType, roles);

    /// <summary>
    /// Revokes roles from an OIDC group.
    /// </summary>
    public Task<bool> RevokeRoles(string groupId, IEnumerable<string> roles) =>
        _client.RestClient.GroupRevokeRoles(groupId, GroupType, roles);

    /// <summary>
    /// Gets all roles assigned to an OIDC group.
    /// </summary>
    public async Task<IEnumerable<RoleInfo>> GetRoles(string groupId, bool? includeFullRoles = null)
    {
        var roles = await _client.RestClient.GroupRolesGet(groupId, GroupType, includeFullRoles);
        return roles.Select(r => new RoleInfo(
            r.Name ?? string.Empty,
            (r.Permissions ?? []).Select(p => new PermissionInfo(
                p.Action.ToEnumMemberString() ?? string.Empty
            ))
        ));
    }
}
