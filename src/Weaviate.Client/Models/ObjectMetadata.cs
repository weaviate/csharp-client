namespace Weaviate.Client.Models;

/// <summary>
/// The metadata
/// </summary>
public record Metadata
{
    /// <summary>
    /// Gets or sets the value of the creation time
    /// </summary>
    public DateTime? CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the value of the last update time
    /// </summary>
    public DateTime? LastUpdateTime { get; set; }

    /// <summary>
    /// Gets or inits the value of the distance
    /// </summary>
    public double? Distance { get; init; }

    /// <summary>
    /// Gets or inits the value of the certainty
    /// </summary>
    public double? Certainty { get; init; }

    /// <summary>
    /// Gets or inits the value of the score
    /// </summary>
    public double? Score { get; init; }

    /// <summary>
    /// Gets or inits the value of the explain score
    /// </summary>
    public string? ExplainScore { get; init; }

    /// <summary>
    /// Gets or inits the value of the is consistent
    /// </summary>
    public bool? IsConsistent { get; init; }

    /// <summary>
    /// Gets or inits the value of the rerank score
    /// </summary>
    public double? RerankScore { get; init; }
}
