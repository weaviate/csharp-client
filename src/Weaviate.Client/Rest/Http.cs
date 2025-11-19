using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

public static class HttpResponseMessageExtensions
{
    private static async Task<HttpStatusCode> EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        SortedSet<HttpStatusCode> codes,
        string error = ""
    )
    {
        if (codes.Contains(response.StatusCode))
        {
            return response.StatusCode;
        }

        var content = await response.Content.ReadAsStringAsync();

        if (response.Content != null)
            response.Content.Dispose();

        var errorMessage =
            $"Unexpected status code {response.StatusCode}. Expected: {string.Join(", ", codes)}. {error}. Server replied: {content}";

        throw new WeaviateUnexpectedStatusCodeException(response.StatusCode, codes, errorMessage);
    }

    private static Task<HttpStatusCode> EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        SortedSet<int> codes,
        string error = ""
    ) => EnsureExpectedStatusCodeAsync(response, [.. codes.Select(x => (HttpStatusCode)x)], error);

    private static Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        int code,
        string error = ""
    ) => EnsureExpectedStatusCodeAsync(response, [(HttpStatusCode)code], error);

    private static Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        HttpStatusCode code,
        string error = ""
    ) => EnsureExpectedStatusCodeAsync(response, [code], error);

    private static Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response) =>
        EnsureExpectedStatusCodeAsync(response, [HttpStatusCode.OK]);

    public static async Task ManageStatusCode(
        this HttpResponseMessage response,
        IEnumerable<HttpStatusCode> expectedCodes,
        string error = "",
        ResourceType resourceType = ResourceType.Unknown
    )
    {
        try
        {
            await response.EnsureExpectedStatusCodeAsync(
                new SortedSet<HttpStatusCode>(expectedCodes),
                error
            );

            // TODO
            // HttpStatusCode.BadRequest
            // HttpStatusCode.Unauthorized
            // HttpStatusCode.Forbidden
            // HttpStatusCode.InternalServerError
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new WeaviateNotFoundException(ex, resourceType);
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
            when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            throw new WeaviateConflictException($"Conflict accessing {resourceType}", ex);
        }
    }

    /// <summary>
    /// Deserializes the HTTP response content to the specified DTO type using Weaviate's standard serializer options.
    /// </summary>
    /// <typeparam name="TDto">The DTO type to deserialize to.</typeparam>
    /// <param name="response">The HTTP response message to deserialize.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The deserialized DTO, or throws if deserialization fails.</returns>
    /// <exception cref="WeaviateRestClientException">Thrown when deserialization results in null.</exception>
    internal static async Task<TDto> DecodeAsync<TDto>(
        this HttpResponseMessage response,
        CancellationToken cancellationToken = default
    )
    {
        return await response.Content.ReadFromJsonAsync<TDto>(
                WeaviateRestClient.RestJsonSerializerOptions,
                cancellationToken: cancellationToken
            ) ?? throw new WeaviateRestClientException();
    }
}

internal class WeaviateUnexpectedStatusCodeException : WeaviateServerException
{
    public HttpStatusCode StatusCode { get; private set; }
    public ISet<HttpStatusCode> ExpectedStatusCodes { get; private set; }

    public WeaviateUnexpectedStatusCodeException(
        HttpStatusCode statusCode,
        ISet<HttpStatusCode> expectedStatusCodes,
        string content
    )
        : base(content)
    {
        StatusCode = statusCode;
        ExpectedStatusCodes = expectedStatusCodes;
    }
}
