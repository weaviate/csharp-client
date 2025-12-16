namespace Weaviate.Client.Models;

/// <summary>
/// Configuration for the inverted index used for filtering and keyword search.
/// </summary>
/// <remarks>
/// The inverted index is used for filtering operations and BM25 keyword search.
/// This configuration controls various aspects of index behavior including cleanup intervals,
/// BM25 scoring, stopword handling, and timestamp indexing.
/// </remarks>
public record InvertedIndexConfig : IEquatable<InvertedIndexConfig>
{
    private static readonly Lazy<InvertedIndexConfig> defaultInstance =
        new Lazy<InvertedIndexConfig>(() => new());

    /// <summary>
    /// Gets the default inverted index configuration with standard settings.
    /// </summary>
    public static InvertedIndexConfig Default => defaultInstance.Value;

    /// <summary>
    /// Gets or sets the BM25 configuration for keyword search scoring. Defaults to <see cref="BM25Config.Default"/>.
    /// </summary>
    public BM25Config? Bm25 { get; set; } = BM25Config.Default;

    /// <summary>
    /// Gets or sets the interval in seconds between index cleanup operations. Defaults to 60 seconds.
    /// </summary>
    public int CleanupIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets a value indicating whether to index null values for filtering. Defaults to false.
    /// </summary>
    public bool IndexNullState { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to index property lengths for filtering. Defaults to false.
    /// </summary>
    public bool IndexPropertyLength { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to index object timestamps (creation and update times). Defaults to false.
    /// </summary>
    public bool IndexTimestamps { get; set; } = false;

    /// <summary>
    /// Gets or sets the stopword configuration for text processing. Defaults to <see cref="StopwordConfig.Default"/>.
    /// </summary>
    public StopwordConfig? Stopwords { get; set; } = StopwordConfig.Default;

    /// <summary>
    /// Gets or sets a value indicating whether to use BlockMax WAND algorithm for faster BM25 queries.
    /// When null, Weaviate uses its default setting.
    /// </summary>
    public bool? UsingBlockMaxWAND { get; set; } = null;

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
        return hash.ToHashCode();
    }

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

        if (UsingBlockMaxWAND != other.UsingBlockMaxWAND)
            return false;

        return true;
    }
}
