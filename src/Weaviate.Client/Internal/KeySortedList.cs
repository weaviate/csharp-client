using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Weaviate.Client.Internal;

public class KeySortedList<TKey, TValue>(Func<TValue, TKey> KeySelector)
    : IEnumerable<KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>
    where TKey : notnull, IComparable<TKey>
{
    private readonly SortedList<TKey, TValue> _innerList = [];

    public void Add(TValue item)
    {
        var key = KeySelector(item);
        _innerList.Add(key, item);
    }

    public bool ContainsKey(TKey key)
    {
        return _innerList.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return _innerList.Remove(key);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _innerList.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _innerList.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(TKey key, TValue value)
    {
        ((IDictionary<TKey, TValue>)_innerList).Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Add(item);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Remove(item);
    }

    public TValue this[TKey key] => _innerList[key];

    public IEnumerable<TKey> Keys => _innerList.Keys;

    public IEnumerable<TValue> Values => _innerList.Values;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys =>
        ((IDictionary<TKey, TValue>)_innerList).Keys;

    ICollection<TValue> IDictionary<TKey, TValue>.Values =>
        ((IDictionary<TKey, TValue>)_innerList).Values;

    public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_innerList).IsReadOnly;

    TValue IDictionary<TKey, TValue>.this[TKey key]
    {
        get => ((IDictionary<TKey, TValue>)_innerList)[key];
        set => ((IDictionary<TKey, TValue>)_innerList)[key] = value;
    }
}

public class MultiKeySortedList<TKey, TValue>(Func<TValue, TKey> KeySelector)
    : IDictionary<TKey, TValue[]>
    where TKey : notnull, IComparable<TKey>
{
    private readonly SortedList<TKey, List<TValue>> _innerList = [];

    public void AddRange(IEnumerable<TValue> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    public void Add(TValue item)
    {
        var key = KeySelector(item);
        Add(key, item);
    }

    public void Add(TKey key, TValue value)
    {
        if (!_innerList.TryGetValue(key, out var list))
        {
            list = [];
            _innerList[key] = list;
        }
        list.Add(value);
    }

    public void Add(TKey key, TValue[] value)
    {
        foreach (var item in value)
        {
            Add(key, item);
        }
    }

    public bool ContainsKey(TKey key)
    {
        return _innerList.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        return _innerList.Remove(key);
    }

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

    public void Add(KeyValuePair<TKey, TValue[]> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _innerList.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue[]> item)
    {
        if (_innerList.TryGetValue(item.Key, out var list))
        {
            return item.Value.SequenceEqual(list);
        }
        return false;
    }

    public void CopyTo(KeyValuePair<TKey, TValue[]>[] array, int arrayIndex)
    {
        foreach (var kvp in _innerList)
        {
            array[arrayIndex++] = new KeyValuePair<TKey, TValue[]>(kvp.Key, [.. kvp.Value]);
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue[]> item)
    {
        if (Contains(item))
        {
            return Remove(item.Key);
        }
        return false;
    }

    public IEnumerator<KeyValuePair<TKey, TValue[]>> GetEnumerator()
    {
        foreach (var kvp in _innerList)
        {
            yield return new KeyValuePair<TKey, TValue[]>(kvp.Key, [.. kvp.Value]);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public TValue[] this[TKey key] => _innerList.TryGetValue(key, out var list) ? [.. list] : [];

    public IEnumerable<TKey> Keys => _innerList.Keys;

    public IEnumerable<TValue> Values => _innerList.Values.SelectMany(list => list);

    ICollection<TKey> IDictionary<TKey, TValue[]>.Keys => _innerList.Keys;

    ICollection<TValue[]> IDictionary<TKey, TValue[]>.Values =>
        [.. _innerList.Values.Select(list => (TValue[])[.. list])];

    public int Count => _innerList.Count;

    public bool IsReadOnly => false;

    TValue[] IDictionary<TKey, TValue[]>.this[TKey key]
    {
        get => _innerList.TryGetValue(key, out var list) ? [.. list] : [];
        set { _innerList[key] = [.. value]; }
    }
}
