namespace Weaviate.Client.Models;

/// <summary>
/// The data reference
/// </summary>
public record DataReference(Guid From, string FromProperty, IEnumerable<Guid> To)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataReference"/> class
    /// </summary>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="to">The to</param>
    public DataReference(Guid from, string fromProperty, params Guid[] to)
        : this(from, fromProperty, (IEnumerable<Guid>)to) { }
}

/// <summary>
/// The batch reference return
/// </summary>
public record BatchReferenceReturn
{
    /// <summary>
    /// Gets or inits the value of the elapsed seconds
    /// </summary>
    public float ElapsedSeconds { get; init; } = 0.0f;

    /// <summary>
    /// Gets or inits the value of the errors
    /// </summary>
    public Dictionary<int, WeaviateException[]> Errors { get; init; } = new();

    /// <summary>
    /// Gets or inits the value of the has errors
    /// </summary>
    public bool HasErrors { get; init; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchReferenceReturn"/> class
    /// </summary>
    /// <param name="elapsedSeconds">The elapsed seconds</param>
    /// <param name="errors">The errors</param>
    public BatchReferenceReturn(float elapsedSeconds, Dictionary<int, WeaviateException[]> errors)
    {
        ElapsedSeconds = elapsedSeconds;
        Errors = errors;
        HasErrors = errors.Count > 0;
    }

    /// <summary>
    /// Combines two BatchReferenceReturn results
    /// </summary>
    /// <param name="left">The left operand</param>
    /// <param name="right">The right operand</param>
    /// <returns>A combined BatchReferenceReturn</returns>
    public static BatchReferenceReturn operator +(
        BatchReferenceReturn left,
        BatchReferenceReturn right
    )
    {
        var result = new BatchReferenceReturn(
            left.ElapsedSeconds + right.ElapsedSeconds,
            new Dictionary<int, WeaviateException[]>(left.Errors)
        );
        foreach (var kvp in right.Errors)
        {
            if (result.Errors.ContainsKey(kvp.Key))
                result.Errors[kvp.Key] = kvp.Value;
            else
                result.Errors.Add(kvp.Key, kvp.Value);
        }

        return result with
        {
            HasErrors = left.HasErrors || right.HasErrors,
        };
    }

    /// <summary>
    /// Returns the string
    /// </summary>
    /// <returns>The string</returns>
    public override string ToString()
    {
        var errorsStr = string.Join(", ", Errors.Select(x => $"({x.Key}, {x.Value})"));
        if (errorsStr.Length > 0)
            errorsStr = $"{{...}}";
        return $"ElapsedSeconds: {ElapsedSeconds}, Errors: [{errorsStr}], HasErrors: {HasErrors}";
    }
}
