
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest.Responses;

internal struct ListCollectionResponse
{
    [JsonPropertyName("classes")]
    public required IEnumerable<CollectionGeneric> Collections { get; set; }
}