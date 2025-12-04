namespace Weaviate.Client.Models;

// BM25Config tuning parameters for the BM25 algorithm
//

public record BM25Config : IEquatable<BM25Config>
{
    private static readonly Lazy<BM25Config> _default = new Lazy<BM25Config>(() => new());
    public static BM25Config Default => _default.Value;

    private float _b = 0.75f;
    private float _k1 = 1.2f;

    /// <summary>
    /// Calibrates term-weight scaling based on the document length (default: 0.75).
    /// </summary>
    public double B
    {
        get => _b;
        set => _b = (float)value;
    }

    /// <summary>
    /// Calibrates term-weight scaling based on the document length (default: 1.2).
    /// </summary>
    public double K1
    {
        get => _k1;
        set => _k1 = (float)value;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_b);
        hash.Add(_k1);
        return hash.ToHashCode();
    }

    public virtual bool Equals(BM25Config? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return _b == other._b && _k1 == other._k1;
    }
}
