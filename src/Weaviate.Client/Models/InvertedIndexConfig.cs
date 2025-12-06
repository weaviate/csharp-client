namespace Weaviate.Client.Models;

public record InvertedIndexConfig : IEquatable<InvertedIndexConfig>
{
    private static readonly Lazy<InvertedIndexConfig> defaultInstance =
        new Lazy<InvertedIndexConfig>(() => new());

    public static InvertedIndexConfig Default => defaultInstance.Value;

    public BM25Config? Bm25 { get; set; } = BM25Config.Default;
    public int CleanupIntervalSeconds { get; set; } = 60;
    public bool IndexNullState { get; set; } = false;
    public bool IndexPropertyLength { get; set; } = false;
    public bool IndexTimestamps { get; set; } = false;
    public StopwordConfig? Stopwords { get; set; } = StopwordConfig.Default;
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
