namespace Weaviate.Client.Models;

public record QueryReference
{
    public string LinkOn { get; init; }
    public string[] Fields { get; init; }
    public MetadataQuery? Metadata { get; init; }
    public IList<QueryReference>? References { get; init; }

    public QueryReference(string linkOn, string[] fields, MetadataQuery? metadata = null, params QueryReference[]? references)
    {
        LinkOn = linkOn;
        Fields = fields;
        Metadata = metadata;
        References = references;
    }
}