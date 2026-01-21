using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Weaviate.Client.Rest;

/// <summary>
/// The http response message extensions class
/// </summary>
internal static class HttpResponseMessageExtensions
{
    /// <summary>
    /// Ensures the expected status code using the specified response
    /// </summary>
    /// <param name="response">The response</param>
    /// <param name="codes">The codes</param>
    /// <param name="error">The error</param>
    /// <exception cref="WeaviateUnexpectedStatusCodeException"></exception>
    /// <returns>A task containing the http status code</returns>
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

    /// <summary>
    /// Ensures the expected status code using the specified response
    /// </summary>
    /// <param name="response">The response</param>
    /// <param name="codes">The codes</param>
    /// <param name="error">The error</param>
    /// <returns>A task containing the http status code</returns>
    private static Task<HttpStatusCode> EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        SortedSet<int> codes,
        string error = ""
    ) => EnsureExpectedStatusCodeAsync(response, [.. codes.Select(x => (HttpStatusCode)x)], error);

    /// <summary>
    /// Ensures the expected status code using the specified response
    /// </summary>
    /// <param name="response">The response</param>
    /// <param name="code">The code</param>
    /// <param name="error">The error</param>
    private static Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        int code,
        string error = ""
    ) => EnsureExpectedStatusCodeAsync(response, [(HttpStatusCode)code], error);

    /// <summary>
    /// Ensures the expected status code using the specified response
    /// </summary>
    /// <param name="response">The response</param>
    /// <param name="code">The code</param>
    /// <param name="error">The error</param>
    private static Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        HttpStatusCode code,
        string error = ""
    ) => EnsureExpectedStatusCodeAsync(response, [code], error);

    /// <summary>
    /// Ensures the success status code using the specified response
    /// </summary>
    /// <param name="response">The response</param>
    private static Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response) =>
        EnsureExpectedStatusCodeAsync(response, [HttpStatusCode.OK]);

    /// <summary>
    /// Manages the status code using the specified response
    /// </summary>
    /// <param name="response">The response</param>
    /// <param name="expectedCodes">The expected codes</param>
    /// <param name="error">The error</param>
    /// <param name="resourceType">The resource type</param>
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
        }
        catch (WeaviateUnexpectedStatusCodeException ex)
        {
            // Use centralized exception mapping helper
            throw Internal.ExceptionHelper.MapHttpException(
                ex.StatusCode,
                ex.Message,
                ex,
                resourceType
            );
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
        try
        {
            return await response.Content.ReadFromJsonAsync<TDto>(
                    WeaviateRestClient.RestJsonSerializerOptions,
                    cancellationToken: cancellationToken
                ) ?? throw new WeaviateRestClientException("Deserialization resulted in null.");
        }
        catch (JsonException ex)
        {
            throw new WeaviateRestClientException(ex.Message, ex);
        }
    }
}

/// <summary>
/// The weaviate unexpected status code exception class
/// </summary>
/// <seealso cref="WeaviateServerException"/>
internal class WeaviateUnexpectedStatusCodeException : WeaviateServerException
{
    /// <summary>
    /// Gets or sets the value of the status code
    /// </summary>
    public HttpStatusCode StatusCode { get; private set; }

    /// <summary>
    /// Gets or sets the value of the expected status codes
    /// </summary>
    public ISet<HttpStatusCode> ExpectedStatusCodes { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateUnexpectedStatusCodeException"/> class
    /// </summary>
    /// <param name="statusCode">The status code</param>
    /// <param name="expectedStatusCodes">The expected status codes</param>
    /// <param name="content">The content</param>
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
