using Weaviate.Client.Internal;

namespace Weaviate.Client.Models;

/// <summary>
/// Represents input for a near-vector search, including vector, certainty, and distance.
/// </summary>
public record NearVectorInput(
    VectorSearchInput Vector,
    float? Certainty = null,
    float? Distance = null
)
{
    /// <summary>
    /// Delegate for lambda builder pattern with NearVectorInput.
    /// </summary>
    public delegate NearVectorInput FactoryFn(NearVectorInputBuilder builder);

    /// <summary>
    /// Constructor overload accepting lambda builder for vector input.
    /// </summary>
    public NearVectorInput(
        VectorSearchInput.FactoryFn Vector,
        float? Certainty = null,
        float? Distance = null
    )
        : this(
            Vector: Vector(new VectorSearchInput.Builder()),
            Certainty: Certainty,
            Distance: Distance
        ) { }

    /// <summary>
    /// Implicitly converts VectorSearchInput to NearVectorInput
    /// </summary>
    /// <param name="vectors">The vector search input</param>
    public static implicit operator NearVectorInput(VectorSearchInput vectors) => new(vectors);
};

/// <summary>
/// Represents input for a near-text search, including concepts, certainty, distance, and move parameters.
/// </summary>
public record NearTextInput(
    AutoArray<string> Query,
    TargetVectors? TargetVectors = null,
    float? Certainty = null,
    float? Distance = null,
    Move? MoveTo = null,
    Move? MoveAway = null
)
{
    /// <summary>
    /// Delegate for lambda builder pattern with NearTextInput.
    /// </summary>
    public delegate NearTextInput FactoryFn(NearTextInputBuilder builder);

    /// <summary>
    /// Constructor overload accepting lambda builder for target vectors.
    /// </summary>
    public NearTextInput(
        AutoArray<string> Query,
        TargetVectors.FactoryFn TargetVectors,
        float? Certainty = null,
        float? Distance = null,
        Move? MoveTo = null,
        Move? MoveAway = null
    )
        : this(
            Query: Query,
            Certainty: Certainty,
            Distance: Distance,
            MoveTo: MoveTo,
            MoveAway: MoveAway,
            TargetVectors: TargetVectors(new TargetVectors.Builder())
        ) { }
};

/// <summary>
/// The bm 25 operator
/// </summary>
public abstract record BM25Operator(string Operator)
{
    /// <summary>
    /// The and
    /// </summary>
    public record And() : BM25Operator("And");

    /// <summary>
    /// The or
    /// </summary>
    public record Or(int MinimumMatch) : BM25Operator("Or");
}
