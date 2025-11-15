using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    // Roles
    internal async Task<IEnumerable<Dto.Role>> RolesList(
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Roles(), cancellationToken);
        await response.EnsureExpectedStatusCodeAsync([200], "list roles");
        var list = await response.Content.ReadFromJsonAsync<List<Dto.Role>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return list is null ? Array.Empty<Dto.Role>() : list;
    }

    internal async Task<Dto.Role?> RoleGet(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Role(id), cancellationToken);
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "get role");
            return await response.Content.ReadFromJsonAsync<Dto.Role>(
                RestJsonSerializerOptions,
                cancellationToken
            );
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }
    }

    internal async Task RoleDelete(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Role(id), cancellationToken);
        await response.EnsureExpectedStatusCodeAsync([204], "delete role");
    }

    internal async Task<Dto.Role> RoleCreate(
        Dto.Role role,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Roles(),
            role,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
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
        try
        {
            var created = await RoleGet(role.Name!);
            return created!;
        }
        catch (WeaviateNotFoundException ex)
        {
            throw new WeaviateRestClientException(
                $"Role '{role.Name}' was not found after creation.",
                ex
            );
        }
    }

    internal async Task<Dto.Role> RoleAddPermissions(
        string id,
        IEnumerable<Dto.Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        var body = new { permissions = permissions };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleAddPermissions(id),
            body,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
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
        var updated = await RoleGet(id, cancellationToken);
        return updated ?? throw new WeaviateRestClientException();
    }

    internal async Task<Dto.Role> RoleRemovePermissions(
        string id,
        IEnumerable<Dto.Permission> permissions,
        CancellationToken cancellationToken = default
    )
    {
        var body = new { permissions = permissions };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleRemovePermissions(id),
            body,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
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
        var updated = await RoleGet(id, cancellationToken);
        return updated ?? throw new WeaviateRestClientException();
    }

    internal async Task<bool> RoleHasPermission(
        string id,
        Dto.Permission permission,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.RoleHasPermission(id),
            permission,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.EnsureExpectedStatusCodeAsync([200], "has permission");
        var result = await response.Content.ReadFromJsonAsync<bool>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return result;
    }

    private static Models.RbacUserType MapUserType(Dto.UserTypeOutput userType)
    {
        // Custom mapping: db_user/db_env_user → Database, oidc → Oidc
        return userType switch
        {
            Dto.UserTypeOutput.Db_user => Models.RbacUserType.Database,
            Dto.UserTypeOutput.Db_env_user => Models.RbacUserType.Database,
            Dto.UserTypeOutput.Oidc => Models.RbacUserType.Oidc,
            _ => throw new ArgumentOutOfRangeException(
                nameof(userType),
                userType,
                $"Unknown UserTypeOutput: {userType}"
            ),
        };
    }

    private static Models.RbacGroupType MapGroupType(Dto.GroupType groupType)
    {
        // Custom mapping: oidc → Oidc
        return groupType switch
        {
            Dto.GroupType.Oidc => Models.RbacGroupType.Oidc,
            _ => throw new ArgumentOutOfRangeException(
                nameof(groupType),
                groupType,
                $"Unknown GroupType: {groupType}"
            ),
        };
    }

    // Role assignments
    internal record RoleUserAssignment(string userId, Dto.UserTypeOutput userType)
    {
        public Models.UserRoleAssignment ToModel() => new(userId, MapUserType(userType));
    }

    internal record RoleGroupAssignment(string groupId, Dto.GroupType groupType)
    {
        public Models.GroupRoleAssignment ToModel() => new(groupId, MapGroupType(groupType));
    }

    internal async Task<IEnumerable<RoleUserAssignment>> RoleUserAssignments(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.RoleUserAssignments(id),
            cancellationToken
        );

        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "role user assignments");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }

        var list = await response.Content.ReadFromJsonAsync<IEnumerable<RoleUserAssignment>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return list ?? Array.Empty<RoleUserAssignment>();
    }

    internal async Task<IEnumerable<RoleGroupAssignment>> RoleGroupAssignments(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.RoleGroupAssignments(id),
            cancellationToken
        );

        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "role group assignments");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }

        var list = await response.Content.ReadFromJsonAsync<IEnumerable<RoleGroupAssignment>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return list ?? Array.Empty<RoleGroupAssignment>();
    }

    // User role operations
    internal async Task UserAssignRoles(
        string userId,
        string userType,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    )
    {
        var body = new { roles = roles, userType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzUserAssign(userId),
            body,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "assign roles to user");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User | ResourceType.Role);
        }
    }

    internal async Task UserRevokeRoles(
        string userId,
        string userType,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    )
    {
        var body = new { roles = roles, userType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzUserRevoke(userId),
            body,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "revoke roles from user");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User | ResourceType.Role);
        }
    }

    internal async Task<IEnumerable<Dto.Role>> UserRolesGet(
        string userId,
        string userType,
        bool? includeFullRoles = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.AuthzUserRoles(userId, userType, includeFullRoles),
            cancellationToken
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "get roles for user");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User);
        }
        var list = await response.Content.ReadFromJsonAsync<List<Dto.Role>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return list ?? [];
    }

    // Group role operations
    internal async Task GroupAssignRoles(
        string groupId,
        string groupType,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    )
    {
        var body = new { roles = roles, groupType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzGroupAssign(groupId),
            body,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "assign roles to group");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Group);
        }
    }

    internal async Task GroupRevokeRoles(
        string groupId,
        string groupType,
        IEnumerable<string> roles,
        CancellationToken cancellationToken = default
    )
    {
        var body = new { roles = roles, groupType };
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.AuthzGroupRevoke(groupId),
            body,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "revoke roles from group");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Group);
        }
    }

    internal async Task<IEnumerable<Dto.Role>> GroupRolesGet(
        string groupId,
        string groupType,
        bool? includeFullRoles = null,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.AuthzGroupRoles(groupId, groupType, includeFullRoles),
            cancellationToken
        );
        try
        {
            await response.EnsureExpectedStatusCodeAsync([200], "get roles for group");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Group);
        }
        var list = await response.Content.ReadFromJsonAsync<List<Dto.Role>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return list ?? [];
    }

    internal async Task<IEnumerable<string>> GroupsList(
        string groupType,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.AuthzGroups(groupType),
            cancellationToken
        );
        await response.EnsureExpectedStatusCodeAsync([200], "list groups");
        var list = await response.Content.ReadFromJsonAsync<IEnumerable<string>>(
            RestJsonSerializerOptions,
            cancellationToken
        );
        return list ?? Array.Empty<string>();
    }
}
