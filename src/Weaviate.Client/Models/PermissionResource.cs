namespace Weaviate.Client.Models;

/// <summary>
/// Represents a backup resource, optionally scoped to a collection.
/// </summary>
/// <param name="Collection">Optional collection name to scope the backup resource to</param>
public record BackupsResource(string? Collection);

/// <summary>
/// Represents a data resource, optionally scoped to a collection and tenant. Defaults to all ("*").
/// </summary>
/// <param name="Collection">Optional collection name (defaults to "*" for all collections)</param>
/// <param name="Tenant">Optional tenant name (defaults to "*" for all tenants)</param>
public record DataResource(string? Collection = "*", string? Tenant = "*");

/// <summary>
/// Represents a nodes resource, optionally scoped to a collection and verbosity.
/// </summary>
/// <param name="Collection">Optional collection name (defaults to "*" for all collections)</param>
/// <param name="Verbosity">Optional verbosity level (defaults to Minimal)</param>
public record NodesResource(
    string? Collection = "*",
    NodeVerbosity? Verbosity = NodeVerbosity.Minimal
);

/// <summary>
/// Represents a users resource, optionally scoped to a user or all users ("*").
/// </summary>
/// <param name="Users">Optional user name or "*" for all users (defaults to "*")</param>
public record UsersResource(string? Users = "*");

/// <summary>
/// Represents a groups resource, optionally scoped to a group and group type.
/// </summary>
/// <param name="Group">Optional group name (defaults to "*" for all groups)</param>
/// <param name="GroupType">Optional group type (defaults to Oidc)</param>
public record GroupsResource(string? Group = "*", RbacGroupType? GroupType = RbacGroupType.Oidc);

/// <summary>
/// Represents a tenants resource, optionally scoped to a collection and tenant.
/// </summary>
/// <param name="Collection">Optional collection name (defaults to "*" for all collections)</param>
/// <param name="Tenant">Optional tenant name (defaults to "*" for all tenants)</param>
public record TenantsResource(string? Collection = "*", string? Tenant = "*");

/// <summary>
/// Represents a roles resource, optionally scoped to a role and scope.
/// </summary>
/// <param name="Role">Optional role name (defaults to "*" for all roles)</param>
/// <param name="Scope">Optional scope for role matching (defaults to Match)</param>
public record RolesResource(string? Role = "*", RolesScope? Scope = RolesScope.Match);

/// <summary>
/// Represents a collections resource, optionally scoped to a collection.
/// </summary>
/// <param name="Collection">Optional collection name to scope the resource to</param>
public record CollectionsResource(string? Collection);

/// <summary>
/// Represents a replicate resource, optionally scoped to a collection and shard.
/// </summary>
/// <param name="Collection">Optional collection name (defaults to "*" for all collections)</param>
/// <param name="Shard">Optional shard name (defaults to "*" for all shards)</param>
public record ReplicateResource(string? Collection = "*", string? Shard = "*");

/// <summary>
/// Represents an aliases resource, optionally scoped to a collection and alias.
/// </summary>
/// <param name="Collection">Optional collection name (defaults to "*" for all collections)</param>
/// <param name="Alias">Optional alias name (defaults to "*" for all aliases)</param>
public record AliasesResource(string? Collection = "*", string? Alias = "*");

/// <summary>
/// The permission resource extensions class
/// </summary>
internal static class PermissionResourceExtensions
{
    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto backups</returns>
    internal static Rest.Dto.Backups ToDto(this BackupsResource resource)
    {
        return new Rest.Dto.Backups { Collection = resource.Collection };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto data</returns>
    internal static Rest.Dto.Data ToDto(this DataResource resource)
    {
        return new Rest.Dto.Data { Collection = resource.Collection, Tenant = resource.Tenant };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto nodes</returns>
    internal static Rest.Dto.Nodes ToDto(this NodesResource resource)
    {
        return new Rest.Dto.Nodes
        {
            Collection = resource.Collection,
            Verbosity = resource.Verbosity.HasValue
                ? (Rest.Dto.NodesVerbosity)resource.Verbosity.Value
                : null,
        };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto users</returns>
    internal static Rest.Dto.Users ToDto(this UsersResource resource)
    {
        return new Rest.Dto.Users { Users1 = resource.Users };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto groups</returns>
    internal static Rest.Dto.Groups ToDto(this GroupsResource resource)
    {
        return new Rest.Dto.Groups
        {
            Group = resource.Group,
            GroupType = resource.GroupType.HasValue
                ? (Rest.Dto.GroupType)resource.GroupType.Value
                : null,
        };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto tenants</returns>
    internal static Rest.Dto.Tenants ToDto(this TenantsResource resource)
    {
        return new Rest.Dto.Tenants { Collection = resource.Collection, Tenant = resource.Tenant };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto roles</returns>
    internal static Rest.Dto.Roles ToDto(this RolesResource resource)
    {
        return new Rest.Dto.Roles
        {
            Role = resource.Role,
            Scope = resource.Scope.HasValue ? (Rest.Dto.RolesScope)resource.Scope.Value : null,
        };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto collections</returns>
    internal static Rest.Dto.Collections ToDto(this CollectionsResource resource)
    {
        return new Rest.Dto.Collections { Collection = resource.Collection };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto replicate</returns>
    internal static Rest.Dto.Replicate ToDto(this ReplicateResource resource)
    {
        return new Rest.Dto.Replicate { Collection = resource.Collection, Shard = resource.Shard };
    }

    /// <summary>
    /// Returns the dto using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <returns>The rest dto aliases</returns>
    internal static Rest.Dto.Aliases ToDto(this AliasesResource resource)
    {
        return new Rest.Dto.Aliases { Collection = resource.Collection, Alias = resource.Alias };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Aliases resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Aliases == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Permissions.Alias(resource.Collection, resource.Alias)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_aliases),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_aliases),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_aliases),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_aliases),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Data resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Data == resource).Select(p => p.Action).ToHashSet();

        return new Permissions.Data(resource.Collection, resource.Tenant)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_data),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_data),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_data),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_data),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Backups resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Backups == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Permissions.Backups(resource.Collection)
        {
            Manage = actions.Contains(Rest.Dto.PermissionAction.Manage_backups),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Nodes resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Nodes == resource).Select(p => p.Action).ToHashSet();

        return new Permissions.Nodes(resource.Collection, (NodeVerbosity?)resource.Verbosity)
        {
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_nodes),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Roles resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Roles == resource).Select(p => p.Action).ToHashSet();

        return new Permissions.Roles(resource.Role, (RolesScope?)resource.Scope)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_roles),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_roles),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_roles),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_roles),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Users resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Users == resource).Select(p => p.Action).ToHashSet();

        return new Permissions.Users(resource.Users1)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_users),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_users),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_users),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_users),
            AssignAndRevoke = actions.Contains(Rest.Dto.PermissionAction.Assign_and_revoke_users),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Tenants resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Tenants == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Permissions.Tenants(resource.Collection, resource.Tenant)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_tenants),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_tenants),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_tenants),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_tenants),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Groups resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Groups == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Permissions.Groups(resource.Group, (RbacGroupType?)resource.GroupType)
        {
            AssignAndRevoke = actions.Contains(Rest.Dto.PermissionAction.Assign_and_revoke_groups),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_groups),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Replicate resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Replicate == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Permissions.Replicate(resource.Collection, resource.Shard)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_replicate),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_replicate),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_replicate),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_replicate),
        };
    }

    /// <summary>
    /// Returns the model using the specified resource
    /// </summary>
    /// <param name="resource">The resource</param>
    /// <param name="permissions">The permissions</param>
    /// <returns>The permission scope</returns>
    internal static PermissionScope ToModel(
        this Rest.Dto.Collections resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Collections == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Permissions.Collections(resource.Collection)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_collections),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_collections),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_collections),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_collections),
        };
    }
}
