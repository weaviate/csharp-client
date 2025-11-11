namespace Weaviate.Client.Models;

using System.Collections;
using System.Linq;
using System.Runtime.Serialization;

/// <summary>
/// Database user types (subset of underlying DTO enum values)
/// </summary>
public enum DatabaseUserType
{
    DbUser,
    DbEnvUser,
}

/// <summary>
/// RBAC user types for role assignment endpoints.
/// </summary>
public enum RbacUserType
{
    [EnumMember(Value = "db")]
    Database,

    [EnumMember(Value = "oidc")]
    Oidc,
}

/// <summary>
/// RBAC group types for role assignment endpoints.
/// </summary>
public enum RbacGroupType
{
    [EnumMember(Value = "oidc")]
    Oidc,
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
public record RoleInfo(string Name, IEnumerable<PermissionScope> Permissions);

/// <summary>
/// Internal permission representation for REST DTO conversion only.
/// </summary>
internal record PermissionInfo
{
    public string ActionRaw { get; }
    public RbacPermissionAction Action =>
        ActionRaw.FromEnumMemberString<RbacPermissionAction>(RbacPermissionAction.Custom);

    public PermissionResource? Resources { get; }

    internal PermissionInfo(string actionRaw, PermissionResource? resources = null)
    {
        ActionRaw = actionRaw ?? string.Empty;
        Resources = resources;
    }

    internal PermissionInfo(RbacPermissionAction action, PermissionResource? resources = null)
        : this(action.ToEnumMemberString(), resources) { }

    public Rest.Dto.Permission ToDto()
    {
        return new Rest.Dto.Permission
        {
            Action = Action.ToEnumMemberString().FromEnumMemberString<Rest.Dto.PermissionAction>(),
        };
    }
}

/// <summary>
/// Role assignment for a user.
/// </summary>
public record UserRoleAssignment(string UserId, RbacUserType UserType);

/// <summary>
/// Role assignment for a group.
/// </summary>
public record GroupRoleAssignment(string GroupId, RbacGroupType GroupType);
