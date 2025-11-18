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
    public Task AssignRoles(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    ) =>
        _client.RestClient.UserAssignRoles(
            userId,
            UserType.ToEnumMemberString(),
            roles,
            cancellationToken
        );

    /// <summary>
    /// Revokes roles from an OIDC user.
    /// </summary>
    public Task RevokeRoles(
        string userId,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    ) =>
        _client.RestClient.UserRevokeRoles(
            userId,
            UserType.ToEnumMemberString(),
            roles,
            cancellationToken
        );

    /// <summary>
    /// Gets all roles assigned to an OIDC user.
    /// </summary>
    public async Task<IEnumerable<RoleInfo>> GetRoles(
        string userId,
        bool? includeFullRoles = null,
        CancellationToken cancellationToken = default
    )
    {
        var roles = await _client.RestClient.UserRolesGet(
            userId,
            UserType.ToEnumMemberString(),
            includeFullRoles,
            cancellationToken
        );
        return roles.Select(r => r.ToModel());
    }
}
