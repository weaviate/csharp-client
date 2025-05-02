namespace Weaviate.Client.Models;

public record WeaviateGroup
{
    public required string Name { get; init; }
    public required WeaviateGroupByObject[] Objects { get; init; } = Array.Empty<WeaviateGroupByObject>();
}

public record WeaviateGroupByObject : WeaviateObject
{
    public required string BelongsToGroup { get; init; }
    public WeaviateGroupByObject(string collectionName) : base(collectionName) { }
}

public record GroupByResult(IEnumerable<WeaviateGroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups)
{
    public static implicit operator (IEnumerable<WeaviateGroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups)(GroupByResult value)
    {
        return (value.Objects, value.Groups);
    }

    public static implicit operator GroupByResult((IEnumerable<WeaviateGroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups) value)
    {
        return new GroupByResult(value.Objects, value.Groups);
    }
}