using System.Collections;

namespace Weaviate.Client.Models;

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

    public static implicit operator NearVectorInput(VectorSearchInput vectors) => new(vectors);
};

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

public abstract record BM25Operator(string Operator)
{
    public record And() : BM25Operator("And");

    public record Or(int MinimumMatch) : BM25Operator("Or");
}
