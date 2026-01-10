namespace Weaviate.Client.Models;

// ============================================================================
// NearVector Builder Infrastructure
// ============================================================================

/// <summary>
/// Delegate for creating a NearVector builder. The builder is directly callable
/// to set certainty and distance parameters, then chainable to configure target vectors.
/// </summary>
/// <example>
/// v => v(certainty: 0.8).TargetVectorsManualWeights(
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
    NearVectorInput TargetVectorsManualWeights(
        params (string Name, double Weight, Vector Vector)[] targets
    );

    /// <summary>
    /// Creates a NearVectorInput that sums all target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, vector)</param>
    NearVectorInput TargetVectorsSum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput that averages all target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, vector)</param>
    NearVectorInput TargetVectorsAverage(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput using minimum combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, vector)</param>
    NearVectorInput TargetVectorsMinimum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a NearVectorInput using relative score combination of target vectors.
    /// </summary>
    /// <param name="targets">Tuples of (targetName, weight, vector)</param>
    NearVectorInput TargetVectorsRelativeScore(
        params (string Name, double Weight, Vector Vector)[] targets
    );
}

/// <summary>
/// Internal implementation of INearVectorBuilder.
/// </summary>
internal sealed class NearVectorBuilder : INearVectorBuilder
{
    /// <summary>
    /// The certainty
    /// </summary>
    private readonly float? _certainty;

    /// <summary>
    /// The distance
    /// </summary>
    private readonly float? _distance;

    /// <summary>
    /// Initializes a new instance of the <see cref="NearVectorBuilder"/> class
    /// </summary>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    public NearVectorBuilder(float? certainty = null, float? distance = null)
    {
        _certainty = certainty;
        _distance = distance;
    }

    /// <summary>
    /// Targets the vectors manual weights using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near vector input</returns>
    public NearVectorInput TargetVectorsManualWeights(
        params (string Name, double Weight, Vector Vector)[] targetVectors
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsManualWeights(
            targetVectors.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    /// <summary>
    /// Targets the vectors sum using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near vector input</returns>
    public NearVectorInput TargetVectorsSum(params (string Name, Vector Vector)[] targetVectors)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsSum(
            targetVectors.Select(t => (t.Name, t.Vector)).ToArray()
        );
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    /// <summary>
    /// Targets the vectors average using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near vector input</returns>
    public NearVectorInput TargetVectorsAverage(params (string Name, Vector Vector)[] targetVectors)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsAverage(
            targetVectors.Select(t => (t.Name, t.Vector)).ToArray()
        );
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    /// <summary>
    /// Targets the vectors minimum using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near vector input</returns>
    public NearVectorInput TargetVectorsMinimum(params (string Name, Vector Vector)[] targetVectors)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsMinimum(
            targetVectors.Select(t => (t.Name, t.Vector)).ToArray()
        );
        return new NearVectorInput(vectorSearchInput, _certainty, _distance);
    }

    /// <summary>
    /// Targets the vectors relative score using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near vector input</returns>
    public NearVectorInput TargetVectorsRelativeScore(
        params (string Name, double Weight, Vector Vector)[] targetVectors
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsRelativeScore(
            targetVectors.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
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
    /// <param name="targetVectors"></param>
    NearTextInput TargetVectorsManualWeights(params (string Name, double Weight)[] targetVectors);

    /// <summary>
    /// Creates a NearTextInput that sums all target vectors.
    /// </summary>
    /// <param name="targetVectors"></param>
    NearTextInput TargetVectorsSum(params string[] targetVectors);

    /// <summary>
    /// Creates a NearTextInput that averages all target vectors.
    /// </summary>
    /// <param name="targetVectors"></param>
    NearTextInput TargetVectorsAverage(params string[] targetVectors);

    /// <summary>
    /// Creates a NearTextInput using minimum combination of target vectors.
    /// </summary>
    /// <param name="targetVectors"></param>
    NearTextInput TargetVectorsMinimum(params string[] targetVectors);

    /// <summary>
    /// Creates a NearTextInput using relative score combination of target vectors.
    /// </summary>
    /// <param name="targetVectors"></param>
    NearTextInput TargetVectorsRelativeScore(params (string Name, double Weight)[] targetVectors);
}

/// <summary>
/// Internal implementation of INearTextBuilder.
/// </summary>
internal sealed class NearTextBuilder : INearTextBuilder
{
    /// <summary>
    /// The query
    /// </summary>
    private readonly string[] _query;

    /// <summary>
    /// The certainty
    /// </summary>
    private readonly float? _certainty;

    /// <summary>
    /// The distance
    /// </summary>
    private readonly float? _distance;

    /// <summary>
    /// The move to
    /// </summary>
    private readonly Move? _moveTo;

    /// <summary>
    /// The move away
    /// </summary>
    private readonly Move? _moveAway;

    /// <summary>
    /// Initializes a new instance of the <see cref="NearTextBuilder"/> class
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="moveTo">The move to</param>
    /// <param name="moveAway">The move away</param>
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

    /// <summary>
    /// Targets the vectors manual weights using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near text input</returns>
    public NearTextInput TargetVectorsManualWeights(
        params (string Name, double Weight)[] targetVectors
    )
    {
        var targetVectorsObj = TargetVectors.ManualWeights(targetVectors);
        return new NearTextInput(
            _query,
            targetVectorsObj,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
    }

    /// <summary>
    /// Targets the vectors sum using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near text input</returns>
    public NearTextInput TargetVectorsSum(params string[] targetVectors)
    {
        var targetVectorsObj = TargetVectors.Sum(targetVectors);
        return new NearTextInput(
            _query,
            targetVectorsObj,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
    }

    /// <summary>
    /// Targets the vectors average using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near text input</returns>
    public NearTextInput TargetVectorsAverage(params string[] targetVectors)
    {
        var targetVectorsObj = TargetVectors.Average(targetVectors);
        return new NearTextInput(
            _query,
            targetVectorsObj,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
    }

    /// <summary>
    /// Targets the vectors minimum using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near text input</returns>
    public NearTextInput TargetVectorsMinimum(params string[] targetVectors)
    {
        var targetVectorsObj = TargetVectors.Minimum(targetVectors);
        return new NearTextInput(
            _query,
            targetVectorsObj,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
    }

    /// <summary>
    /// Targets the vectors relative score using the specified target vectors
    /// </summary>
    /// <param name="targetVectors">The target vectors</param>
    /// <returns>The near text input</returns>
    public NearTextInput TargetVectorsRelativeScore(
        params (string Name, double Weight)[] targetVectors
    )
    {
        var targetVectorsObj = TargetVectors.RelativeScore(targetVectors);
        return new NearTextInput(
            _query,
            targetVectorsObj,
            _certainty,
            _distance,
            _moveTo,
            _moveAway
        );
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
    public HybridNearVectorBuilder NearVector()
    {
        return new HybridNearVectorBuilder();
    }

    /// <summary>
    /// Configures hybrid search with NearText and optional search parameters.
    /// </summary>
    public HybridNearTextBuilder NearText(
        AutoArray<string> query,
        Move? moveTo = null,
        Move? moveAway = null
    )
    {
        return new HybridNearTextBuilder(query, moveTo, moveAway);
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
    HybridVectorInput TargetVectorsManualWeights(
        params (string Name, double Weight, Vector Vector)[] targets
    );

    /// <summary>
    /// Creates a HybridVectorInput that sums all target vectors for NearVector search.
    /// </summary>
    HybridVectorInput TargetVectorsSum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput that averages all target vectors for NearVector search.
    /// </summary>
    HybridVectorInput TargetVectorsAverage(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput using minimum combination for NearVector search.
    /// </summary>
    HybridVectorInput TargetVectorsMinimum(params (string Name, Vector Vector)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput using relative score combination for NearVector search.
    /// </summary>
    HybridVectorInput TargetVectorsRelativeScore(
        params (string Name, double Weight, Vector Vector)[] targets
    );
}

/// <summary>
/// Internal implementation of IHybridNearVectorBuilder.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class HybridNearVectorBuilder : IHybridNearVectorBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HybridNearVectorBuilder"/> class
    /// </summary>
    public HybridNearVectorBuilder() { }

    /// <summary>
    /// Targets the vectors manual weights using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsManualWeights(
        params (string Name, double Weight, Vector Vector)[] targets
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsManualWeights(
            targets.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    /// <summary>
    /// Targets the vectors sum using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsSum(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsSum(
            targets.Select(t => (t.Name, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    /// <summary>
    /// Targets the vectors average using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsAverage(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsAverage(
            targets.Select(t => (t.Name, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    /// <summary>
    /// Targets the vectors minimum using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsMinimum(params (string Name, Vector Vector)[] targets)
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsMinimum(
            targets.Select(t => (t.Name, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput);
        return HybridVectorInput.FromNearVector(nearVectorInput);
    }

    /// <summary>
    /// Targets the vectors relative score using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsRelativeScore(
        params (string Name, double Weight, Vector Vector)[] targets
    )
    {
        var builder = new VectorSearchInput.Builder();
        var vectorSearchInput = builder.TargetVectorsRelativeScore(
            targets.Select(t => (t.Name, t.Weight, t.Vector)).ToArray()
        );
        var nearVectorInput = new NearVectorInput(vectorSearchInput);
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
    HybridVectorInput TargetVectorsManualWeights(params (string Name, double Weight)[] targets);

    /// <summary>
    /// Creates a HybridVectorInput that sums all target vectors for NearText search.
    /// </summary>
    HybridVectorInput TargetVectorsSum(params string[] targetNames);

    /// <summary>
    /// Creates a HybridVectorInput that averages all target vectors for NearText search.
    /// </summary>
    HybridVectorInput TargetVectorsAverage(params string[] targetNames);

    /// <summary>
    /// Creates a HybridVectorInput using minimum combination for NearText search.
    /// </summary>
    HybridVectorInput TargetVectorsMinimum(params string[] targetNames);

    /// <summary>
    /// Creates a HybridVectorInput using relative score combination for NearText search.
    /// </summary>
    HybridVectorInput TargetVectorsRelativeScore(params (string Name, double Weight)[] targets);
}

/// <summary>
/// Internal implementation of IHybridNearTextBuilder.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public sealed class HybridNearTextBuilder : IHybridNearTextBuilder
{
    /// <summary>
    /// The query
    /// </summary>
    private readonly string[] _query;

    /// <summary>
    /// The move to
    /// </summary>
    private readonly Move? _moveTo;

    /// <summary>
    /// The move away
    /// </summary>
    private readonly Move? _moveAway;

    /// <summary>
    /// Initializes a new instance of the <see cref="HybridNearTextBuilder"/> class
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="moveTo">The move to</param>
    /// <param name="moveAway">The move away</param>
    public HybridNearTextBuilder(AutoArray<string> query, Move? moveTo, Move? moveAway)
    {
        // Convert AutoArray to string[] for storage (AutoArray can't be stored as field)
        _query = query.ToArray();
        _moveTo = moveTo;
        _moveAway = moveAway;
    }

    public static implicit operator HybridVectorInput(HybridNearTextBuilder builder)
    {
        var nearTextInput = new NearTextInput(
            builder._query,
            (Weaviate.Client.Models.TargetVectors?)null,
            null,
            null,
            builder._moveTo,
            builder._moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    /// <summary>
    /// Targets the vectors manual weights using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsManualWeights(
        params (string Name, double Weight)[] targets
    )
    {
        var targetVectors = TargetVectors.ManualWeights(targets);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            null,
            null,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    /// <summary>
    /// Targets the vectors sum using the specified target names
    /// </summary>
    /// <param name="targetNames">The target names</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsSum(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Sum(targetNames);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            null,
            null,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    /// <summary>
    /// Targets the vectors average using the specified target names
    /// </summary>
    /// <param name="targetNames">The target names</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsAverage(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Average(targetNames);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            null,
            null,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    /// <summary>
    /// Targets the vectors minimum using the specified target names
    /// </summary>
    /// <param name="targetNames">The target names</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsMinimum(params string[] targetNames)
    {
        var targetVectors = TargetVectors.Minimum(targetNames);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            null,
            null,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }

    /// <summary>
    /// Targets the vectors relative score using the specified targets
    /// </summary>
    /// <param name="targets">The targets</param>
    /// <returns>The hybrid vector input</returns>
    public HybridVectorInput TargetVectorsRelativeScore(
        params (string Name, double Weight)[] targets
    )
    {
        var targetVectors = TargetVectors.RelativeScore(targets);
        var nearTextInput = new NearTextInput(
            _query,
            targetVectors,
            null,
            null,
            _moveTo,
            _moveAway
        );
        return HybridVectorInput.FromNearText(nearTextInput);
    }
}

// ============================================================================
// Factory Function Delegates
// ============================================================================

/// <summary>
/// The vector input builder factories class
/// </summary>
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
