namespace Weaviate.Client.Models;

/// <summary>
/// The user metadata
/// </summary>
public record UserMetadata(Guid id)
{
    /// <summary>
    /// Gets the value of the id
    /// </summary>
    public Guid ID => id;
}

/// <summary>
/// The user class
/// </summary>
public class User(WeaviateClient client, UserMetadata metadata)
{
    /// <summary>
    /// Gets the value of the client
    /// </summary>
    internal WeaviateClient Client => client;

    /// <summary>
    /// Gets the value of the metadata
    /// </summary>
    public UserMetadata Metadata => metadata;
}
