namespace Weaviate.Client.Models;

/// <summary>
/// Represents a database user returned by the Users API.
/// </summary>
public record DatabaseUser(
    string UserId,
    bool Active,
    DatabaseUserType DbUserType,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? LastUsedAt,
    string? ApiKeyFirstLetters,
    IEnumerable<string> Roles
)
{
    /// <summary>
    /// When the user's API key expires. <c>null</c> if it never expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>
/// Represents the current authenticated user's info (own-info endpoint).
/// </summary>
public record CurrentUserInfo(
    string Username,
    IEnumerable<RoleInfo> Roles,
    IEnumerable<string>? Groups
)
{
    /// <summary>
    /// When the user's API key expires. <c>null</c> if it never expires.
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>
/// Simplified role representation.
/// </summary>
public record RoleInfo(string Name, IEnumerable<PermissionScope> Permissions);

/// <summary>
/// Role assignment for a user.
/// </summary>
public record UserRoleAssignment(string UserId, RbacUserType UserType);

/// <summary>
/// Role assignment for a group.
/// </summary>
public record GroupRoleAssignment(string GroupId, RbacGroupType GroupType);
