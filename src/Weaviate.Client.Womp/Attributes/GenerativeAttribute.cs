using Weaviate.Client.Models;

namespace Weaviate.Client.Womp.Attributes;

/// <summary>
/// Base class for generative attributes. Used for runtime type inspection.
/// Do not use this directly - use GenerativeAttribute&lt;TModule&gt; instead.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public abstract class GenerativeAttributeBase : Attribute
{
    /// <summary>
    /// Gets the generative module type.
    /// </summary>
    public abstract Type ModuleType { get; }
}

/// <summary>
/// Configures the generative AI module for RAG (Retrieval Augmented Generation) use cases.
/// Applied at the class level to enable generate() operations on the collection.
/// </summary>
/// <typeparam name="TModule">The generative module type (e.g., GenerativeConfig.OpenAI, GenerativeConfig.Anthropic).</typeparam>
/// <example>
/// <code>
/// // Simple OpenAI configuration
/// [WeaviateCollection("Articles")]
/// [Generative&lt;GenerativeConfig.OpenAI&gt;(Model = "gpt-4")]
/// public class Article
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
/// }
///
/// // Anthropic with custom configuration
/// [Generative&lt;GenerativeConfig.Anthropic&gt;(
///     Model = "claude-3-5-sonnet-20241022",
///     MaxTokens = 4096,
///     Temperature = 0.7,
///     ConfigMethod = nameof(ConfigureGenerative)
/// )]
/// public class Article
/// {
///     public string Title { get; set; }
///     public string Content { get; set; }
///
///     public static GenerativeConfig.Anthropic ConfigureGenerative(
///         GenerativeConfig.Anthropic prebuilt)
///     {
///         prebuilt.StopSequences = new[] { "\n\nHuman:", "\n\nAssistant:" };
///         prebuilt.TopK = 50;
///         return prebuilt;
///     }
/// }
///
/// // Azure OpenAI
/// [Generative&lt;GenerativeConfig.AzureOpenAI&gt;(
///     ResourceName = "my-resource",
///     DeploymentId = "gpt-4-deployment",
///     Model = "gpt-4"
/// )]
/// public class Document { }
/// </code>
/// </example>
public class GenerativeAttribute<TModule> : GenerativeAttributeBase
    where TModule : IGenerativeConfig
{
    /// <inheritdoc/>
    public override Type ModuleType => typeof(TModule);

    // Common properties across most generative modules
    /// <summary>
    /// Gets or sets the model name.
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Gets or sets the base URL for the generative API.
    /// </summary>
    public string? BaseURL { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of tokens to generate.
    /// Set to -1 to use provider default.
    /// </summary>
    public int MaxTokens { get; set; } = -1;

    /// <summary>
    /// Gets or sets the temperature for generation (typically 0.0 to 1.0).
    /// Controls randomness: lower values are more deterministic.
    /// Set to -1 to use provider default.
    /// </summary>
    public double Temperature { get; set; } = -1;

    /// <summary>
    /// Gets or sets the top-p (nucleus sampling) parameter.
    /// Controls diversity: lower values are more focused.
    /// Set to -1 to use provider default.
    /// </summary>
    public double TopP { get; set; } = -1;

    /// <summary>
    /// Gets or sets the top-k parameter for token sampling.
    /// Set to -1 to use provider default.
    /// </summary>
    public int TopK { get; set; } = -1;

    // Azure OpenAI specific
    /// <summary>
    /// Gets or sets the Azure resource name (Azure OpenAI only).
    /// </summary>
    public string? ResourceName { get; set; }

    /// <summary>
    /// Gets or sets the deployment ID (Azure OpenAI only).
    /// </summary>
    public string? DeploymentId { get; set; }

    /// <summary>
    /// Gets or sets the API version (some modules like Azure OpenAI).
    /// </summary>
    public string? ApiVersion { get; set; }

    // OpenAI specific
    /// <summary>
    /// Gets or sets the frequency penalty (OpenAI).
    /// Set to -999 to use provider default.
    /// </summary>
    public int FrequencyPenalty { get; set; } = -999;

    /// <summary>
    /// Gets or sets the presence penalty (OpenAI).
    /// Set to -999 to use provider default.
    /// </summary>
    public int PresencePenalty { get; set; } = -999;

    /// <summary>
    /// Gets or sets the reasoning effort (OpenAI o1/o3 models).
    /// </summary>
    public string? ReasoningEffort { get; set; }

    // AWS specific
    /// <summary>
    /// Gets or sets the AWS region (AWS Bedrock).
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Gets or sets the AWS service name (AWS Bedrock).
    /// </summary>
    public string? Service { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URL (various providers).
    /// </summary>
    public string? Endpoint { get; set; }

    // Google specific
    /// <summary>
    /// Gets or sets the Google project ID (Google Vertex AI).
    /// </summary>
    public string? ProjectId { get; set; }

    /// <summary>
    /// Gets or sets the Google endpoint ID (Google Vertex AI).
    /// </summary>
    public string? EndpointId { get; set; }

    // Databricks specific
    /// <summary>
    /// Gets or sets the API endpoint for Databricks.
    /// </summary>
    public string? ApiEndpoint { get; set; }

    // Advanced configuration
    /// <summary>
    /// Gets or sets the name of a static method that configures the generative module.
    /// The method signature must be: static TModule MethodName(TModule prebuilt)
    /// The method receives a pre-built module with properties from the attribute already set.
    /// If ConfigMethodClass is not specified, the method is looked up in the same class.
    /// </summary>
    /// <example>
    /// <code>
    /// // Same class
    /// [Generative&lt;GenerativeConfig.OpenAI&gt;(
    ///     Model = "gpt-4",
    ///     ConfigMethod = nameof(ConfigureOpenAI)
    /// )]
    /// public class Article
    /// {
    ///     public static GenerativeConfig.OpenAI ConfigureOpenAI(
    ///         GenerativeConfig.OpenAI prebuilt)
    ///     {
    ///         prebuilt.Temperature = 0.7;
    ///         prebuilt.MaxTokens = 500;
    ///         return prebuilt;
    ///     }
    /// }
    ///
    /// // Different class (type-safe)
    /// [Generative&lt;GenerativeConfig.Anthropic&gt;(
    ///     Model = "claude-3-5-sonnet-20241022",
    ///     ConfigMethod = nameof(GenerativeConfigurations.ConfigureAnthropic),
    ///     ConfigMethodClass = typeof(GenerativeConfigurations)
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
    /// [Generative&lt;GenerativeConfig.OpenAI&gt;(
    ///     ConfigMethod = nameof(GenerativeConfigurations.ConfigureOpenAI),
    ///     ConfigMethodClass = typeof(GenerativeConfigurations)
    /// )]
    /// public class Article { }
    /// </code>
    /// </example>
    public Type? ConfigMethodClass { get; set; }
}
