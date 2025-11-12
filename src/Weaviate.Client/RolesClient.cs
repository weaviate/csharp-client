using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing roles and role permissions.
/// </summary>
public class RolesClient
{
    private readonly WeaviateClient _client;

    internal RolesClient(WeaviateClient client) => _client = client;

    public async Task<IEnumerable<RoleInfo>> ListAll()
    {
        var roles = await _client.RestClient.RolesList();
        return roles.Select(r => r.ToModel());
    }

    public async Task<RoleInfo?> Get(string id)
    {
        var role = await _client.RestClient.RoleGet(id);
        return role is null ? null : role.ToModel();
    }

    public async Task<RoleInfo> Create(string name, IEnumerable<PermissionScope> permissions)
    {
        var dto = new Rest.Dto.Role
        {
            Name = name,
            Permissions = permissions.SelectMany(p => p.ToDto()).Select(p => p.ToDto()).ToList(),
        };
        var created = await _client.RestClient.RoleCreate(dto);
        return created.ToModel();
    }

    public Task Delete(string id) => _client.RestClient.RoleDelete(id);

    public async Task<RoleInfo> AddPermissions(string id, IEnumerable<PermissionScope> permissions)
    {
        var dtos = permissions.SelectMany(p => p.ToDto()).Select(p => p.ToDto());
        var updated = await _client.RestClient.RoleAddPermissions(id, dtos);
        return updated.ToModel();
    }

    public async Task<RoleInfo> RemovePermissions(
        string id,
        IEnumerable<PermissionScope> permissions
    )
    {
        var dtos = permissions.SelectMany(p => p.ToDto()).Select(p => p.ToDto());
        var updated = await _client.RestClient.RoleRemovePermissions(id, dtos);
        return updated.ToModel();
    }

    public Task<bool> HasPermission(string id, PermissionScope permission)
    {
        var dto = permission.ToDto().Single().ToDto();
        return _client.RestClient.RoleHasPermission(id, dto);
    }

    public async Task<IEnumerable<UserRoleAssignment>> GetUserAssignments(string roleId)
    {
        var list = await _client.RestClient.RoleUserAssignments(roleId);
        return list.Select(a => a.ToModel());
    }

    public async Task<IEnumerable<GroupRoleAssignment>> GetGroupAssignments(string roleId)
    {
        var list = await _client.RestClient.RoleGroupAssignments(roleId);
        return list.Select(a => a.ToModel());
    }
}
