
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest.Responses;

/// <summary>
/// Represents the response for a list of objects from Weaviate.
/// </summary>
internal struct ListObjectsResponse
{
    /// <summary>
    /// The actual list of Objects.
    /// </summary>
    [JsonPropertyName("objects")]
    public IEnumerable<WeaviateObject> Objects { get; set; }

    /// <summary>
    /// deprecations
    /// </summary>
    [JsonPropertyName("deprecations")]
    public IEnumerable<Deprecation> Deprecations { get; set; }

    /// <summary>
    /// The total number of Objects for the query.
    /// </summary>
    /// <remarks>
    /// The number of items in a response may be smaller due to paging.
    /// </remarks>
    public long TotalResults { get; set; }
}

