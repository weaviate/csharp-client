using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// Marker interface for hybrid search vector inputs, allowing both vector-based and text-based queries.
/// </summary>
public interface IHybridVectorInput
{
    // This interface is used to mark hybrid vectors, which can be either near vector or near text.
    // It allows for polymorphic behavior in the Hybrid methods.
}

/// <summary>
/// Marker interface for near-vector search inputs.
/// </summary>
public interface INearVectorInput { }

/// <summary>
/// Represents a collection of vectors for near-vector search queries.
/// </summary>
/// <remarks>
/// Supports multiple vectors with different names for multi-vector collections.
/// Provides implicit conversions from various vector formats for ease of use.
/// </remarks>
public record NearVectorInput : IEnumerable<Vector>, IHybridVectorInput, INearVectorInput
{
    private readonly List<Vector> _vectors = [];

    /// <summary>
    /// Gets the vectors grouped by their names.
    /// </summary>
    public IReadOnlyDictionary<string, Vector[]> Vectors =>
        _vectors.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.ToArray());

    /// <summary>
    /// Initializes a new instance of the <see cref="NearVectorInput"/> class.
    /// </summary>
    public NearVectorInput() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NearVectorInput"/> class with the specified vectors.
    /// </summary>
    /// <param name="vectors">The vectors to include.</param>
    public NearVectorInput(params Vector[] vectors)
    {
        foreach (var v in vectors)
        {
            Add(v.Name, v);
        }
    }

    /// <summary>
    /// Implicitly converts a single <see cref="Vector"/> to a <see cref="NearVectorInput"/>.
    /// </summary>
    public static implicit operator NearVectorInput(Vector vector)
    {
        return new NearVectorInput([vector]);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vector"/> array to a <see cref="NearVectorInput"/>.
    /// </summary>
    public static implicit operator NearVectorInput(Vector[] vector)
    {
        return new NearVectorInput(vector);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Vectors"/> collection to a <see cref="NearVectorInput"/>.
    /// </summary>
    public static implicit operator NearVectorInput(Vectors vectors)
    {
        return new NearVectorInput([.. vectors.Values]);
    }

    /// <summary>
    /// Adds one or more vectors with the specified name.
    /// </summary>
    /// <param name="name">The name for the vectors.</param>
    /// <param name="values">The vectors to add.</param>
    public void Add(string name, params Vector[] values) =>
        _vectors.AddRange(values.Select(v => Vector.Create(name, v)));

    /// <summary>
    /// Adds vectors from a <see cref="Vectors"/> collection.
    /// </summary>
    /// <param name="vectors">The vectors to add.</param>
    public void Add(Models.Vectors vectors)
    {
        _vectors.AddRange(vectors.Values);
    }

    private static NearVectorInput FromVectorDictionary(
        IEnumerable<KeyValuePair<string, IEnumerable<Vector>>> vectors
    )
    {
        var ret = new NearVectorInput();
        foreach (var (name, values) in vectors)
        {
            ret.Add(name, [.. values]);
        }
        return ret;
    }

    private static NearVectorInput FromSingleVectorDictionary<T>(
        Dictionary<string, T> vectors,
        Func<T, Vector> converter
    )
    {
        var ret = new NearVectorInput();
        foreach (var (name, value) in vectors)
        {
            ret.Add(name, converter(value));
        }
        return ret;
    }

    public static implicit operator NearVectorInput(Dictionary<string, Vector[]> vectors) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.AsEnumerable()))
        );

    public static implicit operator NearVectorInput(Dictionary<string, float[]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(Dictionary<string, double[]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(Dictionary<string, float[,]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(Dictionary<string, double[,]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<float[]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<double[]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<float[,]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<double[,]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    IEnumerator<Vector> IEnumerable<Vector>.GetEnumerator()
    {
        return ((IEnumerable<Vector>)_vectors).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_vectors).GetEnumerator();
    }
}

/// <summary>
/// Configures a vector-based search component for hybrid queries.
/// </summary>
/// <param name="Vector">The vector to search near.</param>
/// <param name="Certainty">Optional minimum certainty score for results (0.0 to 1.0). Deprecated in favor of <paramref name="Distance"/>.</param>
/// <param name="Distance">Optional maximum distance from the query vector.</param>
/// <param name="targetVector">Optional target vector names for multi-vector collections.</param>
/// <remarks>
/// Used in hybrid searches to combine vector similarity with keyword (BM25) search.
/// </remarks>
public record HybridNearVector(
    NearVectorInput Vector,
    float? Certainty = null,
    float? Distance = null,
    TargetVectors? targetVector = null
) : IHybridVectorInput { };

/// <summary>
/// Configures a text-based semantic search component for hybrid queries.
/// </summary>
/// <param name="Query">The text query to search for semantically.</param>
/// <param name="Certainty">Optional minimum certainty score for results (0.0 to 1.0). Deprecated in favor of <paramref name="Distance"/>.</param>
/// <param name="Distance">Optional maximum distance from the query vector.</param>
/// <param name="MoveTo">Optional move operation to shift results towards specific concepts or objects.</param>
/// <param name="MoveAway">Optional move operation to shift results away from specific concepts or objects.</param>
/// <remarks>
/// Used in hybrid searches to combine semantic text search with keyword (BM25) search.
/// The query text is converted to a vector using the collection's configured vectorizer.
/// </remarks>
/// <example>
/// <code>
/// // Hybrid search with text query
/// var nearText = new HybridNearText(
///     Query: "machine learning",
///     Distance: 0.5f,
///     MoveTo: new Move(0.3f, concepts: new[] { "neural networks" })
/// );
/// </code>
/// </example>
public record HybridNearText(
    string Query,
    float? Certainty = null,
    float? Distance = null,
    Move? MoveTo = null,
    Move? MoveAway = null
) : IHybridVectorInput;

/// <summary>
/// Specifies the operator to use when combining multiple BM25 (keyword) search terms.
/// </summary>
/// <param name="Operator">The operator name.</param>
public abstract record BM25Operator(string Operator)
{
    /// <summary>
    /// Requires all search terms to match (boolean AND).
    /// </summary>
    public record And() : BM25Operator("And");

    /// <summary>
    /// Requires a minimum number of search terms to match (boolean OR with minimum match).
    /// </summary>
    /// <param name="MinimumMatch">The minimum number of terms that must match.</param>
    public record Or(int MinimumMatch) : BM25Operator("Or");
}
