using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;

namespace Weaviate.Client.Rest;

public static class HttpResponseMessageExtensions
{
    public static async Task EnsureExpectedStatusCodeAsync(this HttpResponseMessage response, SortedSet<HttpStatusCode> codes, string error = "")
    {
        if (codes.Contains(response.StatusCode))
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync();

        if (response.Content != null)
            response.Content.Dispose();

        content = $"Unexpected status code {response.StatusCode}. Expected: {string.Join(", ", codes)}. {error}. Server replied: {content}";

        throw new SimpleHttpResponseException(response.StatusCode, codes, content);
    }

    public static async Task EnsureExpectedStatusCodeAsync(this HttpResponseMessage response, SortedSet<int> codes, string error = "")
    {
        await EnsureExpectedStatusCodeAsync(response, [.. codes.Select(x => (HttpStatusCode)x)], error);
    }

    public static async Task EnsureExpectedStatusCodeAsync(this HttpResponseMessage response, int code, string error = "")
    {
        await EnsureExpectedStatusCodeAsync(response, [(HttpStatusCode)code], error);
    }

    public static async Task EnsureExpectedStatusCodeAsync(this HttpResponseMessage response, HttpStatusCode code, string error = "")
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
    public ISet<HttpStatusCode> ExpectedStatusCodes { get; private set; } = new SortedSet<HttpStatusCode>();

    public SimpleHttpResponseException(HttpStatusCode statusCode, SortedSet<HttpStatusCode> expectedStatusCodes, string content) : base(content)
    {
        StatusCode = statusCode;
        ExpectedStatusCodes = expectedStatusCodes;
    }
}

public class LoggingHandler : DelegatingHandler
{
    private readonly Action<string> _log;

    public LoggingHandler(Action<string> log)
    {
        _log = log;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _log($"Request: {request.Method} {request.RequestUri}");

        if (request.Content != null)
        {
            var requestContent = await request.Content.ReadAsStringAsync();
            _log($"Request Content: {requestContent}");

            // Buffer the content so it can be read again.
            request.Content = new StringContent(requestContent, System.Text.Encoding.UTF8, "application/json");
        }

        foreach (var header in request.Headers)
        {
            _log($"Request Header: {header.Key}: {string.Join(", ", header.Value)}");
        }

        var response = await base.SendAsync(request, cancellationToken);

        _log($"Response: {response.StatusCode}");

        foreach (var header in response.Headers)
        {
            _log($"Response Header: {header.Key}: {string.Join(", ", header.Value)}");
        }

        if (response.Content != null)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _log($"Response Content: {responseContent}");
        }

        return response;
    }
}

public class WeaviateRestClient : IDisposable
{
    private readonly bool _ownershipClient;
    private readonly HttpClient _httpClient;

    internal WeaviateRestClient(WeaviateClient client)
    {
        _ownershipClient = true;

        _httpClient = new HttpClient(new LoggingHandler(str =>
        {
            Debug.WriteLine(str);
        })
        {
            InnerHandler = new HttpClientHandler() // or SocketsHttpHandler
        });

        var ub = new UriBuilder(client.Configuration.Host);

        ub.Port = client.Configuration.RestPort;
        ub.Path = "v1/";

        _httpClient.BaseAddress = ub.Uri;
    }

    internal WeaviateRestClient(WeaviateClient client, HttpClient httpClient)
    {
        _httpClient = httpClient;
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
        var response = await _httpClient.GetAsync("schema");

        await response.EnsureExpectedStatusCodeAsync([200], "collection list");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Schema>();

        return contents;
    }

    internal async Task<Dto.Class?> CollectionGet(string name)
    {
        var response = await _httpClient.GetAsync($"schema/{name}");

        await response.EnsureExpectedStatusCodeAsync([200], "collection get");

        if (response.Content.Headers.ContentLength == 0)
        {
            return default;
        }

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task CollectionDelete(string name)
    {
        var response = await _httpClient.DeleteAsync($"schema/{name}");

        await response.EnsureExpectedStatusCodeAsync([200], "collection delete");
    }

    internal async Task<Dto.Class> CollectionCreate(Dto.Class collection)
    {
        var response = await _httpClient.PostAsJsonAsync($"schema", collection);

        await response.EnsureExpectedStatusCodeAsync([200], "collection create");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Class>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return collection;
    }

    internal async Task<Dto.Object> ObjectInsert(string collectionName, Dto.Object data)
    {
        var response = await _httpClient.PostAsJsonAsync($"objects", data);

        await response.EnsureExpectedStatusCodeAsync([200], "insert object");

        var contents = await response.Content.ReadFromJsonAsync<Dto.Object>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task DeleteObject(string collectionName, Guid id)
    {
        var response = await _httpClient.DeleteAsync($"objects/{collectionName}/{id}");

        await response.EnsureExpectedStatusCodeAsync([204, 404], "delete object");
    }

    internal async Task ReferenceAdd(string collectionName, Guid from, string fromProperty, Guid to)
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";

        var beacons = DataClient<object>.MakeBeacons(to);
        var reference = beacons.First();

        var response = await _httpClient.PostAsJsonAsync(path, reference);

        await response.EnsureExpectedStatusCodeAsync([200], "reference add");
    }

    internal async Task ReferenceReplace(string collectionName, Guid from, string fromProperty, Guid[] to)
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";

        var beacons = DataClient<object>.MakeBeacons(to);
        var reference = beacons;

        var response = await _httpClient.PutAsJsonAsync(path, reference);

        await response.EnsureExpectedStatusCodeAsync([200], "reference replace");
    }


    internal async Task ReferenceDelete(string collectionName, Guid from, string fromProperty, Guid to)
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";

        var beacons = DataClient<object>.MakeBeacons(to);
        var reference = beacons.First();

        var request = new HttpRequestMessage(HttpMethod.Delete, path);
        request.Content = JsonContent.Create(reference, mediaType: null, null);

        var response = await _httpClient.SendAsync(request);

        await response.EnsureExpectedStatusCodeAsync([200], "reference delete");
    }
}