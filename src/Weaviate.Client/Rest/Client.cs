using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Rest;

public partial class WeaviateRestClient : IDisposable
{
    private readonly bool _ownershipClient;

    private readonly HttpClient _httpClient;

    internal static readonly JsonSerializerOptions RestJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true, // For readability
        Converters =
        {
            new EnumMemberJsonConverterFactory(),
            new JsonStringEnumConverter(namingPolicy: JsonNamingPolicy.CamelCase),
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

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

    public void Dispose()
    {
        if (_ownershipClient)
        {
            _httpClient.Dispose();
        }
    }
}
