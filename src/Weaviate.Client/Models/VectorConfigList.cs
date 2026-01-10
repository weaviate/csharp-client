using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Weaviate.Client.Models;

/// <summary>
/// The vector config list
/// </summary>
public record VectorConfigList
    : IReadOnlyDictionary<string, VectorConfig>,
        IEquatable<VectorConfigList>
{
    /// <summary>
    /// The internal list
    /// </summary>
    private SortedList<string, VectorConfig> _internalList = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorConfigList"/> class
    /// </summary>
    /// <param name="vectorConfigs">The vector configs</param>
    public VectorConfigList(params VectorConfig[] vectorConfigs)
    {
        _internalList.Clear();

        foreach (var c in vectorConfigs)
        {
            _internalList.Add(c.Name, c);
        }
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of vector config</returns>
    public IEnumerator<VectorConfig> GetEnumerator()
    {
        return ((IEnumerable<VectorConfig>)_internalList).GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_internalList).GetEnumerator();
    }

    /// <summary>
    /// Implicitly converts a VectorConfig array to a VectorConfigList
    /// </summary>
    /// <param name="configs">The vector configs</param>
    public static implicit operator VectorConfigList(VectorConfig[] configs)
    {
        return new(configs);
    }

    /// <summary>
    /// Implicitly converts a single VectorConfig to a VectorConfigList
    /// </summary>
    /// <param name="config">The vector config</param>
    public static implicit operator VectorConfigList(VectorConfig config)
    {
        return new(config);
    }

    /// <summary>
    /// Adds the vector configs
    /// </summary>
    /// <param name="vectorConfigs">The vector configs</param>
    public void Add(params VectorConfig[] vectorConfigs)
    {
        foreach (var c in vectorConfigs)
        {
            _internalList.Add(c.Name, c);
        }
    }

    // IReadOnlyDictionary<string, VectorConfig> implementation
    /// <summary>
    /// The key
    /// </summary>
    public VectorConfig this[string key] =>
        _internalList[key] ?? throw new KeyNotFoundException($"The key '{key}' was not found.");

    /// <summary>
    /// Gets the value of the keys
    /// </summary>
    public IEnumerable<string> Keys => _internalList.Keys;

    /// <summary>
    /// Gets the value of the values
    /// </summary>
    public IEnumerable<VectorConfig> Values => _internalList.Values;

    /// <summary>
    /// Gets the value of the count
    /// </summary>
    public int Count => _internalList.Count;

    /// <summary>
    /// Containses the key using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <returns>The bool</returns>
    public bool ContainsKey(string key) => _internalList.ContainsKey(key);

    /// <summary>
    /// Tries the get value using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>The bool</returns>
    public bool TryGetValue(string key, [NotNullWhen(true)] out VectorConfig? value)
    {
        return _internalList.TryGetValue(key, out value);
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of key value pair string and vector config</returns>
    IEnumerator<KeyValuePair<string, VectorConfig>> IEnumerable<
        KeyValuePair<string, VectorConfig>
    >.GetEnumerator() => _internalList.GetEnumerator();

    /// <summary>
    /// Gets the hash code
    /// </summary>
    /// <returns>The int</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var kvp in _internalList)
        {
            hash.Add(kvp.Key);
            hash.Add(kvp.Value);
        }
        return hash.ToHashCode();
    }

    /// <summary>
    /// Equalses the other
    /// </summary>
    /// <param name="other">The other</param>
    /// <returns>The bool</returns>
    public virtual bool Equals(VectorConfigList? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (_internalList.Count != other._internalList.Count)
            return false;

        foreach (var kvp in _internalList)
        {
            if (!other._internalList.TryGetValue(kvp.Key, out var otherValue))
                return false;
            if (!EqualityComparer<VectorConfig>.Default.Equals(kvp.Value, otherValue))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Removes the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The bool</returns>
    internal bool Remove(string name)
    {
        return _internalList.Remove(name);
    }
}
