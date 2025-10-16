using System.Text.Json.Serialization;
using Weaviate.Client.Models;
using Weaviate.Client.Rest;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client;

public class NodesClient
{
    private readonly WeaviateRestClient _client;

    internal NodesClient(WeaviateRestClient client)
    {
        _client = client;
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Verbosity
    {
        [JsonPropertyName("minimal")]
        Minimal,

        [JsonPropertyName("verbose")]
        Verbose,
    }

    public async Task<MinimalNode[]> NodesMinimal()
    {
        var nodes = await _client.Nodes(null, "minimal");
        if (nodes == null)
            return Array.Empty<MinimalNode>();

        return nodes
            .Where(n => n != null)
            .Select(n => new MinimalNode
            {
                GitHash = n.GitHash ?? string.Empty,
                Name = n.Name ?? string.Empty,
                Status = n.Status?.ToString() ?? "Unknown",
                Version = n.Version ?? string.Empty,
            })
            .ToArray();
    }

    public async Task<VerboseNode[]> NodesVerbose(string? collection = null)
    {
        var nodes = await _client.Nodes(collection, "verbose");
        if (nodes == null)
            return Array.Empty<VerboseNode>();

        return nodes
            .Where(n => n != null)
            .Select(n => new VerboseNode
            {
                GitHash = n.GitHash ?? string.Empty,
                Name = n.Name ?? string.Empty,
                Status = n.Status?.ToString() ?? "Unknown",
                Version = n.Version ?? string.Empty,
                Stats =
                    n.Stats != null
                        ? new Stats
                        {
                            ObjectCount = (int)(n.Stats!.ObjectCount ?? 0),
                            ShardCount = (int)(n.Stats.ShardCount ?? 0),
                        }
                        : null,
                Shards = (n.Shards ?? new List<NodeShardStatus>())
                    .Where(s =>
                        s != null && !string.IsNullOrEmpty(s.Class) && !string.IsNullOrEmpty(s.Name)
                    )
                    .Select(s => new Shard
                    {
                        Collection = s.Class!,
                        Name = s.Name!,
                        Node = s.Name!, // Using shard name as node name since that's what the original code did
                        ObjectCount = (int)(s.ObjectCount ?? 0),
                        VectorIndexingStatus = ParseVectorIndexingStatus(s.VectorIndexingStatus),
                        VectorQueueLength = (int)(s.VectorQueueLength ?? 0),
                        Compressed = ParseBooleanValue(s.Compressed),
                        Loaded = s.Loaded,
                    })
                    .ToArray(),
            })
            .ToArray();
    }

    private static VectorIndexingStatus ParseVectorIndexingStatus(object? status)
    {
        if (status == null)
            return VectorIndexingStatus.Ready;

        var statusString = status.ToString()?.ToUpperInvariant();
        return statusString switch
        {
            "READONLY" => VectorIndexingStatus.ReadOnly,
            "INDEXING" => VectorIndexingStatus.Indexing,
            "READY" => VectorIndexingStatus.Ready,
            _ => VectorIndexingStatus.Ready,
        };
    }

    private static bool ParseBooleanValue(object? value)
    {
        if (value == null)
            return false;

        if (value is bool boolValue)
            return boolValue;

        var stringValue = value.ToString()?.ToLowerInvariant();
        return stringValue is "true" or "1" or "yes";
    }
}
