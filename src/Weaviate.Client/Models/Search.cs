using System.Collections;

namespace Weaviate.Client.Models;

public record NearVectorInput(
    VectorSearchInput Vector,
    float? Certainty = null,
    float? Distance = null
)
{
    public static implicit operator NearVectorInput(VectorSearchInput vectors) => new(vectors);
};

public record NearTextInput(
    AutoArray<string> Query,
    float? Certainty = null,
    float? Distance = null,
    Move? MoveTo = null,
    Move? MoveAway = null,
    TargetVectors? TargetVectors = null
)
{
    /// <summary>
    /// Constructor overload accepting lambda builder for target vectors.
    /// </summary>
    public NearTextInput(
        AutoArray<string> Query,
        Func<TargetVectorsBuilder, TargetVectors> targetVectorsBuilder,
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
            TargetVectors: targetVectorsBuilder(new TargetVectorsBuilder())
        ) { }

    public static implicit operator NearTextInput(string query) => new(Query: query);
};

public abstract record BM25Operator(string Operator)
{
    public record And() : BM25Operator("And");

    public record Or(int MinimumMatch) : BM25Operator("Or");
}
