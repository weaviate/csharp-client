using System.Text.Json;

namespace Weaviate.Client.Models;

public abstract record VectorIndexConfig(string Identifier, dynamic? Configuration)
{
    private static readonly Lazy<VectorIndexConfig> _default = new(() => new HNSW());

    public static VectorIndexConfig Default => _default.Value;

    internal static VectorIndexConfig Factory(string type, object? vectorIndexConfig)
    {
        if (vectorIndexConfig is JsonElement vic)
        {
            vectorIndexConfig = ObjectHelper.JsonElementToExpandoObject(vic);
        }

        return type switch
        {
            "hnsw" => new HNSW() { Configuration = vectorIndexConfig },
            "flat" => new Flat() { Configuration = vectorIndexConfig },
            "dynamic" => new Dynamic() { Configuration = vectorIndexConfig },
            _ => VectorIndexConfig.Default,
        };
    }

    public sealed record HNSW() : VectorIndexConfig("hnsw", new { });

    public sealed record Flat() : VectorIndexConfig("flat", new { });

    public sealed record Dynamic() : VectorIndexConfig("dynamic", new { });
};
