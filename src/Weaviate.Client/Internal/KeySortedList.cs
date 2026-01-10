using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Weaviate.Client.Internal;

/// <summary>
/// The key sorted list class
/// </summary>
/// <seealso cref="IEnumerable{T}"/>
/// <seealso cref="IDictionary{TKey, TValue}"/>
public class KeySortedList<TKey, TValue>(Func<TValue, TKey> KeySelector)
    : IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>
    where TKey : notnull, IComparable<TKey>
{
    /// <summary>
    /// The inner list
    /// </summary>
    private readonly SortedList<TKey, TValue> _innerList = [];

    /// <summary>
    /// Adds the item
    /// </summary>
    /// <param name="item">The item</param>
    public void Add(TValue item)
    {
        var key = KeySelector(item);
        _innerList.Add(key, item);
    }

    /// <summary>
    /// Containses the key using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool ContainsKey(TKey key)
    {
        return _innerList.ContainsKey(key);
    }

    /// <summary>
    /// Removes the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool Remove(TKey key)
    {
        return _innerList.Remove(key);
    }

    /// <summary>
    /// Tries the get value using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>The bool</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _innerList.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of key value pair t key and t value</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _innerList.GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    public void Add(TKey key, TValue value)
    {
        ((IDictionary<TKey, TValue>)_innerList).Add(key, value);
    }

    /// <summary>
    /// Adds the item
    /// </summary>
    /// <param name="item">The item</param>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Add(item);
    }

    /// <summary>
    /// Clears this instance
    /// </summary>
    public void Clear()
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Clear();
    }

    /// <summary>
    /// Containses the item
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The bool</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Contains(item);
    }

    /// <summary>
    /// Copies the to using the specified array
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="arrayIndex">The array index</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).CopyTo(array, arrayIndex);
    }

    /// <summary>
    /// Removes the item
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The bool</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Remove(item);
    }

    /// <summary>
    /// The key
    /// </summary>
    public TValue this[TKey key] => _innerList[key];

    /// <summary>
    /// Gets the value of the keys
    /// </summary>
    public IEnumerable<TKey> Keys => _innerList.Keys;

    /// <summary>
    /// Gets the value of the values
    /// </summary>
    public IEnumerable<TValue> Values => _innerList.Values;

    /// <summary>
    /// Gets the value of the keys
    /// </summary>
    ICollection<TKey> IDictionary<TKey, TValue>.Keys =>
        ((IDictionary<TKey, TValue>)_innerList).Keys;

    /// <summary>
    /// Gets the value of the values
    /// </summary>
    ICollection<TValue> IDictionary<TKey, TValue>.Values =>
        ((IDictionary<TKey, TValue>)_innerList).Values;

    /// <summary>
    /// Gets the value of the count
    /// </summary>
    public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Count;

    /// <summary>
    /// Gets the value of the is read only
    /// </summary>
    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).IsReadOnly;

    /// <summary>
    /// The value
    /// </summary>
    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => ((IDictionary<TKey, TValue>)_innerList)[key];
        set => ((IDictionary<TKey, TValue>)_innerList)[key] = value;
    }
}

/// <summary>
/// A sorted list that allows multiple values per key, storing them as arrays.
/// Implements <see cref="IDictionary{TKey, TValue}"/> where TValue is an array.
/// </summary>
public class MultiKeySortedList<TKey, TValue>(Func<TValue, TKey> KeySelector)
    : IDictionary<TKey, TValue[]>
    where TKey : notnull, IComparable<TKey>
{
    /// <summary>
    /// The inner list
    /// </summary>
    private readonly SortedList<TKey, List<TValue>> _innerList = [];

    /// <summary>
    /// Adds the range using the specified items
    /// </summary>
    /// <param name="items">The items</param>
    public void AddRange(IEnumerable<TValue> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Adds the item
    /// </summary>
    /// <param name="item">The item</param>
    public void Add(TValue item)
    {
        var key = KeySelector(item);
        Add(key, item);
    }

    /// <summary>
    /// Adds the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    public void Add(TKey key, TValue value)
    {
        if (!_innerList.TryGetValue(key, out var list))
        {
            list = [];
            _innerList[key] = list;
        }
        list.Add(value);
    }

    /// <summary>
    /// Adds the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    public void Add(TKey key, TValue[] value)
    {
        foreach (var item in value)
        {
            Add(key, item);
        }
    }

    /// <summary>
    /// Containses the key using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool ContainsKey(TKey key)
    {
        return _innerList.ContainsKey(key);
    }

    /// <summary>
    /// Removes the key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool Remove(TKey key)
    {
        return _innerList.Remove(key);
    }

    /// <summary>
    /// Tries the get value using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>The bool</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue[] value)
    {
        if (_innerList.TryGetValue(key, out var list))
        {
            value = [.. list];
            return true;
        }
        value = null;
        return false;
    }

    /// <summary>
    /// Adds the item
    /// </summary>
    /// <param name="item">The item</param>
    public void Add(KeyValuePair<TKey, TValue[]> item)
    {
        Add(item.Key, item.Value);
    }

    /// <summary>
    /// Clears this instance
    /// </summary>
    public void Clear()
    {
        _innerList.Clear();
    }

    /// <summary>
    /// Containses the item
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The bool</returns>
    public bool Contains(KeyValuePair<TKey, TValue[]> item)
    {
        if (_innerList.TryGetValue(item.Key, out var list))
        {
            return item.Value.SequenceEqual(list);
        }
        return false;
    }

    /// <summary>
    /// Copies the to using the specified array
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="arrayIndex">The array index</param>
    public void CopyTo(KeyValuePair<TKey, TValue[]>[] array, int arrayIndex)
    {
        foreach (var kvp in _innerList)
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue[]>(kvp.Key, [.. kvp.Value]);
        }
    }

    /// <summary>
    /// Removes the item
    /// </summary>
    /// <param name="item">The item</param>
    /// <returns>The bool</returns>
    public bool Remove(KeyValuePair<TKey, TValue[]> item)
    {
        if (Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of key value pair t key and t value array</returns>
    public IEnumerator<KeyValuePair<TKey, TValue[]>> GetEnumerator()
    {
        foreach (var kvp in _innerList)
        {
            yield return new KeyValuePair<TKey, TValue[]>(kvp.Key, [.. kvp.Value]);
        }
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// The list
    /// </summary>
    public TValue[] this[TKey key] => _innerList.TryGetValue(key, out var list) ? [.. list] : [];

    /// <summary>
    /// Gets the value of the keys
    /// </summary>
    public IEnumerable<TKey> Keys => _innerList.Keys;

    /// <summary>
    /// Gets the value of the values
    /// </summary>
    public IEnumerable<TValue> Values => _innerList.Values.SelectMany(list => list);

    /// <summary>
    /// Gets the value of the keys
    /// </summary>
    ICollection<TKey> IDictionary<TKey, TValue[]>.Keys => _innerList.Keys;

    /// <summary>
    /// Gets the value of the values
    /// </summary>
    ICollection<TValue[]> IDictionary<TKey, TValue[]>.Values =>
        [.. _innerList.Values.Select(list => (TValue[])[.. list])];

    /// <summary>
    /// Gets the value of the count
    /// </summary>
    public int Count => _innerList.Count;

    /// <summary>
    /// Gets the value of the is read only
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// The value
    /// </summary>
    TValue[] IDictionary<TKey, TValue[]>.this[TKey key]
    {
        get => _innerList.TryGetValue(key, out var list) ? [.. list] : [];
        set { _innerList[key] = [.. value]; }
    }
}
