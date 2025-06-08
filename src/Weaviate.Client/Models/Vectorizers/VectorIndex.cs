namespace Weaviate.Client.Models.Vectorizers;

public abstract record VectorIndexConfig(string Identifier, dynamic? Configuration)
{
    private static readonly Lazy<VectorIndexConfig> _default = new(() => new VectorIndexConfigHNSW()
    );

    public static VectorIndexConfig Default => _default.Value;
};

public sealed record VectorIndexConfigHNSW() : VectorIndexConfig("hnsw", new { });

public sealed record VectorIndexConfigFlat() : VectorIndexConfig("flat", new { });

public sealed record VectorIndexConfigDynamic() : VectorIndexConfig("dynamic", new { });
