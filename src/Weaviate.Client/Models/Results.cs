using System.Collections;

namespace Weaviate.Client.Models;

#region Generics
/// <summary>
/// Represents a group of objects in Weaviate grouped by a key.
/// </summary>
public record WeaviateGroup<TGroup>
{
    /// <summary>
    /// The group name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The objects in the group.
    /// </summary>
    public required IList<TGroup> Objects { get; init; } = [];

    /// <summary>
    /// The minimum distance in the group.
    /// </summary>
    public float MinDistance { get; init; } = 0;

    /// <summary>
    /// The maximum distance in the group.
    /// </summary>
    public float MaxDistance { get; init; } = 0;

    /// <summary>
    /// The number of objects in the group.
    /// </summary>
    public int NumberOfObjects => Objects.Count;
}

/// <summary>
/// Represents the result of a group-by operation.
/// </summary>
public record GroupByResult<TObject, TGroup>(
    IList<TObject> Objects,
    IDictionary<string, TGroup> Groups
);

/// <summary>
/// Represents a result set of Weaviate objects.
/// </summary>
public record WeaviateResult<TObject> : IEnumerable<TObject>
{
    /// <summary>
    /// The objects in the result set.
    /// </summary>
    public IList<TObject> Objects { get; init; } = Array.Empty<TObject>();

    /// <inheritdoc/>
    public IEnumerator<TObject> GetEnumerator() => Objects.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
#endregion

#region Direct Query Results
/// <summary>
/// Represents the result of a group-by operation for direct queries.
/// </summary>
public record GroupByResult(IList<GroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups)
    : GroupByResult<GroupByObject, WeaviateGroup>(Objects, Groups)
{
    private static readonly GroupByResult _empty = new(
        Array.Empty<GroupByObject>(),
        new Dictionary<string, WeaviateGroup>()
    );

    /// <summary>
    /// An empty group-by result.
    /// </summary>
    public static GroupByResult Empty => _empty;
}

/// <summary>
/// Represents a group in a group-by result.
/// </summary>
public record WeaviateGroup : WeaviateGroup<GroupByObject>;

/// <summary>
/// Represents an object in a group-by result.
/// </summary>
public record GroupByObject : WeaviateObject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GroupByObject"/> class.
    /// </summary>
    /// <param name="from">The source object.</param>
    internal GroupByObject(WeaviateObject from)
        : base(from) { }

    /// <summary>
    /// The group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }
}

/// <summary>
/// Represents a Weaviate object.
/// </summary>
public record WeaviateObject
{
    /// <summary>
    /// The object's UUID.
    /// </summary>
    public Guid? UUID { get; set; }

    /// <summary>
    /// The collection name.
    /// </summary>
    public string? Collection { get; init; }

    /// <summary>
    /// The object's metadata.
    /// </summary>
    public Metadata Metadata { get; set; } = new Metadata();

    /// <summary>
    /// The tenant name.
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// The object's properties.
    /// </summary>
    public IDictionary<string, object?> Properties { get; set; } =
        new Dictionary<string, object?>();

    /// <summary>
    /// The object's references.
    /// </summary>
    public IDictionary<string, IList<WeaviateObject>> References { get; set; } =
        new Dictionary<string, IList<WeaviateObject>>();

    /// <summary>
    /// The object's vectors.
    /// </summary>
    public Vectors Vectors { get; set; } = new Vectors();
}

/// <summary>
/// Represents a result set of Weaviate objects.
/// </summary>
public record WeaviateResult : WeaviateResult<WeaviateObject>
{
    private static readonly WeaviateResult _empty = new();

    /// <summary>
    /// An empty result set.
    /// </summary>
    public static WeaviateResult Empty => _empty;
}
#endregion

#region Generative Query Results
/// <summary>
/// Represents the result of a generative group-by query.
/// </summary>
public record GenerativeGroupByResult(
    IList<GenerativeGroupByObject> Objects,
    IDictionary<string, GenerativeWeaviateGroup> Groups,
    GenerativeResult Generative
) : GroupByResult<GenerativeGroupByObject, GenerativeWeaviateGroup>(Objects, Groups)
{
    private static readonly GenerativeGroupByResult _empty = new(
        Array.Empty<GenerativeGroupByObject>(),
        new Dictionary<string, GenerativeWeaviateGroup>(),
        new GenerativeResult(Array.Empty<GenerativeReply>())
    );

    /// <summary>
    /// An empty generative group-by result.
    /// </summary>
    public static GenerativeGroupByResult Empty => _empty;
}

/// <summary>
/// Represents a generative group in a group-by result.
/// </summary>
public record GenerativeWeaviateGroup : WeaviateGroup<GenerativeGroupByObject>
{
    /// <summary>
    /// The generative result for the group.
    /// </summary>
    public GenerativeResult? Generative { get; set; }
}

/// <summary>
/// Represents an object in a generative group-by result.
/// </summary>
public record GenerativeGroupByObject : GenerativeWeaviateObject
{
    /// <summary>
    /// The group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }
}

/// <summary>
/// Represents a generative Weaviate object.
/// </summary>
public record GenerativeWeaviateObject : WeaviateObject
{
    /// <summary>
    /// The generative result for the object.
    /// </summary>
    public GenerativeResult? Generative { get; set; }
}

/// <summary>
/// Represents a result set of generative Weaviate objects.
/// </summary>
public record GenerativeWeaviateResult : WeaviateResult<GenerativeWeaviateObject>
{
    private static readonly GenerativeWeaviateResult _empty = new()
    {
        Generative = GenerativeResult.Empty,
    };

    /// <summary>
    /// An empty generative result set.
    /// </summary>
    public static GenerativeWeaviateResult Empty => _empty;

    /// <summary>
    /// The generative result for the result set.
    /// </summary>
    public required GenerativeResult Generative { get; init; }
}
#endregion

#region Generative Specific Types
/// <summary>
/// Represents debug information for a generative query.
/// </summary>
public record GenerativeDebug(string? FullPrompt = null);

/// <summary>
/// Represents a generative reply.
/// </summary>
public record GenerativeReply(string Text);

/// <summary>
/// Represents a collection of generative replies as a result.
/// </summary>
public record GenerativeResult(IList<GenerativeReply> Values) : IList<string>
{
    private static readonly GenerativeResult _empty = new(Array.Empty<GenerativeReply>());

    /// <summary>
    /// An empty generative result.
    /// </summary>
    public static readonly GenerativeResult Empty = _empty;

    /// <inheritdoc/>
    public string this[int index]
    {
        get => Values[index].Text;
        set => Values[index] = Values[index] with { Text = value };
    }

    /// <inheritdoc/>
    public int Count => Values.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => Values.IsReadOnly;

    /// <inheritdoc/>
    public void Add(string item)
    {
        Values.Add(new GenerativeReply(item));
    }

    /// <inheritdoc/>
    public void Clear()
    {
        Values.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(string item)
    {
        return Values.Any(v => v.Text == item);
    }

    /// <inheritdoc/>
    public void CopyTo(string[] array, int arrayIndex)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            array[arrayIndex + i] = Values[i].Text;
        }
    }

    /// <inheritdoc/>
    public IEnumerator<string> GetEnumerator()
    {
        return Values.Select(v => v.Text).GetEnumerator();
    }

    /// <inheritdoc/>
    public int IndexOf(string item)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            if (Values[i].Text == item)
                return i;
        }
        return -1;
    }

    /// <inheritdoc/>
    public void Insert(int index, string item)
    {
        Values.Insert(index, new GenerativeReply(item));
    }

    /// <inheritdoc/>
    public bool Remove(string item)
    {
        int idx = IndexOf(item);
        if (idx >= 0)
        {
            Values.RemoveAt(idx);
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public void RemoveAt(int index)
    {
        Values.RemoveAt(index);
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
#endregion
