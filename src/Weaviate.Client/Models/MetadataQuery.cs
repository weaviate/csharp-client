namespace Weaviate.Client.Models;

[Flags]
public enum MetadataOptions
{
    None = 0,
    Vector = 1 << 0, // 2^0
    CreationTime = 1 << 1, // 2^1
    LastUpdateTime = 1 << 2, // 2^2
    Distance = 1 << 3, // 2^3
    Certainty = 1 << 4, // 2^4
    Score = 1 << 5, // 2^5
    ExplainScore = 1 << 6, // 2^6
    IsConsistent = 1 << 7, // 2^7
}

public record MetadataQuery(
    MetadataOptions Options = MetadataOptions.None,
    HashSet<string>? Vectors = null
)
{
    // Implicit conversion from MetadataOptions to MetadataQuery
    public static implicit operator MetadataQuery(MetadataOptions options) =>
        new MetadataQuery(options);

    // Implicit conversion from HashSet<string> to MetadataQuery
    public static implicit operator MetadataQuery(string[] vectors) =>
        new MetadataQuery(MetadataOptions.None, [.. vectors]);

    // Implicit conversion from (MetadataOptions, HashSet<string>) to MetadataQuery
    public static implicit operator MetadataQuery(
        (MetadataOptions options, string[] vectors) metadata
    ) => new MetadataQuery(metadata.options, [.. metadata.vectors]);

    readonly HashSet<string> _vectors = [.. Vectors ?? new HashSet<string>()];

    public bool Vector => (Options & MetadataOptions.Vector) != 0;
    public bool CreationTime => (Options & MetadataOptions.CreationTime) != 0;
    public bool LastUpdateTime => (Options & MetadataOptions.LastUpdateTime) != 0;
    public bool Distance => (Options & MetadataOptions.Distance) != 0;
    public bool Certainty => (Options & MetadataOptions.Certainty) != 0;
    public bool Score => (Options & MetadataOptions.Score) != 0;
    public bool ExplainScore => (Options & MetadataOptions.ExplainScore) != 0;
    public bool IsConsistent => (Options & MetadataOptions.IsConsistent) != 0;

    public HashSet<string> Vectors => _vectors;
}
