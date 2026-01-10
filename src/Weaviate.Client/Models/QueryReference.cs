namespace Weaviate.Client.Models;

/// <summary>
/// The query reference
/// </summary>
public record QueryReference
{
    /// <summary>
    /// Gets or inits the value of the link on
    /// </summary>
    public string LinkOn { get; init; }

    /// <summary>
    /// Gets or inits the value of the fields
    /// </summary>
    public string[]? Fields { get; init; }

    /// <summary>
    /// Gets or inits the value of the metadata
    /// </summary>
    public MetadataQuery? Metadata { get; init; }

    /// <summary>
    /// Gets or inits the value of the references
    /// </summary>
    public IList<QueryReference>? References { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryReference"/> class
    /// </summary>
    /// <param name="linkOn">The link on</param>
    /// <param name="fields">The fields</param>
    /// <param name="metadata">The metadata</param>
    /// <param name="references">The references</param>
    public QueryReference(
        string linkOn,
        string[]? fields = null,
        MetadataQuery? metadata = null,
        params QueryReference[]? references
    )
    {
        LinkOn = linkOn;
        Fields = fields;
        Metadata = metadata;
        References = references;
    }
}
