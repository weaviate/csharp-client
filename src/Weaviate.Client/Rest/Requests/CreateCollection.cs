namespace Weaviate.Client.Rest.Requests;

using Weaviate.Client.Models;
using Weaviate.Client.Rest.Models;

public class CreateCollectionRequest
{
    public required Collection Collection { get; set; }
}