using System.Collections;
using System.Dynamic;

namespace Weaviate.Client.Models;

public partial record WeaviateResult : IEnumerable<WeaviateObject>
{
    private static readonly WeaviateResult _empty = new();
    public static WeaviateResult Empty => _empty;

    public IEnumerable<WeaviateObject> Objects { get; init; } = Array.Empty<WeaviateObject>();

    public IEnumerator<WeaviateObject> GetEnumerator() => Objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public partial record WeaviateObject
{
    public Guid? ID { get; set; }

    public string? Collection { get; init; }

    public Metadata Metadata { get; set; } = new Metadata();

    public string? Tenant { get; set; }

    public IDictionary<string, object?> Properties { get; set; } =
        new Dictionary<string, object?>();

    public IDictionary<string, IList<WeaviateObject>> References { get; set; } =
        new Dictionary<string, IList<WeaviateObject>>();

    public VectorContainer Vectors { get; set; } = new VectorContainer();
}

public static class WeaviateObjectExtensions
{
    public static T? As<T>(this WeaviateObject obj)
        where T : new()
    {
        return ObjectHelper.UnmarshallProperties<T>(obj.Properties);
    }

    public static void Do<T>(this WeaviateObject obj, Action<T> action)
        where T : new()
    {
        var data = obj.As<T>();
        if (data is not null)
        {
            action(data);
        }
    }

    public static void Do(this WeaviateObject obj, Action<dynamic> action)
    {
        obj.Do<ExpandoObject>(action);
    }

    public static TResult? Get<TSource, TResult>(
        this WeaviateObject obj,
        Func<TSource, TResult> func
    )
        where TSource : new()
    {
        var data = obj.As<TSource>();
        if (data is not null)
        {
            return func(data);
        }
        return default;
    }

    public static TResult? Get<TResult>(this WeaviateObject obj, Func<dynamic, TResult> func)
    {
        return obj.Get<ExpandoObject, TResult>(func);
    }
}
