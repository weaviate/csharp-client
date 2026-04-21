namespace Weaviate.Client.Models;

/// <summary>
/// The inverted index config
/// </summary>
public record InvertedIndexConfig : IEquatable<InvertedIndexConfig>
{
    /// <summary>
    /// The inverted index config
    /// </summary>
    private static readonly Lazy<InvertedIndexConfig> defaultInstance =
        new Lazy<InvertedIndexConfig>(() => new());

    /// <summary>
    /// Gets the value of the default
    /// </summary>
    public static InvertedIndexConfig Default => defaultInstance.Value;

    /// <summary>
    /// Gets or sets the value of the bm 25
    /// </summary>
    public BM25Config? Bm25 { get; set; } = BM25Config.Default;

    /// <summary>
    /// Gets or sets the value of the cleanup interval seconds
    /// </summary>
    public int CleanupIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the value of the index null state
    /// </summary>
    public bool IndexNullState { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the index property length
    /// </summary>
    public bool IndexPropertyLength { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the index timestamps
    /// </summary>
    public bool IndexTimestamps { get; set; } = false;

    /// <summary>
    /// Gets or sets the value of the stopwords
    /// </summary>
    public StopwordConfig? Stopwords { get; set; } = StopwordConfig.Default;

    /// <summary>
    /// Gets or sets the value of the using block max wand
    /// </summary>
    public bool? UsingBlockMaxWAND { get; set; } = null;

    /// <summary>
    /// Optional named stopword presets defined at the collection level.
    /// Each entry is a preset name → list of stopwords. Individual properties
    /// can reference a preset via <see cref="TextAnalyzerConfig.StopwordPreset"/>.
    /// Requires Weaviate ≥ 1.37.0.
    /// </summary>
    public IDictionary<string, IList<string>>? StopwordPresets { get; set; } = null;

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Bm25?.GetHashCode() ?? 0);
        hash.Add(CleanupIntervalSeconds);
        hash.Add(IndexNullState);
        hash.Add(IndexPropertyLength);
        hash.Add(IndexTimestamps);
        hash.Add(Stopwords?.GetHashCode() ?? 0);
        hash.Add(UsingBlockMaxWAND);
        if (StopwordPresets is not null)
        {
            foreach (var kvp in StopwordPresets.OrderBy(kvp => kvp.Key, StringComparer.Ordinal))
            {
                hash.Add(kvp.Key);
                foreach (var word in kvp.Value)
                    hash.Add(word);
            }
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
    public virtual bool Equals(InvertedIndexConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (!EqualityComparer<BM25Config?>.Default.Equals(Bm25, other.Bm25))
            return false;

        if (CleanupIntervalSeconds != other.CleanupIntervalSeconds)
            return false;

        if (IndexNullState != other.IndexNullState)
            return false;

        if (IndexPropertyLength != other.IndexPropertyLength)
            return false;

        if (IndexTimestamps != other.IndexTimestamps)
            return false;

        if (!EqualityComparer<StopwordConfig?>.Default.Equals(Stopwords, other.Stopwords))
            return false;

        if (
            UsingBlockMaxWAND is not null
            && other.UsingBlockMaxWAND is not null
            && UsingBlockMaxWAND != other.UsingBlockMaxWAND
        )
            return false;

        if (!StopwordPresetsEqual(StopwordPresets, other.StopwordPresets))
            return false;

        return true;
    }

    private static bool StopwordPresetsEqual(
        IDictionary<string, IList<string>>? a,
        IDictionary<string, IList<string>>? b
    )
    {
        if (ReferenceEquals(a, b))
            return true;
        if (a is null || b is null)
            return false;
        if (a.Count != b.Count)
            return false;
        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var otherValue))
                return false;
            if (!kvp.Value.SequenceEqual(otherValue))
                return false;
        }
        return true;
    }
}
