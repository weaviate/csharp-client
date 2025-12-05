using System.Reflection;

namespace Weaviate.Client.Models;

/// <summary>
/// Registry for VectorizerConfig types that auto-discovers types marked with VectorizerAttribute.
/// </summary>
internal static class VectorizerRegistry
{
    private static readonly Lazy<Dictionary<string, VectorizerInfo>> _registry = new(BuildRegistry);

    /// <summary>
    /// Information about a registered vectorizer type.
    /// </summary>
    internal readonly record struct VectorizerInfo(Type Type, VectorType VectorType);

    /// <summary>
    /// Gets the registry of vectorizer types keyed by identifier.
    /// </summary>
    public static IReadOnlyDictionary<string, VectorizerInfo> ConfigTypes => _registry.Value;

    /// <summary>
    /// Tries to get vectorizer info for the specified identifier.
    /// </summary>
    public static bool TryGetVectorizer(string identifier, out VectorizerInfo info)
    {
        return _registry.Value.TryGetValue(identifier, out info);
    }

    /// <summary>
    /// Checks if a vectorizer identifier supports multi-vector embeddings.
    /// </summary>
    public static bool SupportsMultiVector(string identifier)
    {
        return _registry.Value.TryGetValue(identifier, out var info)
            && info.VectorType.HasFlag(VectorType.MultiVector);
    }

    /// <summary>
    /// Checks if a vectorizer identifier supports single vector embeddings.
    /// </summary>
    public static bool SupportsVector(string identifier)
    {
        return _registry.Value.TryGetValue(identifier, out var info)
            && info.VectorType.HasFlag(VectorType.Vector);
    }

    private static Dictionary<string, VectorizerInfo> BuildRegistry()
    {
        var registry = new Dictionary<string, VectorizerInfo>();

        var assembly = typeof(VectorizerConfig).Assembly;
        var vectorizerConfigType = typeof(VectorizerConfig);

        foreach (var type in assembly.GetTypes())
        {
            if (!type.IsClass || type.IsAbstract || !vectorizerConfigType.IsAssignableFrom(type))
                continue;

            var attribute = type.GetCustomAttribute<VectorizerAttribute>();
            if (attribute == null)
                continue;

            registry[attribute.Identifier] = new VectorizerInfo(type, attribute.VectorType);
        }

        return registry;
    }
}
