namespace Weaviate.Client.Models;

public class WeaviateObject<TData, TVector>
{
    public CollectionClient<TData>? Collection { get; }

    public string? CollectionName { get; }

    public required TData? Data { get; set; }

    public Guid? ID { get; set; }

    public IDictionary<string, object> Additional { get; set; } = new Dictionary<string, object>();

    public DateTime? CreationTime { get; set; }

    public DateTime? LastUpdateTime { get; set; }

    public string? Tenant { get; set; }

    public IDictionary<string, IEnumerable<TVector>> Vectors { get; set; } = new Dictionary<string, IEnumerable<TVector>>();

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
    // {
    //     get
    //     {
    //         return Vectors.ContainsKey("default") ? Vectors["default"] : Vectors["default"] = [];
    //     }
    //     set
    //     {
    //         if (value != null)
    //         {
    //             Vectors["default"] = value;
    //         }
    //     }
    // }


    public static IList<TVector> EmptyVector()
    {
        return new List<TVector>();
    }
}

public class WeaviateObject<TData> : WeaviateObject<TData, float>
{
    [System.Text.Json.Serialization.JsonConstructor]
    public WeaviateObject(CollectionClient<TData>? collection = null) : base(collection) { }

    public WeaviateObject(string collectionName) : base(collectionName) { }
}
