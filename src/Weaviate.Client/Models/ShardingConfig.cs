namespace Weaviate.Client.Models;

public record ShardingConfig
{
    /// <summary>
    /// Gets the default configuration.
    /// </summary>
    private static readonly Lazy<ShardingConfig> _default = new(() => new());

    public static ShardingConfig Default => _default.Value;

    public int VirtualPerPhysical { get; set; } = 128;
    public int DesiredCount { get; set; } = 1;
    public int ActualCount { get; set; } = 1;
    public int DesiredVirtualCount { get; set; } = 128;
    public int ActualVirtualCount { get; set; } = 128;
    public string Key { get; set; } = "_id";
    public string Strategy { get; set; } = "hash";
    public string Function { get; set; } = "murmur3";
}
