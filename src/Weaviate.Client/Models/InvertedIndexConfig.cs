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

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Bm25);
        hash.Add(CleanupIntervalSeconds);
        hash.Add(IndexNullState);
        hash.Add(IndexPropertyLength);
        hash.Add(IndexTimestamps);
        hash.Add(Stopwords);
        return hash.ToHashCode();
    }

    public virtual bool Equals(InvertedIndexConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return EqualityComparer<BM25Config?>.Default.Equals(Bm25, other.Bm25)
            && CleanupIntervalSeconds == other.CleanupIntervalSeconds
            && IndexNullState == other.IndexNullState
            && IndexPropertyLength == other.IndexPropertyLength
            && IndexTimestamps == other.IndexTimestamps
            && EqualityComparer<StopwordConfig?>.Default.Equals(Stopwords, other.Stopwords);
    }
}
