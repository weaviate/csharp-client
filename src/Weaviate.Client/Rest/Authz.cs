using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    // Roles
    internal async Task<IEnumerable<Dto.Role>> RolesList()
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Roles());
        await response.EnsureExpectedStatusCodeAsync([200], "list roles");
        var list = await response.Content.ReadFromJsonAsync<Dto.RolesListResponse>(
            RestJsonSerializerOptions
        );
        return list is null ? Array.Empty<Dto.Role>() : list;
    }

    internal async Task<Dto.Role?> RoleGet(string id)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Role(id));
        var status = await response.EnsureExpectedStatusCodeAsync([200, 404], "get role");
        return status == HttpStatusCode.OK
            ? await response.Content.ReadFromJsonAsync<Dto.Role>(RestJsonSerializerOptions)
            : null;
    }

    internal async Task<bool> RoleDelete(string id)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Role(id));
        var status = await response.EnsureExpectedStatusCodeAsync([204, 404], "delete role");
        return status == HttpStatusCode.NoContent;
    }

    internal async Task<bool> RoleCreate(Dto.Role role)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Roles(),
            role,
            options: RestJsonSerializerOptions
        );
        var status = await response.EnsureExpectedStatusCodeAsync([201, 409], "create role");
        return status == HttpStatusCode.Created;
    }

    internal async Task<bool> RoleAddPermissions(string id, IEnumerable<Dto.Permission> permissions)
    {
        var body = new { permissions = permissions };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleAddPermissions(id),
            body,
            options: RestJsonSerializerOptions
        );
        var status = await response.EnsureExpectedStatusCodeAsync([200, 404], "add permissions");
        return status == HttpStatusCode.OK;
    }

    internal async Task<bool> RoleRemovePermissions(
        string id,
        IEnumerable<Dto.Permission> permissions
    )
    {
        var body = new { permissions = permissions };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleRemovePermissions(id),
            body,
            options: RestJsonSerializerOptions
        );
        var status = await response.EnsureExpectedStatusCodeAsync([200, 404], "remove permissions");
        return status == HttpStatusCode.OK;
    }

    internal async Task<bool?> RoleHasPermission(string id, Dto.Permission permission)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleHasPermission(id),
            permission,
            options: RestJsonSerializerOptions
        );
        var status = await response.EnsureExpectedStatusCodeAsync([200, 404], "has permission");
        if (status == HttpStatusCode.NotFound)
            return null;
        var result = await response.Content.ReadFromJsonAsync<bool>(RestJsonSerializerOptions);
        return result;
    }

    // Role assignments
    internal record RoleUserAssignment(string userId, Dto.UserTypeOutput userType);

    internal record RoleGroupAssignment(string groupId, Dto.GroupType groupType);

    internal async Task<IEnumerable<RoleUserAssignment>> RoleUserAssignments(string id)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.RoleUserAssignments(id));
        await response.EnsureExpectedStatusCodeAsync([200, 404], "role user assignments");
        var list = await response.Content.ReadFromJsonAsync<IEnumerable<RoleUserAssignment>>(
            RestJsonSerializerOptions
        );
        return list ?? Array.Empty<RoleUserAssignment>();
    }

    internal async Task<IEnumerable<RoleGroupAssignment>> RoleGroupAssignments(string id)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.RoleGroupAssignments(id));
        await response.EnsureExpectedStatusCodeAsync([200, 404], "role group assignments");
        var list = await response.Content.ReadFromJsonAsync<IEnumerable<RoleGroupAssignment>>(
            RestJsonSerializerOptions
        );
        return list ?? Array.Empty<RoleGroupAssignment>();
    }

    // User role operations
    internal async Task<bool> UserAssignRoles(
        string userId,
        string userType,
        IEnumerable<string> roles
    )
    {
        var body = new { roles = roles, userType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzUserAssign(userId),
            body,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "assign roles to user");
        return true;
    }

    internal async Task<bool> UserRevokeRoles(
        string userId,
        string userType,
        IEnumerable<string> roles
    )
    {
        var body = new { roles = roles, userType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzUserRevoke(userId),
            body,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "revoke roles from user");
        return true;
    }

    internal async Task<IEnumerable<Dto.Role>> UserRolesGet(
        string userId,
        string userType,
        bool? includeFullRoles = null
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.AuthzUserRoles(userId, userType, includeFullRoles)
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "get roles for user");
        var list = await response.Content.ReadFromJsonAsync<Dto.RolesListResponse>(
            RestJsonSerializerOptions
        );
        return list is null ? Array.Empty<Dto.Role>() : list;
    }

    // Group role operations
    internal async Task<bool> GroupAssignRoles(
        string groupId,
        string groupType,
        IEnumerable<string> roles
    )
    {
        var body = new { roles = roles, groupType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzGroupAssign(groupId),
            body,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "assign roles to group");
        return true;
    }

    internal async Task<bool> GroupRevokeRoles(
        string groupId,
        string groupType,
        IEnumerable<string> roles
    )
    {
        var body = new { roles = roles, groupType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzGroupRevoke(groupId),
            body,
            options: RestJsonSerializerOptions
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "revoke roles from group");
        return true;
    }

    internal async Task<IEnumerable<Dto.Role>> GroupRolesGet(
        string groupId,
        string groupType,
        bool? includeFullRoles = null
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.AuthzGroupRoles(groupId, groupType, includeFullRoles)
        );
        await response.EnsureExpectedStatusCodeAsync([200, 404], "get roles for group");
        var list = await response.Content.ReadFromJsonAsync<Dto.RolesListResponse>(
            RestJsonSerializerOptions
        );
        return list is null ? Array.Empty<Dto.Role>() : list;
    }

    internal async Task<IEnumerable<string>> GroupsList(string groupType)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.AuthzGroups(groupType));
        await response.EnsureExpectedStatusCodeAsync([200], "list groups");
        var list = await response.Content.ReadFromJsonAsync<IEnumerable<string>>(
            RestJsonSerializerOptions
        );
        return list ?? Array.Empty<string>();
    }
}
