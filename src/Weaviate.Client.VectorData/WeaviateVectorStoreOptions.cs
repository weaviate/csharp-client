namespace Weaviate.Client.VectorData;

/// <summary>
/// Options for configuring <see cref="WeaviateVectorStore"/>.
/// </summary>
public class WeaviateVectorStoreOptions
{
    /// <summary>
    /// Optional factory that produces per-collection options based on collection name.
    /// </summary>
    public Func<
        string,
        WeaviateVectorStoreCollectionOptions
    >? CollectionOptionsFactory { get; set; }
}
