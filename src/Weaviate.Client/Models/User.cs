namespace Weaviate.Client.Models;

public record UserMetadata(Guid id)
{
    public Guid ID => id;
}

public class User(WeaviateClient client, UserMetadata metadata)
{
    internal WeaviateClient Client => client;
    public UserMetadata Metadata => metadata;
}
