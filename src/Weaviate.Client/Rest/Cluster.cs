using System.Net.Http.Json;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient
{
    internal async Task<Dto.NodeStatus[]> Nodes(string? collection, string verbosity)
    {
        var path = WeaviateEndpoints.Nodes(collection, verbosity);
        var response = await _httpClient.GetAsync(path);

        await response.EnsureExpectedStatusCodeAsync([200], "get nodes");

        var nodes = await response.Content.ReadFromJsonAsync<NodesStatusResponse>(
            options: RestJsonSerializerOptions
        );

        return nodes?.Nodes?.ToArray() ?? Array.Empty<Dto.NodeStatus>();
    }
}
