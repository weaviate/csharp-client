using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing roles and role permissions.
/// </summary>
public class RolesClient
{
    /// <summary>
    /// The client
    /// </summary>
    private readonly WeaviateClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="RolesClient"/> class
    /// </summary>
    /// <param name="client">The client</param>
    internal RolesClient(WeaviateClient client) => _client = client;

    /// <summary>
    /// Lists the all using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of role info</returns>
    public async Task<IEnumerable<RoleInfo>> ListAll(CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        var roles = await _client.RestClient.RolesList(cancellationToken);
        return roles.Select(r => r.ToModel());
    }

    /// <summary>
    /// Gets the id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the role info</returns>
    public async Task<RoleInfo?> Get(string id, CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        var role = await _client.RestClient.RoleGet(id, cancellationToken);
        return role is null ? null : role.ToModel();
    }

    /// <summary>
    /// Creates the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <param name="permissions">The permissions</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the role info</returns>
    public async Task<RoleInfo> Create(
        string name,
        IEnumerable<PermissionScope> permissions,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dto = new Rest.Dto.Role
        {
            Name = name,
            Permissions = permissions.SelectMany(p => p.ToDto()).ToList(),
        };
        var created = await _client.RestClient.RoleCreate(dto, cancellationToken);
        return created.ToModel();
    }

    /// <summary>
    /// Deletes the id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    public async Task Delete(string id, CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        await _client.RestClient.RoleDelete(id, cancellationToken);
    }

    /// <summary>
    /// Adds the permissions using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="permissions">The permissions</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the role info</returns>
    public async Task<RoleInfo> AddPermissions(
        string id,
        IEnumerable<PermissionScope> permissions,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dtos = permissions.SelectMany(p => p.ToDto()).ToList();
        var updated = await _client.RestClient.RoleAddPermissions(id, dtos, cancellationToken);
        return updated.ToModel();
    }

    /// <summary>
    /// Removes the permissions using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="permissions">The permissions</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the role info</returns>
    public async Task<RoleInfo> RemovePermissions(
        string id,
        IEnumerable<PermissionScope> permissions,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dtos = permissions.SelectMany(p => p.ToDto()).ToList();
        var updated = await _client.RestClient.RoleRemovePermissions(id, dtos, cancellationToken);
        return updated.ToModel();
    }

    /// <summary>
    /// Hases the permission using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="permission">The permission</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the bool</returns>
    public async Task<bool> HasPermission(
        string id,
        PermissionScope permission,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var dto = permission.ToDto().Single();
        return await _client.RestClient.RoleHasPermission(id, dto, cancellationToken);
    }

    /// <summary>
    /// Gets the user assignments using the specified role id
    /// </summary>
    /// <param name="roleId">The role id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of user role assignment</returns>
    public async Task<IEnumerable<UserRoleAssignment>> GetUserAssignments(
        string roleId,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var list = await _client.RestClient.RoleUserAssignments(roleId, cancellationToken);
        return list.Select(a => a.ToModel());
    }

    /// <summary>
    /// Gets the group assignments using the specified role id
    /// </summary>
    /// <param name="roleId">The role id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of group role assignment</returns>
    public async Task<IEnumerable<GroupRoleAssignment>> GetGroupAssignments(
        string roleId,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var list = await _client.RestClient.RoleGroupAssignments(roleId, cancellationToken);
        return list.Select(a => a.ToModel());
    }
}
