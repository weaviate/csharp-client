using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Weaviate.Client.Rest.Dto;

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

public class SimpleHttpResponseException : Exception
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

public class WeaviateRestClient : IDisposable
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

    internal async Task<Dto.Schema?> CollectionList()
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Collection());

        await response.EnsureExpectedStatusCodeAsync([200], "collection list");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Schema>(
            options: RestJsonSerializerOptions
        );

        return contents;
    }

    internal async Task<Dto.Class?> CollectionGet(string name)
    {
        var response = await _httpClient.GetAsync(WeaviateEndpoints.Collection(name));

        await response.EnsureExpectedStatusCodeAsync([200], "collection get");

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>(
            options: RestJsonSerializerOptions
        );

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task CollectionDelete(string name)
    {
        var response = await _httpClient.DeleteAsync(WeaviateEndpoints.Collection(name));

        await response.EnsureExpectedStatusCodeAsync([200], "collection delete");
    }

    internal async Task<Dto.Class> CollectionCreate(Dto.Class collection)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Collection(),
            collection,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "collection create");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>(
            options: RestJsonSerializerOptions
        );

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task<Dto.Object> ObjectInsert(string collectionName, Dto.Object data)
    {
        var response = await _httpClient.PostAsJsonAsync(
            WeaviateEndpoints.Objects(),
            data,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "insert object");

        return await response.Content.ReadFromJsonAsync<Dto.Object>()
            ?? throw new WeaviateRestException();
    }

    internal async Task DeleteObject(string collectionName, Guid id)
    {
        var response = await _httpClient.DeleteAsync(
            WeaviateEndpoints.CollectionObject(collectionName, id)
        );

        await response.EnsureExpectedStatusCodeAsync([204, 404], "delete object");
    }

    internal async Task ReferenceAdd(string collectionName, Guid from, string fromProperty, Guid to)
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty);

        var beacons = ObjectHelper.MakeBeacons(to);
        var reference = beacons.First();

        var response = await _httpClient.PostAsJsonAsync(
            path,
            reference,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "reference add");
    }

    internal async Task ReferenceReplace(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid[] to
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty);

        var beacons = ObjectHelper.MakeBeacons(to);
        var reference = beacons;

        var response = await _httpClient.PutAsJsonAsync(
            path,
            reference,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "reference replace");
    }

    internal async Task ReferenceDelete(
        string collectionName,
        Guid from,
        string fromProperty,
        Guid to
    )
    {
        var path = WeaviateEndpoints.Reference(collectionName, from, fromProperty);

        var beacons = ObjectHelper.MakeBeacons(to);
        var reference = beacons.First();

        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        request.Content = JsonContent.Create(
            reference,
            mediaType: null,
            options: RestJsonSerializerOptions
        );

        var response = await _httpClient.SendAsync(request);

        await response.EnsureExpectedStatusCodeAsync([200], "reference delete");
    }

    internal async Task CollectionAddProperty(string collectionName, Property property)
    {
        var path = WeaviateEndpoints.CollectionProperties(collectionName);

        var response = await _httpClient.PostAsJsonAsync(
            path,
            property,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "collection property add");
    }

    internal async Task<BatchReferenceResponse[]> ReferenceAddMany(
        string collectionName,
        Models.DataReference[] references
    )
    {
        var batchRefs = references.SelectMany(r =>
            ObjectHelper
                .MakeBeacons(r.To)
                .SelectMany(b => b.Values)
                .Select(beacon => new BatchReference
                {
                    From = new Uri(
                        ObjectHelper.MakeBeaconSource(collectionName, r.From, r.FromProperty)
                    ),
                    To = new Uri(beacon),
                })
        );

        var path = WeaviateEndpoints.ReferencesAdd();

        var response = await _httpClient.PostAsJsonAsync(
            path,
            batchRefs,
            options: RestJsonSerializerOptions
        );

        await response.EnsureExpectedStatusCodeAsync([200], "reference add many");

        return await response.Content.ReadFromJsonAsync<BatchReferenceResponse[]>(
                RestJsonSerializerOptions
            ) ?? throw new WeaviateRestException();
    }

    internal async Task<bool> CollectionExists(object collectionName)
    {
        var path = WeaviateEndpoints.Collection();

        var response = await _httpClient.GetAsync(path);

        await response.EnsureExpectedStatusCodeAsync([200], "collection property add");

        var schema = await response.Content.ReadFromJsonAsync<Schema>(
            options: RestJsonSerializerOptions
        );

        return schema?.Classes?.Any(c => c.Class1 is not null && c.Class1!.Equals(collectionName))
            ?? false;
    }

    internal async Task<Dto.Meta?> GetMeta()
    {
        var path = WeaviateEndpoints.Meta();

        var response = await _httpClient.GetAsync(path);

        await response.EnsureExpectedStatusCodeAsync([200], "get meta endpoint");

        var meta = await response.Content.ReadFromJsonAsync<Meta>(
            options: RestJsonSerializerOptions
        );

        return meta;
    }
}
