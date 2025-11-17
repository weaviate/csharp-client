using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
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

        var meta = await response.Content.ReadFromJsonAsync<Meta>(
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        return meta;
    }
}
