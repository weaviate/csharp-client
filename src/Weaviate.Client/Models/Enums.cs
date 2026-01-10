using System.Runtime.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// The hybrid fusion enum
/// </summary>
public enum HybridFusion
{
    /// <summary>
    /// The ranked hybrid fusion
    /// </summary>
    Ranked = 0,

    /// <summary>
    /// The relative score hybrid fusion
    /// </summary>
    RelativeScore = 1,
}

/// <summary>
/// The near media type enum
/// </summary>
public enum NearMediaType
{
    /// <summary>
    /// The audio near media type
    /// </summary>
    Audio,

    /// <summary>
    /// The depth near media type
    /// </summary>
    Depth,

    /// <summary>
    /// The image near media type
    /// </summary>
    Image,

    /// <summary>
    /// The imu near media type
    /// </summary>
    IMU,

    /// <summary>
    /// The thermal near media type
    /// </summary>
    Thermal,

    /// <summary>
    /// The video near media type
    /// </summary>
    Video,
}

/// <summary>
/// The node verbosity enum
/// </summary>
public enum NodeVerbosity
{
    /// <summary>
    /// The verbose node verbosity
    /// </summary>
    [EnumMember(Value = "verbose")]
    Verbose,

    /// <summary>
    /// The minimal node verbosity
    /// </summary>
    [EnumMember(Value = "minimal")]
    Minimal,
}

/// <summary>
/// The roles scope enum
/// </summary>
public enum RolesScope
{
    /// <summary>
    /// The all roles scope
    /// </summary>
    [EnumMember(Value = "all")]
    All = 0,

    /// <summary>
    /// The match roles scope
    /// </summary>
    [EnumMember(Value = "match")]
    Match = 1,
}

/// <summary>
/// Database user types (subset of underlying DTO enum values)
/// </summary>
public enum DatabaseUserType
{
    /// <summary>
    /// The db user database user type
    /// </summary>
    DbUser,

    /// <summary>
    /// The db env user database user type
    /// </summary>
    DbEnvUser,
}

/// <summary>
/// RBAC user types for role assignment endpoints.
/// </summary>
public enum RbacUserType
{
    /// <summary>
    /// The database rbac user type
    /// </summary>
    [EnumMember(Value = "db")]
    Database,

    /// <summary>
    /// The oidc rbac user type
    /// </summary>
    [EnumMember(Value = "oidc")]
    Oidc,
}

/// <summary>
/// RBAC group types for role assignment endpoints.
/// </summary>
public enum RbacGroupType
{
    /// <summary>
    /// The oidc rbac group type
    /// </summary>
    [EnumMember(Value = "oidc")]
    Oidc,
}
