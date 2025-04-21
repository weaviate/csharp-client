namespace Weaviate.Client.Models;

public class WeaviateObject<TData, TVector>
{
    public CollectionClient? Collection { get; }

    public string? CollectionName { get; }

    public required TData? Data { get; set; }

    public IList<TVector> Vector { get; set; } = new List<TVector>();

    public Guid? ID { get; set; }

    public IDictionary<string, object> Additional { get; set; } = new Dictionary<string, object>();

    public DateTime? CreationTime { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public string? Tenant { get; set; }

    public object? VectorWeights { get; set; }

    public IDictionary<string, object> Vectors { get; set; } = new Dictionary<string, object>();

    public WeaviateObject(CollectionClient? collection = null) : this(collection?.Name ?? typeof(TData).Name)
    {
        Collection = collection;
    }
    public WeaviateObject(string collectionName)
    {
        CollectionName = collectionName;
    }

    public static IList<TVector> EmptyVector()
    {
        return new List<TVector>();
    }
}

public class WeaviateObject<TData> : WeaviateObject<TData, float>
{
    [System.Text.Json.Serialization.JsonConstructor]
    public WeaviateObject(CollectionClient? collection = null) : base(collection) { }

    public WeaviateObject(string collectionName) : base(collectionName) { }
}
