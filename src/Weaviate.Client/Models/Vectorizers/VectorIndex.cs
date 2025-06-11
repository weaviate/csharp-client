namespace Weaviate.Client.Models.Vectorizers;

public abstract record VectorIndexConfig(string Identifier, dynamic? Configuration)
{
    private static readonly Lazy<VectorIndexConfig> _default = new(() => new HNSW());

    public static VectorIndexConfig Default => _default.Value;

    internal static VectorIndexConfig Factory(string type, object? vectorIndexConfig)
    {
        return type switch
        {
            "hnsw" => new VectorIndexConfig.HNSW() { Configuration = vectorIndexConfig },
            "flat" => new VectorIndexConfig.Flat() { Configuration = vectorIndexConfig },
            "dynamic" => new VectorIndexConfig.Dynamic() { Configuration = vectorIndexConfig },
            _ => VectorIndexConfig.Default,
        };
    }

    public sealed record HNSW() : VectorIndexConfig("hnsw", new { });

    public sealed record Flat() : VectorIndexConfig("flat", new { });

    public sealed record Dynamic() : VectorIndexConfig("dynamic", new { });
};
