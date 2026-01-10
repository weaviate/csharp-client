namespace Weaviate.Client.Models;

/// <summary>
/// The rerank
/// </summary>
public record Rerank
{
    /// <summary>
    /// Gets or inits the value of the property
    /// </summary>
    public required string Property { get; init; }

    /// <summary>
    /// Gets or inits the value of the query
    /// </summary>
    public string? Query { get; init; } = null;
}
