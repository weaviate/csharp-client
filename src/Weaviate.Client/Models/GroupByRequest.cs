namespace Weaviate.Client.Models;

public record GroupByRequest
{
    public required string PropertyName { get; set; }
    public uint NumberOfGroups { get; set; }
    public uint ObjectsPerGroup { get; set; }
}
