using System.Security.Authentication;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient : IDisposable
{
    private readonly GrpcChannel _channel;
    internal Metadata? _defaultHeaders = null;
    private readonly V1.Weaviate.WeaviateClient _grpcClient;
    private readonly ILogger<WeaviateGrpcClient> _logger;

    AsyncAuthInterceptor _AuthInterceptorFactory(ITokenService tokenService)
    {
        return async (context, metadata) =>
        {
            try
            {
                var token = await tokenService.GetAccessTokenAsync();

                if (tokenService.IsAuthenticated())
                {
                    metadata.Add("Authorization", $"Bearer {token}");
                }
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "Failed to retrieve access token");
                return;
            }
        };
    }

    public WeaviateGrpcClient(
        Uri grpcUri,
        string? wcdHost,
        ITokenService? tokenService,
        Dictionary<string, string>? headers = null,
        ILogger<WeaviateGrpcClient>? logger = null,
        ulong? maxMessageSize = null
    )
    {
        _logger =
            logger
            ?? LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<WeaviateGrpcClient>();

        var options = new GrpcChannelOptions();

        if (maxMessageSize != null)
        {
            options.MaxReceiveMessageSize = (int)maxMessageSize;

            options.MaxSendMessageSize = (int)maxMessageSize;
        }

        if (tokenService != null)
        {
            var credentials = CallCredentials.FromInterceptor(
                _AuthInterceptorFactory(tokenService)
            );

            if (grpcUri.Scheme == Uri.UriSchemeHttps)
            {
                options.Credentials = ChannelCredentials.Create(
                    ChannelCredentials.SecureSsl,
                    credentials
                );
            }
            else if (grpcUri.Scheme == Uri.UriSchemeHttp)
            {
                _logger.LogWarning(
                    "Insecure HTTP connection specified. Consider using HTTPS for secure communication."
                );

                options.UnsafeUseInsecureChannelCallCredentials = true;
                options.Credentials = ChannelCredentials.Create(
                    ChannelCredentials.Insecure,
                    credentials
                );
            }
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
                throw new WeaviateClientException(
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
            throw new WeaviateClientException(
                "GRPC health check failed and "
                    + grpcUri.AbsoluteUri
                    + " is not reachable. Please check if the Weaviate instance is running and accessible. Details:"
                    + ex.Status.Detail,
                ex
            );
        }

        // Create default headers
        if (!string.IsNullOrEmpty(wcdHost))
        {
            _defaultHeaders = new Metadata { { "X-Weaviate-Cluster-URL", wcdHost } };
        }
        if (headers != null)
        {
            if (_defaultHeaders == null)
            {
                _defaultHeaders = new Metadata();
            }

            foreach (var header in headers)
            {
                _defaultHeaders.Add(header.Key, header.Value);
            }
        }

        _grpcClient = new V1.Weaviate.WeaviateClient(_channel);
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
