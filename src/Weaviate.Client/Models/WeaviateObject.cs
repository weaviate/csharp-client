using System.Collections;
using System.Dynamic;

namespace Weaviate.Client.Models;

public partial record WeaviateResult : IEnumerable<WeaviateObject>
{
    public required IEnumerable<WeaviateObject> Objects { get; init; } = [];

    public IEnumerator<WeaviateObject> GetEnumerator() => Objects.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public partial class NamedVector<T> : List<T>
{
    public NamedVector(IEnumerable<T> value)
        : base(value) { }
}

public partial class NamedVector : NamedVector<float>
{
    public NamedVector(IEnumerable<float> value)
        : base(value) { }
}

public partial class NamedVectors : Dictionary<string, NamedVector>
{
    public void Add(string name, params float[] values)
    {
        Add(name, new NamedVector(values));
    }
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

    public NamedVectors Vectors { get; set; } = new NamedVectors();

    public T? As<T>()
        where T : new()
    {
        return ObjectHelper.UnmarshallProperties<T>(Properties);
    }

    public void Do<T>(Action<T> action)
        where T : new()
    {
        var data = As<T>();
        if (data is not null)
        {
            action(data);
        }
    }

    public void Do(Action<dynamic> action)
    {
        Do<ExpandoObject>(action);
    }

    public TResult? Get<TSource, TResult>(Func<TSource, TResult> func)
        where TSource : new()
    {
        var data = ObjectHelper.UnmarshallProperties<TSource>(Properties);
        if (data is not null)
        {
            return func(data);
        }
        return default;
    }

    public TResult? Get<TResult>(Func<dynamic, TResult> func)
    {
        return Get<ExpandoObject, TResult>(func);
    }
}
