using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// Client for managing roles and role permissions.
/// </summary>
public class RolesClient
{
    private readonly WeaviateClient _client;

    internal RolesClient(WeaviateClient client) => _client = client;

    public async Task<IEnumerable<RoleInfo>> List()
    {
        var roles = await _client.RestClient.RolesList();
        return roles.Select(ToModel);
    }

    public async Task<RoleInfo?> Get(string id)
    {
        var role = await _client.RestClient.RoleGet(id);
        return role is null ? null : ToModel(role);
    }

    public Task<bool> Create(string name, IEnumerable<PermissionInfo> permissions)
    {
        var dto = new Rest.Dto.Role
        {
            Name = name,
            Permissions = permissions
                .Select(p => new Rest.Dto.Permission
                {
                    Action = p.Action.FromEnumMemberString<Rest.Dto.PermissionAction>(),
                })
                .ToList(),
        };
        return _client.RestClient.RoleCreate(dto);
    }

    public Task<bool> Delete(string id) => _client.RestClient.RoleDelete(id);

    public Task<bool> AddPermissions(string id, IEnumerable<PermissionInfo> permissions)
    {
        var dtos = permissions.Select(p => new Rest.Dto.Permission
        {
            Action = p.Action.FromEnumMemberString<Rest.Dto.PermissionAction>(),
        });
        return _client.RestClient.RoleAddPermissions(id, dtos);
    }

    public Task<bool> RemovePermissions(string id, IEnumerable<PermissionInfo> permissions)
    {
        var dtos = permissions.Select(p => new Rest.Dto.Permission
        {
            Action = p.Action.FromEnumMemberString<Rest.Dto.PermissionAction>(),
        });
        return _client.RestClient.RoleRemovePermissions(id, dtos);
    }

    public Task<bool?> HasPermission(string id, PermissionInfo permission)
    {
        var dto = new Rest.Dto.Permission
        {
            Action = permission.Action.FromEnumMemberString<Rest.Dto.PermissionAction>(),
        };
        return _client.RestClient.RoleHasPermission(id, dto);
    }

    public async Task<IEnumerable<UserRoleAssignment>> UserAssignments(string roleId)
    {
        var list = await _client.RestClient.RoleUserAssignments(roleId);
        return list.Select(a => new UserRoleAssignment(a.userId, a.userType.ToEnumMemberString()!));
    }

    public async Task<IEnumerable<GroupRoleAssignment>> GroupAssignments(string roleId)
    {
        var list = await _client.RestClient.RoleGroupAssignments(roleId);
        return list.Select(a => new GroupRoleAssignment(
            a.groupId,
            a.groupType.ToEnumMemberString()!
        ));
    }

    public Task<bool> AssignToUser(string userId, string userType, IEnumerable<string> roles) =>
        _client.RestClient.UserAssignRoles(userId, userType, roles);

    public Task<bool> RevokeFromUser(string userId, string userType, IEnumerable<string> roles) =>
        _client.RestClient.UserRevokeRoles(userId, userType, roles);

    public Task<bool> AssignToGroup(string groupId, string groupType, IEnumerable<string> roles) =>
        _client.RestClient.GroupAssignRoles(groupId, groupType, roles);

    public Task<bool> RevokeFromGroup(
        string groupId,
        string groupType,
        IEnumerable<string> roles
    ) => _client.RestClient.GroupRevokeRoles(groupId, groupType, roles);

    public async Task<IEnumerable<RoleInfo>> RolesForUser(
        string userId,
        string userType,
        bool? includeFullRoles = null
    )
    {
        var roles = await _client.RestClient.UserRolesGet(userId, userType, includeFullRoles);
        return roles.Select(ToModel);
    }

    public async Task<IEnumerable<RoleInfo>> RolesForGroup(
        string groupId,
        string groupType,
        bool? includeFullRoles = null
    )
    {
        var roles = await _client.RestClient.GroupRolesGet(groupId, groupType, includeFullRoles);
        return roles.Select(ToModel);
    }

    private static RoleInfo ToModel(Rest.Dto.Role dto) =>
        new(
            dto.Name ?? string.Empty,
            (dto.Permissions ?? []).Select(p => new PermissionInfo(
                p.Action.ToEnumMemberString() ?? string.Empty
            ))
        );
}
