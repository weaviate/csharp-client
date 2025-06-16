using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Weaviate.Client.Models;

public record VectorConfigList : IReadOnlyDictionary<string, VectorConfig>
{
    private List<VectorConfig> _internalList = new();

    public VectorConfigList(params VectorConfig[] vectorConfigs)
    {
        _internalList = [.. vectorConfigs];
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

    // public static implicit operator Dictionary<string, VectorConfig>(VectorConfigList configs) =>
    //     configs._internalList.ToDictionary(config => config.Name, config => config);

    // IReadOnlyDictionary<string, VectorConfig> implementation
    public VectorConfig this[string key] =>
        _internalList.FirstOrDefault(config => config.Name == key)
        ?? throw new KeyNotFoundException($"The key '{key}' was not found.");

    public IEnumerable<string> Keys => _internalList.Select(config => config.Name);

    public IEnumerable<VectorConfig> Values => _internalList;

    public int Count => _internalList.Count;

    public bool ContainsKey(string key) => _internalList.Any(config => config.Name == key);

    public bool TryGetValue(string key, [NotNullWhen(true)] out VectorConfig? value)
    {
        value = _internalList.FirstOrDefault(config => config.Name == key);
        return value != null;
    }

    IEnumerator<KeyValuePair<string, VectorConfig>> IEnumerable<
        KeyValuePair<string, VectorConfig>
    >.GetEnumerator()
    {
        return _internalList
            .Select(config => new KeyValuePair<string, VectorConfig>(config.Name, config))
            .GetEnumerator();
    }
}
