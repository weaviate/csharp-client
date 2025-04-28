namespace Weaviate.Client.Rest.Dto;

public class WeaviateGroup
{
    public string Name { get; set; }
    public WeaviateGroupByObject[] Objects { get; set; }
}

public class WeaviateGroupByObject : WeaviateObject
{
    public string BelongsToGroup { get; set; }
}
