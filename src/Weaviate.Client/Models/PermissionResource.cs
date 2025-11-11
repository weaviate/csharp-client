namespace Weaviate.Client.Models;

/// <summary>
/// Represents the resource scope for a permission.
/// </summary>
public record PermissionResource(
    BackupsResource? Backups = null,
    DataResource? Data = null,
    NodesResource? Nodes = null,
    UsersResource? Users = null,
    GroupsResource? Groups = null,
    TenantsResource? Tenants = null,
    RolesResource? Roles = null,
    CollectionsResource? Collections = null,
    ReplicateResource? Replicate = null,
    AliasesResource? Aliases = null
);

public record BackupsResource(string? Collection);

public record DataResource(string? Collection, string? Tenant, string? Object);

public record NodesResource(string? Collection, string? Verbosity);

public record UsersResource(string? Users);

public record GroupsResource(string? Group, string? GroupType);

public record TenantsResource(string? Collection, string? Tenant);

public record RolesResource(string? Role, string? Scope);

public record CollectionsResource(string? Collection);

public record ReplicateResource(string? Collection, string? Shard);

public record AliasesResource(string? Collection, string? Alias);
