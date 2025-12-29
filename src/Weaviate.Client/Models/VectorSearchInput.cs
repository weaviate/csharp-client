namespace Weaviate.Client.Models;

using V1 = Grpc.Protobuf.V1;

/// <summary>
/// Represents a vector search input with optional target vectors and combination strategy.
/// Can be constructed using collection expression syntax: [namedVector1, namedVector2, ...]
/// </summary>
public sealed class VectorSearchInput : IEnumerable<NamedVector>
{
    private readonly List<NamedVector> _vectors;

    /// <summary>
    /// Initializes a new instance of VectorSearchInput. Supports collection initializer syntax.
    /// </summary>
    public VectorSearchInput()
    {
        _vectors = [];
    }

    internal VectorSearchInput(
        IEnumerable<NamedVector> vectors,
        IReadOnlyList<string>? targets = null,
        V1.CombinationMethod combination = V1.CombinationMethod.Unspecified,
        IReadOnlyDictionary<string, IReadOnlyList<double>>? weights = null
    )
    {
        _vectors = [.. vectors];
        Targets = targets;
        Combination = combination;
        Weights = weights;
    }

    public IReadOnlyDictionary<string, NamedVector[]> Vectors =>
        _vectors.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.ToArray());

    public IReadOnlyList<string>? Targets { get; }
    internal V1.CombinationMethod Combination { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<double>>? Weights { get; }

    public IEnumerator<NamedVector> GetEnumerator() => _vectors.GetEnumerator();

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <summary>
    /// Adds named vectors to the collection. Supports collection initializer syntax.
    /// </summary>
    public void Add(string name, params Vector[] values) =>
        _vectors.AddRange(values.Select(v => new NamedVector(name, v)));

    /// <summary>
    /// Adds a Vectors collection. Supports collection initializer syntax.
    /// </summary>
    public void Add(Vectors vectors)
    {
        _vectors.AddRange(vectors.Select(v => new NamedVector(v.Key, v.Value)));
    }

    internal IEnumerable<(string name, double? weight)> GetVectorWithWeights()
    {
        var targets = Targets ?? Vectors.Keys.ToList();
        foreach (var target in targets)
        {
            if (Weights?.TryGetValue(target, out var weightList) ?? false)
            {
                foreach (var weight in weightList)
                    yield return (target, weight);
            }
            else
            {
                yield return (target, null);
            }
        }
    }

    // Implicit conversions to reduce API overload count

    /// <summary>
    /// Implicit conversion from float array to VectorSearchInput (creates single unnamed "default" vector)
    /// </summary>
    public static implicit operator VectorSearchInput(float[] values) =>
        new([new NamedVector(values)]);

    /// <summary>
    /// Implicit conversion from double array to VectorSearchInput (creates single unnamed "default" vector)
    /// </summary>
    public static implicit operator VectorSearchInput(double[] values) =>
        new([new NamedVector(values)]);

    /// <summary>
    /// Implicit conversion from Vector to VectorSearchInput (creates single unnamed "default" vector)
    /// </summary>
    public static implicit operator VectorSearchInput(Vector vector) =>
        new([new NamedVector(vector)]);

    /// <summary>
    /// Implicit conversion from Vectors to VectorSearchInput (creates multiple named vectors)
    /// </summary>
    public static implicit operator VectorSearchInput(Vectors vectors)
    {
        return new VectorSearchInput() { vectors };
    }

    /// <summary>
    /// Implicit conversion from NamedVector to VectorSearchInput (creates single named vector)
    /// </summary>
    public static implicit operator VectorSearchInput(NamedVector vector) => new([vector]);

    /// <summary>
    /// Implicit conversion from NamedVector array to VectorSearchInput (creates multiple named vectors)
    /// </summary>
    public static implicit operator VectorSearchInput(NamedVector[] vectors) => new(vectors);

    /// <summary>
    /// Implicit conversion from Dictionary of Vector arrays to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(Dictionary<string, Vector[]> vectors) =>
        new(vectors: vectors.SelectMany(kvp => kvp.Value.Select(v => new NamedVector(kvp.Key, v))));

    /// <summary>
    /// Implicit conversion from Dictionary of float arrays to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(Dictionary<string, float[]> vectors) =>
        new(vectors: vectors.Select(kvp => new NamedVector(kvp.Key, kvp.Value)));

    /// <summary>
    /// Implicit conversion from Dictionary of double arrays to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(Dictionary<string, double[]> vectors) =>
        new(vectors: vectors.Select(kvp => new NamedVector(kvp.Key, kvp.Value)));

    /// <summary>
    /// Implicit conversion from Dictionary of 2D float arrays to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(Dictionary<string, float[,]> vectors) =>
        new(vectors: vectors.Select(kvp => new NamedVector(kvp.Key, kvp.Value)));

    /// <summary>
    /// Implicit conversion from Dictionary of 2D double arrays to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(Dictionary<string, double[,]> vectors) =>
        new(vectors: vectors.Select(kvp => new NamedVector(kvp.Key, kvp.Value)));

    /// <summary>
    /// Implicit conversion from Dictionary of float array enumerables to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(
        Dictionary<string, IEnumerable<float[]>> vectors
    ) =>
        new(vectors: vectors.SelectMany(kvp => kvp.Value.Select(v => new NamedVector(kvp.Key, v))));

    /// <summary>
    /// Implicit conversion from Dictionary of double array enumerables to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(
        Dictionary<string, IEnumerable<double[]>> vectors
    ) =>
        new(vectors: vectors.SelectMany(kvp => kvp.Value.Select(v => new NamedVector(kvp.Key, v))));

    /// <summary>
    /// Implicit conversion from Dictionary of 2D float array enumerables to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(
        Dictionary<string, IEnumerable<float[,]>> vectors
    ) =>
        new(vectors: vectors.SelectMany(kvp => kvp.Value.Select(v => new NamedVector(kvp.Key, v))));

    /// <summary>
    /// Implicit conversion from Dictionary of 2D double array enumerables to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(
        Dictionary<string, IEnumerable<double[,]>> vectors
    ) =>
        new(vectors: vectors.SelectMany(kvp => kvp.Value.Select(v => new NamedVector(kvp.Key, v))));

    /// <summary>
    /// Implicit conversion from tuple of (name, float[]) to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput((string name, float[] vector) tuple) =>
        new([new NamedVector(tuple.name, tuple.vector)]);

    /// <summary>
    /// Implicit conversion from tuple of (name, double[]) to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput((string name, double[] vector) tuple) =>
        new([new NamedVector(tuple.name, tuple.vector)]);

    /// <summary>
    /// Implicit conversion from tuple of (name, float[,]) to VectorSearchInput (for multi-vector/ColBERT)
    /// </summary>
    public static implicit operator VectorSearchInput((string name, float[,] vectors) tuple) =>
        new([new NamedVector(tuple.name, tuple.vectors)]);

    /// <summary>
    /// Implicit conversion from tuple of (name, double[,]) to VectorSearchInput (for multi-vector/ColBERT)
    /// </summary>
    public static implicit operator VectorSearchInput((string name, double[,] vectors) tuple) =>
        new([new NamedVector(tuple.name, tuple.vectors)]);

    /// <summary>
    /// Implicit conversion from FactoryFn to VectorSearchInput
    /// </summary>
    public static implicit operator VectorSearchInput(FactoryFn factory) => factory(new Builder());

    /// <summary>
    /// Builder for creating VectorSearchInput with multi-target combinations via lambda syntax.
    /// </summary>
    public sealed class Builder
    {
        internal Builder() { }

        /// <summary>
        /// Creates a multi-target query with Sum combination.
        /// </summary>
        public VectorSearchInput Sum(params (string name, Vector vector)[] vectors)
        {
            var vectorList = vectors.Select(v => new NamedVector(v.name, v.vector)).ToList();
            var targets = vectors.Select(v => v.name).Distinct().ToList();

            return new VectorSearchInput(
                vectors: vectorList,
                targets: targets,
                combination: V1.CombinationMethod.TypeSum
            );
        }

        /// <summary>
        /// Creates a multi-target query with Average combination.
        /// </summary>
        public VectorSearchInput Average(params (string name, Vector vector)[] vectors)
        {
            var vectorList = vectors.Select(v => new NamedVector(v.name, v.vector)).ToList();
            var targets = vectors.Select(v => v.name).Distinct().ToList();

            return new VectorSearchInput(
                vectors: vectorList,
                targets: targets,
                combination: V1.CombinationMethod.TypeAverage
            );
        }

        /// <summary>
        /// Creates a multi-target query with Minimum combination.
        /// </summary>
        public VectorSearchInput Minimum(params (string name, Vector vector)[] vectors)
        {
            var vectorList = vectors.Select(v => new NamedVector(v.name, v.vector)).ToList();
            var targets = vectors.Select(v => v.name).Distinct().ToList();

            return new VectorSearchInput(
                vectors: vectorList,
                targets: targets,
                combination: V1.CombinationMethod.TypeMin
            );
        }

        /// <summary>
        /// Creates a multi-target query with ManualWeights combination.
        /// Weight comes before the vector in each tuple.
        /// </summary>
        public VectorSearchInput ManualWeights(
            params (string name, double weight, Vector vector)[] entries
        )
        {
            var vectorList = entries.Select(e => new NamedVector(e.name, e.vector));
            var targets = entries.Select(e => e.name).Distinct().ToList();
            var weights = entries
                .GroupBy(e => e.name)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<double>)g.Select(e => e.weight).ToList()
                );

            return new VectorSearchInput(
                vectors: vectorList,
                targets: targets,
                combination: V1.CombinationMethod.TypeManual,
                weights: weights
            );
        }

        /// <summary>
        /// Creates a multi-target query with RelativeScore combination.
        /// Weight comes before the vector in each tuple.
        /// </summary>
        public VectorSearchInput RelativeScore(
            params (string name, double weight, Vector vector)[] entries
        )
        {
            var vectorList = entries.Select(e => new NamedVector(e.name, e.vector)).ToList();
            var targets = entries.Select(e => e.name).Distinct().ToList();
            var weights = entries
                .GroupBy(e => e.name)
                .ToDictionary(
                    g => g.Key,
                    g => (IReadOnlyList<double>)g.Select(e => e.weight).ToList()
                );

            return new VectorSearchInput(
                vectors: vectorList,
                targets: targets,
                combination: V1.CombinationMethod.TypeRelativeScore,
                weights: weights
            );
        }
    }

    /// <summary>
    /// Factory delegate for creating VectorSearchInput using a builder pattern.
    /// Example: FactoryFn factory = b => b.Sum(("title", vec1), ("desc", vec2))
    /// </summary>
    public delegate VectorSearchInput FactoryFn(Builder builder);

    public static class CollectionBuilder
    {
        public static VectorSearchInput Create(ReadOnlySpan<NamedVector> items)
        {
            return new VectorSearchInput(items.ToArray());
        }
    }

    /// <summary>
    /// Combines a TargetVectors configuration with named vectors to create a VectorSearchInput.
    /// Supports multiple vectors per target (weights matched by order).
    /// </summary>
    public static VectorSearchInput Combine(
        TargetVectors targetVectors,
        params (string name, Vector vector)[] vectors
    )
    {
        var namedVectors = vectors.Select(v => new NamedVector(v.name, v.vector));
        return Combine(targetVectors, namedVectors);
    }

    /// <summary>
    /// Combines a TargetVectors configuration with a Vectors collection.
    /// Note: Vectors only supports one vector per target name.
    /// </summary>
    public static VectorSearchInput Combine(TargetVectors targetVectors, Vectors vectors)
    {
        var namedVectors = vectors.Select(kvp => new NamedVector(kvp.Key, kvp.Value));
        return Combine(targetVectors, namedVectors);
    }

    /// <summary>
    /// Combines a TargetVectors configuration with an enumerable of NamedVectors.
    /// Supports multiple vectors per target (weights matched by order).
    /// </summary>
    public static VectorSearchInput Combine(
        TargetVectors targetVectors,
        IEnumerable<NamedVector> vectors
    )
    {
        var targets = targetVectors.Targets.ToList();

        IReadOnlyDictionary<string, IReadOnlyList<double>>? weights = null;
        if (targetVectors is WeightedTargetVectors weighted)
        {
            weights = weighted.Weights;
        }

        return new VectorSearchInput(
            vectors: vectors,
            targets: targets,
            combination: targetVectors.Combination,
            weights: weights
        );
    }
}
