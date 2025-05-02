namespace Weaviate.Client.Models;

public record WeaviateObject<TData, TVector>
{
    public CollectionClient<TData>? Collection { get; }

    public string? CollectionName { get; }

    public required TData? Data { get; set; }

    public Guid? ID { get; set; }

    public IDictionary<string, object> Additional { get; set; } = new Dictionary<string, object>();

    public DateTime? CreationTime { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public string? Tenant { get; set; }

    public IDictionary<string, IList<TVector>> Vectors { get; set; } = new Dictionary<string, IList<TVector>>();

    public WeaviateObject(CollectionClient<TData>? collection = null) : this(collection?.Name ?? typeof(TData).Name)
    {
        Collection = collection;
    }
    public WeaviateObject(string collectionName)
    {
        CollectionName = collectionName;
    }

    /// Vector associated with the Object.
    /// </summary>
    [Obsolete("Use Vectors instead.")]
    public IList<TVector>? Vector { get; set; } = new List<TVector>();


    public static IList<TVector> EmptyVector()
    {
        return new List<TVector>();
    }
}

public record WeaviateObject<TData> : WeaviateObject<TData, float>
{
    [System.Text.Json.Serialization.JsonConstructor]
    public WeaviateObject(CollectionClient<TData>? collection = null) : base(collection) { }

    public WeaviateObject(string collectionName) : base(collectionName) { }
}

public record WeaviateObject : WeaviateObject<dynamic>
{
    public WeaviateObject(string collectionName) : base(collectionName) { }
}
