namespace Weaviate.Client.Models;

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
