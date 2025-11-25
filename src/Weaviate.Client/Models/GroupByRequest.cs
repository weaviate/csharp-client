namespace Weaviate.Client.Models;

public record GroupByRequest(string PropertyName)
{
    public uint NumberOfGroups { get; set; }
    public uint ObjectsPerGroup { get; set; }
}
