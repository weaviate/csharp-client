namespace Weaviate.Client.Models;

public interface IHybridVectorInput
{
    // This interface is used to mark hybrid vectors, which can be either near vector or near text.
    // It allows for polymorphic behavior in the Hybrid methods.
}

public record HybridNearVector(
    Models.Vectors? Vector,
    float? Certainty = null,
    float? Distance = null,
    TargetVectors? targetVector = null
) : IHybridVectorInput { };

public record HybridNearText(
    string Query,
    float? Certainty = null,
    float? Distance = null,
    Move? MoveTo = null,
    Move? MoveAway = null
) : IHybridVectorInput;

public abstract record BM25Operator(string Operator)
{
    public record And() : BM25Operator("And");

    public record Or(int MinimumMatch) : BM25Operator("Or");
}
