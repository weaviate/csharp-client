namespace Weaviate.Client.Models;

// StopwordConfig fine-grained control over stopword list usage
//
public record StopwordConfig : IEquatable<StopwordConfig>
{
    public enum Presets
    {
        [System.Runtime.Serialization.EnumMember(Value = "none")]
        None,

        [System.Runtime.Serialization.EnumMember(Value = "en")]
        EN,
    }

    private static readonly Lazy<StopwordConfig> defaultInstance = new Lazy<StopwordConfig>(() =>
        new StopwordConfig()
    );

    public static StopwordConfig Default => defaultInstance.Value;

    /// <summary>
    /// Stopwords to be considered additionally (default: []). Can be any array of custom strings.
    /// </summary>
    public List<string> Additions { get; set; } = new();

    /// <summary>
    /// Pre-existing list of common words by language (default: 'en'). Options: ['en', 'none'].
    /// </summary>
    public Presets Preset { get; set; } = Presets.EN;

    /// <summary>
    /// Stopwords to be removed from consideration (default: []). Can be any array of custom strings.
    /// </summary>
    public List<string> Removals { get; set; } = new();

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Additions);
        hash.Add(Preset);
        hash.Add(Removals);
        return hash.ToHashCode();
    }

    public virtual bool Equals(StopwordConfig? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Additions.SequenceEqual(other.Additions)
            && Preset == other.Preset
            && Removals.SequenceEqual(other.Removals);
    }
}
