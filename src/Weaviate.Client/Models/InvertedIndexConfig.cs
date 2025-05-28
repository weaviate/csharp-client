namespace Weaviate.Client.Models;

public record InvertedIndexConfig
{
    private static readonly Lazy<InvertedIndexConfig> defaultInstance = new Lazy<InvertedIndexConfig>(() => new());

    public static InvertedIndexConfig Default => defaultInstance.Value;

    public BM25Config? Bm25 { get; set; } = BM25Config.Default;
    public int CleanupIntervalSeconds { get; set; } = 60;
    public bool IndexNullState { get; set; } = false;
    public bool IndexPropertyLength { get; set; } = false;
    public bool IndexTimestamps { get; set; } = false;
    public StopwordConfig? Stopwords { get; set; } = new StopwordConfig();
}
