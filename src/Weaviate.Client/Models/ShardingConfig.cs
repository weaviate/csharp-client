namespace Weaviate.Client.Models;

public record ShardingConfig : IEquatable<ShardingConfig>
{
    public enum Strategies
    {
        [System.Runtime.Serialization.EnumMember(Value = "")]
        None,

        [System.Runtime.Serialization.EnumMember(Value = "hash")]
        Hash,
    }

    public enum Functions
    {
        [System.Runtime.Serialization.EnumMember(Value = "")]
        None,

        [System.Runtime.Serialization.EnumMember(Value = "murmur3")]
        Murmur3,
    }

    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    private static readonly Lazy<ShardingConfig> _default = new(() => new());

    public static ShardingConfig Default => _default.Value;

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

    public int VirtualPerPhysical { get; set; } = 128;
    public int DesiredCount { get; set; } = 1;
    public int ActualCount { get; set; } = 1;
    public int DesiredVirtualCount { get; set; } = 128;
    public int ActualVirtualCount { get; set; } = 128;
    public string Key { get; set; } = "_id";
    public Strategies Strategy { get; set; } = Strategies.Hash;
    public Functions Function { get; set; } = Functions.Murmur3;
}
