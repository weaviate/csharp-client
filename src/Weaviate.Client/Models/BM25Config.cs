namespace Weaviate.Client.Models;

// BM25Config tuning parameters for the BM25 algorithm
//
/// <summary>
/// The bm 25 config
/// </summary>
public record BM25Config
{
    /// <summary>
    /// The bm 25 config
    /// </summary>
    private static readonly Lazy<BM25Config> _default = new Lazy<BM25Config>(() => new());

    /// <summary>
    /// Gets the value of the default
    /// </summary>
    public static BM25Config Default => _default.Value;

    /// <summary>
    /// Calibrates term-weight scaling based on the document length (default: 0.75).
    /// </summary>
    public double B { get; set; } = 0.75;

    /// <summary>
    /// Calibrates term-weight scaling based on the term frequency within a document (default: 1.2).
    /// </summary>
    public double K1 { get; set; } = 1.2;

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(B);
        hash.Add(K1);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
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
