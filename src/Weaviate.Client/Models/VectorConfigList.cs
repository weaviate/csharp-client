using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Models;

public record VectorConfigList
{
    private List<VectorConfig> _internalList = new();

    private static VectorConfigList _Builder(params VectorConfig[] configs)
    {
        return new() { _internalList = [.. configs] };
    }

    public static implicit operator VectorConfigList(VectorConfig[] configs)
    {
        return _Builder(configs);
    }

    public static implicit operator VectorConfigList(VectorConfig config)
    {
        return _Builder(config);
    }

    public static implicit operator Dictionary<string, VectorConfig>(VectorConfigList configs)
    {
        var dict = configs._internalList.ToDictionary(
            config => config.Name,
            config => new VectorConfig(config.Name)
            {
                Vectorizer = config.Vectorizer,
                VectorIndexConfig = config.VectorIndexConfig ?? VectorIndexConfig.Default,
            }
        );

        return dict;
    }
}
