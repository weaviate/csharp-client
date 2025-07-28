using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    internal Metadata? _defaultHeaders = null;
    private readonly V1.Weaviate.WeaviateClient _grpcClient;

    AsyncAuthInterceptor _AuthInterceptorFactory(string apiKey)
    {
        return (
            async (context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {apiKey}");
                await Task.CompletedTask;
            }
        );
    }

    public WeaviateGrpcClient(Uri grpcUri, string? apiKey = null, string? wcdHost = null)
    {
        var options = new GrpcChannelOptions();

        if (apiKey != null)
        {
            var credentials = CallCredentials.FromInterceptor(_AuthInterceptorFactory(apiKey));
            options.Credentials = ChannelCredentials.Create(new SslCredentials(), credentials);
        }

        _channel = GrpcChannel.ForAddress(grpcUri, options);
        var healthClient = new Health.HealthClient(_channel);
        var request = new HealthCheckRequest();
        try
        {
            var response = healthClient.Check(request);

            // Check if service is serving
            if (response.Status != HealthCheckResponse.Types.ServingStatus.Serving)
            {
                throw new WeaviateException(
                    "GRPC health check failed and "
                        + grpcUri.AbsoluteUri
                        + " is not reachable. Please check if the Weaviate instance is running and accessible. Details: "
                        + response.Status
                );
            }
        }
        catch (RpcException ex)
        {
            // Handle gRPC specific exceptions
            throw new WeaviateException(
                "GRPC health check failed and "
                    + grpcUri.AbsoluteUri
                    + " is not reachable. Please check if the Weaviate instance is running and accessible. Details:"
                    + ex.Status.Detail
            );
        }
        // Create default headers
        if (!string.IsNullOrEmpty(wcdHost))
        {
            _defaultHeaders = new Metadata { { "X-Weaviate-Cluster-URL", wcdHost } };
        }
        _grpcClient = new V1.Weaviate.WeaviateClient(_channel);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
