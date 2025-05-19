namespace Weaviate.Client.Models;

public record Metadata
{
    public DateTime? CreationTime { get; set; }
    public DateTime? LastUpdateTime { get; set; }
    public double? Distance { get; init; }
    public double? Certainty { get; init; }
    public double? Score { get; init; }
    public string? ExplainScore { get; init; }
    public bool? IsConsistent { get; init; }
    public double? RerankScore { get; init; }
}
