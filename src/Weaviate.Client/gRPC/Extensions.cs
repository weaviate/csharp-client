using System.Collections.Generic;

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
            Combination = targetVectors.Combination switch
            {
                Client.Models.CombinationMethod.Unspecified => CombinationMethod.Unspecified,
                Client.Models.CombinationMethod.Sum => CombinationMethod.TypeSum,
                Client.Models.CombinationMethod.Minimum => CombinationMethod.TypeMin,
                Client.Models.CombinationMethod.Average => CombinationMethod.TypeAverage,
                Client.Models.CombinationMethod.ManualWeights => CombinationMethod.TypeManual,
                Client.Models.CombinationMethod.RelativeScore =>
                    CombinationMethod.TypeRelativeScore,
                _ => throw new NotSupportedException(
                    $"Combination method {targetVectors.Combination} is not supported."
                ),
            },
        };

        return grpcTargets;
    }

    public static implicit operator Targets(Client.Models.TargetVectors targetVectors)
    {
        return ToGrpcTargets(targetVectors);
    }
}
