namespace Weaviate.Client.Models.Typed;

/// <summary>
/// Strongly-typed wrapper for WeaviateObject with automatic serialization/deserialization.
/// Provides both raw dictionary access (Properties) and strongly-typed access (Object).
/// Internally wraps an untyped WeaviateObject instance.
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record WeaviateObject<T>
    where T : class, new()
{
    private readonly WeaviateObject _untyped;
    private T? _object;
    private bool _objectLoaded = false;

    protected WeaviateObject(WeaviateObject untyped)
    {
        _untyped = untyped;
    }

    /// <summary>
    /// The object ID (UUID).
    /// </summary>
    public Guid? ID
    {
        get => _untyped.ID;
        internal set => _untyped.ID = value;
    }

    /// <summary>
    /// The collection name this object belongs to.
    /// </summary>
    public string? Collection
    {
        get => _untyped.Collection;
    }

    /// <summary>
    /// Metadata about the object (creation time, update time, distance, score, etc.).
    /// </summary>
    public Metadata Metadata
    {
        get => _untyped.Metadata;
        internal set => _untyped.Metadata = value;
    }

    /// <summary>
    /// The tenant name for multi-tenancy support.
    /// </summary>
    public string? Tenant
    {
        get => _untyped.Tenant;
        internal set => _untyped.Tenant = value;
    }

    /// <summary>
    /// Raw properties as a dictionary. This is always available for dynamic access.
    /// When Object is set, this dictionary is automatically updated.
    /// </summary>
    public IDictionary<string, object?> Properties
    {
        get => _untyped.Properties;
        internal set => _untyped.Properties = value;
    }

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
        internal set
        {
            _object = value;
            _objectLoaded = true;

            // Sync back to Properties dictionary
            var props = ObjectHelper.BuildDataTransferObject(value);
            _untyped.Properties = props.ToDictionary(kvp => kvp.Key, kvp => (object?)kvp.Value);
        }
    }

    /// <summary>
    /// Cross-references to other objects. Remains untyped as references can point to different collection types.
    /// </summary>
    public IDictionary<string, IList<WeaviateObject>> References
    {
        get => _untyped.References;
        internal set => _untyped.References = value;
    }

    /// <summary>
    /// Vector embeddings for this object.
    /// </summary>
    public Vectors Vectors
    {
        get => _untyped.Vectors;
        internal set => _untyped.Vectors = value;
    }

    /// <summary>
    /// Creates a WeaviateObject&lt;T&gt; from an untyped WeaviateObject.
    /// </summary>
    public static WeaviateObject<T> FromUntyped(WeaviateObject obj)
    {
        return new WeaviateObject<T>(obj);
    }

    /// <summary>
    /// Converts this typed object to an untyped WeaviateObject.
    /// </summary>
    public WeaviateObject ToUntyped()
    {
        return _untyped;
    }
}

/// <summary>
/// Strongly-typed grouped result object.
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record GroupByObject<T> : WeaviateObject<T>
    where T : class, new()
{
    protected GroupByObject(WeaviateObject untyped)
        : base(untyped) { }

    /// <summary>
    /// The group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }

    /// <summary>
    /// Creates a GroupByObject&lt;T&gt; from an untyped GroupByObject.
    /// </summary>
    public static GroupByObject<T> FromUntyped(GroupByObject obj)
    {
        return new GroupByObject<T>(obj) { BelongsToGroup = obj.BelongsToGroup };
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

#region Generative Query Results

/// <summary>
/// Strongly-typed object with generative AI results.
/// Each object can have its own generative AI data.
/// </summary>
/// <typeparam name="T">The C# type to deserialize properties into.</typeparam>
public record GenerativeWeaviateObject<T> : WeaviateObject<T>
    where T : class, new()
{
    protected GenerativeWeaviateObject(GenerativeWeaviateObject untyped)
        : base(untyped)
    {
        Generative = untyped.Generative;
    }

    public static GenerativeWeaviateObject<T> FromUntyped(GenerativeWeaviateObject obj)
    {
        return new GenerativeWeaviateObject<T>(obj);
    }

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
    protected GenerativeGroupByObject(GenerativeGroupByObject untyped)
        : base(untyped)
    {
        BelongsToGroup = untyped.BelongsToGroup;
        Generative = untyped.Generative;
    }

    /// <summary>
    /// The group this object belongs to.
    /// </summary>
    public required string BelongsToGroup { get; init; }

    /// <summary>
    /// Creates a GenerativeGroupByObject&lt;T&gt; from an untyped GenerativeGroupByObject.
    /// </summary>
    public static GenerativeGroupByObject<T> FromUntyped(GenerativeGroupByObject obj)
    {
        return new GenerativeGroupByObject<T>(obj)
        {
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
public record GenerativeWeaviateResult<T> : Models.WeaviateResult<GenerativeWeaviateObject<T>>
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
