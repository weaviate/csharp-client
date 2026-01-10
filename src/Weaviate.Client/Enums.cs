namespace Weaviate.Client;

/// <summary>
/// The resource type enum
/// </summary>
[Flags]
public enum ResourceType
{
    /// <summary>
    /// The alias resource type
    /// </summary>
    Alias,

    /// <summary>
    /// The collection resource type
    /// </summary>
    Collection,

    /// <summary>
    /// The object resource type
    /// </summary>
    Object,

    /// <summary>
    /// The property resource type
    /// </summary>
    Property,

    /// <summary>
    /// The user resource type
    /// </summary>
    User,

    /// <summary>
    /// The role resource type
    /// </summary>
    Role,

    /// <summary>
    /// The backup resource type
    /// </summary>
    Backup,

    /// <summary>
    /// The group resource type
    /// </summary>
    Group,

    /// <summary>
    /// The reference resource type
    /// </summary>
    Reference,

    /// <summary>
    /// The shard resource type
    /// </summary>
    Shard,

    /// <summary>
    /// The tenant resource type
    /// </summary>
    Tenant,

    /// <summary>
    /// The replication resource type
    /// </summary>
    Replication,

    /// <summary>
    /// The unknown resource type
    /// </summary>
    Unknown,
}

/// <summary>
/// The consistency levels when writing to Weaviate with replication enabled.
/// </summary>
public enum ConsistencyLevels
{
    /// <summary>
    /// Wait for confirmation of write success from all, `N`, replicas.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// Wait for confirmation of write success from all, `N`, replicas.
    /// </summary>
    All = 3,

    /// <summary>
    /// Wait for confirmation of write success from only one replica.
    /// </summary>
    One = 1,

    /// <summary>
    /// Wait for confirmation of write success from a quorum: `N/2+1`, of replicas.
    /// </summary>
    Quorum = 2,
}
