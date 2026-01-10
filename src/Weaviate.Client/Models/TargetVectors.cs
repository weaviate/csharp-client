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
    /// <summary>
    /// Initializes a new instance of the <see cref="TargetVectors"/> class
    /// </summary>
    internal TargetVectors() { } // Prevent external inheritance

    /// <summary>
    /// Factory delegate for creating VectorSearchInput using a builder pattern.
    /// Example: FactoryFn factory = b => b.Sum(("title", vec1), ("desc", vec2))
    /// </summary>
    public delegate TargetVectors FactoryFn(Builder builder);

    /// <summary>
    /// Gets the value of the targets
    /// </summary>
    public abstract IReadOnlyList<string> Targets { get; }

    /// <summary>
    /// Gets the value of the combination
    /// </summary>
    internal abstract V1.CombinationMethod Combination { get; }

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>An enumerator of string</returns>
    public IEnumerator<string> GetEnumerator() => Targets.GetEnumerator();

    /// <summary>
    /// Gets the enumerator
    /// </summary>
    /// <returns>The system collections enumerator</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();

    // Static helpers to build target vectors from VectorSearchInput
    /// <summary>
    /// Unspecifieds the vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Unspecified(AutoArray<string> vectors)
    {
        return new SimpleTargetVectors(vectors?.ToArray() ?? [], V1.CombinationMethod.Unspecified);
    }

    /// <summary>
    /// Sums the vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Sum(VectorSearchInput vectors)
    {
        var targets = vectors.Targets ?? [.. vectors.Vectors.Keys];
        return new SimpleTargetVectors(targets, V1.CombinationMethod.TypeSum);
    }

    /// <summary>
    /// Averages the vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Average(VectorSearchInput vectors)
    {
        var targets = vectors.Targets ?? [.. vectors.Vectors.Keys];
        return new SimpleTargetVectors(targets, V1.CombinationMethod.TypeAverage);
    }

    /// <summary>
    /// Minimums the vectors
    /// </summary>
    /// <param name="vectors">The vectors</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Minimum(VectorSearchInput vectors)
    {
        var targets = vectors.Targets ?? [.. vectors.Vectors.Keys];
        return new SimpleTargetVectors(targets, V1.CombinationMethod.TypeMin);
    }

    // Static helpers for simple target vectors
    /// <summary>
    /// Sums the targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Sum(params string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.TypeSum);

    /// <summary>
    /// Averages the targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Average(params string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.TypeAverage);

    /// <summary>
    /// Minimums the targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The target vectors</returns>
    public static TargetVectors Minimum(params string[] targets) =>
        new SimpleTargetVectors(targets, V1.CombinationMethod.TypeMin);

    // Static helpers for weighted target vectors
    // Supports multiple weights per target (e.g., ManualWeights(("a", 1.0), ("a", 2.0)))
    /// <summary>
    /// Manuals the weights using the specified weights
    /// </summary>
    /// <param name="weights">The weights</param>
    /// <returns>The target vectors</returns>
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

    /// <summary>
    /// Relatives the score using the specified weights
    /// </summary>
    /// <param name="weights">The weights</param>
    /// <returns>The target vectors</returns>
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
        /// <summary>
        /// Initializes a new instance of the <see cref="Builder"/> class
        /// </summary>
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
    /// <summary>
    /// Initializes a new instance of the <see cref="SimpleTargetVectors"/> class
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <param name="combination">The combination</param>
    internal SimpleTargetVectors(
        IReadOnlyList<string> targets,
        V1.CombinationMethod combination = V1.CombinationMethod.Unspecified
    )
    {
        Targets = targets;
        Combination = combination;
    }

    /// <summary>
    /// Gets the value of the targets
    /// </summary>
    public override IReadOnlyList<string> Targets { get; }

    /// <summary>
    /// Gets the value of the combination
    /// </summary>
    internal override V1.CombinationMethod Combination { get; }
}

/// <summary>
/// Weighted target vectors with per-target weights (ManualWeights, RelativeScore).
/// </summary>
public sealed record WeightedTargetVectors : TargetVectors
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedTargetVectors"/> class
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <param name="combination">The combination</param>
    /// <param name="weights">The weights</param>
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

    /// <summary>
    /// Gets the value of the targets
    /// </summary>
    public override IReadOnlyList<string> Targets { get; }

    /// <summary>
    /// Gets the value of the combination
    /// </summary>
    internal override V1.CombinationMethod Combination { get; }

    /// <summary>
    /// Gets the value of the weights
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<double>> Weights { get; }

    /// <summary>
    /// Gets the target with weights
    /// </summary>
    /// <returns>An enumerable of string name and double weight</returns>
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
