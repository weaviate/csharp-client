namespace Weaviate.Client.Models;

public record WeaviateObject<TData, TVector>
{
    public record ObjectMetadata
    {
        public DateTime? CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public double? Distance { get; init; }
        public double? Certainty { get; init; }
        public double? Score { get; init; }
        public string? ExplainScore { get; init; }
        public bool? IsConsistent { get; init; }
        public double? RerankScore { get; init; }
    }

    public string? CollectionName { get; }

    public required TData? Data { get; set; }

    public IDictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();

    public IDictionary<string, IList<WeaviateObject>> References { get; set; } = new Dictionary<string, IList<WeaviateObject>>();

    public ObjectMetadata Metadata { get; set; } = new ObjectMetadata();

    public Guid? ID { get; set; }

    public IDictionary<string, dynamic> Additional { get; set; } = new Dictionary<string, object>();

    public string? Tenant { get; set; }

    public IDictionary<string, IList<TVector>> Vectors { get; set; } = new Dictionary<string, IList<TVector>>();

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
    public WeaviateObject(string? collectionName = null) : base(collectionName ?? typeof(TData).Name) { }
}

public record WeaviateObject : WeaviateObject<dynamic>
{
    public WeaviateObject(string collectionName) : base(collectionName) { }
}
