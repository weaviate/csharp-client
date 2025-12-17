using System.Collections;

namespace Weaviate.Client.Models;

public static class AutoArrayBuilder
{
    public static AutoArray<T> Create<T>(ReadOnlySpan<T> items)
    {
        AutoArray<T> array = new();
        foreach (var item in items)
        {
            array.Add(item);
        }
        return array;
    }
}

[System.Runtime.CompilerServices.CollectionBuilder(
    typeof(AutoArrayBuilder),
    nameof(AutoArrayBuilder.Create)
)]
public class AutoArray<T> : IEnumerable<T>
{
    private readonly List<T> _items = [];

    internal AutoArray() { }

    private AutoArray(IEnumerable<T> items)
    {
        _items.AddRange(items);
    }

    private AutoArray(params T[] items)
        : this(items.AsEnumerable()) { }

    public static implicit operator AutoArray<T>(T item) => new(item);

    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(items))]
    public static implicit operator AutoArray<T>?(T[]? items) => items is null ? null : [.. items];

    public static implicit operator AutoArray<T>(List<T> items) => [.. items];

    public static explicit operator T[](AutoArray<T> list) => [.. list._items];

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    public void Add(T item)
    {
        _items.Add(item);
    }

    public void Add(params T[] items)
    {
        if (items == null)
            return;
        _items.AddRange(items);
    }
}
