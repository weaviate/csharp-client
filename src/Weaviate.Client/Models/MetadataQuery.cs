using System.Collections;

namespace Weaviate.Client.Models;

public class VectorQuery : IEnumerable<string>
{
    private List<string>? _vectors = null;

    public VectorQuery() { }

    public VectorQuery(IEnumerable<string>? vectors)
    {
        _vectors = vectors?.ToList();
    }

    public string[]? Vectors => _vectors?.ToArray();

    public void Add(string vector)
    {
        _vectors ??= new List<string>();
        _vectors.Add(vector);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return _vectors?.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Implicit conversion from bool to VectorQuery
    // false stores null, true stores empty array
    public static implicit operator VectorQuery(bool includeVectors) =>
        new(includeVectors ? [] : null);

    // Implicit conversion from string array to VectorQuery
    public static implicit operator VectorQuery(string[] vectors) => new(vectors);

    // Implicit conversion from string to VectorQuery
    public static implicit operator VectorQuery(string vector) => new[] { vector };
}

[Flags]
public enum MetadataOptions
{
    None = 0,
    CreationTime = 1 << 1, // 2^1
    LastUpdateTime = 1 << 2, // 2^2
    Distance = 1 << 3, // 2^3
    Certainty = 1 << 4, // 2^4
    Score = 1 << 5, // 2^5
    ExplainScore = 1 << 6, // 2^6
    IsConsistent = 1 << 7, // 2^7
    All =
        CreationTime | LastUpdateTime | Distance | Certainty | Score | ExplainScore | IsConsistent,
}

public record MetadataQuery
{
    public MetadataQuery(MetadataOptions options = MetadataOptions.None)
    {
        Options = options;
    }

    public MetadataQuery Enable(MetadataOptions enableOptions)
    {
        Options |= enableOptions;
        return this;
    }

    public MetadataQuery Disable(MetadataOptions disableOptions)
    {
        Options &= ~disableOptions;
        return this;
    }

    // Implicit conversion from MetadataOptions to MetadataQuery
    public static implicit operator MetadataQuery(MetadataOptions options) => new(options);

    public bool CreationTime => (Options & MetadataOptions.CreationTime) != 0;
    public bool LastUpdateTime => (Options & MetadataOptions.LastUpdateTime) != 0;
    public bool Distance => (Options & MetadataOptions.Distance) != 0;
    public bool Certainty => (Options & MetadataOptions.Certainty) != 0;
    public bool Score => (Options & MetadataOptions.Score) != 0;
    public bool ExplainScore => (Options & MetadataOptions.ExplainScore) != 0;
    public bool IsConsistent => (Options & MetadataOptions.IsConsistent) != 0;

    public MetadataOptions Options { get; private set; } = MetadataOptions.None;
}
