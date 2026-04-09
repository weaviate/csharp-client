namespace Weaviate.Client.Models;

/// <summary>Per-search-type execution profile within a shard.</summary>
public record SearchProfile
{
    /// <summary>Opaque metric name → value map. Keys vary by search type and server version.</summary>
    public IDictionary<string, string> Details { get; init; } = new Dictionary<string, string>();
}

/// <summary>Execution profile for one shard.</summary>
public record ShardProfile
{
    /// <summary>Shard identifier.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Cluster node that executed this shard's search.</summary>
    public string Node { get; init; } = string.Empty;

    /// <summary>Map of search type key (e.g. "vector", "keyword") to its profile.</summary>
    public IDictionary<string, SearchProfile> Searches { get; init; } =
        new Dictionary<string, SearchProfile>();
}

/// <summary>
/// Per-shard query profiling data returned when <see cref="MetadataOptions.QueryProfile"/> is enabled.
/// Contains timing breakdowns for each shard and search type.
/// Metric keys are dynamic; do not hardcode them.
/// </summary>
public record QueryProfile
{
    /// <summary>Profiles for each shard that participated in the query.</summary>
    public IList<ShardProfile> Shards { get; init; } = [];
}
