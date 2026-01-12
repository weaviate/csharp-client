using System.Net;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    /// <summary>
    /// Nodeses the collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="verbosity">The verbosity</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing a list of dto node status</returns>
    internal async Task<IList<NodeStatus>> Nodes(
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

        return nodes?.Nodes ?? Array.Empty<NodeStatus>();
    }
}
