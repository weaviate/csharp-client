using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing roles and role permissions.
/// </summary>
public class RolesClient
{
    private readonly WeaviateClient _client;

    internal RolesClient(WeaviateClient client) => _client = client;

    public async Task<IEnumerable<RoleInfo>> ListAll(CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        var roles = await _client.RestClient.RolesList(cancellationToken);
        return roles.Select(r => r.ToModel());
    }

    public async Task<RoleInfo?> Get(string id, CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        var role = await _client.RestClient.RoleGet(id, cancellationToken);
        return role is null ? null : role.ToModel();
    }

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

    public async Task Delete(string id, CancellationToken cancellationToken = default)
    {
        await _client.EnsureInitializedAsync();
        await _client.RestClient.RoleDelete(id, cancellationToken);
    }

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

    public async Task<IEnumerable<UserRoleAssignment>> GetUserAssignments(
        string roleId,
        CancellationToken cancellationToken = default
    )
    {
        await _client.EnsureInitializedAsync();
        var list = await _client.RestClient.RoleUserAssignments(roleId, cancellationToken);
        return list.Select(a => a.ToModel());
    }

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
