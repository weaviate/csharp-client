namespace Weaviate.Client.Models;

public record GroupByConstraint
{
    public required string PropertyName { get; set; }
    public uint NumberOfGroups { get; set; }
    public uint ObjectsPerGroup { get; set; }
}
