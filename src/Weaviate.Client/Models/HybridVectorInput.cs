namespace Weaviate.Client.Models;

/// <summary>
/// Discriminated union for hybrid search vector inputs.
/// Can hold exactly one of: VectorSearchInput, NearTextInput, or NearVectorInput.
/// </summary>
public sealed class HybridVectorInput
{
    public VectorSearchInput? VectorSearch { get; }
    public NearTextInput? NearText { get; }
    public NearVectorInput? NearVector { get; }

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

    public static implicit operator HybridVectorInput(VectorSearchInput vectorSearch) =>
        FromVectorSearch(vectorSearch);

    public static implicit operator HybridVectorInput(NearTextInput nearText) =>
        FromNearText(nearText);

    public static implicit operator HybridVectorInput(NearVectorInput nearVector) =>
        FromNearVector(nearVector);

    public static implicit operator HybridVectorInput(float[] vector) => FromVectorSearch(vector);

    public static implicit operator HybridVectorInput(double[] vector) => FromVectorSearch(vector);

    public static implicit operator HybridVectorInput(Vectors vectors) => FromVectorSearch(vectors);

    public static implicit operator HybridVectorInput(Vector vector) =>
        FromVectorSearch(new Vectors(vector));

    public static implicit operator HybridVectorInput(NamedVector namedVector) =>
        FromVectorSearch(namedVector);

    public static implicit operator HybridVectorInput(string query) =>
        FromNearText(new NearTextInput(query));

    public static implicit operator HybridVectorInput((string name, float[] vector) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vector } });

    public static implicit operator HybridVectorInput((string name, double[] vector) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vector } });

    public static implicit operator HybridVectorInput((string name, float[,] vectors) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vectors } });

    public static implicit operator HybridVectorInput((string name, double[,] vectors) tuple) =>
        FromVectorSearch(new VectorSearchInput { { tuple.name, tuple.vectors } });
}
