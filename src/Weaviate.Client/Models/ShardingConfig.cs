using System.Text.Json.Serialization;

namespace Weaviate.Client.Models;

/// <summary>
/// The sharding config
/// </summary>
public record ShardingConfig : IEquatable<ShardingConfig>
{
    /// <summary>
    /// The strategies enum
    /// </summary>
    public enum Strategies
    {
        /// <summary>
        /// Unknown strategy
        /// </summary>
        None,

        /// <summary>
        /// The hash strategies
        /// </summary>
        [JsonStringEnumMemberName("hash")]
        Hash,
    }

    /// <summary>
    /// The functions enum
    /// </summary>
    public enum Functions
    {
        /// <summary>
        /// The none functions
        /// </summary>
        None,

        /// <summary>
        /// The murmur functions
        /// </summary>
        [JsonStringEnumMemberName("murmur3")]
        Murmur3,
    }

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    private static readonly Lazy<ShardingConfig> _default = new(() => new());

    /// <summary>
    /// Gets the value of the default
    /// </summary>
    public static ShardingConfig Default => _default.Value;

    /// <summary>
    /// Gets the value of the zero
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
    /// Gets or sets the value of the virtual per physical
    /// </summary>
    public int VirtualPerPhysical { get; set; } = 128;

    /// <summary>
    /// Gets or sets the value of the desired count
    /// </summary>
    public int DesiredCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the value of the actual count
    /// </summary>
    public int ActualCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the value of the desired virtual count
    /// </summary>
    public int DesiredVirtualCount { get; set; } = 128;

    /// <summary>
    /// Gets or sets the value of the actual virtual count
    /// </summary>
    public int ActualVirtualCount { get; set; } = 128;

    /// <summary>
    /// Gets or sets the value of the key
    /// </summary>
    public string Key { get; set; } = "_id";

    /// <summary>
    /// Gets or sets the value of the strategy
    /// </summary>
    [JsonConverter(typeof(EmptyStringEnumConverter<Strategies>))]
    public Strategies Strategy { get; set; } = Strategies.Hash;

    /// <summary>
    /// Gets or sets the value of the function
    /// </summary>
    [JsonConverter(typeof(EmptyStringEnumConverter<Functions>))]
    public Functions Function { get; set; } = Functions.Murmur3;
}
