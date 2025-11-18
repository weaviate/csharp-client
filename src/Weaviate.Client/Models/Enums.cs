using System.Runtime.Serialization;

namespace Weaviate.Client.Models;

public enum HybridFusion
{
    Ranked = 0,
    RelativeScore = 1,
}

public enum NearMediaType
{
    Audio,
    Depth,
    Image,
    IMU,
    Thermal,
    Video,
}

public enum NodeVerbosity
{
    [EnumMember(Value = "verbose")]
    Verbose,

    [EnumMember(Value = "minimal")]
    Minimal,
}

public enum RolesScope
{
    [EnumMember(Value = "all")]
    All = 0,

    [EnumMember(Value = "match")]
    Match = 1,
}

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
