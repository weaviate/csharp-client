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

    public async Task<RoleInfo> Create(string name, IEnumerable<PermissionInfo> permissions)
    {
        var dto = new Rest.Dto.Role
        {
            Name = name,
            Permissions = permissions
                .Select(p => new Rest.Dto.Permission { Action = MapActionOrThrow(p) })
                .ToList(),
        };
        var created = await _client.RestClient.RoleCreate(dto);
        return created.ToModel();
    }

    public Task Delete(string id) => _client.RestClient.RoleDelete(id);

    public async Task<RoleInfo> AddPermissions(string id, IEnumerable<PermissionInfo> permissions)
    {
        var dtos = permissions.Select(p => new Rest.Dto.Permission
        {
            Action = MapActionOrThrow(p),
        });
        var updated = await _client.RestClient.RoleAddPermissions(id, dtos);
        return updated.ToModel();
    }

    public async Task<RoleInfo> RemovePermissions(
        string id,
        IEnumerable<PermissionInfo> permissions
    )
    {
        var dtos = permissions.Select(p => new Rest.Dto.Permission
        {
            Action = MapActionOrThrow(p),
        });
        var updated = await _client.RestClient.RoleRemovePermissions(id, dtos);
        return updated.ToModel();
    }

    public Task<bool> HasPermission(string id, PermissionInfo permission)
    {
        var dto = new Rest.Dto.Permission { Action = MapActionOrThrow(permission) };
        return _client.RestClient.RoleHasPermission(id, dto);
    }

    public async Task<IEnumerable<UserRoleAssignment>> GetUserAssignments(string roleId)
    {
        var list = await _client.RestClient.RoleUserAssignments(roleId);
        return list.Select(a => new UserRoleAssignment(
            a.userId,
            a.userType.ToEnumMemberString().FromEnumMemberString<RbacUserType>()
        ));
    }

    public async Task<IEnumerable<GroupRoleAssignment>> GetGroupAssignments(string roleId)
    {
        var list = await _client.RestClient.RoleGroupAssignments(roleId);
        return list.Select(a => new GroupRoleAssignment(
            a.groupId,
            a.groupType.ToEnumMemberString().FromEnumMemberString<RbacGroupType>()
        ));
    }

    private static Rest.Dto.PermissionAction MapActionOrThrow(PermissionInfo permission)
    {
        var mapped = permission.ActionRaw.FromEnumMemberString<Rest.Dto.PermissionAction>();
        // Detect unknown mapping: if the enum's wire value does not match the raw string, the raw string is unsupported.
        if (mapped.ToEnumMemberString() != permission.ActionRaw)
        {
            throw new InvalidOperationException(
                $"Unknown permission action '{permission.ActionRaw}'. Update the Weaviate C# client to a newer version that knows this action."
            );
        }
        return mapped;
    }
}
