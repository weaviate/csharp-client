using System.Net;
using System.Net.Http.Json;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

internal partial class WeaviateRestClient
{
    /// <summary>
    /// Calls <c>POST /v1/tokenize</c>.
    /// </summary>
    internal async Task<TokenizeResponse?> Tokenize(
        TokenizeRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Tokenize(),
            request,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.ManageStatusCode([HttpStatusCode.OK], "tokenize");

        return await response.DecodeAsync<TokenizeResponse>(cancellationToken);
    }

    /// <summary>
    /// Calls <c>POST /v1/schema/{className}/properties/{propertyName}/tokenize</c>.
    /// </summary>
    internal async Task<TokenizeResponse?> TokenizeProperty(
        string className,
        string propertyName,
        PropertyTokenizeRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.TokenizeProperty(className, propertyName),
            request,
            options: RestJsonSerializerOptions,
            cancellationToken: cancellationToken
        );

        await response.ManageStatusCode(
            [HttpStatusCode.OK],
            "tokenize property",
            ResourceType.Property
        );

        return await response.DecodeAsync<TokenizeResponse>(cancellationToken);
    }
}
