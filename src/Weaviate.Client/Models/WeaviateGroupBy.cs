namespace Weaviate.Client.Models;

public record WeaviateGroup
{
    public required string Name { get; init; }
    public required IEnumerable<GroupByObject> Objects { get; init; } = [];
}

public record GroupByObject : WeaviateObject
{
    internal GroupByObject(WeaviateObject from)
        : base(from) { }

    public required string BelongsToGroup { get; init; }
}

public record GroupByResult(
    IEnumerable<GroupByObject> Objects,
    IDictionary<string, WeaviateGroup> Groups
)
{
    private static readonly GroupByResult _empty = new(
        Enumerable.Empty<GroupByObject>(),
        new Dictionary<string, WeaviateGroup>()
    );
    public static GroupByResult Empty => _empty;

    public static implicit operator (
        IEnumerable<GroupByObject> Objects,
        IDictionary<string, WeaviateGroup> Groups
    )(GroupByResult value)
    {
        return (value.Objects, value.Groups);
    }

    public static implicit operator GroupByResult(
        (IEnumerable<GroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups) value
    )
    {
        return new GroupByResult(value.Objects, value.Groups);
    }
}
