namespace Weaviate.Client.Models.Typed;

/// <summary>
/// Strongly-typed wrapper for WeaviateObject with automatic serialization/deserialization.
/// Provides both raw dictionary access (Properties) and strongly-typed access (Object).
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record WeaviateObject<T>
    where T : class, new()
{
    private T? _object;
    private bool _objectLoaded = false;

    /// <summary>
    /// The object ID (UUID).
    /// </summary>
    public Guid? ID { get; set; }

    /// <summary>
    /// The collection name this object belongs to.
    /// </summary>
    public string? Collection { get; init; }

    /// <summary>
    /// Metadata about the object (creation time, update time, distance, score, etc.).
    /// </summary>
    public Metadata Metadata { get; set; } = new Metadata();

    /// <summary>
    /// The tenant name for multi-tenancy support.
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// Raw properties as a dictionary. This is always available for dynamic access.
    /// When Object is set, this dictionary is automatically updated.
    /// </summary>
    public IDictionary<string, object?> Properties { get; set; } =
        new Dictionary<string, object?>();

    /// <summary>
    /// Strongly-typed object deserialized from Properties.
    /// Lazy-loaded on first access. Setting this property updates the Properties dictionary.
    /// </summary>
    public T Object
    {
        get
        {
            if (!_objectLoaded)
            {
                _object = ObjectHelper.UnmarshallProperties<T>(Properties) ?? new T();
                _objectLoaded = true;
            }
            return _object!;
        }
        set
        {
            _object = value;
            _objectLoaded = true;

            // Sync back to Properties dictionary
            var props = ObjectHelper.BuildDataTransferObject(value);
            Properties = props.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
        }
    }

    /// <summary>
    /// Cross-references to other objects. Remains untyped as references can point to different collection types.
    /// </summary>
    public IDictionary<string, IList<WeaviateObject>> References { get; set; } =
        new Dictionary<string, IList<WeaviateObject>>();

    /// <summary>
    /// Vector embeddings for this object.
    /// </summary>
    public Vectors Vectors { get; set; } = new Vectors();

    /// <summary>
    /// Creates a WeaviateObject&lt;T&gt; from an untyped WeaviateObject.
    /// </summary>
    public static WeaviateObject<T> FromUntyped(WeaviateObject obj)
    {
        return new WeaviateObject<T>
        {
            ID = obj.ID,
            Collection = obj.Collection,
            Metadata = obj.Metadata,
            Tenant = obj.Tenant,
            Properties = obj.Properties,
            References = obj.References,
            Vectors = obj.Vectors,
        };
    }

    /// <summary>
    /// Converts this typed object to an untyped WeaviateObject.
    /// </summary>
    public WeaviateObject ToUntyped()
    {
        return new WeaviateObject
        {
            ID = ID,
            Collection = Collection,
            Metadata = Metadata,
            Tenant = Tenant,
            Properties = Properties,
            References = References,
            Vectors = Vectors,
        };
    }
}

/// <summary>
/// Strongly-typed grouped result object.
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record GroupByObject<T> : WeaviateObject<T>
    where T : class, new()
{
    /// <summary>
    /// The group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }

    /// <summary>
    /// Creates a GroupByObject&lt;T&gt; from an untyped GroupByObject.
    /// </summary>
    public static GroupByObject<T> FromUntyped(GroupByObject obj)
    {
        var typed = WeaviateObject<T>.FromUntyped(obj);
        return new GroupByObject<T>
        {
            ID = typed.ID,
            Collection = typed.Collection,
            Metadata = typed.Metadata,
            Tenant = typed.Tenant,
            Properties = typed.Properties,
            References = typed.References,
            Vectors = typed.Vectors,
            BelongsToGroup = obj.BelongsToGroup,
        };
    }
}

/// <summary>
/// Base strongly-typed group containing multiple objects.
/// Generic over both property type T and object type TObject.
/// </summary>
/// <typeparam name="T">The C# property type for objects in this group.</typeparam>
/// <typeparam name="TObject">The object type (GroupByObject&lt;T&gt; or GenerativeGroupByObject&lt;T&gt;).</typeparam>
public record WeaviateGroup<T, TObject>
    where T : class, new()
{
    /// <summary>
    /// The group name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Objects belonging to this group.
    /// </summary>
    public required IList<TObject> Objects { get; init; } = [];

    /// <summary>
    /// Minimum distance among objects in this group.
    /// </summary>
    public float MinDistance { get; init; } = 0;

    /// <summary>
    /// Maximum distance among objects in this group.
    /// </summary>
    public float MaxDistance { get; init; } = 0;

    /// <summary>
    /// Number of objects in this group.
    /// </summary>
    public int NumberOfObjects => Objects.Count;
}

/// <summary>
/// Strongly-typed group containing non-generative objects.
/// </summary>
/// <typeparam name="T">The C# type of objects in this group.</typeparam>
public record WeaviateGroup<T> : WeaviateGroup<T, GroupByObject<T>>
    where T : class, new() { }

/// <summary>
/// Strongly-typed group-by query result.
/// </summary>
/// <typeparam name="T">The C# type of objects in the result.</typeparam>
public record GroupByResult<T>(
    IList<GroupByObject<T>> Objects,
    IDictionary<string, WeaviateGroup<T>> Groups
) : GroupByResult<GroupByObject<T>, WeaviateGroup<T>>(Objects, Groups)
    where T : class, new() { }

/// <summary>
/// Strongly-typed query result containing a collection of objects.
/// </summary>
/// <typeparam name="T">The C# type of objects in the result.</typeparam>
public record WeaviateResult<T>
    where T : class, new()
{
    /// <summary>
    /// The collection of objects returned by the query.
    /// </summary>
    public ICollection<WeaviateObject<T>> Objects { get; init; } = Array.Empty<WeaviateObject<T>>();
}

#region Generative Query Results

/// <summary>
/// Strongly-typed object with generative AI results.
/// Each object can have its own generative AI data.
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record GenerativeWeaviateObject<T> : WeaviateObject<T>
    where T : class, new()
{
    /// <summary>
    /// Generative AI results associated with this object.
    /// </summary>
    public GenerativeResult? Generative { get; set; }
}

/// <summary>
/// Strongly-typed grouped object with generative AI results.
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record GenerativeGroupByObject<T> : GenerativeWeaviateObject<T>
    where T : class, new()
{
    /// <summary>
    /// The group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }

    /// <summary>
    /// Creates a GenerativeGroupByObject&lt;T&gt; from an untyped GenerativeGroupByObject.
    /// </summary>
    public static GenerativeGroupByObject<T> FromUntyped(GenerativeGroupByObject obj)
    {
        var typed = WeaviateObject<T>.FromUntyped(obj);
        return new GenerativeGroupByObject<T>
        {
            ID = typed.ID,
            Collection = typed.Collection,
            Metadata = typed.Metadata,
            Tenant = typed.Tenant,
            Properties = typed.Properties,
            References = typed.References,
            Vectors = typed.Vectors,
            BelongsToGroup = obj.BelongsToGroup,
            Generative = obj.Generative,
        };
    }
}

/// <summary>
/// Strongly-typed group with generative AI results.
/// Each group can have its own generative AI data.
/// </summary>
/// <typeparam name="T">The C# type of objects in this group.</typeparam>
public record GenerativeWeaviateGroup<T> : WeaviateGroup<T, GenerativeGroupByObject<T>>
    where T : class, new()
{
    /// <summary>
    /// Generative AI results associated with this group.
    /// </summary>
    public GenerativeResult? Generative { get; set; }
}

/// <summary>
/// Strongly-typed group-by query result with three-level generative AI support:
/// result-set level, group level, and object level.
/// </summary>
/// <typeparam name="T">The C# type of objects in the result.</typeparam>
public record GenerativeGroupByResult<T>(
    IList<GenerativeGroupByObject<T>> Objects,
    IDictionary<string, GenerativeWeaviateGroup<T>> Groups,
    GenerativeResult Generative
) : GroupByResult<GenerativeGroupByObject<T>, GenerativeWeaviateGroup<T>>(Objects, Groups)
    where T : class, new()
{
    private static readonly GenerativeGroupByResult<T> _empty = new(
        Array.Empty<GenerativeGroupByObject<T>>(),
        new Dictionary<string, GenerativeWeaviateGroup<T>>(),
        new GenerativeResult(Array.Empty<GenerativeReply>())
    );

    /// <summary>
    /// Empty generative group-by result.
    /// </summary>
    public static GenerativeGroupByResult<T> Empty => _empty;
}

/// <summary>
/// Strongly-typed query result with generative AI results.
/// Supports both object-level and result-set level generative data.
/// </summary>
/// <typeparam name="T">The C# type of objects in the result.</typeparam>
public record GenerativeWeaviateResult<T> : WeaviateResult<T>
    where T : class, new()
{
    private static readonly GenerativeWeaviateResult<T> _empty = new()
    {
        Generative = GenerativeResult.Empty,
    };

    /// <summary>
    /// Empty generative result.
    /// </summary>
    public static GenerativeWeaviateResult<T> Empty => _empty;

    /// <summary>
    /// Generative AI results at the result-set level.
    /// </summary>
    public required GenerativeResult Generative { get; init; }
}

#endregion
