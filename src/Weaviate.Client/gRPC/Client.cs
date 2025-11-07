using System.Security.Authentication;
using Grpc.Core;
using Grpc.Core.Interceptors;
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
    private readonly TimeSpan? _timeout;
    private readonly RetryPolicy? _retryPolicy;

    /// <summary>
    /// Internal constructor for testing. Accepts a pre-configured GrpcChannel to bypass network initialization.
    /// </summary>
    internal WeaviateGrpcClient(
        GrpcChannel channel,
        string? wcdHost = null,
        TimeSpan? timeout = null,
        RetryPolicy? retryPolicy = null,
        Dictionary<string, string>? headers = null,
        ILogger<WeaviateGrpcClient>? logger = null
    )
    {
        _logger =
            logger
            ?? LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<WeaviateGrpcClient>();

        _timeout = timeoue;
        _retryPolicy = retryPolicy;
        _channel = channel;

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

        if (_retryPolicy is not null && _retryPolicy.MaxRetries > 0)
        {
            var invoker = _channel.Intercept(new RetryInterceptor(_retryPolicy, _logger));
            _grpcClient = new V1.Weaviate.WeaviateClient(invoker);
        }
        else
        {
            _grpcClient = new V1.Weaviate.WeaviateClient(_channel);
        }
    }

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

    /// <summary>
    /// Creates a WeaviateGrpcClient with a real network connection.
    /// </summary>
    public static WeaviateGrpcClient Create(
        Uri grpcUri,
        string? wcdHost,
        TimeSpan? timeout = null,
        ulong? maxMessageSize = null,
        RetryPolicy? retryPolicy = null,
        ITokenService? tokenService,
        Dictionary<string, string>? headers = null,
        ILogger<WeaviateGrpcClient>? logger = null
    )
    {
        var loggerInstance =
            logger
            ?? LoggerFactory
                .Create(builder => builder.AddConsole())
                .CreateLogger<WeaviateGrpcClient>();

        var channel = CreateChannel(grpcUri, tokenService, maxMessageSize, loggerInstance);

        // Perform health check
        PerformHealthCheck(channel);

        return new WeaviateGrpcClient(channel, wcdHost, timeout, retryPolicy, headers, logger);
    }

    private static GrpcChannel CreateChannel(
        Uri grpcUri,
        ITokenService? tokenService,
        ulong? maxMessageSize,
        ILogger<WeaviateGrpcClient> logger
    )
    {
        var options = new GrpcChannelOptions();

        if (maxMessageSize != null)
        {
            options.MaxReceiveMessageSize = (int)maxMessageSize;
            options.MaxSendMessageSize = (int)maxMessageSize;
        }

        if (tokenService != null)
        {
            var credentials = CallCredentials.FromInterceptor(
                async (context, metadata) =>
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
                        logger.LogError(ex, "Failed to retrieve access token");
                        return;
                    }
                }
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
                logger.LogWarning(
                    "Insecure HTTP connection specified. Consider using HTTPS for secure communication."
                );

                options.UnsafeUseInsecureChannelCallCredentials = true;
                options.Credentials = ChannelCredentials.Create(
                    ChannelCredentials.Insecure,
                    credentials
                );
            }
        }

        return GrpcChannel.ForAddress(grpcUri, options);
    }

    private void PerformHealthCheck(GrpcChannel channel)
    {
        var healthClient = new Health.HealthClient(channel);
        var request = new HealthCheckRequest();
        try
        {
            var response = healthClient.Check(request);

            // Check if service is serving
            if (response.Status != HealthCheckResponse.Types.ServingStatus.Serving)
            {
                channel.Dispose();
                throw new WeaviateClientException(
                    "GRPC health check failed and "
                        + channel.Target
                        + " is not reachable. Please check if the Weaviate instance is running and accessible. Details: "
                        + response.Status
                );
            }
        }
        catch (RpcException ex)
        {
            channel.Dispose();
            throw new WeaviateClientException(
                "GRPC health check failed and "
                    + channel.Target
                    + " is not reachable. Please check if the Weaviate instance is running and accessible. Details:"
                    + ex.Status.Detail,
                ex
            );
        }
    }

    /// <summary>
    /// Creates CallOptions with timeout and default headers.
    /// </summary>
    internal CallOptions CreateCallOptions(CancellationToken cancellationToken = default)
    {
        var options = new CallOptions(
            headers: _defaultHeaders,
            cancellationToken: cancellationToken
        );

        if (_timeout.HasValue)
        {
            options = options.WithDeadline(DateTime.UtcNow.Add(_timeout.Value));
        }

        return options;
    }

    public void Dispose()
    {
        _channel.Dispose();
    }
}
