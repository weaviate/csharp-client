using Weaviate.Client.Models;

namespace Weaviate.Client.Orm.Attributes;

/// <summary>
/// Base class for reranker attributes. Used for runtime type inspection.
/// Do not use this directly - use RerankerAttribute&lt;TModule&gt; instead.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class RerankerAttributeBase : Attribute
{
    /// <summary>
    /// Gets the reranker module type.
    /// </summary>
    public abstract Type ModuleType { get; }
}

/// <summary>
/// Configures the reranker module for advanced result reranking.
/// Applied at the class level to enable reranking of search results based on relevance.
/// </summary>
/// <typeparam name="TModule">The reranker module type (e.g., Reranker.Cohere, Reranker.VoyageAI).</typeparam>
/// <example>
/// <code>
/// // Cohere reranker
/// [WeaviateCollection("Articles")]
/// [Reranker&lt;Reranker.Cohere&gt;(Model = "rerank-english-v2.0")]
/// public class Article
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
/// }
///
/// // VoyageAI reranker
/// [Reranker&lt;Reranker.VoyageAI&gt;(Model = "rerank-2.5")]
/// public class Document { }
///
/// // Transformers (local reranker - no model needed)
/// [Reranker&lt;Reranker.Transformers&gt;]
/// public class Product { }
///
/// // JinaAI with custom configuration
/// [Reranker&lt;Reranker.JinaAI&gt;(
///     Model = "jina-reranker-v2-base-multilingual",
///     ConfigMethod = nameof(ConfigureReranker)
/// )]
/// public class Article
/// {
///     public static Reranker.JinaAI ConfigureReranker(
///         Reranker.JinaAI prebuilt)
///     {
///         // Additional configuration if needed
///         return prebuilt;
///     }
/// }
/// </code>
/// </example>
public class RerankerAttribute<TModule> : RerankerAttributeBase
    where TModule : IRerankerConfig
{
    /// <inheritdoc/>
    public override Type ModuleType => typeof(TModule);

    // Common properties across reranker modules
    /// <summary>
    /// Gets or sets the model name.
    /// Required for most rerankers (Cohere, VoyageAI, JinaAI, Nvidia).
    /// Not required for Transformers (uses default model).
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the reranker API.
    /// Applicable to some rerankers like Nvidia.
    /// </summary>
    public string? BaseURL { get; set; }

    // ContextualAI specific
    /// <summary>
    /// Gets or sets the instruction for contextual reranking (ContextualAI).
    /// Provides context about what makes a result relevant.
    /// </summary>
    public string? Instruction { get; set; }

    /// <summary>
    /// Gets or sets the top N results to return after reranking (ContextualAI).
    /// Set to -1 to use provider default.
    /// </summary>
    public int TopN { get; set; } = -1;

    // Advanced configuration
    /// <summary>
    /// Gets or sets the name of a static method that configures the reranker module.
    /// The method signature must be: static TModule MethodName(TModule prebuilt)
    /// The method receives a pre-built module with properties from the attribute already set.
    /// If ConfigMethodClass is not specified, the method is looked up in the same class.
    /// </summary>
    /// <example>
    /// <code>
    /// // Same class
    /// [Reranker&lt;Reranker.Cohere&gt;(
    ///     Model = "rerank-english-v2.0",
    ///     ConfigMethod = nameof(ConfigureReranker)
    /// )]
    /// public class Article
    /// {
    ///     public static Reranker.Cohere ConfigureReranker(
    ///         Reranker.Cohere prebuilt)
    ///     {
    ///         // Custom configuration
    ///         return prebuilt;
    ///     }
    /// }
    ///
    /// // Different class (type-safe)
    /// [Reranker&lt;Reranker.VoyageAI&gt;(
    ///     Model = "rerank-2.5",
    ///     ConfigMethod = nameof(RerankerConfigurations.ConfigureVoyage),
    ///     ConfigMethodClass = typeof(RerankerConfigurations)
    /// )]
    /// public class Article { }
    /// </code>
    /// </example>
    public string? ConfigMethod { get; set; }

    /// <summary>
    /// Gets or sets the class containing the ConfigMethod.
    /// If not specified, the method is looked up in the same class.
    /// This provides compile-time type safety when referencing methods in different classes.
    /// </summary>
    /// <example>
    /// <code>
    /// [Reranker&lt;Reranker.Cohere&gt;(
    ///     ConfigMethod = nameof(RerankerConfigurations.ConfigureCohere),
    ///     ConfigMethodClass = typeof(RerankerConfigurations)
    /// )]
    /// public class Article { }
    /// </code>
    /// </example>
    public Type? ConfigMethodClass { get; set; }
}
