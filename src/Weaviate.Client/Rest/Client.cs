using System.Diagnostics;
using System.Net.Http.Json;
using Weaviate.Client.Rest.Dto;
using Weaviate.Client.Rest.Responses;

namespace Weaviate.Client.Rest;

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

public class ExpectedStatusCodes
{
    public List<int> Ok { get; private set; }
    public string Error { get; }

    public ExpectedStatusCodes(object okIn, string error)
    {
        Error = error;
        Ok = InitializeOk(okIn);
    }

    private List<int> InitializeOk(object okIn)
    {
        if (okIn is int singleCode)
        {
            return new List<int> { singleCode };
        }
        else if (okIn is List<int> codes)
        {
            return codes;
        }
        else
        {
            throw new ArgumentException("okIn must be either an int or a List<int>");
        }
    }
}

public class HttpClientWrapper : IDisposable
{
    private readonly bool _ownershipClient;
    private readonly HttpClient _httpClient;

    internal HttpClientWrapper(WeaviateClient client)
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

    internal HttpClientWrapper(WeaviateClient client, HttpClient httpClient)
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

    private HttpResponseMessage ValidateResponseStatusCode(HttpResponseMessage response, ExpectedStatusCodes expectedStatusCodes)
    {
        if (!expectedStatusCodes.Ok.Contains((int)response.StatusCode))
        {
            throw new HttpRequestException($"Unexpected status code: {response.StatusCode}. Expected one of: {string.Join(", ", expectedStatusCodes.Ok)}");
        }

        return response;
    }

    internal async Task<HttpResponseMessage> GetAsync(string requestUri, ExpectedStatusCodes expectedStatusCodes)
    {
        var response = await _httpClient.GetAsync(requestUri);

        return ValidateResponseStatusCode(response, expectedStatusCodes);
    }

    internal async Task<HttpResponseMessage> DeleteAsync(string requestUri, ExpectedStatusCodes expectedStatusCodes)
    {
        var response = await _httpClient.DeleteAsync(requestUri);

        return ValidateResponseStatusCode(response, expectedStatusCodes);
    }

    internal async Task<HttpResponseMessage> PostAsJsonAsync<TValue>(string? requestUri, TValue value, ExpectedStatusCodes expectedStatusCodes)
    {
        var response = await _httpClient.PostAsJsonAsync(requestUri, value);

        return ValidateResponseStatusCode(response, expectedStatusCodes);
    }

    internal async Task<HttpResponseMessage> PutAsJsonAsync<TValue>(string? requestUri, TValue value, ExpectedStatusCodes expectedStatusCodes)
    {
        var response = await _httpClient.PutAsJsonAsync(requestUri, value);

        return ValidateResponseStatusCode(response, expectedStatusCodes);
    }

    internal async Task<HttpResponseMessage> DeleteAsJsonAsync<TValue>(string? requestUri, TValue value, ExpectedStatusCodes expectedStatusCodes)
    {

        var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Content = JsonContent.Create(value, mediaType: null, null);

        var response = await _httpClient.SendAsync(request);

        return ValidateResponseStatusCode(response, expectedStatusCodes);
    }

}


public class WeaviateRestClient : IDisposable
{
    private readonly bool _ownershipClient;
    private readonly HttpClientWrapper _httpClient;

    internal WeaviateRestClient(WeaviateClient client)
    {
        _ownershipClient = true;
        _httpClient = new HttpClientWrapper(client);
    }

    internal WeaviateRestClient(WeaviateClient client, HttpClientWrapper httpClient)
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


    internal async Task<ListCollectionResponse> CollectionList()
    {
        var response = await _httpClient.GetAsync("schema", new ExpectedStatusCodes(new List<int> { 200 }, "collection list"));

        var contents = await response.Content.ReadFromJsonAsync<ListCollectionResponse>();

        return contents;
    }

    internal async Task<CollectionGeneric> CollectionGet(string name)
    {
        var response = await _httpClient.GetAsync($"schema/{name}", new ExpectedStatusCodes(new List<int> { 200 }, "collection get"));

        if (response.Content.Headers.ContentLength == 0)
        {
            return new CollectionGeneric()
            {
                Class = ""
            };
        }
        var contents = await response.Content.ReadFromJsonAsync<CollectionGeneric>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task CollectionDelete(object name)
    {
        await _httpClient.DeleteAsync($"schema/{name}", new ExpectedStatusCodes(new List<int> { 200 }, "collection delete"));
    }

    internal async Task<CollectionGeneric> CollectionCreate(CollectionGeneric collection)
    {
        var response = await _httpClient.PostAsJsonAsync($"schema", collection, new ExpectedStatusCodes(new List<int> { 200 }, "collection create"));

        var contents = await response.Content.ReadFromJsonAsync<CollectionGeneric>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return collection;
    }

    internal async Task<WeaviateObject> ObjectInsert(string collectionName, WeaviateObject data)
    {
        var response = await _httpClient.PostAsJsonAsync($"objects", data, new ExpectedStatusCodes(new List<int> { 200 }, "insert object"));

        var contents = await response.Content.ReadFromJsonAsync<WeaviateObject>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task DeleteObject(string collectionName, Guid id)
    {
        await _httpClient.DeleteAsync($"objects/{collectionName}/{id}", new ExpectedStatusCodes(new List<int> { 204, 404 }, "delete object"));
    }

    internal async Task ReferenceAdd(string collectionName, Guid from, string fromProperty, Guid to)
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";

        var beacons = DataClient<object>.MakeBeacons(to);
        var reference = beacons.First();

        var response = await _httpClient.PostAsJsonAsync(path, reference, new ExpectedStatusCodes(new List<int> { 200 }, "reference add"));
    }

    internal async Task ReferenceReplace(string collectionName, Guid from, string fromProperty, Guid[] to)
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";

        var beacons = DataClient<object>.MakeBeacons(to);
        var reference = beacons;

        var response = await _httpClient.PutAsJsonAsync(path, reference, new ExpectedStatusCodes(new List<int> { 200 }, "reference replace"));
    }


    internal async Task ReferenceDelete(string collectionName, Guid from, string fromProperty, Guid to)
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";

        var beacons = DataClient<object>.MakeBeacons(to);
        var reference = beacons.First();

        var response = await _httpClient.DeleteAsJsonAsync(path, reference, new ExpectedStatusCodes(new List<int> { 200 }, "reference delete"));
    }
}