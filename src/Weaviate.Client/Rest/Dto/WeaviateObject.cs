
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest.Dto;

/// <summary>
/// Represents a Weaviate object with its associated properties and metadata.
/// </summary>
public class WeaviateObject
{
    /// <summary>
    /// Additional properties associated with the object.
    /// </summary>
    [JsonPropertyName("additional")]
    public IDictionary<string, object> Additional { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Class of the Object, defined in the schema.
    /// </summary>
    [JsonPropertyName("class")]
    public string? Class { get; set; }

    /// <summary>
    /// Unique identifier of the Object.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid? ID { get; set; }

    /// <summary>
    /// Timestamp of creation of this object in milliseconds since epoch UTC.
    /// </summary>
    [JsonPropertyName("creationTimeUnix")]
    public long? CreationTimeUnix { get; set; }

    /// <summary>
    /// Timestamp of the last object update in milliseconds since epoch UTC.
    /// </summary>
    [JsonPropertyName("lastUpdateTimeUnix")]
    public long? LastUpdateTimeUnix { get; set; }

    /// <summary>
    /// Properties of the object.
    /// </summary>
    [JsonPropertyName("properties")]
    public object? Properties { get; set; }

    /// <summary>
    /// Name of the object's tenant.
    /// </summary>
    [JsonPropertyName("tenant")]
    public string? Tenant { get; set; }

    /// <summary>
    /// Vector associated with the Object.
    /// </summary>
    [Obsolete("Use Vectors instead.")]
    [JsonPropertyName("vector")]
    public IList<float>? Vector { get; set; } = new List<float>();
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

    /// <summary>
    /// Weights of the vector.
    /// </summary>
    [Obsolete("Use Vectors instead.")]
    [JsonPropertyName("vectorWeights")]
    public object? VectorWeights { get; set; }

    /// <summary>
    /// Vectors associated with the Object.
    /// </summary>
    [JsonPropertyName("vectors")]
    public IDictionary<string, IEnumerable<float>>? Vectors { get; set; } = new Dictionary<string, IEnumerable<float>>();
}