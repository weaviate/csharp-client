namespace Weaviate.Client.Models;

// BM25Config tuning parameters for the BM25 algorithm
//
public record BM25Config
{
    private static readonly Lazy<BM25Config> _default = new Lazy<BM25Config>(() => new());

    public static BM25Config Default => _default.Value;

    /// <summary>
    /// Calibrates term-weight scaling based on the document length (default: 0.75).
    /// </summary>
    public double B { get; set; } = 0.75;

    /// <summary>
    /// Calibrates term-weight scaling based on the term frequency within a document (default: 1.2).
    /// </summary>
    public double K1 { get; set; } = 1.2;

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(B);
        hash.Add(K1);
        return hash.ToHashCode();
    }

    public virtual bool Equals(BM25Config? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Math.Round(B, 6).Equals(Math.Round(other.B, 6))
            && Math.Round(K1, 6).Equals(Math.Round(other.K1, 6));
    }
}
