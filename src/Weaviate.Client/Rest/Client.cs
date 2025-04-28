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

        if (response.Content != null)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            _log($"Response Content: {responseContent}");
        }

        foreach (var header in response.Headers)
        {
            _log($"Response Header: {header.Key}: {string.Join(", ", header.Value)}");
        }

        return response;
    }
}

public class WeaviateRestClient : IDisposable
{
    private readonly bool _ownershipClient;
    private readonly HttpClient _httpClient;
    private WeaviateClient _client;

    internal WeaviateRestClient(WeaviateClient client)
    {
        _client = client;
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
        _client = client;
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
        var response = await _httpClient.GetAsync("schema");

        var contents = await response.Content.ReadFromJsonAsync<ListCollectionResponse>();

        return contents;
    }

    internal async Task<CollectionGeneric> CollectionGet(string name)
    {
        var response = await _httpClient.GetAsync($"schema/{name}");

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
        await _httpClient.DeleteAsync($"schema/{name}");
    }

    internal async Task<CollectionGeneric> CollectionCreate(CollectionGeneric collection)
    {
        var response = await _httpClient.PostAsJsonAsync($"schema", collection);

        var contents = await response.Content.ReadFromJsonAsync<CollectionGeneric>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return collection;
    }

    internal async Task<WeaviateObject> ObjectInsert(string collectionName, WeaviateObject data)
    {
        var response = await _httpClient.PostAsJsonAsync($"objects", data);

        var contents = await response.Content.ReadFromJsonAsync<WeaviateObject>();

        if (contents is null)
        {
            throw new WeaviateRestException();
        }

        return contents;
    }

    internal async Task DeleteObject(string collectionName, Guid id)
    {
        await _httpClient.DeleteAsync($"objects/{collectionName}/{id}");
    }
}