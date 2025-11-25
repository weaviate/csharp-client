namespace Weaviate.Client.Models.Typed;

/// <summary>
/// Converts between untyped and typed result objects.
/// Provides helper methods to convert WeaviateObject to WeaviateObject&lt;T&gt;,
/// GroupByResult to GroupByResult&lt;T&gt;, etc.
/// </summary>
public static class TypedResultConverter
{
    /// <summary>
    /// Converts an untyped WeaviateObject to a typed WeaviateObject&lt;T&gt;.
    /// </summary>
    public static WeaviateObject<T> ToTyped<T>(this WeaviateObject obj)
        where T : class, new()
    {
        return WeaviateObject<T>.FromUntyped(obj);
    }

    /// <summary>
    /// Converts an untyped GroupByObject to a typed GroupByObject&lt;T&gt;.
    /// </summary>
    public static GroupByObject<T> ToTyped<T>(this GroupByObject obj)
        where T : class, new()
    {
        return GroupByObject<T>.FromUntyped(obj);
    }

    /// <summary>
    /// Converts an untyped WeaviateGroup to a typed WeaviateGroup&lt;T&gt;.
    /// </summary>
    public static WeaviateGroup<T> ToTyped<T>(this WeaviateGroup obj)
        where T : class, new()
    {
        return new WeaviateGroup<T>
        {
            Name = obj.Name,
            Objects = obj.Objects.Select(o => o.ToTyped<T>()).ToList(),
            MinDistance = obj.MinDistance,
            MaxDistance = obj.MaxDistance,
        };
    }

    /// <summary>
    /// Converts an untyped GroupByResult to a typed GroupByResult&lt;T&gt;.
    /// </summary>
    public static GroupByResult<T> ToTyped<T>(this GroupByResult result)
        where T : class, new()
    {
        return new GroupByResult<T>(
            result.Objects.Select(o => o.ToTyped<T>()).ToList(),
            result.Groups.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToTyped<T>())
        );
    }

    /// <summary>
    /// Converts an untyped WeaviateResult to a typed WeaviateResult&lt;WeaviateObject&lt;T&gt;&gt;.
    /// </summary>
    public static Models.WeaviateResult<WeaviateObject<T>> ToTyped<T>(this WeaviateResult result)
        where T : class, new()
    {
        return new Models.WeaviateResult<WeaviateObject<T>>
        {
            Objects = result.Objects.Select(o => o.ToTyped<T>()).ToList(),
        };
    }

    #region Generative Conversions

    /// <summary>
    /// Converts an untyped GenerativeWeaviateObject to a typed GenerativeWeaviateObject&lt;T&gt;.
    /// </summary>
    public static GenerativeWeaviateObject<T> ToTyped<T>(this GenerativeWeaviateObject obj)
        where T : class, new()
    {
        return GenerativeWeaviateObject<T>.FromUntyped(obj);
    }

    /// <summary>
    /// Converts an untyped GenerativeGroupByObject to a typed GenerativeGroupByObject&lt;T&gt;.
    /// </summary>
    public static GenerativeGroupByObject<T> ToTyped<T>(this GenerativeGroupByObject obj)
        where T : class, new()
    {
        return GenerativeGroupByObject<T>.FromUntyped(obj);
    }

    /// <summary>
    /// Converts an untyped GenerativeWeaviateGroup to a typed GenerativeWeaviateGroup&lt;T&gt;.
    /// </summary>
    public static GenerativeWeaviateGroup<T> ToTyped<T>(this GenerativeWeaviateGroup group)
        where T : class, new()
    {
        return new GenerativeWeaviateGroup<T>
        {
            Name = group.Name,
            Objects = group.Objects.Select(o => o.ToTyped<T>()).ToList(),
            MinDistance = group.MinDistance,
            MaxDistance = group.MaxDistance,
            Generative = group.Generative,
        };
    }

    /// <summary>
    /// Converts an untyped GenerativeGroupByResult to a typed GenerativeGroupByResult&lt;T&gt;.
    /// </summary>
    public static GenerativeGroupByResult<T> ToTyped<T>(this GenerativeGroupByResult result)
        where T : class, new()
    {
        return new GenerativeGroupByResult<T>(
            result.Objects.Select(o => o.ToTyped<T>()).ToList(),
            result.Groups.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToTyped<T>()),
            result.Generative
        );
    }

    /// <summary>
    /// Converts an untyped GenerativeWeaviateResult to a typed GenerativeWeaviateResult&lt;T&gt;.
    /// </summary>
    public static GenerativeWeaviateResult<T> ToTyped<T>(this GenerativeWeaviateResult result)
        where T : class, new()
    {
        return new GenerativeWeaviateResult<T>
        {
            Objects = result.Objects.Select(o => o.ToTyped<T>()).ToList(),
            Generative = result.Generative,
        };
    }

    #endregion

    #region Collection Conversions

    /// <summary>
    /// Converts a collection of untyped WeaviateObjects to typed WeaviateObject&lt;T&gt; objects.
    /// </summary>
    public static IEnumerable<WeaviateObject<T>> ToTyped<T>(
        this IEnumerable<WeaviateObject> objects
    )
        where T : class, new()
    {
        return objects.Select(o => o.ToTyped<T>());
    }

    /// <summary>
    /// Converts a collection of untyped GenerativeWeaviateObjects to typed GenerativeWeaviateObject&lt;T&gt; objects.
    /// </summary>
    public static IEnumerable<GenerativeWeaviateObject<T>> ToTyped<T>(
        this IEnumerable<GenerativeWeaviateObject> objects
    )
        where T : class, new()
    {
        return objects.Select(o => o.ToTyped<T>());
    }

    #endregion

    #region Reverse Conversions (Typed to Untyped)

    /// <summary>
    /// Converts a typed WeaviateObject&lt;T&gt; back to an untyped WeaviateObject.
    /// Useful when you need to pass a typed object to an API that expects untyped objects.
    /// </summary>
    public static WeaviateObject ToUntyped<T>(this WeaviateObject<T> obj)
        where T : class, new()
    {
        return obj.ToUntyped();
    }

    /// <summary>
    /// Converts a collection of typed WeaviateObject&lt;T&gt; objects back to untyped WeaviateObjects.
    /// </summary>
    public static IEnumerable<WeaviateObject> ToUntyped<T>(
        this IEnumerable<WeaviateObject<T>> objects
    )
        where T : class, new()
    {
        return objects.Select(o => o.ToUntyped());
    }

    #endregion
}
