namespace Weaviate.Client.Models;

/// <summary>
/// Configuration for data sharding, controlling how data is distributed across cluster nodes.
/// </summary>
/// <remarks>
/// Sharding enables horizontal scaling by distributing data across multiple physical nodes.
/// Virtual shards provide flexibility for rebalancing data as the cluster grows or shrinks.
/// </remarks>
public record ShardingConfig : IEquatable<ShardingConfig>
{
    /// <summary>
    /// Sharding strategy for distributing data across nodes.
    /// </summary>
    public enum Strategies
    {
        /// <summary>
        /// No sharding strategy specified.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "")]
        None,

        /// <summary>
        /// Hash-based sharding strategy, distributing data based on hash of the shard key.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "hash")]
        Hash,
    }

    /// <summary>
    /// Hash function used for the sharding strategy.
    /// </summary>
    public enum Functions
    {
        /// <summary>
        /// No hash function specified.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "")]
        None,

        /// <summary>
        /// MurmurHash3 algorithm, providing fast and well-distributed hashing.
        /// </summary>
        [System.Runtime.Serialization.EnumMember(Value = "murmur3")]
        Murmur3,
    }

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    private static readonly Lazy<ShardingConfig> _default = new(() => new());

    /// <summary>
    /// Gets the default sharding configuration with standard settings.
    /// </summary>
    public static ShardingConfig Default => _default.Value;

    /// <summary>
    /// Gets a zero-initialized sharding configuration (all values set to empty/zero).
    /// </summary>
    public static ShardingConfig Zero =>
        new()
        {
            ActualCount = 0,
            ActualVirtualCount = 0,
            DesiredCount = 0,
            DesiredVirtualCount = 0,
            Function = 0,
            Key = "",
            Strategy = Strategies.None,
            VirtualPerPhysical = 0,
        };

    /// <summary>
    /// Gets or sets the number of virtual shards per physical shard. Defaults to 128.
    /// Virtual shards enable finer-grained data distribution and rebalancing.
    /// </summary>
    public int VirtualPerPhysical { get; set; } = 128;

    /// <summary>
    /// Gets or sets the desired number of physical shards. Defaults to 1.
    /// This is the target number of shards; actual count may differ during rebalancing.
    /// </summary>
    public int DesiredCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the actual current number of physical shards. Defaults to 1.
    /// This reflects the current state and may differ from <see cref="DesiredCount"/> during transitions.
    /// </summary>
    public int ActualCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the desired total number of virtual shards. Defaults to 128.
    /// Typically equals <see cref="DesiredCount"/> × <see cref="VirtualPerPhysical"/>.
    /// </summary>
    public int DesiredVirtualCount { get; set; } = 128;

    /// <summary>
    /// Gets or sets the actual current number of virtual shards. Defaults to 128.
    /// Typically equals <see cref="ActualCount"/> × <see cref="VirtualPerPhysical"/>.
    /// </summary>
    public int ActualVirtualCount { get; set; } = 128;

    /// <summary>
    /// Gets or sets the property name used as the sharding key. Defaults to "_id".
    /// Data is distributed based on the hash of this property's value.
    /// </summary>
    public string Key { get; set; } = "_id";

    /// <summary>
    /// Gets or sets the sharding strategy. Defaults to <see cref="Strategies.Hash"/>.
    /// </summary>
    public Strategies Strategy { get; set; } = Strategies.Hash;

    /// <summary>
    /// Gets or sets the hash function used for sharding. Defaults to <see cref="Functions.Murmur3"/>.
    /// </summary>
    public Functions Function { get; set; } = Functions.Murmur3;
}
