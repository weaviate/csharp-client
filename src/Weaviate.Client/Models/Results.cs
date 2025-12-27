using System.Collections;

namespace Weaviate.Client.Models;

#region Generics
/// <summary>
/// Represents a group of objects when using GroupBy queries.
/// </summary>
/// <typeparam name="TGroup">The type of objects contained in the group.</typeparam>
public record WeaviateGroup<TGroup>
{
    /// <summary>
    /// Gets the name/value of the grouping property for this group.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Gets the list of objects belonging to this group.
    /// </summary>
    public required IList<TGroup> Objects { get; init; } = [];

    /// <summary>
    /// Gets the minimum distance from the query among all objects in this group.
    /// </summary>
    public float MinDistance { get; init; } = 0;

    /// <summary>
    /// Gets the maximum distance from the query among all objects in this group.
    /// </summary>
    public float MaxDistance { get; init; } = 0;

    /// <summary>
    /// Gets the number of objects in this group.
    /// </summary>
    public int NumberOfObjects => Objects.Count;
}

/// <summary>
/// Generic base class for GroupBy query results.
/// </summary>
/// <typeparam name="TObject">The type of individual objects in the result.</typeparam>
/// <typeparam name="TGroup">The type of grouped results.</typeparam>
/// <param name="Objects">All objects returned by the query.</param>
/// <param name="Groups">Objects organized by their grouping property values.</param>
public record GroupByResult<TObject, TGroup>(
    IList<TObject> Objects,
    IDictionary<string, TGroup> Groups
) { };

/// <summary>
/// Generic base class for query results containing a list of objects.
/// </summary>
/// <typeparam name="TObject">The type of objects in the result.</typeparam>
/// <remarks>
/// Implements IEnumerable to allow iteration over the objects directly.
/// </remarks>
public record WeaviateResult<TObject> : IEnumerable<TObject>
{
    /// <summary>
    /// Gets the list of objects returned by the query.
    /// </summary>
    public IList<TObject> Objects { get; init; } = Array.Empty<TObject>();

    /// <summary>
    /// Returns an enumerator that iterates through the objects.
    /// </summary>
    public IEnumerator<TObject> GetEnumerator() => Objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
#endregion

#region Direct Query Results
/// <summary>
/// Results from a GroupBy query, containing both individual objects and grouped results.
/// </summary>
/// <param name="Objects">All objects returned by the query.</param>
/// <param name="Groups">Objects organized by their grouping property values.</param>
/// <remarks>
/// GroupBy queries allow you to organize results by a specific property while still accessing individual objects.
/// </remarks>
public record GroupByResult(IList<GroupByObject> Objects, IDictionary<string, WeaviateGroup> Groups)
    : GroupByResult<GroupByObject, WeaviateGroup>(Objects, Groups)
{
    private static readonly GroupByResult _empty = new(
        Array.Empty<GroupByObject>(),
        new Dictionary<string, WeaviateGroup>()
    );

    /// <summary>
    /// Gets an empty GroupByResult with no objects or groups.
    /// </summary>
    public static GroupByResult Empty => _empty;
}

/// <summary>
/// A group of objects in a non-generic GroupBy query result.
/// </summary>
public record WeaviateGroup : WeaviateGroup<GroupByObject>;

/// <summary>
/// Represents an object in a GroupBy query result, including which group it belongs to.
/// </summary>
public record GroupByObject : WeaviateObject
{
    internal GroupByObject(WeaviateObject from)
        : base(from) { }

    /// <summary>
    /// Gets the name of the group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }
}

/// <summary>
/// Represents a data object retrieved from Weaviate.
/// </summary>
/// <remarks>
/// This is the primary class for working with Weaviate objects. It contains:
/// - UUID: The unique identifier
/// - Properties: The object's data fields
/// - Vectors: The object's vector embeddings
/// - References: Cross-references to other objects
/// - Metadata: Query-specific information (distance, score, timestamps, etc.)
/// </remarks>
/// <example>
/// <code>
/// var result = await client.Query.NearText("machine learning", limit: 10);
/// foreach (var obj in result.Objects)
/// {
///     Console.WriteLine($"ID: {obj.UUID}");
///     Console.WriteLine($"Title: {obj.Properties["title"]}");
///     Console.WriteLine($"Distance: {obj.Metadata.Distance}");
/// }
/// </code>
/// </example>
public record WeaviateObject
{
    /// <summary>
    /// Gets or sets the unique identifier of the object.
    /// </summary>
    public Guid? UUID { get; set; }

    /// <summary>
    /// Gets the name of the collection this object belongs to.
    /// </summary>
    public string? Collection { get; init; }

    /// <summary>
    /// Gets or sets the metadata associated with this object.
    /// </summary>
    /// <remarks>
    /// Metadata includes query-specific information like distance, score, timestamps, etc.
    /// Fields are only populated when explicitly requested via <see cref="MetadataQuery"/>.
    /// </remarks>
    public Metadata Metadata { get; set; } = new Metadata();

    /// <summary>
    /// Gets or sets the tenant this object belongs to (for multi-tenant collections).
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// Gets or sets the object's data properties.
    /// </summary>
    /// <remarks>
    /// Property names are case-insensitive. Values can be primitives, arrays, or nested objects.
    /// For type-safe access, consider using the typed query client or casting to your model type.
    /// </remarks>
    public IDictionary<string, object?> Properties { get; set; } =
        new Dictionary<string, object?>();

    /// <summary>
    /// Gets or sets the cross-references to other objects.
    /// </summary>
    /// <remarks>
    /// References are keyed by the reference property name.
    /// Each reference can point to multiple objects (one-to-many relationships).
    /// </remarks>
    public IDictionary<string, IList<WeaviateObject>> References { get; set; } =
        new Dictionary<string, IList<WeaviateObject>>();

    /// <summary>
    /// Gets or sets the vector embeddings for this object.
    /// </summary>
    /// <remarks>
    /// For single-vector collections, contains one vector.
    /// For multi-vector collections, contains named vectors.
    /// Only populated when vectors are explicitly requested in the query.
    /// </remarks>
    public Vectors Vectors { get; set; } = new Vectors();
}

/// <summary>
/// Results from a standard query, containing a list of <see cref="WeaviateObject"/> instances.
/// </summary>
/// <remarks>
/// This is the return type for most query operations (NearText, NearVector, BM25, Hybrid, etc.).
/// Implements IEnumerable to allow direct iteration over the objects.
/// </remarks>
public record WeaviateResult : WeaviateResult<WeaviateObject>
{
    private static readonly WeaviateResult _empty = new();

    /// <summary>
    /// Gets an empty WeaviateResult with no objects.
    /// </summary>
    public static WeaviateResult Empty => _empty;
}
#endregion

#region Generative Query Results
/// <summary>
/// Results from a GroupBy query with generative AI capabilities.
/// </summary>
/// <param name="Objects">All objects returned by the query, each potentially with generative output.</param>
/// <param name="Groups">Grouped objects, each group potentially with generative output.</param>
/// <param name="Generative">Overall generative output for the entire result set.</param>
/// <remarks>
/// Generative queries use RAG (Retrieval-Augmented Generation) to produce AI-generated text
/// based on the retrieved objects. Generation can occur at three levels:
/// - Per object (single result generation)
/// - Per group (grouped task generation)
/// - Overall (entire result set generation)
/// </remarks>
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
    /// Gets an empty GenerativeGroupByResult.
    /// </summary>
    public static GenerativeGroupByResult Empty => _empty;
}

/// <summary>
/// A group of objects in a generative GroupBy query result.
/// </summary>
public record GenerativeWeaviateGroup : WeaviateGroup<GenerativeGroupByObject>
{
    /// <summary>
    /// Gets or sets the generative AI output for this group.
    /// </summary>
    public GenerativeResult? Generative { get; set; }
};

/// <summary>
/// Represents an object in a generative GroupBy query result.
/// </summary>
public record GenerativeGroupByObject : GenerativeWeaviateObject
{
    /// <summary>
    /// Gets the name of the group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }
}

/// <summary>
/// Represents a Weaviate object with associated generative AI output.
/// </summary>
/// <remarks>
/// Extends <see cref="WeaviateObject"/> to include generative results when using RAG queries.
/// </remarks>
public record GenerativeWeaviateObject : WeaviateObject
{
    /// <summary>
    /// Gets or sets the generative AI output for this specific object.
    /// </summary>
    /// <remarks>
    /// Populated when using single result generation, where each retrieved object
    /// is used as context to generate a unique response.
    /// </remarks>
    public GenerativeResult? Generative { get; set; }
}

/// <summary>
/// Results from a generative query, containing objects and AI-generated content.
/// </summary>
/// <remarks>
/// This is the return type for generative query operations that combine retrieval with
/// AI text generation (RAG - Retrieval-Augmented Generation).
/// </remarks>
public record GenerativeWeaviateResult : WeaviateResult<GenerativeWeaviateObject>
{
    private static readonly GenerativeWeaviateResult _empty = new()
    {
        Generative = GenerativeResult.Empty,
    };

    /// <summary>
    /// Gets an empty GenerativeWeaviateResult.
    /// </summary>
    public static GenerativeWeaviateResult Empty => _empty;

    /// <summary>
    /// Gets the generative AI output for the entire result set.
    /// </summary>
    /// <remarks>
    /// Contains AI-generated text using all retrieved objects as context.
    /// </remarks>
    public required GenerativeResult Generative { get; init; }
}
#endregion

#region Generative Specific Types
/// <summary>
/// Debug information for generative AI queries.
/// </summary>
/// <param name="FullPrompt">The complete prompt sent to the generative AI model.</param>
public record GenerativeDebug(string? FullPrompt = null);

/// <summary>
/// A single reply from a generative AI model.
/// </summary>
/// <param name="Text">The generated text content.</param>
public record GenerativeReply(
    string Text
// GenerativeDebug? Debug = null,
// object? Metadata = null,
);

/// <summary>
/// Contains one or more AI-generated text responses.
/// </summary>
/// <param name="Values">The list of generative replies.</param>
/// <remarks>
/// Implements IList&lt;string&gt; to provide convenient access to the generated text.
/// For single result generation, contains one reply per object.
/// For grouped or overall generation, typically contains one reply.
/// </remarks>
/// <example>
/// <code>
/// var result = await client.Generate.NearText("AI trends", "Summarize this article");
/// foreach (var generatedText in result.Generative)
/// {
///     Console.WriteLine(generatedText);
/// }
/// </code>
/// </example>
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
