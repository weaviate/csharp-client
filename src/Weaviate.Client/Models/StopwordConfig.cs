namespace Weaviate.Client.Models;

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