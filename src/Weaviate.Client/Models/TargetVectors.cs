namespace Weaviate.Client.Models;

public class TargetVectors : IEnumerable<string>
{
    public List<string> Targets { get; } = new List<string>();
    internal V1.CombinationMethod Combination { get; } = V1.CombinationMethod.Unspecified;
    public Dictionary<string, List<double>>? Weights { get; }

    public TargetVectors() { }

    public void Add(string target)
    {
        Targets.Add(target);
    }

    public void AddRange(IEnumerable<string> targets)
    {
        Targets.AddRange(targets);
    }

    public IEnumerator<string> GetEnumerator()
    {
        return Targets.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private TargetVectors(IEnumerable<string> targets)
    {
        Targets.AddRange(targets);
    }

    private TargetVectors(
        IEnumerable<string>? targets = null,
        V1.CombinationMethod combination = V1.CombinationMethod.Unspecified,
        Dictionary<string, List<double>>? weights = null
    )
    {
        Targets = new(targets ?? weights?.Keys ?? Enumerable.Empty<string>());
        Combination = combination;
        Weights = weights;
    }

    // Implicit conversion from string[]
    public static implicit operator TargetVectors(string[] names) => new TargetVectors(names);

    // Implicit conversion from List<string>
    public static implicit operator TargetVectors(List<string> names) => new TargetVectors(names);

    // Sum
    public static TargetVectors Sum(IEnumerable<string> names) =>
        new TargetVectors(names, V1.CombinationMethod.TypeSum);

    // Minimum
    public static TargetVectors Minimum(IEnumerable<string> names) =>
        new TargetVectors(names, V1.CombinationMethod.TypeMin);

    // Average
    public static TargetVectors Average(IEnumerable<string> names) =>
        new TargetVectors(names, V1.CombinationMethod.TypeAverage);

    // ManualWeights
    public static TargetVectors ManualWeights(
        params (string name, OneOrManyOf<double> weight)[] weights
    )
    {
        var dict = weights.ToDictionary(w => w.name, w => w.weight.ToList());

        return new TargetVectors(dict.Keys, V1.CombinationMethod.TypeManual, dict);
    }

    // RelativeScore
    public static TargetVectors RelativeScore(
        params (string name, OneOrManyOf<double> weight)[] weights
    )
    {
        var dict = weights.ToDictionary(w => w.name, w => w.weight.ToList());

        return new TargetVectors(dict.Keys, V1.CombinationMethod.TypeRelativeScore, dict);
    }
}
