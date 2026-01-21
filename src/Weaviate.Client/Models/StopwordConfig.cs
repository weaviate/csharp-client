using System.Collections.Immutable;

namespace Weaviate.Client.Models;

// StopwordConfig fine-grained control over stopword list usage
//
/// <summary>
/// The stopword config
/// </summary>
public record StopwordConfig
{
    /// <summary>
    /// The presets enum
    /// </summary>
    public enum Presets
    {
        /// <summary>
        /// The none presets
        /// </summary>
        [System.Text.Json.Serialization.JsonStringEnumMemberName("none")]
        None,

        /// <summary>
        /// The en presets
        /// </summary>
        [System.Text.Json.Serialization.JsonStringEnumMemberName("en")]
        EN,
    }

    /// <summary>
    /// The stopword config
    /// </summary>
    private static readonly Lazy<StopwordConfig> defaultInstance = new(() => new StopwordConfig());

    /// <summary>
    /// Gets the value of the default
    /// </summary>
    public static StopwordConfig Default => defaultInstance.Value;

    /// <summary>
    /// Stopwords to be considered additionally (default: []). Can be any array of custom strings.
    /// </summary>
    public ImmutableList<string> Additions { get; set; } = [];

    /// <summary>
    /// Pre-existing list of common words by language (default: 'en'). Options: ['en', 'none'].
    /// </summary>
    [System.Text.Json.Serialization.JsonConverter(
        typeof(System.Text.Json.Serialization.JsonStringEnumConverter)
    )]
    public Presets Preset { get; set; } = Presets.EN;

    /// <summary>
    /// Stopwords to be removed from consideration (default: []). Can be any array of custom strings.
    /// </summary>
    public ImmutableList<string> Removals { get; set; } = [];

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Additions);
        hash.Add(Preset);
        hash.Add(Removals);
        return hash.ToHashCode();
    }

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
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
