namespace Weaviate.Client.VectorData;

/// <summary>
/// Options for configuring a <see cref="WeaviateVectorStoreCollection{TKey, TRecord}"/>.
/// </summary>
public class WeaviateVectorStoreCollectionOptions
{
    /// <summary>
    /// Optional tenant name to pin all operations to.
    /// </summary>
    public string? Tenant { get; set; }

    /// <summary>
    /// Optional consistency level for operations.
    /// </summary>
    public ConsistencyLevels? ConsistencyLevel { get; set; }
}
