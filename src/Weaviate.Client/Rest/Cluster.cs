using System.Net;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    internal async Task<IList<Dto.NodeStatus>> Nodes(
        string? collection,
        string verbosity,
        CancellationToken cancellationToken = default
    )
    {
        var path = WeaviateEndpoints.Nodes(collection, verbosity);
        var response = await _httpClient.GetAsync(path, cancellationToken);

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK,
                // HttpStatusCode.BadRequest,
                // HttpStatusCode.Unauthorized,
                // HttpStatusCode.Forbidden,
                // HttpStatusCode.InternalServerError,
            ],
            "get nodes"
        );

        var nodes = await response.DecodeAsync<NodesStatusResponse>(cancellationToken);

        return nodes?.Nodes ?? Array.Empty<Dto.NodeStatus>();
    }
}
