using Weaviate.Client.Models.Vectorizers;

namespace Weaviate.Client.Models;

public record NamedVectorConfigList
{
    private List<NamedVectorConfig> _internalList = new();

    private static NamedVectorConfigList _Builder(params NamedVectorConfig[] configs)
    {
        return new() { _internalList = [.. configs] };
    }

    public static implicit operator NamedVectorConfigList(NamedVectorConfig[] configs)
    {
        return _Builder(configs);
    }

    public static implicit operator NamedVectorConfigList(NamedVectorConfig config)
    {
        return _Builder(config);
    }

    public static implicit operator Dictionary<string, VectorConfig>(NamedVectorConfigList configs)
    {
        var dict = configs._internalList.ToDictionary(
            config => config.Name,
            config => new VectorConfig
            {
                Vectorizer = config.Vectorizer,
                VectorIndexConfig = config.VectorIndex ?? VectorIndexConfig.Default,
            }
        );

        return dict;
    }
}

public record NamedVectorConfig
{
    public string Name { get; }
    public VectorizerConfig Vectorizer { get; }
    public VectorIndexConfig? VectorIndex { get; }

    private NamedVectorConfig(
        string name,
        VectorizerConfig vectorizer,
        VectorIndexConfig? vectorIndex = null
    )
    {
        Name = name;
        Vectorizer = vectorizer;
        VectorIndex = vectorIndex;
    }

    // TODO Maybe have a global config value for default named vector
    public static NamedVectorConfig None(string name = "default") => New(name);

    public static NamedVectorConfig New(
        string namedVector,
        VectorizerConfig? config = null,
        VectorIndexConfig? vectorIndexConfig = null
    )
    {
        config ??= new NoneConfig();
        vectorIndexConfig ??= new VectorIndexConfigHNSW();

        return new(namedVector, config, vectorIndexConfig);
    }
}
