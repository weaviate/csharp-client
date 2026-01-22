using System.Text.Json;
using System.Text.Json.Serialization;

namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate rest client class
/// </summary>
/// <seealso cref="IDisposable"/>
internal partial class WeaviateRestClient : IDisposable
{
    /// <summary>
    /// The ownership client
    /// </summary>
    private readonly bool _ownershipClient;

    /// <summary>
    /// The http client
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// The when writing null
    /// </summary>
    internal static readonly JsonSerializerOptions RestJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        // WriteIndented = true, // For readability
        // Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="WeaviateRestClient"/> class
    /// </summary>
    /// <param name="restUri">The rest uri</param>
    /// <param name="httpClient">The http client</param>
    internal WeaviateRestClient(Uri restUri, HttpClient? httpClient = null)
    {
        if (httpClient is null)
        {
            httpClient = new HttpClient();
            _ownershipClient = true;
        }

        _httpClient = httpClient;
        _httpClient.BaseAddress = restUri;
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        if (_ownershipClient)
        {
            _httpClient.Dispose();
        }
    }
}
