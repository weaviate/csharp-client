using System.Collections.Immutable;

namespace Weaviate.Client.Models;

// StopwordConfig fine-grained control over stopword list usage
//
public record StopwordConfig
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
    public ImmutableList<string> Additions { get; set; } = [];

    /// <summary>
    /// Pre-existing list of common words by language (default: 'en'). Options: ['en', 'none'].
    /// </summary>
    public Presets Preset { get; set; } = Presets.EN;

    /// <summary>
    /// Stopwords to be removed from consideration (default: []). Can be any array of custom strings.
    /// </summary>
    public ImmutableList<string> Removals { get; set; } = [];
}
