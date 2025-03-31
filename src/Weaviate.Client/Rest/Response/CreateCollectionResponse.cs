namespace Weaviate.Client.Rest.Responses;

using Weaviate.Client.Models;
using Weaviate.Client.Rest.Models;

public struct CreateCollectionResponse
{
    public Collection? Collection { get; internal set; }
}