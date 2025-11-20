namespace Weaviate.Client.Models;

public record DataReference(Guid From, string FromProperty, IEnumerable<Guid> To);

public record BatchReferenceReturn
{
    public float ElapsedSeconds { get; init; } = 0.0f;
    public Dictionary<int, WeaviateException[]> Errors { get; init; } = new();
    public bool HasErrors { get; init; } = false;

    public BatchReferenceReturn(float elapsedSeconds, Dictionary<int, WeaviateException[]> errors)
    {
        ElapsedSeconds = elapsedSeconds;
        Errors = errors;
        HasErrors = errors.Count > 0;
    }

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

    public override string ToString()
    {
        var errorsStr = string.Join(", ", Errors.Select(x => $"({x.Key}, {x.Value})"));
        if (errorsStr.Length > 0)
            errorsStr = $"{{...}}";
        return $"ElapsedSeconds: {ElapsedSeconds}, Errors: [{errorsStr}], HasErrors: {HasErrors}";
    }
}
