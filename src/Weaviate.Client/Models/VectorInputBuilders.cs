namespace Weaviate.Client.Models;

// ============================================================================
// NearVector Builder Infrastructure
// ============================================================================

/// <summary>
/// Delegate for creating a NearVector builder. The builder is directly callable
/// to set certainty and distance parameters, then chainable to configure target vectors.
/// </summary>
/// <example>
/// v => v(certainty: 0.8).ManualWeights(
///     ("title", 1.2, new[] { 1f, 2f }),
///     ("description", 0.8, new[] { 3f, 4f })
/// )
/// </example>
public delegate INearVectorBuilder NearVectorInputBuilder(
    float? certainty = null,
    float? distance = null
);

/// <summary>
/// Builder interface for creating NearVectorInput with integrated target vectors.
/// </summary>
public interface INearVectorBuilder
{
    /// <summary>
    /// Creates a NearVectorInput with manually weighted target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight, vector)</param>
    NearVectorInput ManualWeights(params (string Name, double Weight, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput that sums all target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, vector)</param>
    NearVectorInput Sum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput that averages all target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, vector)</param>
    NearVectorInput Average(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput using minimum combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, vector)</param>
    NearVectorInput Minimum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput using relative score combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight, vector)</param>
    NearVectorInput RelativeScore(params (string Name, double Weight, Vector Vector)[] targets);
}

/// <summary>
/// Internal implementation of INearVectorBuilder.
/// </summary>
internal sealed class NearVectorBuilder : INearVectorBuilder
{
    private readonly float? _certainty;
    private readonly float? _distance;

    public NearVectorBuilder(float? certainty = null, float? distance = null)
    {
        _certainty = certainty;
        _distance = distance;
    }

    public NearVectorInput ManualWeights(
        params (string Name, double Weight, Vector Vector)[] targets
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.ManualWeights(
            targets.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    public NearVectorInput Sum(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.Sum(targets.Select(t => (t.Name, t.Vector)).ToArray());
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    public NearVectorInput Average(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.Average(targets.Select(t => (t.Name, t.Vector)).ToArray());
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    public NearVectorInput Minimum(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.Minimum(targets.Select(t => (t.Name, t.Vector)).ToArray());
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    public NearVectorInput RelativeScore(
        params (string Name, double Weight, Vector Vector)[] targets
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.RelativeScore(
            targets.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }
}

// ============================================================================
// NearText Builder Infrastructure
// ============================================================================

/// <summary>
/// Delegate for creating a NearText builder. The builder is directly callable
/// to set query text and optional parameters, then chainable to configure target vectors.
/// </summary>
/// <example>
/// v => v(["concept1", "concept2"]).ManualWeights(
///     ("title", 1.2),
///     ("description", 0.8)
/// )
/// </example>
public delegate INearTextBuilder NearTextInputBuilder(
    AutoArray<string> query,
    float? certainty = null,
    float? distance = null,
    Move? moveTo = null,
    Move? moveAway = null
);

/// <summary>
/// Builder interface for creating NearTextInput with integrated target vectors.
/// </summary>
public interface INearTextBuilder
{
    /// <summary>
    /// Creates a NearTextInput with manually weighted target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight)</param>
    NearTextInput ManualWeights(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Creates a NearTextInput that sums all target vectors.
    /// </summary>
    /// <param name="targetNames">Names of target vectors</param>
    NearTextInput Sum(params string[] targetNames);

    /// <summary>
    /// Creates a NearTextInput that averages all target vectors.
    /// </summary>
    /// <param name="targetNames">Names of target vectors</param>
    NearTextInput Average(params string[] targetNames);

    /// <summary>
    /// Creates a NearTextInput using minimum combination of target vectors.
    /// </summary>
    /// <param name="targetNames">Names of target vectors</param>
    NearTextInput Minimum(params string[] targetNames);

    /// <summary>
    /// Creates a NearTextInput using relative score combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight)</param>
    NearTextInput RelativeScore(params (string Name, double Weight)[] targets);
}

/// <summary>
/// Internal implementation of INearTextBuilder.
/// </summary>
internal sealed class NearTextBuilder : INearTextBuilder
{
    private readonly string[] _query;
    private readonly float? _certainty;
    private readonly float? _distance;
    private readonly Move? _moveTo;
    private readonly Move? _moveAway;

    public NearTextBuilder(
        AutoArray<string> query,
        float? certainty,
        float? distance,
        Move? moveTo,
        Move? moveAway
    )
    {
        // Convert AutoArray to string[] for storage (AutoArray can't be stored as field)
        _query = query.ToArray();
        _certainty = certainty;
        _distance = distance;
        _moveTo = moveTo;
        _moveAway = moveAway;
    }

    public NearTextInput ManualWeights(params (string Name, double Weight)[] targets)
    {
        var targetVectors = TargetVectors.ManualWeights(targets);
        return new NearTextInput(_query, targetVectors, _certainty, _distance, _moveTo, _moveAway);
    }

    public NearTextInput Sum(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Sum(targetNames);
        return new NearTextInput(_query, targetVectors, _certainty, _distance, _moveTo, _moveAway);
    }

    public NearTextInput Average(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Average(targetNames);
        return new NearTextInput(_query, targetVectors, _certainty, _distance, _moveTo, _moveAway);
    }

    public NearTextInput Minimum(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Minimum(targetNames);
        return new NearTextInput(_query, targetVectors, _certainty, _distance, _moveTo, _moveAway);
    }

    public NearTextInput RelativeScore(params (string Name, double Weight)[] targets)
    {
        var targetVectors = TargetVectors.RelativeScore(targets);
        return new NearTextInput(_query, targetVectors, _certainty, _distance, _moveTo, _moveAway);
    }
}

// ============================================================================
// Hybrid Builder Infrastructure
// ============================================================================

/// <summary>
/// Builder class for creating HybridVectorInput. Provides methods to configure
/// either NearVector or NearText searches with target vectors.
/// </summary>
/// <example>
/// v => v.NearVector().ManualWeights(
///     ("title", 1.2, new[] { 1f, 2f }),
///     ("description", 0.8, new[] { 3f, 4f })
/// )
/// </example>
public sealed class HybridVectorInputBuilder
{
    /// <summary>
    /// Configures hybrid search with NearVector and optional search parameters.
    /// </summary>
    public IHybridNearVectorBuilder NearVector(float? certainty = null, float? distance = null)
    {
        return new HybridNearVectorBuilder(certainty, distance);
    }

    /// <summary>
    /// Configures hybrid search with NearText and optional search parameters.
    /// </summary>
    public IHybridNearTextBuilder NearText(
        AutoArray<string> query,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null
    )
    {
        return new HybridNearTextBuilder(query, certainty, distance, moveTo, moveAway);
    }

    /// <summary>
    /// Creates a HybridVectorInput directly from named vectors without a target vector combination method.
    /// The combination method will be Unspecified (server default behavior).
    /// Use this when providing vectors that don't require multi-target combination.
    /// </summary>
    /// <example>
    /// v => v.Vectors(
    ///     ("title", new[] { 1f, 2f }),
    ///     ("description", new[] { 3f, 4f })
    /// )
    /// </example>
    public HybridVectorInput Vectors(params (string Name, Vector Vector)[] namedVectors)
    {
        // Extract unique target names from the input
        var targetNames = namedVectors.Select(nv => nv.Name).Distinct().ToArray();

        // Create TargetVectors using implicit conversion (no combination method, Unspecified)
        TargetVectors targets = targetNames;

        // Combine the targets with the vectors
        var vectorSearch = VectorSearchInput.Combine(targets, namedVectors);
        return HybridVectorInput.FromVectorSearch(vectorSearch);
    }
}

/// <summary>
/// Builder interface for creating HybridVectorInput with NearVector configuration.
/// </summary>
public interface IHybridNearVectorBuilder
{
    /// <summary>
    /// Creates a HybridVectorInput with manually weighted target vectors for NearVector search.
    /// </summary>
    HybridVectorInput ManualWeights(params (string Name, double Weight, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput that sums all target vectors for NearVector search.
    /// </summary>
    HybridVectorInput Sum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput that averages all target vectors for NearVector search.
    /// </summary>
    HybridVectorInput Average(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput using minimum combination for NearVector search.
    /// </summary>
    HybridVectorInput Minimum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput using relative score combination for NearVector search.
    /// </summary>
    HybridVectorInput RelativeScore(params (string Name, double Weight, Vector Vector)[] targets);
}

/// <summary>
/// Internal implementation of IHybridNearVectorBuilder.
/// </summary>
internal sealed class HybridNearVectorBuilder : IHybridNearVectorBuilder
{
    private readonly float? _certainty;
    private readonly float? _distance;

    public HybridNearVectorBuilder(float? certainty, float? distance)
    {
        _certainty = certainty;
        _distance = distance;
    }

    public HybridVectorInput ManualWeights(
        params (string Name, double Weight, Vector Vector)[] targets
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.ManualWeights(
            targets.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput, _certainty, _distance);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    public HybridVectorInput Sum(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.Sum(targets.Select(t => (t.Name, t.Vector)).ToArray());
        var nearVectorInput = new NearVectorInput(vectorSearchInput, _certainty, _distance);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    public HybridVectorInput Average(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.Average(targets.Select(t => (t.Name, t.Vector)).ToArray());
        var nearVectorInput = new NearVectorInput(vectorSearchInput, _certainty, _distance);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    public HybridVectorInput Minimum(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.Minimum(targets.Select(t => (t.Name, t.Vector)).ToArray());
        var nearVectorInput = new NearVectorInput(vectorSearchInput, _certainty, _distance);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    public HybridVectorInput RelativeScore(
        params (string Name, double Weight, Vector Vector)[] targets
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.RelativeScore(
            targets.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput, _certainty, _distance);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }
}

/// <summary>
/// Builder interface for creating HybridVectorInput with NearText configuration.
/// </summary>
public interface IHybridNearTextBuilder
{
    /// <summary>
    /// Creates a HybridVectorInput with manually weighted target vectors for NearText search.
    /// </summary>
    HybridVectorInput ManualWeights(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput that sums all target vectors for NearText search.
    /// </summary>
    HybridVectorInput Sum(params string[] targetNames);

    /// <summary>
    /// Creates a HybridVectorInput that averages all target vectors for NearText search.
    /// </summary>
    HybridVectorInput Average(params string[] targetNames);

    /// <summary>
    /// Creates a HybridVectorInput using minimum combination for NearText search.
    /// </summary>
    HybridVectorInput Minimum(params string[] targetNames);

    /// <summary>
    /// Creates a HybridVectorInput using relative score combination for NearText search.
    /// </summary>
    HybridVectorInput RelativeScore(params (string Name, double Weight)[] targets);
}

/// <summary>
/// Internal implementation of IHybridNearTextBuilder.
/// </summary>
internal sealed class HybridNearTextBuilder : IHybridNearTextBuilder
{
    private readonly string[] _query;
    private readonly float? _certainty;
    private readonly float? _distance;
    private readonly Move? _moveTo;
    private readonly Move? _moveAway;

    public HybridNearTextBuilder(
        AutoArray<string> query,
        float? certainty,
        float? distance,
        Move? moveTo,
        Move? moveAway
    )
    {
        // Convert AutoArray to string[] for storage (AutoArray can't be stored as field)
        _query = query.ToArray();
        _certainty = certainty;
        _distance = distance;
        _moveTo = moveTo;
        _moveAway = moveAway;
    }

    public HybridVectorInput ManualWeights(params (string Name, double Weight)[] targets)
    {
        var targetVectors = TargetVectors.ManualWeights(targets);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    public HybridVectorInput Sum(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Sum(targetNames);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    public HybridVectorInput Average(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Average(targetNames);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    public HybridVectorInput Minimum(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Minimum(targetNames);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    public HybridVectorInput RelativeScore(params (string Name, double Weight)[] targets)
    {
        var targetVectors = TargetVectors.RelativeScore(targets);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }
}

// ============================================================================
// Factory Function Delegates
// ============================================================================

internal static class VectorInputBuilderFactories
{
    /// <summary>
    /// Creates a NearVectorInputBuilder delegate for use in lambda expressions.
    /// </summary>
    public static NearVectorInputBuilder CreateNearVectorBuilder() =>
        (certainty, distance) => new NearVectorBuilder(certainty, distance);

    /// <summary>
    /// Creates a NearTextInputBuilder delegate for use in lambda expressions.
    /// </summary>
    public static NearTextInputBuilder CreateNearTextBuilder() =>
        (query, certainty, distance, moveTo, moveAway) =>
            new NearTextBuilder(query, certainty, distance, moveTo, moveAway);

    /// <summary>
    /// Creates a HybridVectorInputBuilder for use in lambda expressions.
    /// </summary>
    public static HybridVectorInputBuilder CreateHybridBuilder() => new();
}
