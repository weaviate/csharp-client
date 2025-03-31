namespace Weaviate.Client.Models;

public record ObjectMetadata(Guid id)
{
    public Guid ID => id;
}

public class Object(WeaviateClient client, ObjectMetadata metadata)
{
    internal WeaviateClient Client => client;
    public ObjectMetadata Metadata => metadata;
}