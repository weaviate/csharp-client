using System.Collections;

namespace Weaviate.Client.Models;

public record VectorConfigList : IEnumerable<VectorConfig>
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

    public static implicit operator Dictionary<string, VectorConfig>(VectorConfigList configs)
    {
        var dict = configs._internalList.ToDictionary(config => config.Name, config => config);

        return dict;
    }
}
