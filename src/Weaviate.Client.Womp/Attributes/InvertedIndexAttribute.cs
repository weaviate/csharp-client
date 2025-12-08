namespace Weaviate.Client.Womp.Attributes;

/// <summary>
/// Configures inverted index settings for a collection.
/// Apply this attribute at the class level to control indexing behavior.
/// </summary>
/// <example>
/// <code>
/// [WeaviateCollection("Articles")]
/// [InvertedIndex(
///     IndexTimestamps = true,
///     IndexNullState = true,
///     CleanupIntervalSeconds = 120
/// )]
/// public class Article
/// {
///     // Properties...
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InvertedIndexAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a value indicating whether to index object timestamps (creation, update).
    /// Required for filtering by creation/update time.
    /// </summary>
    public bool IndexTimestamps { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to index null state of properties.
    /// Allows filtering for null/non-null values.
    /// </summary>
    public bool IndexNullState { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to index property length.
    /// Allows filtering by property length.
    /// </summary>
    public bool IndexPropertyLength { get; set; }

    /// <summary>
    /// Gets or sets the cleanup interval in seconds for the inverted index.
    /// Default is 60 seconds.
    /// </summary>
    public int CleanupIntervalSeconds { get; set; } = 60;
}
