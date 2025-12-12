namespace Weaviate.Client.Models;

public record BackupsResource(string? Collection);

public record DataResource(string? Collection = "*", string? Tenant = "*");

public record NodesResource(
    string? Collection = "*",
    NodeVerbosity? Verbosity = NodeVerbosity.Minimal
);

public record UsersResource(string? Users = "*");

public record GroupsResource(string? Group = "*", RbacGroupType? GroupType = RbacGroupType.Oidc);

public record TenantsResource(string? Collection = "*", string? Tenant = "*");

public record RolesResource(string? Role = "*", RolesScope? Scope = RolesScope.Match);

public record CollectionsResource(string? Collection);

public record ReplicateResource(string? Collection = "*", string? Shard = "*");

public record AliasesResource(string? Collection = "*", string? Alias = "*");

internal static class PermissionResourceExtensions
{
    internal static Rest.Dto.Backups ToDto(this BackupsResource resource)
    {
        return new Rest.Dto.Backups { Collection = resource.Collection };
    }

    internal static Rest.Dto.Data ToDto(this DataResource resource)
    {
        return new Rest.Dto.Data { Collection = resource.Collection, Tenant = resource.Tenant };
    }

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

    internal static Rest.Dto.Users ToDto(this UsersResource resource)
    {
        return new Rest.Dto.Users { Users1 = resource.Users };
    }

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

    internal static Rest.Dto.Tenants ToDto(this TenantsResource resource)
    {
        return new Rest.Dto.Tenants { Collection = resource.Collection, Tenant = resource.Tenant };
    }

    internal static Rest.Dto.Roles ToDto(this RolesResource resource)
    {
        return new Rest.Dto.Roles
        {
            Role = resource.Role,
            Scope = resource.Scope.HasValue ? (Rest.Dto.RolesScope)resource.Scope.Value : null,
        };
    }

    internal static Rest.Dto.Collections ToDto(this CollectionsResource resource)
    {
        return new Rest.Dto.Collections { Collection = resource.Collection };
    }

    internal static Rest.Dto.Replicate ToDto(this ReplicateResource resource)
    {
        return new Rest.Dto.Replicate { Collection = resource.Collection, Shard = resource.Shard };
    }

    internal static Rest.Dto.Aliases ToDto(this AliasesResource resource)
    {
        return new Rest.Dto.Aliases { Collection = resource.Collection, Alias = resource.Alias };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Aliases resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Aliases == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Models.Permissions.Alias(resource.Collection, resource.Alias)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_aliases),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_aliases),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_aliases),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_aliases),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Data resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Data == resource).Select(p => p.Action).ToHashSet();

        return new Models.Permissions.Data(resource.Collection, resource.Tenant)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_data),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_data),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_data),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_data),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Backups resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Backups == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Models.Permissions.Backups(resource.Collection)
        {
            Manage = actions.Contains(Rest.Dto.PermissionAction.Manage_backups),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Nodes resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Nodes == resource).Select(p => p.Action).ToHashSet();

        return new Models.Permissions.Nodes(
            resource.Collection,
            (Models.NodeVerbosity?)resource.Verbosity
        )
        {
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_nodes),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Roles resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Roles == resource).Select(p => p.Action).ToHashSet();

        return new Models.Permissions.Roles(resource.Role, (Models.RolesScope?)resource.Scope)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_roles),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_roles),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_roles),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_roles),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Users resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions.Where(p => p.Users == resource).Select(p => p.Action).ToHashSet();

        return new Models.Permissions.Users(resource.Users1)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_users),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_users),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_users),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_users),
            AssignAndRevoke = actions.Contains(Rest.Dto.PermissionAction.Assign_and_revoke_users),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Tenants resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Tenants == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Models.Permissions.Tenants(resource.Collection, resource.Tenant)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_tenants),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_tenants),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_tenants),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_tenants),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Groups resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Groups == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Models.Permissions.Groups(
            resource.Group,
            (Models.RbacGroupType?)resource.GroupType
        )
        {
            AssignAndRevoke = actions.Contains(Rest.Dto.PermissionAction.Assign_and_revoke_groups),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_groups),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Replicate resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Replicate == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Models.Permissions.Replicate(resource.Collection, resource.Shard)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_replicate),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_replicate),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_replicate),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_replicate),
        };
    }

    internal static PermissionScope ToModel(
        this Rest.Dto.Collections resource,
        IEnumerable<Rest.Dto.Permission> permissions
    )
    {
        var actions = permissions
            .Where(p => p.Collections == resource)
            .Select(p => p.Action)
            .ToHashSet();

        return new Models.Permissions.Collections(resource.Collection)
        {
            Create = actions.Contains(Rest.Dto.PermissionAction.Create_collections),
            Read = actions.Contains(Rest.Dto.PermissionAction.Read_collections),
            Update = actions.Contains(Rest.Dto.PermissionAction.Update_collections),
            Delete = actions.Contains(Rest.Dto.PermissionAction.Delete_collections),
        };
    }
}
