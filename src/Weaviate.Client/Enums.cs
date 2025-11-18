namespace Weaviate.Client;

[Flags]
public enum ResourceType
{
    Alias,
    Collection,
    Object,
    Property,
    User,
    Role,
    Backup,
    Group,
    Reference,
    Shard,
    Tenant,
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
