using System.Net;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
internal partial class WeaviateRestClient
{
    /// <summary>
    /// Gets the meta using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The meta</returns>
    internal async Task<Dto.Meta?> GetMeta(CancellationToken cancellationToken = default)
    {
        var path = WeaviateEndpoints.Meta();

        var response = await _httpClient.GetAsync(path, cancellationToken);

        await response.ManageStatusCode(
            [
                HttpStatusCode.OK, // 200
                // HttpStatusCode.BadRequest, // 400
                // HttpStatusCode.Unauthorized, // 401
                // HttpStatusCode.Forbidden, // 403
                // HttpStatusCode.InternalServerError, // 500
            ],
            "get meta endpoint"
        );

        var meta = await response.DecodeAsync<Meta>(cancellationToken);

        return meta;
    }
}
