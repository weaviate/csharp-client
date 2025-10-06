namespace Weaviate.V1;

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
            TargetVectors = { targets },
            WeightsForTargets = { weightsForTargets },
            Combination = targetVectors.Combination,
        };

        return grpcTargets;
    }

    public static implicit operator Targets(Client.Models.TargetVectors targetVectors)
    {
        return ToGrpcTargets(targetVectors);
    }
}
