using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// The vector query class
/// </summary>
/// <seealso cref="IEnumerable{T}"/>
public class VectorQuery : IEnumerable<string>
{
    /// <summary>
    /// The vectors
    /// </summary>
    private List<string>? _vectors = null;

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorQuery"/> class
    /// </summary>
    public VectorQuery() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorQuery"/> class
    /// </summary>
    /// <param name="vectors">The vectors</param>
    public VectorQuery(IEnumerable<string>? vectors)
    {
        _vectors = vectors?.ToList();
    }

    /// <summary>
    /// Gets the value of the vectors
    /// </summary>
    public string[]? Vectors => _vectors?.ToArray();

    /// <summary>
    /// Adds the vector
    /// </summary>
    /// <param name="vector">The vector</param>
    public void Add(string vector)
    {
        _vectors ??= new List<string>();
        _vectors.Add(vector);
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of string</returns>
    public IEnumerator<string> GetEnumerator()
    {
        return _vectors?.GetEnumerator() ?? Enumerable.Empty<string>().GetEnumerator();
    }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    // Implicit conversion from bool to VectorQuery
    // false stores null, true stores empty array
    /// <summary>
    /// Implicitly converts a boolean to a VectorQuery
    /// </summary>
    /// <param name="includeVectors">Whether to include all vectors</param>
    public static implicit operator VectorQuery(bool includeVectors) =>
        new(includeVectors ? [] : null);

    // Implicit conversion from string array to VectorQuery
    /// <summary>
    /// Implicitly converts a string array to a VectorQuery
    /// </summary>
    /// <param name="vectors">The vector names</param>
    public static implicit operator VectorQuery(string[] vectors) => new(vectors);

    // Implicit conversion from string to VectorQuery
    /// <summary>
    /// Implicitly converts a string to a VectorQuery
    /// </summary>
    /// <param name="vector">The vector name</param>
    public static implicit operator VectorQuery(string vector) => new[] { vector };
}

/// <summary>
/// The metadata options enum
/// </summary>
[Flags]
public enum MetadataOptions
{
    /// <summary>
    /// The none metadata options
    /// </summary>
    None = 0,

    /// <summary>
    /// The creation time metadata options
    /// </summary>
    CreationTime = 1 << 1, // 2^1

    /// <summary>
    /// The last update time metadata options
    /// </summary>
    LastUpdateTime = 1 << 2, // 2^2

    /// <summary>
    /// The distance metadata options
    /// </summary>
    Distance = 1 << 3, // 2^3

    /// <summary>
    /// The certainty metadata options
    /// </summary>
    Certainty = 1 << 4, // 2^4

    /// <summary>
    /// The score metadata options
    /// </summary>
    Score = 1 << 5, // 2^5

    /// <summary>
    /// The explain score metadata options
    /// </summary>
    ExplainScore = 1 << 6, // 2^6

    /// <summary>
    /// The is consistent metadata options
    /// </summary>
    IsConsistent = 1 << 7, // 2^7

    /// <summary>
    /// The all metadata options
    /// </summary>
    All =
        CreationTime | LastUpdateTime | Distance | Certainty | Score | ExplainScore | IsConsistent,
}

/// <summary>
/// The metadata query
/// </summary>
public record MetadataQuery
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataQuery"/> class
    /// </summary>
    /// <param name="options">The options</param>
    public MetadataQuery(MetadataOptions options = MetadataOptions.None)
    {
        Options = options;
    }

    /// <summary>
    /// Enables the enable options
    /// </summary>
    /// <param name="enableOptions">The enable options</param>
    /// <returns>The metadata query</returns>
    public MetadataQuery Enable(MetadataOptions enableOptions)
    {
        Options |= enableOptions;
        return this;
    }

    /// <summary>
    /// Disables the disable options
    /// </summary>
    /// <param name="disableOptions">The disable options</param>
    /// <returns>The metadata query</returns>
    public MetadataQuery Disable(MetadataOptions disableOptions)
    {
        Options &= ~disableOptions;
        return this;
    }

    // Implicit conversion from MetadataOptions to MetadataQuery
    /// <summary>
    /// Implicitly converts MetadataOptions to a MetadataQuery
    /// </summary>
    /// <param name="options">The metadata options</param>
    public static implicit operator MetadataQuery(MetadataOptions options) => new(options);

    /// <summary>
    /// Gets the value of the creation time
    /// </summary>
    public bool CreationTime => (Options & MetadataOptions.CreationTime) != 0;

    /// <summary>
    /// Gets the value of the last update time
    /// </summary>
    public bool LastUpdateTime => (Options & MetadataOptions.LastUpdateTime) != 0;

    /// <summary>
    /// Gets the value of the distance
    /// </summary>
    public bool Distance => (Options & MetadataOptions.Distance) != 0;

    /// <summary>
    /// Gets the value of the certainty
    /// </summary>
    public bool Certainty => (Options & MetadataOptions.Certainty) != 0;

    /// <summary>
    /// Gets the value of the score
    /// </summary>
    public bool Score => (Options & MetadataOptions.Score) != 0;

    /// <summary>
    /// Gets the value of the explain score
    /// </summary>
    public bool ExplainScore => (Options & MetadataOptions.ExplainScore) != 0;

    /// <summary>
    /// Gets the value of the is consistent
    /// </summary>
    public bool IsConsistent => (Options & MetadataOptions.IsConsistent) != 0;

    /// <summary>
    /// Gets or sets the value of the options
    /// </summary>
    public MetadataOptions Options { get; private set; } = MetadataOptions.None;
}
