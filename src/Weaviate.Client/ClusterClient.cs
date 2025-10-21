using System.Text.Json.Serialization;
using Weaviate.Client.Models;
using Weaviate.Client.Rest;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client;

public class ClusterClient
{
    private readonly WeaviateRestClient _client;

    internal ClusterClient(WeaviateRestClient client)
    {
        _client = client;
        Nodes = new NodesClient(_client);
    }

    public NodesClient Nodes { get; }
}
