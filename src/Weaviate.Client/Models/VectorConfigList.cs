using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Weaviate.Client.Models;

public record VectorConfigList
    : IReadOnlyDictionary<string, VectorConfig>,
        IEquatable<VectorConfigList>
{
    private SortedList<string, VectorConfig> _internalList = new();

    public VectorConfigList(params VectorConfig[] vectorConfigs)
    {
        _internalList.Clear();

        foreach (var c in vectorConfigs)
        {
            _internalList.Add(c.Name, c);
        }
    }

    public IEnumerator<VectorConfig> GetEnumerator()
    {
        return ((IEnumerable<VectorConfig>)_internalList).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_internalList).GetEnumerator();
    }

    public static implicit operator VectorConfigList(VectorConfig[] configs)
    {
        return new(configs);
    }

    public static implicit operator VectorConfigList(VectorConfig config)
    {
        return new(config);
    }

    public void Add(params VectorConfig[] vectorConfigs)
    {
        foreach (var c in vectorConfigs)
        {
            _internalList.Add(c.Name, c);
        }
    }

    // IReadOnlyDictionary<string, VectorConfig> implementation
    public VectorConfig this[string key] =>
        _internalList[key] ?? throw new KeyNotFoundException($"The key '{key}' was not found.");

    public IEnumerable<string> Keys => _internalList.Keys;

    public IEnumerable<VectorConfig> Values => _internalList.Values;

    public int Count => _internalList.Count;

    public bool ContainsKey(string key) => _internalList.ContainsKey(key);

    public bool TryGetValue(string key, [NotNullWhen(true)] out VectorConfig? value)
    {
        return _internalList.TryGetValue(key, out value);
    }

    IEnumerator<KeyValuePair<string, VectorConfig>> IEnumerable<
        KeyValuePair<string, VectorConfig>
    >.GetEnumerator() => _internalList.GetEnumerator();

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_internalList);
        return hash.ToHashCode();
    }

    public virtual bool Equals(VectorConfigList? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return _internalList.SequenceEqual(other._internalList);
    }

    internal bool Remove(string name)
    {
        return _internalList.Remove(name);
    }
}
