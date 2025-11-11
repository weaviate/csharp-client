using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
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
        if (status == HttpStatusCode.OK)
        {
            return await response.Content.ReadFromJsonAsync<Dto.Role>(RestJsonSerializerOptions);
        }
        else if (status == HttpStatusCode.NotFound)
        {
            var ex = new WeaviateRestServerException(HttpStatusCode.NotFound);
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }
        else
        {
            return null;
        }
    }

    internal async Task RoleDelete(string id)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Role(id));
        await response.EnsureExpectedStatusCodeAsync([204], "delete role");
    }

    internal async Task<Dto.Role> RoleCreate(Dto.Role role)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Roles(),
            role,
            options: RestJsonSerializerOptions
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([201], "create role");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            throw new WeaviateConflictException($"Role '{role.Name}' already exists.", ex);
        }

        // Re-fetch the created role since the API doesn't return it in the response
        var created = await RoleGet(role.Name!);
        return created ?? throw new WeaviateRestClientException();
    }

    internal async Task<Dto.Role> RoleAddPermissions(
        string id,
        IEnumerable<Dto.Permission> permissions
    )
    {
        var body = new { permissions = permissions };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleAddPermissions(id),
            body,
            options: RestJsonSerializerOptions
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "add permissions");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }

        // Re-fetch role to get updated permissions
        var updated = await RoleGet(id);
        return updated ?? throw new WeaviateRestClientException();
    }

    internal async Task<Dto.Role> RoleRemovePermissions(
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
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "remove permissions");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }

        // Re-fetch role to get updated permissions
        var updated = await RoleGet(id);
        return updated ?? throw new WeaviateRestClientException();
    }

    internal async Task<bool> RoleHasPermission(string id, Dto.Permission permission)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleHasPermission(id),
            permission,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "has permission");
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
