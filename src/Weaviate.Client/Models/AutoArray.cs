using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// The auto array builder class
/// </summary>
public static class AutoArrayBuilder
{
    /// <summary>
    /// Creates the items
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="items">The items</param>
    /// <returns>The array</returns>
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

/// <summary>
/// The auto array class
/// </summary>
/// <seealso cref="IEnumerable{T}"/>
[System.Runtime.CompilerServices.CollectionBuilder(
    typeof(AutoArrayBuilder),
    nameof(AutoArrayBuilder.Create)
)]
public class AutoArray<T> : IEnumerable<T>
{
    /// <summary>
    /// The items
    /// </summary>
    private readonly List<T> _items = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoArray{T}"/> class
    /// </summary>
    internal AutoArray() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoArray{T}"/> class
    /// </summary>
    /// <param name="items">The items</param>
    private AutoArray(IEnumerable<T> items)
    {
        _items.AddRange(items);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoArray{T}"/> class
    /// </summary>
    /// <param name="items">The items</param>
    private AutoArray(params T[] items)
        : this(items.AsEnumerable()) { }

    /// <summary>
    /// Implicitly converts a single item to an AutoArray
    /// </summary>
    /// <param name="item">The item</param>
    public static implicit operator AutoArray<T>(T item) => new(item);

    /// <summary>
    /// Implicitly converts an array to an AutoArray
    /// </summary>
    /// <param name="items">The items</param>
    [return: System.Diagnostics.CodeAnalysis.NotNullIfNotNull(nameof(items))]
    public static implicit operator AutoArray<T>?(T[]? items) => items is null ? null : [.. items];

    /// <summary>
    /// Implicitly converts a List to an AutoArray
    /// </summary>
    /// <param name="items">The items</param>
    public static implicit operator AutoArray<T>(List<T> items) => [.. items];

    /// <summary>
    /// Explicitly converts an AutoArray to an array
    /// </summary>
    /// <param name="list">The auto array</param>
    public static explicit operator T[](AutoArray<T> list) => [.. list._items];

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of t</returns>
    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();

    /// <summary>
    /// Adds the item
    /// </summary>
    /// <param name="item">The item</param>
    public void Add(T item)
    {
        _items.Add(item);
    }

    /// <summary>
    /// Adds the items
    /// </summary>
    /// <param name="items">The items</param>
    public void Add(params T[] items)
    {
        if (items == null)
            return;
        _items.AddRange(items);
    }
}
