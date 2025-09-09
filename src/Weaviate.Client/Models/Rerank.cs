namespace Weaviate.Client.Models;

public record Rerank
{
    public required string Property { get; init; }
    public string? Query { get; init; } = null;
}
