namespace Weaviate.Client.Models;

using System.Runtime.CompilerServices;
using V1 = Grpc.Protobuf.V1;

/// <summary>
/// Base class for target vector configuration for text/media-based searches.
/// Cannot be constructed directly - use lambda syntax with builder methods.
/// </summary>
[CollectionBuilder(typeof(TargetVectors), nameof(Create))]
public abstract record TargetVectors : IEnumerable<string>
{
    internal TargetVectors() { } // Prevent external inheritance

    /// <summary>
    /// Factory delegate for creating VectorSearchInput using a builder pattern.
    /// Example: FactoryFn factory = b => b.Sum(("title", vec1), ("desc", vec2))
    /// </summary>
    public delegate TargetVectors FactoryFn(Builder builder);

    public abstract IReadOnlyList<string> Targets { get; }
    internal abstract V1.CombinationMethod Combination { get; }

    public IEnumerator<string> GetEnumerator() => Targets.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();

    // Static helpers to build target vectors from VectorSearchInput
    public static TargetVectors Sum(VectorSearchInput vectors)
    {
        var targets = vectors.Targets ?? [.. vectors.Vectors.Keys];
        return new SimpleTargetVectors(targets, V1.CombinationMethod.TypeSum);
    }

    public static TargetVectors Average(VectorSearchInput vectors)
    {
        var targets = vectors.Targets ?? [.. vectors.Vectors.Keys];
        return new SimpleTargetVectors(targets, V1.CombinationMethod.TypeAverage);
    }

    public static TargetVectors Minimum(VectorSearchInput vectors)
    {
        var targets = vectors.Targets ?? [.. vectors.Vectors.Keys];
        return new SimpleTargetVectors(targets, V1.CombinationMethod.TypeMin);
    }

    // Static helpers for simple target vectors
    public static TargetVectors Sum(params string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.TypeSum);

    public static TargetVectors Average(params string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.TypeAverage);

    public static TargetVectors Minimum(params string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.TypeMin);

    // Static helpers for weighted target vectors
    // Supports multiple weights per target (e.g., ManualWeights(("a", 1.0), ("a", 2.0)))
    public static TargetVectors ManualWeights(params (string name, double weight)[] weights)
    {
        var dict = weights
            .GroupBy(w => w.name)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<double>)g.Select(w => w.weight).ToList());
        return new WeightedTargetVectors(
            targets: weights.Select(w => w.name).Distinct().ToList(),
            combination: V1.CombinationMethod.TypeManual,
            weights: dict
        );
    }

    public static TargetVectors RelativeScore(params (string name, double weight)[] weights)
    {
        var dict = weights
            .GroupBy(w => w.name)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<double>)g.Select(w => w.weight).ToList());
        return new WeightedTargetVectors(
            targets: weights.Select(w => w.name).Distinct().ToList(),
            combination: V1.CombinationMethod.TypeRelativeScore,
            weights: dict
        );
    }

    // Implicit conversion from string array for convenience
    public static implicit operator TargetVectors(string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.Unspecified);

    /// <summary>
    /// Creates a TargetVectors from a collection expression.
    /// Enables syntax like: targets: ["title", "description"]
    /// </summary>
    public static TargetVectors Create(ReadOnlySpan<string> targets) =>
        new SimpleTargetVectors(targets.ToArray(), V1.CombinationMethod.Unspecified);

    /// <summary>
    /// Builder for creating TargetVectors via lambda syntax.
    /// </summary>
    public sealed class Builder
    {
        internal Builder() { }

        /// <summary>
        /// Specifies target vector names without a combination method.
        /// </summary>
        public SimpleTargetVectors Targets(params string[] names)
        {
            return new SimpleTargetVectors(
                targets: names,
                combination: V1.CombinationMethod.Unspecified
            );
        }

        /// <summary>
        /// Creates a multi-target configuration with Sum combination.
        /// </summary>
        public SimpleTargetVectors Sum(params string[] names)
        {
            return new SimpleTargetVectors(
                targets: names,
                combination: V1.CombinationMethod.TypeSum
            );
        }

        /// <summary>
        /// Creates a multi-target configuration with Average combination.
        /// </summary>
        public SimpleTargetVectors Average(params string[] names)
        {
            return new SimpleTargetVectors(
                targets: names,
                combination: V1.CombinationMethod.TypeAverage
            );
        }

        /// <summary>
        /// Creates a multi-target configuration with Minimum combination.
        /// </summary>
        public SimpleTargetVectors Minimum(params string[] names)
        {
            return new SimpleTargetVectors(
                targets: names,
                combination: V1.CombinationMethod.TypeMin
            );
        }

        /// <summary>
        /// Creates a multi-target configuration with ManualWeights.
        /// Supports multiple weights per target.
        /// </summary>
        public WeightedTargetVectors ManualWeights(params (string name, double weight)[] weights)
        {
            var dict = weights
                .GroupBy(w => w.name)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<double>)g.Select(w => w.weight).ToList()
                );
            return new WeightedTargetVectors(
                targets: weights.Select(w => w.name).Distinct().ToList(),
                combination: V1.CombinationMethod.TypeManual,
                weights: dict
            );
        }

        /// <summary>
        /// Creates a multi-target configuration with RelativeScore.
        /// Supports multiple weights per target.
        /// </summary>
        public WeightedTargetVectors RelativeScore(params (string name, double weight)[] weights)
        {
            var dict = weights
                .GroupBy(w => w.name)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<double>)g.Select(w => w.weight).ToList()
                );
            return new WeightedTargetVectors(
                targets: weights.Select(w => w.name).Distinct().ToList(),
                combination: V1.CombinationMethod.TypeRelativeScore,
                weights: dict
            );
        }
    }
}

/// <summary>
/// Simple target vectors without weights (Sum, Average, Minimum, Targets).
/// </summary>
public sealed record SimpleTargetVectors : TargetVectors
{
    internal SimpleTargetVectors(
        IReadOnlyList<string> targets,
        V1.CombinationMethod combination = V1.CombinationMethod.Unspecified
    )
    {
        Targets = targets;
        Combination = combination;
    }

    public override IReadOnlyList<string> Targets { get; }
    internal override V1.CombinationMethod Combination { get; }
}

/// <summary>
/// Weighted target vectors with per-target weights (ManualWeights, RelativeScore).
/// </summary>
public sealed record WeightedTargetVectors : TargetVectors
{
    internal WeightedTargetVectors(
        IReadOnlyList<string> targets,
        V1.CombinationMethod combination,
        IReadOnlyDictionary<string, IReadOnlyList<double>> weights
    )
    {
        Targets = targets;
        Combination = combination;
        Weights = weights;
    }

    public override IReadOnlyList<string> Targets { get; }
    internal override V1.CombinationMethod Combination { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<double>> Weights { get; }

    internal IEnumerable<(string name, double weight)> GetTargetWithWeights()
    {
        foreach (var target in Targets)
        {
            if (Weights.TryGetValue(target, out var weightList))
            {
                foreach (var weight in weightList)
                    yield return (target, weight);
            }
        }
    }
}
