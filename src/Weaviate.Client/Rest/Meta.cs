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

        await response.EnsureExpectedStatusCodeAsync([200], "get meta endpoint");

        var meta = await response.Content.ReadFromJsonAsync<Meta>(
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        return meta;
    }
}
