using System.Collections;

namespace Weaviate.Client.Models;

#region Generics
public record WeaviateGroup<TGroup>
{
    public required string Name { get; init; }
    public required ICollection<TGroup> Objects { get; init; } = [];
}

public record GroupByResult<TObject, TGroup>(
    ICollection<TObject> Objects,
    IDictionary<string, TGroup> Groups
);

public record WeaviateResult<TObject> : IEnumerable<TObject>
{
    public ICollection<TObject> Objects { get; init; } = Array.Empty<TObject>();

    public IEnumerator<TObject> GetEnumerator() => Objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
#endregion

#region Direct Query Results
public record GroupByResult(
    ICollection<GroupByObject> Objects,
    IDictionary<string, WeaviateGroup> Groups
) : GroupByResult<GroupByObject, WeaviateGroup>(Objects, Groups)
{
    private static readonly GroupByResult _empty = new(
        Array.Empty<GroupByObject>(),
        new Dictionary<string, WeaviateGroup>()
    );
    public static GroupByResult Empty => _empty;
}

public record WeaviateGroup : WeaviateGroup<GroupByObject>;

public record GroupByObject : WeaviateObject
{
    internal GroupByObject(WeaviateObject from)
        : base(from) { }

    public required string BelongsToGroup { get; init; }
}

public record WeaviateObject
{
    public Guid? ID { get; set; }

    public string? Collection { get; init; }

    public Metadata Metadata { get; set; } = new Metadata();

    public string? Tenant { get; set; }

    public IDictionary<string, object?> Properties { get; set; } =
        new Dictionary<string, object?>();

    public IDictionary<string, IList<WeaviateObject>> References { get; set; } =
        new Dictionary<string, IList<WeaviateObject>>();

    public Vectors Vectors { get; set; } = new Vectors();
}

public record WeaviateResult : WeaviateResult<WeaviateObject>
{
    private static readonly WeaviateResult _empty = new();
    public static WeaviateResult Empty => _empty;
}
#endregion

#region Generative Query Results
// Each WeaviateObject has a Generative property that can hold generative AI related data.
// Each WeaviateGroup has a Generative property that can hold generative AI related data.
// Each ResultSet has a Generative property that can hold generative AI related data.
public record GenerativeGroupByResult(
    ICollection<GenerativeGroupByObject> Objects,
    IDictionary<string, GenerativeWeaviateGroup> Groups,
    GenerativeResult Generative
) : GroupByResult<GenerativeGroupByObject, GenerativeWeaviateGroup>(Objects, Groups)
{
    private static readonly GenerativeGroupByResult _empty = new(
        Array.Empty<GenerativeGroupByObject>(),
        new Dictionary<string, GenerativeWeaviateGroup>(),
        new GenerativeResult(Array.Empty<GenerativeReply>())
    );
    public static GenerativeGroupByResult Empty => _empty;
}

public record GenerativeWeaviateGroup : WeaviateGroup<GenerativeGroupByObject>;

public record GenerativeGroupByObject : GenerativeWeaviateObject
{
    public required string BelongsToGroup { get; init; }
}

public record GenerativeWeaviateObject : WeaviateObject
{
    public GenerativeResult? Generative { get; set; }
}

public record GenerativeWeaviateResult : WeaviateResult<GenerativeWeaviateObject>
{
    private static readonly GenerativeWeaviateResult _empty = new()
    {
        Generative = GenerativeResult.Empty,
    };
    public static GenerativeWeaviateResult Empty => _empty;

    public required GenerativeResult Generative { get; init; }
}
#endregion

#region Generative Specific Types
public record GenerativeDebug(string? FullPrompt = null);

public record GenerativeReply(
    string Result,
    GenerativeDebug? Debug = null,
    object? Metadata = null // TODO: GenerativeMetadata?
);

public record GenerativeResult(ICollection<GenerativeReply> Values)
{
    private static readonly GenerativeResult _empty = new(Array.Empty<GenerativeReply>());
    public static readonly GenerativeResult Empty = _empty;
}
#endregion
