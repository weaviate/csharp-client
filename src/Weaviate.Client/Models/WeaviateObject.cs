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
    {
        return UnmarshallProperties<T>(Properties);
    }

    public void Do<T>(Action<T> action)
    {
        var data = UnmarshallProperties<T>(Properties);
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
    {
        var data = UnmarshallProperties<TSource>(Properties);
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

    internal static T? UnmarshallProperties<T>(IDictionary<string, object?> dict)
    {
        if (dict == null)
            throw new ArgumentNullException(nameof(dict));

        // Create an instance of T using the default constructor
        var props = Activator.CreateInstance<T>();

        if (props is IDictionary<string, object?> target)
        {
            foreach (var kvp in dict)
            {
                if (kvp.Value is IDictionary<string, object?> subDict)
                {
                    object? v = UnmarshallProperties<dynamic>(subDict);

                    target[kvp.Key.Capitalize()] = v ?? subDict;
                }
                else
                {
                    target[kvp.Key.Capitalize()] = kvp.Value;
                }
            }
            return props;
        }

        var type = typeof(T);
        var properties = type.GetProperties();

        foreach (var property in properties)
        {
            var matchingKey = dict.Keys.FirstOrDefault(k =>
                string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase)
            );

            if (matchingKey != null)
            {
                var value = dict[matchingKey];
                if (value != null)
                {
                    try
                    {
                        var convertedValue = Convert.ChangeType(value, property.PropertyType);
                        property.SetValue(props, convertedValue);
                    }
                    catch
                    {
                        // Skip if conversion fails
                        continue;
                    }
                }
            }
        }

        return props;
    }
}
