namespace Weaviate.Client.Rest.Responses;

using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

internal struct ListCollectionResponse
{
    [JsonPropertyName("classes")]
    public required IEnumerable<CollectionGeneric> Collections { get; set; }
}