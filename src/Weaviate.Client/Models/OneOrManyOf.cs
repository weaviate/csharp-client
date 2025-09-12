using System.Collections;

namespace Weaviate.Client.Models;

public class OneOrManyOf<T> : IEnumerable<T>
{
    private readonly List<T> _items;

    public OneOrManyOf(IEnumerable<T> items)
    {
        _items = new(items);
    }

    public OneOrManyOf(params T[] item)
    {
        _items = new(item);
    }

    public static implicit operator OneOrManyOf<T>(T item) => new OneOrManyOf<T>(item);

    public static implicit operator OneOrManyOf<T>(T[]? items) =>
        new OneOrManyOf<T>(items ?? Array.Empty<T>());

    public static implicit operator OneOrManyOf<T>(List<T> items) => new OneOrManyOf<T>(items);

    public static explicit operator T[](OneOrManyOf<T> list) => list._items.ToArray();

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
