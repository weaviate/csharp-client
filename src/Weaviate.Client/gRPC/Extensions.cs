namespace Weaviate.Client.Grpc.Protobuf.V1;

/// <summary>
/// The aggregate reply class
/// </summary>
internal partial class AggregateReply
{
    /// <summary>
    /// Gets or sets the value of the collection
    /// </summary>
    public string Collection { get; set; } = string.Empty;
}

/// <summary>
/// The search reply class
/// </summary>
internal partial class SearchReply
{
    /// <summary>
    /// Gets or sets the value of the collection
    /// </summary>
    public string Collection { get; set; } = string.Empty;

    public static implicit operator Models.GroupByResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildGroupByResult(reply);

    public static implicit operator Models.WeaviateResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildResult(reply);

    public static implicit operator Models.GenerativeGroupByResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildGenerativeGroupByResult(reply);

    public static implicit operator Models.GenerativeWeaviateResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildGenerativeResult(reply);
}

/// <summary>
/// The targets class
/// </summary>
internal partial class Targets
{
    /// <summary>
    /// Returns the grpc targets using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The grpc targets</returns>
    private static Targets ToGrpcTargets(Models.TargetVectors targetVectors)
    {
        List<string> targets;
        List<WeightsForTarget> weightsForTargets = new();

        // Handle SimpleTargetVectors vs WeightedTargetVectors
        if (targetVectors is Models.WeightedTargetVectors weighted)
        {
            // For weighted targets, build weights list
            targets = new();
            foreach (var w in weighted.Weights)
            {
                foreach (var v in w.Value)
                {
                    weightsForTargets.Add(
                        new WeightsForTarget { Target = w.Key, Weight = Convert.ToSingle(v) }
                    );
                    targets.Add(w.Key);
                }
            }
        }
        else
        {
            // For simple targets, just use the target names
            targets = new(targetVectors.Targets);
        }

        var grpcTargets = new Targets
        {
            TargetVectors = { targets.Order() },
            WeightsForTargets = { weightsForTargets.OrderBy(wft => wft.Target) },
            Combination = targetVectors.Combination,
        };

        return grpcTargets;
    }

    public static implicit operator Targets(Models.TargetVectors targetVectors)
    {
        return ToGrpcTargets(targetVectors);
    }
}
