using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    // Roles
    /// <summary>
    /// Lists all roles defined in the Weaviate instance.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of dto role</returns>
    internal async Task<IEnumerable<Dto.Role>> RolesList(
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Roles(), cancellationToken);

        await response.ManageStatusCode([HttpStatusCode.OK], "list roles", ResourceType.Role);

        var list = await response.DecodeAsync<List<Dto.Role>>(cancellationToken);
        return list is null ? Array.Empty<Dto.Role>() : list;
    }

    /// <summary>
    /// Roles the get using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the dto role</returns>
    internal async Task<Dto.Role?> RoleGet(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Role(id), cancellationToken);

        await response.ManageStatusCode([HttpStatusCode.OK], "get role", ResourceType.Role);

        return await response.DecodeAsync<Dto.Role>(cancellationToken);
    }

    /// <summary>
    /// Roles the delete using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    internal async Task RoleDelete(string id, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Role(id), cancellationToken);

        await response.ManageStatusCode(
            [HttpStatusCode.NoContent],
            "delete role",
            ResourceType.Role
        );
    }

    /// <summary>
    /// Roles the create using the specified role
    /// </summary>
    /// <param name="role">The role</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateConflictException">Role '{role.Name}' already exists. </exception>
    /// <exception cref="WeaviateRestClientException">Role '{role.Name}' was not found after creation. </exception>
    /// <returns>A task containing the dto role</returns>
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
            await response.ManageStatusCode([HttpStatusCode.Created], "create role");
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

    /// <summary>
    /// Roles the add permissions using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="permissions">The permissions</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
    /// <exception cref="WeaviateRestClientException"></exception>
    /// <returns>A task containing the dto role</returns>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "add permissions");
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

    /// <summary>
    /// Roles the remove permissions using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="permissions">The permissions</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
    /// <exception cref="WeaviateRestClientException"></exception>
    /// <returns>A task containing the dto role</returns>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "remove permissions");
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

    /// <summary>
    /// Roles the has permission using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="permission">The permission</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The result</returns>
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

        await response.ManageStatusCode([HttpStatusCode.OK], "has permission", ResourceType.Role);

        var result = await response.DecodeAsync<bool>(cancellationToken);
        return result;
    }

    /// <summary>
    /// Maps the user type using the specified user type
    /// </summary>
    /// <param name="userType">The user type</param>
    /// <exception cref="ArgumentOutOfRangeException">Unknown UserTypeOutput: {userType}</exception>
    /// <returns>The models rbac user type</returns>
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

    /// <summary>
    /// Maps the group type using the specified group type
    /// </summary>
    /// <param name="groupType">The group type</param>
    /// <exception cref="ArgumentOutOfRangeException">Unknown GroupType: {groupType}</exception>
    /// <returns>The models rbac group type</returns>
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
    /// <summary>
    /// The role user assignment
    /// </summary>
    internal record RoleUserAssignment
    {
        public required string UserId { get; init; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Dto.UserTypeOutput UserType { get; init; }

        /// <summary>
        /// Returns the model
        /// </summary>
        /// <returns>The models user role assignment</returns>
        public Models.UserRoleAssignment ToModel() => new(UserId, MapUserType(UserType));
    }

    /// <summary>
    /// The role group assignment
    /// </summary>
    internal record RoleGroupAssignment(string groupId, Dto.GroupType groupType)
    {
        /// <summary>
        /// Returns the model
        /// </summary>
        /// <returns>The models group role assignment</returns>
        public Models.GroupRoleAssignment ToModel() => new(groupId, MapGroupType(groupType));
    }

    /// <summary>
    /// Roles the user assignments using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
    /// <returns>A task containing an enumerable of role user assignment</returns>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "role user assignments");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }

        var list = await response.DecodeAsync<IEnumerable<RoleUserAssignment>>(cancellationToken);
        return list ?? Array.Empty<RoleUserAssignment>();
    }

    /// <summary>
    /// Roles the group assignments using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
    /// <returns>A task containing an enumerable of role group assignment</returns>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "role group assignments");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Role);
        }

        var list = await response.DecodeAsync<IEnumerable<RoleGroupAssignment>>(cancellationToken);
        return list ?? Array.Empty<RoleGroupAssignment>();
    }

    // User role operations
    /// <summary>
    /// Users the assign roles using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="userType">The user type</param>
    /// <param name="roles">The roles</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "assign roles to user");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User | ResourceType.Role);
        }
    }

    /// <summary>
    /// Users the revoke roles using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="userType">The user type</param>
    /// <param name="roles">The roles</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "revoke roles from user");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User | ResourceType.Role);
        }
    }

    /// <summary>
    /// Users the roles get using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="userType">The user type</param>
    /// <param name="includeFullRoles">The include full roles</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
    /// <returns>A task containing an enumerable of dto role</returns>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "get roles for user");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.User);
        }
        var list = await response.DecodeAsync<List<Dto.Role>>(cancellationToken);
        return list ?? [];
    }

    // Group role operations
    /// <summary>
    /// Groups the assign roles using the specified group id
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="groupType">The group type</param>
    /// <param name="roles">The roles</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "assign roles to group");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Group);
        }
    }

    /// <summary>
    /// Groups the revoke roles using the specified group id
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="groupType">The group type</param>
    /// <param name="roles">The roles</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "revoke roles from group");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Group);
        }
    }

    /// <summary>
    /// Groups the roles get using the specified group id
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="groupType">The group type</param>
    /// <param name="includeFullRoles">The include full roles</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateNotFoundException"></exception>
    /// <returns>A task containing an enumerable of dto role</returns>
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
            await response.ManageStatusCode([HttpStatusCode.OK], "get roles for group");
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, ResourceType.Group);
        }
        var list = await response.DecodeAsync<List<Dto.Role>>(cancellationToken);
        return list ?? [];
    }

    /// <summary>
    /// Groupses the list using the specified group type
    /// </summary>
    /// <param name="groupType">The group type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing an enumerable of string</returns>
    internal async Task<IEnumerable<string>> GroupsList(
        string groupType,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.GetAsync(
            WeaviateEndpoints.AuthzGroups(groupType),
            cancellationToken
        );

        await response.ManageStatusCode([HttpStatusCode.OK], "list groups", ResourceType.Group);

        var list = await response.DecodeAsync<IEnumerable<string>>(cancellationToken);
        return list ?? Array.Empty<string>();
    }
}
