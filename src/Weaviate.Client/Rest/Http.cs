using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest;

public static class HttpResponseMessageExtensions
{
    public static async Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        SortedSet<HttpStatusCode> codes,
        string error = ""
    )
    {
        if (codes.Contains(response.StatusCode))
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();

        if (response.Content != null)
            response.Content.Dispose();

        content =
            $"Unexpected status code {response.StatusCode}. Expected: {string.Join(", ", codes)}. {error}. Server replied: {content}";

        throw new SimpleHttpResponseException(response.StatusCode, codes, content);
    }

    public static async Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        SortedSet<int> codes,
        string error = ""
    )
    {
        await EnsureExpectedStatusCodeAsync(
            response,
            [.. codes.Select(x => (HttpStatusCode)x)],
            error
        );
    }

    public static async Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        int code,
        string error = ""
    )
    {
        await EnsureExpectedStatusCodeAsync(response, [(HttpStatusCode)code], error);
    }

    public static async Task EnsureExpectedStatusCodeAsync(
        this HttpResponseMessage response,
        HttpStatusCode code,
        string error = ""
    )
    {
        await EnsureExpectedStatusCodeAsync(response, [code], error);
    }

    public static async Task EnsureSuccessStatusCodeAsync(this HttpResponseMessage response)
    {
        await EnsureExpectedStatusCodeAsync(response, [HttpStatusCode.OK]);
    }
}

internal class SimpleHttpResponseException : WeaviateServerException
{
    public HttpStatusCode StatusCode { get; private set; }
    public ISet<HttpStatusCode> ExpectedStatusCodes { get; private set; } =
        new SortedSet<HttpStatusCode>();

    public SimpleHttpResponseException(
        HttpStatusCode statusCode,
        SortedSet<HttpStatusCode> expectedStatusCodes,
        string content
    )
        : base(content)
    {
        StatusCode = statusCode;
        ExpectedStatusCodes = expectedStatusCodes;
    }
}
