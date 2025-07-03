using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Weaviate.Client.Models;

public record ModuleConfigList : IDictionary<string, object>, IEquatable<ModuleConfigList>
{
    Dictionary<string, object> _internal = new();

    public object this[string key]
    {
        get => ((IDictionary<string, object>)_internal)[key];
        set => ((IDictionary<string, object>)_internal)[key] = value;
    }

    public ICollection<string> Keys => ((IDictionary<string, object>)_internal).Keys;

    public ICollection<object> Values => ((IDictionary<string, object>)_internal).Values;

    public int Count => ((ICollection<KeyValuePair<string, object>>)_internal).Count;

    public bool IsReadOnly => ((ICollection<KeyValuePair<string, object>>)_internal).IsReadOnly;

    public void Add(string key, object value)
    {
        ((IDictionary<string, object>)_internal).Add(key, value);
    }

    public void Add(KeyValuePair<string, object> item)
    {
        ((ICollection<KeyValuePair<string, object>>)_internal).Add(item);
    }

    public void Clear()
    {
        ((ICollection<KeyValuePair<string, object>>)_internal).Clear();
    }

    public bool Contains(KeyValuePair<string, object> item)
    {
        return ((ICollection<KeyValuePair<string, object>>)_internal).Contains(item);
    }

    public bool ContainsKey(string key)
    {
        return ((IDictionary<string, object>)_internal).ContainsKey(key);
    }

    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, object>>)_internal).CopyTo(array, arrayIndex);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_internal);
        return hash.ToHashCode();
    }

    public virtual bool Equals(ModuleConfigList? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return _internal.Count == other._internal.Count
            && _internal.Keys.SequenceEqual(other._internal.Keys)
            && _internal.Values.SequenceEqual(other._internal.Values);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, object>>)_internal).GetEnumerator();
    }

    public bool Remove(string key)
    {
        return ((IDictionary<string, object>)_internal).Remove(key);
    }

    public bool Remove(KeyValuePair<string, object> item)
    {
        return ((ICollection<KeyValuePair<string, object>>)_internal).Remove(item);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        return ((IDictionary<string, object>)_internal).TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_internal).GetEnumerator();
    }
}
