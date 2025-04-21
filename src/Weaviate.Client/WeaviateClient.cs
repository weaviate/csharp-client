using System.Diagnostics.CodeAnalysis;
using Weaviate.Client.Grpc;
using Weaviate.Client.Rest;

namespace Weaviate.Client;

public record ClientConfiguration
{
    public required Uri Host;
    public ushort RestPort = 8080;
    public ushort GrpcPort = 50051;
    public required string ApiKey;
}

public class WeaviateClient : IDisposable
{
    private bool _isDisposed = false;
    private readonly WeaviateGrpcClient _grpcClient;
    private readonly WeaviateRestClient _restClient;

    [NotNull]
    internal WeaviateRestClient RestClient => _restClient;
    [NotNull]
    internal WeaviateGrpcClient GrpcClient => _grpcClient;

    public ClientConfiguration Configuration { get; }

    public CollectionsClient Collections { get; }

    public WeaviateClient() : this(new ClientConfiguration()
    {
        Host = new Uri("http://localhost"),
        ApiKey = "",
    })
    {
    }

    public WeaviateClient(ClientConfiguration configuration)
    {
        Configuration = configuration;

        _restClient = new WeaviateRestClient(this);
        _grpcClient = new WeaviateGrpcClient(this);

        Collections = new CollectionsClient(this);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _grpcClient?.Dispose();
        RestClient?.Dispose();

        _isDisposed = true;
    }
}
