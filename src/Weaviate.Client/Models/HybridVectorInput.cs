namespace Weaviate.Client.Models;

/// <summary>
/// Discriminated union for hybrid search vector inputs.
/// Can hold exactly one of: VectorSearchInput, NearTextInput, or NearVectorInput.
/// </summary>
public sealed class HybridVectorInput
{
    /// <summary>
    /// Delegate for lambda builder pattern with HybridVectorInput.
    /// </summary>
    public delegate HybridVectorInput FactoryFn(HybridVectorInputBuilder builder);

    /// <summary>
    /// Gets the value of the vector search
    /// </summary>
    public VectorSearchInput? VectorSearch { get; }

    /// <summary>
    /// Gets the value of the near text
    /// </summary>
    public NearTextInput? NearText { get; }

    /// <summary>
    /// Gets the value of the near vector
    /// </summary>
    public NearVectorInput? NearVector { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="HybridVectorInput"/> class
    /// </summary>
    /// <param name="vectorSearch">The vector search</param>
    /// <param name="nearText">The near text</param>
    /// <param name="nearVector">The near vector</param>
    /// <exception cref="ArgumentException">HybridVectorInput must contain exactly one of: VectorSearch, NearText, or NearVector.</exception>
    private HybridVectorInput(
        VectorSearchInput? vectorSearch = null,
        NearTextInput? nearText = null,
        NearVectorInput? nearVector = null
    )
    {
        var setCount =
            (vectorSearch != null ? 1 : 0)
            + (nearText != null ? 1 : 0)
            + (nearVector != null ? 1 : 0);
        if (setCount != 1)
        {
            throw new ArgumentException(
                "HybridVectorInput must contain exactly one of: VectorSearch, NearText, or NearVector."
            );
        }

        VectorSearch = vectorSearch;
        NearText = nearText;
        NearVector = nearVector;
    }

    /// <summary>
    /// Creates a HybridVectorInput from a VectorSearchInput.
    /// </summary>
    public static HybridVectorInput FromVectorSearch(VectorSearchInput vectorSearch)
    {
        ArgumentNullException.ThrowIfNull(vectorSearch);
        return new HybridVectorInput(vectorSearch: vectorSearch);
    }

    /// <summary>
    /// Creates a HybridVectorInput from a NearTextInput.
    /// </summary>
    public static HybridVectorInput FromNearText(NearTextInput nearText)
    {
        ArgumentNullException.ThrowIfNull(nearText);
        return new HybridVectorInput(nearText: nearText);
    }

    /// <summary>
    /// Creates a HybridVectorInput from a NearVectorInput.
    /// </summary>
    public static HybridVectorInput FromNearVector(NearVectorInput nearVector)
    {
        ArgumentNullException.ThrowIfNull(nearVector);
        return new HybridVectorInput(nearVector: nearVector);
    }

    /// <summary>
    /// Pattern matching over the union type.
    /// </summary>
    public TResult Match<TResult>(
        Func<VectorSearchInput, TResult> onVectorSearch,
        Func<NearTextInput, TResult> onNearText,
        Func<NearVectorInput, TResult> onNearVector
    )
    {
        if (VectorSearch != null)
            return onVectorSearch(VectorSearch);
        if (NearText != null)
            return onNearText(NearText);
        if (NearVector != null)
            return onNearVector(NearVector);

        throw new InvalidOperationException("HybridVectorInput is in an invalid state.");
    }

    // Implicit conversions for ergonomic API usage

    /// <summary>
    /// Implicitly converts a VectorSearchInput to a HybridVectorInput.
    /// </summary>
    /// <param name="vectorSearch">The vector search input.</param>
    public static implicit operator HybridVectorInput(VectorSearchInput vectorSearch) =>
        FromVectorSearch(vectorSearch);

    /// <summary>
    /// Implicitly converts a NearTextInput to a HybridVectorInput.
    /// </summary>
    /// <param name="nearText">The near text input.</param>
    public static implicit operator HybridVectorInput(NearTextInput nearText) =>
        FromNearText(nearText);

    /// <summary>
    /// Implicitly converts a NearVectorInput to a HybridVectorInput.
    /// </summary>
    /// <param name="nearVector">The near vector input.</param>
    public static implicit operator HybridVectorInput(NearVectorInput nearVector) =>
        FromNearVector(nearVector);

    /// <summary>
    /// Implicitly converts a float array to a HybridVectorInput.
    /// </summary>
    /// <param name="vector">The float array vector.</param>
    public static implicit operator HybridVectorInput(float[] vector) => FromVectorSearch(vector);

    /// <summary>
    /// Implicitly converts a double array to a HybridVectorInput.
    /// </summary>
    /// <param name="vector">The double array vector.</param>
    public static implicit operator HybridVectorInput(double[] vector) => FromVectorSearch(vector);

    /// <summary>
    /// Implicitly converts a Vectors object to a HybridVectorInput.
    /// </summary>
    /// <param name="vectors">The Vectors object.</param>
    public static implicit operator HybridVectorInput(Vectors vectors) => FromVectorSearch(vectors);

    /// <summary>
    /// Implicitly converts a Vector object to a HybridVectorInput.
    /// </summary>
    /// <param name="vector">The Vector object.</param>
    public static implicit operator HybridVectorInput(Vector vector) =>
        FromVectorSearch(new Vectors(vector));

    /// <summary>
    /// Implicitly converts a NamedVector to a HybridVectorInput.
    /// </summary>
    /// <param name="namedVector">The NamedVector object.</param>
    public static implicit operator HybridVectorInput(NamedVector namedVector) =>
        FromVectorSearch(namedVector);

    /// <summary>
    /// Implicitly converts a string to a HybridVectorInput (as a NearTextInput).
    /// </summary>
    /// <param name="query">The query string.</param>
    public static implicit operator HybridVectorInput(string query) =>
        FromNearText(new NearTextInput(query));

    /// <summary>
    /// Implicitly converts a tuple of (string, float[]) to a HybridVectorInput.
    /// </summary>
    /// <param name="tuple">The tuple containing a name and a float array vector.</param>
    public static implicit operator HybridVectorInput((string name, float[] vector) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vector } });

    /// <summary>
    /// Implicitly converts a tuple of (string, double[]) to a HybridVectorInput.
    /// </summary>
    /// <param name="tuple">The tuple containing a name and a double array vector.</param>
    public static implicit operator HybridVectorInput((string name, double[] vector) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vector } });

    /// <summary>
    /// Implicitly converts a tuple of (string, float[,]) to a HybridVectorInput.
    /// </summary>
    /// <param name="tuple">The tuple containing a name and a float matrix.</param>
    public static implicit operator HybridVectorInput((string name, float[,] vectors) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vectors } });

    /// <summary>
    /// Implicitly converts a tuple of (string, double[,]) to a HybridVectorInput.
    /// </summary>
    /// <param name="tuple">The tuple containing a name and a double matrix.</param>
    public static implicit operator HybridVectorInput((string name, double[,] vectors) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vectors } });
}
