namespace Weaviate.Client.Models;

// InvertedIndexConfig Configure the inverted index built into Weaviate (default: 60).
//
public class InvertedIndexConfig
{
    public BM25Config? Bm25 { get; set; }
    public int CleanupIntervalSeconds { get; set; } = 60;
    public bool IndexNullState { get; set; } = false;
    public bool IndexPropertyLength { get; set; } = false;
    public bool IndexTimestamps { get; set; } = false;
    public StopwordConfig? Stopwords { get; set; } = new StopwordConfig();
}
