namespace Weaviate.Client.Grpc.Protobuf.V1;

internal partial class AggregateReply
{
    public string Collection { get; set; } = string.Empty;
}

internal partial class SearchReply
{
    public string Collection { get; set; } = string.Empty;

    public static implicit operator Client.Models.GroupByResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildGroupByResult(reply);

    public static implicit operator global::Weaviate.Client.Models.WeaviateResult(
        SearchReply reply
    ) => Client.Grpc.WeaviateGrpcClient.BuildResult(reply);

    public static implicit operator Client.Models.GenerativeGroupByResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildGenerativeGroupByResult(reply);

    public static implicit operator Client.Models.GenerativeWeaviateResult(SearchReply reply) =>
        Client.Grpc.WeaviateGrpcClient.BuildGenerativeResult(reply);
}

internal partial class Targets
{
    private static Targets ToGrpcTargets(Client.Models.TargetVectors targetVectors)
    {
        List<string> targets = new(
            (targetVectors.Weights?.Count ?? 0) == 0 ? targetVectors.Targets : []
        );

        List<WeightsForTarget> weightsForTargets = new();
        foreach (var w in targetVectors.Weights ?? [])
        {
            foreach (var v in w.Value)
            {
                weightsForTargets.Add(
                    new WeightsForTarget { Target = w.Key, Weight = Convert.ToSingle(v) }
                );
                targets.Add(w.Key);
            }
        }

        var grpcTargets = new Targets
        {
            TargetVectors = { targets.Order() },
            WeightsForTargets = { weightsForTargets.OrderBy(wft => wft.Target) },
            Combination = targetVectors.Combination,
        };

        return grpcTargets;
    }

    public static implicit operator Targets(Client.Models.TargetVectors targetVectors)
    {
        return ToGrpcTargets(targetVectors);
    }
}
