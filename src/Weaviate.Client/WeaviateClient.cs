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
    string? ApiKey = null
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

    public WeaviateClient Client() => new(this);
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

    public static ClientConfiguration DefaultOptions => _defaultOptions.Value;

    private bool _isDisposed = false;

    [NotNull]
    internal WeaviateRestClient RestClient { get; init; }

    [NotNull]
    internal WeaviateGrpcClient GrpcClient { get; init; }

    public ClientConfiguration Configuration { get; }

    public CollectionsClient Collections { get; }

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

        RestClient = new WeaviateRestClient(Configuration.RestUri, httpClient);
        GrpcClient = new WeaviateGrpcClient(Configuration.GrpcUri, Configuration.ApiKey);

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
