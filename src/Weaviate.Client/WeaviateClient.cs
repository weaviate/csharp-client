using System.Diagnostics.CodeAnalysis;
using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Weaviate.Client.Tests")]

namespace Weaviate.Client;

public sealed record ClientConfiguration(
    string RestAddress = "localhost",
    string GrpcAddress = "localhost",
    ushort RestPort = 8080,
    ushort GrpcPort = 50051,
    bool UseSsl = false,
    string? ApiKey = null,
    bool AddEmbeddingHeader = false
)
{
    public Uri RestUri =>
        new UriBuilder()
        {
            Host = RestAddress,
            Scheme = UseSsl ? "https" : "http",
            Port = RestPort,
            Path = "v1/",
        }.Uri;

    public Uri GrpcUri =>
        new UriBuilder()
        {
            Host = GrpcAddress,
            Scheme = UseSsl ? "https" : "http",
            Port = GrpcPort,
            Path = "",
        }.Uri;

    public WeaviateClient Client(HttpClient? baseClient = null) => new(this, baseClient);
};

public class WeaviateClient : IDisposable
{
    private static readonly Lazy<ClientConfiguration> _defaultOptions = new(() =>
        new()
        {
            ApiKey = null,
            RestPort = 8080,
            GrpcPort = 50051,
        }
    );

    public async Task<MetaInfo> GetMeta()
    {
        return await RestClient.GetMeta();
    }

    public static ClientConfiguration DefaultOptions => _defaultOptions.Value;

    private bool _isDisposed = false;

    [NotNull]
    internal WeaviateRestClient RestClient { get; init; }

    [NotNull]
    internal WeaviateGrpcClient GrpcClient { get; init; }

    public ClientConfiguration Configuration { get; }

    public CollectionsClient Collections { get; }

    static bool IsWeaviateDomain(string url)
    {
        return url.ToLower().Contains("weaviate.io")
            || url.ToLower().Contains("semi.technology")
            || url.ToLower().Contains("weaviate.cloud");
    }

    public WeaviateClient(ClientConfiguration? configuration = null, HttpClient? httpClient = null)
    {
        Configuration = configuration ?? DefaultOptions;

        httpClient ??= new HttpClient();

        if (!string.IsNullOrEmpty(Configuration.ApiKey))
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer",
                    Configuration.ApiKey
                );
        }

        var wcdHost =
            (Configuration.AddEmbeddingHeader && IsWeaviateDomain(Configuration.RestAddress))
                ? Configuration.RestAddress
                : null;

        if (wcdHost != null)
        {
            httpClient.DefaultRequestHeaders.Add(
                "X-Weaviate-Cluster-URL",
                Configuration.RestUri.ToString()
            );
        }

        RestClient = new WeaviateRestClient(Configuration.RestUri, httpClient);
        GrpcClient = new WeaviateGrpcClient(Configuration.GrpcUri, Configuration.ApiKey, wcdHost);

        Collections = new CollectionsClient(this);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        GrpcClient?.Dispose();
        RestClient?.Dispose();

        _isDisposed = true;
    }
}
