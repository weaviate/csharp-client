namespace Weaviate.Client.Rest.Models;

// InvertedIndexConfig Configure the inverted index built into Weaviate (default: 60).
//
public class InvertedIndexConfig
{
    public BM25Config? Bm25 { get; set; } = null;
    public int CleanupIntervalSeconds { get; set; } = 60;
    public bool IndexNullState { get; set; } = false;
    public bool IndexPropertyLength { get; set; } = false;
    public bool IndexTimestamps { get; set; } = false;
    public StopwordConfig? Stopwords { get; set; } = null;
}

// BM25Config tuning parameters for the BM25 algorithm
//
public class BM25Config
{
    /// <summary>
    /// Calibrates term-weight scaling based on the document length (default: 0.75).
    /// </summary>
    public float B { get; set; } = 0.75f;

    /// <summary>
    /// Calibrates term-weight scaling based on the term frequency within a document (default: 1.2).
    /// </summary>
    public float K1 { get; set; } = 1.2f;
}

// StopwordConfig fine-grained control over stopword list usage
//
public class StopwordConfig
{
    /// <summary>
    /// Stopwords to be considered additionally (default: []). Can be any array of custom strings.
    /// </summary>
    public IList<string> Additions { get; set; } = new List<string>();

    /// <summary>
    /// Pre-existing list of common words by language (default: 'en'). Options: ['en', 'none'].
    /// </summary>
    public string Preset { get; set; } = "en";

    /// <summary>
    /// Stopwords to be removed from consideration (default: []). Can be any array of custom strings.
    /// </summary>
    public IList<string> Removals { get; set; } = new List<string>();
}