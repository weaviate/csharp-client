using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// Specifies which vectors to include in query results.
/// </summary>
/// <remarks>
/// Use this to control which named vectors are returned with query results.
/// For single-vector collections, use <c>true</c> to include the vector or <c>false</c> to exclude it.
/// For multi-vector collections, provide specific vector names to include only those vectors.
/// </remarks>
/// <example>
/// <code>
/// // Include all vectors (single-vector collection)
/// VectorQuery allVectors = true;
///
/// // Exclude all vectors
/// VectorQuery noVectors = false;
///
/// // Include specific named vectors (multi-vector collection)
/// VectorQuery specificVectors = new[] { "image_vector", "text_vector" };
///
/// // Include a single named vector
/// VectorQuery singleVector = "embedding";
/// </code>
/// </example>
public class VectorQuery : IEnumerable<string>
{
    private List<string>? _vectors = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorQuery"/> class with no vectors.
    /// </summary>
    public VectorQuery() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorQuery"/> class with the specified vector names.
    /// </summary>
    /// <param name="vectors">The names of vectors to include. Pass null to exclude all vectors, or an empty array to include all.</param>
    public VectorQuery(IEnumerable<string>? vectors)
    {
        _vectors = vectors?.ToList();
    }

    /// <summary>
    /// Gets the array of vector names to include, or null if vectors should be excluded.
    /// </summary>
    public string[]? Vectors => _vectors?.ToArray();

    /// <summary>
    /// Adds a vector name to the query.
    /// </summary>
    /// <param name="vector">The name of the vector to include.</param>
    public void Add(string vector)
    {
        _vectors ??= new List<string>();
        _vectors.Add(vector);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the vector names.
    /// </summary>
    public IEnumerator<string> GetEnumerator()
    {
        return _vectors?.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Implicitly converts a boolean value to a <see cref="VectorQuery"/>.
    /// </summary>
    /// <param name="includeVectors">If true, includes all vectors; if false, excludes all vectors.</param>
    public static implicit operator VectorQuery(bool includeVectors) =>
        new(includeVectors ? [] : null);

    /// <summary>
    /// Implicitly converts a string array to a <see cref="VectorQuery"/>.
    /// </summary>
    /// <param name="vectors">The names of vectors to include.</param>
    public static implicit operator VectorQuery(string[] vectors) => new(vectors);

    /// <summary>
    /// Implicitly converts a single string to a <see cref="VectorQuery"/>.
    /// </summary>
    /// <param name="vector">The name of the vector to include.</param>
    public static implicit operator VectorQuery(string vector) => new[] { vector };
}

/// <summary>
/// Flags enum specifying which metadata fields to include in query results.
/// </summary>
/// <remarks>
/// Multiple options can be combined using the bitwise OR operator (|).
/// Use <see cref="All"/> to include all available metadata fields.
/// </remarks>
[Flags]
public enum MetadataOptions
{
    /// <summary>
    /// No metadata fields.
    /// </summary>
    None = 0,

    /// <summary>
    /// Include the object's creation timestamp.
    /// </summary>
    CreationTime = 1 << 1,

    /// <summary>
    /// Include the object's last update timestamp.
    /// </summary>
    LastUpdateTime = 1 << 2,

    /// <summary>
    /// Include the distance from the search query (for vector searches).
    /// </summary>
    Distance = 1 << 3,

    /// <summary>
    /// Include the certainty score (normalized distance, deprecated in favor of <see cref="Distance"/>).
    /// </summary>
    Certainty = 1 << 4,

    /// <summary>
    /// Include the relevance score for the query result.
    /// </summary>
    Score = 1 << 5,

    /// <summary>
    /// Include a detailed explanation of how the score was calculated.
    /// </summary>
    ExplainScore = 1 << 6,

    /// <summary>
    /// Include the consistency status of the object (for eventual consistency scenarios).
    /// </summary>
    IsConsistent = 1 << 7,

    /// <summary>
    /// Include all available metadata fields.
    /// </summary>
    All =
        CreationTime | LastUpdateTime | Distance | Certainty | Score | ExplainScore | IsConsistent,
}

/// <summary>
/// Specifies which metadata fields to include in query results.
/// </summary>
/// <remarks>
/// Use this to control which metadata about objects is returned with query results.
/// Metadata can include timestamps, distance/score information, and consistency status.
/// Use the fluent <see cref="Enable"/> and <see cref="Disable"/> methods to configure options.
/// </remarks>
/// <example>
/// <code>
/// // Include distance and creation time
/// var metadata = new MetadataQuery()
///     .Enable(MetadataOptions.Distance)
///     .Enable(MetadataOptions.CreationTime);
///
/// // Include all metadata
/// var allMetadata = new MetadataQuery(MetadataOptions.All);
///
/// // Implicit conversion from enum
/// MetadataQuery justDistance = MetadataOptions.Distance;
///
/// // Combine multiple options
/// var combined = new MetadataQuery(MetadataOptions.Distance | MetadataOptions.Score);
/// </code>
/// </example>
public record MetadataQuery
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataQuery"/> class with the specified options.
    /// </summary>
    /// <param name="options">The metadata fields to include. Defaults to <see cref="MetadataOptions.None"/>.</param>
    public MetadataQuery(MetadataOptions options = MetadataOptions.None)
    {
        Options = options;
    }

    /// <summary>
    /// Enables additional metadata fields.
    /// </summary>
    /// <param name="enableOptions">The metadata fields to enable.</param>
    /// <returns>This <see cref="MetadataQuery"/> instance for method chaining.</returns>
    public MetadataQuery Enable(MetadataOptions enableOptions)
    {
        Options |= enableOptions;
        return this;
    }

    /// <summary>
    /// Disables specific metadata fields.
    /// </summary>
    /// <param name="disableOptions">The metadata fields to disable.</param>
    /// <returns>This <see cref="MetadataQuery"/> instance for method chaining.</returns>
    public MetadataQuery Disable(MetadataOptions disableOptions)
    {
        Options &= ~disableOptions;
        return this;
    }

    /// <summary>
    /// Implicitly converts <see cref="MetadataOptions"/> to a <see cref="MetadataQuery"/>.
    /// </summary>
    /// <param name="options">The metadata options to convert.</param>
    public static implicit operator MetadataQuery(MetadataOptions options) => new(options);

    /// <summary>
    /// Gets a value indicating whether creation time is included.
    /// </summary>
    public bool CreationTime => (Options & MetadataOptions.CreationTime) != 0;

    /// <summary>
    /// Gets a value indicating whether last update time is included.
    /// </summary>
    public bool LastUpdateTime => (Options & MetadataOptions.LastUpdateTime) != 0;

    /// <summary>
    /// Gets a value indicating whether distance is included.
    /// </summary>
    public bool Distance => (Options & MetadataOptions.Distance) != 0;

    /// <summary>
    /// Gets a value indicating whether certainty is included.
    /// </summary>
    public bool Certainty => (Options & MetadataOptions.Certainty) != 0;

    /// <summary>
    /// Gets a value indicating whether score is included.
    /// </summary>
    public bool Score => (Options & MetadataOptions.Score) != 0;

    /// <summary>
    /// Gets a value indicating whether score explanation is included.
    /// </summary>
    public bool ExplainScore => (Options & MetadataOptions.ExplainScore) != 0;

    /// <summary>
    /// Gets a value indicating whether consistency status is included.
    /// </summary>
    public bool IsConsistent => (Options & MetadataOptions.IsConsistent) != 0;

    /// <summary>
    /// Gets the combined metadata options.
    /// </summary>
    public MetadataOptions Options { get; private set; } = MetadataOptions.None;
}
