namespace Weaviate.Client.Models;

/// <summary>
/// Specifies the type(s) of vectors a vectorizer supports.
/// </summary>
[Flags]
public enum VectorType
{
    /// <summary>
    /// Supports single vector embeddings.
    /// </summary>
    Vector = 1,

    /// <summary>
    /// Supports multi-vector embeddings.
    /// </summary>
    MultiVector = 2,

    /// <summary>
    /// Supports both single and multi-vector embeddings.
    /// </summary>
    Both = Vector | MultiVector,
}

/// <summary>
/// Attribute to mark VectorizerConfig derived classes with metadata for automatic registration.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class VectorizerAttribute : Attribute
{
    /// <summary>
    /// The unique identifier string for this vectorizer (e.g., "text2vec-openai", "multi2vec-clip").
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// The type(s) of vectors this vectorizer supports.
    /// </summary>
    public VectorType VectorType { get; }

    public VectorizerAttribute(string identifier, VectorType vectorType = VectorType.Vector)
    {
        Identifier = identifier;
        VectorType = vectorType;
    }
}
