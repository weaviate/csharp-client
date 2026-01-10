namespace Weaviate.Client.Models;

/// <summary>
/// The group by request
/// </summary>
public record GroupByRequest(string PropertyName)
{
    /// <summary>
    /// Gets or sets the value of the number of groups
    /// </summary>
    public uint NumberOfGroups { get; set; }

    /// <summary>
    /// Gets or sets the value of the objects per group
    /// </summary>
    public uint ObjectsPerGroup { get; set; }
}
