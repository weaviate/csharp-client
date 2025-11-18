using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing OIDC groups - listing and role assignments.
/// Accessed via <see cref="GroupsClient.Oidc"/>.
/// </summary>
public class GroupsOidcClient
{
    private readonly WeaviateClient _client;
    private const RbacGroupType GroupType = RbacGroupType.Oidc;

    internal GroupsOidcClient(WeaviateClient client) => _client = client;

    /// <summary>
    /// Lists all OIDC groups.
    /// </summary>
    public Task<IEnumerable<string>> GetKnownGroupNames(
        CancellationToken cancellationToken = default
    ) => _client.RestClient.GroupsList(GroupType.ToEnumMemberString(), cancellationToken);

    /// <summary>
    /// Assigns roles to an OIDC group.
    /// </summary>
    public Task AssignRoles(
        string groupId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    ) =>
        _client.RestClient.GroupAssignRoles(
            groupId,
            GroupType.ToEnumMemberString(),
            roles,
            cancellationToken
        );

    /// <summary>
    /// Revokes roles from an OIDC group.
    /// </summary>
    public Task RevokeRoles(
        string groupId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    ) =>
        _client.RestClient.GroupRevokeRoles(
            groupId,
            GroupType.ToEnumMemberString(),
            roles,
            cancellationToken
        );

    /// <summary>
    /// Gets all roles assigned to an OIDC group.
    /// </summary>
    public async Task<IEnumerable<RoleInfo>> GetRoles(
        string groupId,
        bool? includeFullRoles = null,
        CancellationToken cancellationToken = default
    )
    {
        var roles = await _client.RestClient.GroupRolesGet(
            groupId,
            GroupType.ToEnumMemberString(),
            includeFullRoles,
            cancellationToken
        );
        return roles.Select(r => r.ToModel());
    }
}
