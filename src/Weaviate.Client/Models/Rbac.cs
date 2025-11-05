namespace Weaviate.Client.Models;

/// <summary>
/// Database user types (subset of underlying DTO enum values)
/// </summary>
public enum DatabaseUserType
{
    DbUser,
    DbEnvUser,
}

/// <summary>
/// Represents a database user returned by the Users API.
/// </summary>
public record DatabaseUser(
    string UserId,
    bool Active,
    DatabaseUserType DbUserType,
    System.DateTimeOffset? CreatedAt,
    System.DateTimeOffset? LastUsedAt,
    string? ApiKeyFirstLetters,
    IEnumerable<string> Roles
);

/// <summary>
/// Represents the current authenticated user's info (own-info endpoint).
/// </summary>
public record CurrentUserInfo(
    string Username,
    IEnumerable<RoleInfo> Roles,
    IEnumerable<string>? Groups
);

/// <summary>
/// Simplified role representation.
/// </summary>
public record RoleInfo(string Name, IEnumerable<PermissionInfo> Permissions);

/// <summary>
/// Simplified permission representation exposing only the action; full resource scoping is available via Raw.
/// </summary>
public record PermissionInfo(string Action);

/// <summary>
/// Role assignment for a user.
/// </summary>
public record UserRoleAssignment(string UserId, string UserType);

/// <summary>
/// Role assignment for a group.
/// </summary>
public record GroupRoleAssignment(string GroupId, string GroupType);
