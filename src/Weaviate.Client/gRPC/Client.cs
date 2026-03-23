using System.Security.Authentication;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Health.V1;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Weaviate.Client.Grpc;

/// <summary>
/// The weaviate grpc client class
/// </summary>
/// <seealso cref="IDisposable"/>
internal partial class WeaviateGrpcClient : IDisposable
{
    /// <summary>
    /// The channel
    /// </summary>
    private readonly GrpcChannel _channel;

    /// <summary>
    /// The default headers
    /// </summary>
    internal Metadata? _defaultHeaders = null;

    /// <summary>
    /// The grpc client
    /// </summary>
    private readonly Protobuf.V1.Weaviate.WeaviateClient _grpcClient;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger<WeaviateGrpcClient> _logger;

    /// <summary>
    /// The timeout
    /// </summary>
    private readonly TimeSpan? _timeout;

    /// <summary>
    /// The retry policy
    /// </summary>
    private readonly RetryPolicy? _retryPolicy;

    /// <summary>
    /// Whether to use alpha param property or pass a default value (0.7f) for hybrid searches.
    /// Set to true for Weaviate server versions 1.35 and earlier for backward compatibility.
    /// </summary>
    private bool _useAlphaParam = false;

    /// <summary>
    /// Sets whether to use alpha param property for hybrid searches.
    /// </summary>
    internal void SetUseAlphaParam(bool useParam)
    {
        _useAlphaParam = useParam;
    }

    /// <summary>
    /// Internal constructor for testing. Accepts a pre-configured GrpcChannel to bypass network initialization.
    /// </summary>
    internal WeaviateGrpcClient(
        GrpcChannel channel,
        string? wcdHost = null,
        TimeSpan? timeout = null,
        RetryPolicy? retryPolicy = null,
        Dictionary<string, string>? headers = null,
        ILogger<WeaviateGrpcClient>? logger = null,
        ILoggerFactory? loggerFactory = null,
        bool logRequests = false,
        LogLevel requestLoggingLevel = LogLevel.Debug
    )
    {
        var factory = loggerFactory ?? NullLoggerFactory.Instance;
        _logger = logger ?? factory.CreateLogger<WeaviateGrpcClient>();

        _timeout = timeout;
        _retryPolicy = retryPolicy;
        _channel = channel;

        // Always initialize default headers
        _defaultHeaders = new Metadata();

        if (!string.IsNullOrEmpty(wcdHost))
        {
            _defaultHeaders.Add("X-Weaviate-Cluster-URL", wcdHost);
        }

        _defaultHeaders.Add("X-Weaviate-Client", WeaviateDefaults.UserAgent);

        if (headers != null)
        {
            foreach (var header in headers)
            {
                _defaultHeaders.Add(header.Key, header.Value);
            }
        }

        CallInvoker invoker = _channel.CreateCallInvoker();

        if (_retryPolicy is not null && _retryPolicy.MaxRetries > 0)
        {
            invoker = invoker.Intercept(new RetryInterceptor(_retryPolicy, _logger));
        }

        if (logRequests)
        {
            invoker = invoker.Intercept(new LoggingInterceptor(factory, requestLoggingLevel));
        }

        _grpcClient = new Protobuf.V1.Weaviate.WeaviateClient(invoker);
    }

    /// <summary>
    /// Auths the interceptor factory using the specified token service
    /// </summary>
    /// <param name="tokenService">The token service</param>
    /// <param name="logger">The logger</param>
    /// <returns>The async auth interceptor</returns>
    static AsyncAuthInterceptor _AuthInterceptorFactory(ITokenService tokenService, ILogger logger)
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
                logger.LogError(ex, "Failed to retrieve access token");
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
        ITokenService? tokenService,
        TimeSpan? timeout = null,
        ulong? maxMessageSize = null,
        RetryPolicy? retryPolicy = null,
        Dictionary<string, string>? headers = null,
        ILoggerFactory? loggerFactory = null,
        bool logRequests = false,
        LogLevel requestLoggingLevel = LogLevel.Debug
    )
    {
        var factory = loggerFactory ?? NullLoggerFactory.Instance;
        var loggerInstance = factory.CreateLogger<WeaviateGrpcClient>();

        var channel = CreateChannel(grpcUri, tokenService, maxMessageSize, loggerInstance, factory);

        // Perform health check
        PerformHealthCheck(channel);

        return new WeaviateGrpcClient(
            channel,
            wcdHost,
            timeout,
            retryPolicy,
            headers,
            loggerInstance,
            factory,
            logRequests,
            requestLoggingLevel
        );
    }

    /// <summary>
    /// Creates the channel using the specified grpc uri
    /// </summary>
    /// <param name="grpcUri">The grpc uri</param>
    /// <param name="tokenService">The token service</param>
    /// <param name="maxMessageSize">The max message size</param>
    /// <param name="logger">The logger</param>
    /// <param name="loggerFactory">Optional factory to attach to GrpcChannelOptions for built-in channel logging.</param>
    /// <returns>The grpc channel</returns>
    private static GrpcChannel CreateChannel(
        Uri grpcUri,
        ITokenService? tokenService,
        ulong? maxMessageSize,
        ILogger<WeaviateGrpcClient> logger,
        ILoggerFactory? loggerFactory = null
    )
    {
        var options = new GrpcChannelOptions();

        // Attach logger factory so the gRPC channel emits its own built-in diagnostics
        if (loggerFactory is not null)
        {
            options.LoggerFactory = loggerFactory;
        }

        if (maxMessageSize != null)
        {
            options.MaxReceiveMessageSize = (int)maxMessageSize;
            options.MaxSendMessageSize = (int)maxMessageSize;
        }

        if (tokenService != null)
        {
            var credentials = CallCredentials.FromInterceptor(
                _AuthInterceptorFactory(tokenService, logger)
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

    /// <summary>
    /// Performs the health check using the specified channel
    /// </summary>
    /// <param name="channel">The channel</param>
    /// <exception cref="WeaviateClientException"></exception>
    /// <exception cref="WeaviateClientException"></exception>
    private static void PerformHealthCheck(GrpcChannel channel)
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

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        _channel.Dispose();
    }
}
