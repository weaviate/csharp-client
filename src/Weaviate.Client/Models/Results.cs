using System.Collections;

namespace Weaviate.Client.Models;

#region Generics
public record WeaviateGroup<TGroup>
{
    public required string Name { get; init; }
    public required IList<TGroup> Objects { get; init; } = [];
    public float MinDistance { get; init; } = 0;
    public float MaxDistance { get; init; } = 0;
    public int NumberOfObjects => Objects.Count;
}

public record GroupByResult<TObject, TGroup>(
    IList<TObject> Objects,
    IDictionary<string, TGroup> Groups
) { };

public record WeaviateResult<TObject> : IEnumerable<TObject>
{
    public IList<TObject> Objects { get; init; } = Array.Empty<TObject>();

    public IEnumerator<TObject> GetEnumerator() => Objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
#endregion

#region Direct Query Results
public record GroupByResult(IList<GroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups)
    : GroupByResult<GroupByObject, WeaviateGroup>(Objects, Groups)
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
    public static GenerativeGroupByResult Empty => _empty;
}

public record GenerativeWeaviateGroup : WeaviateGroup<GenerativeGroupByObject>
{
    public GenerativeResult? Generative { get; set; }
};

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
    string Text
// GenerativeDebug? Debug = null,
// object? Metadata = null,
);

public record GenerativeResult(IList<GenerativeReply> Values) : IList<string>
{
    private static readonly GenerativeResult _empty = new(Array.Empty<GenerativeReply>());
    public static readonly GenerativeResult Empty = _empty;

    public string this[int index]
    {
        get => Values[index].Text;
        set => Values[index] = Values[index] with { Text = value };
    }

    public int Count => Values.Count;

    public bool IsReadOnly => Values.IsReadOnly;

    public void Add(string item)
    {
        Values.Add(new GenerativeReply(item));
    }

    public void Clear()
    {
        Values.Clear();
    }

    public bool Contains(string item)
    {
        return Values.Any(v => v.Text == item);
    }

    public void CopyTo(string[] array, int arrayIndex)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            array[arrayIndex + i] = Values[i].Text;
        }
    }

    public IEnumerator<string> GetEnumerator()
    {
        return Values.Select(v => v.Text).GetEnumerator();
    }

    public int IndexOf(string item)
    {
        for (int i = 0; i < Values.Count; i++)
        {
            if (Values[i].Text == item)
                return i;
        }
        return -1;
    }

    public void Insert(int index, string item)
    {
        Values.Insert(index, new GenerativeReply(item));
    }

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

    public void RemoveAt(int index)
    {
        Values.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
#endregion
