using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

using System.Net.Http;

/// <summary>
/// The weaviate client builder class
/// </summary>
public partial class WeaviateClientBuilder
{
    /// <summary>
    /// The rest endpoint
    /// </summary>
    private string _restEndpoint = "localhost";

    /// <summary>
    /// The rest path
    /// </summary>
    private string _restPath = "v1/";

    /// <summary>
    /// The grpc endpoint
    /// </summary>
    private string _grpcEndpoint = "localhost";

    /// <summary>
    /// The grpc path
    /// </summary>
    private string _grpcPath = "";

    /// <summary>
    /// The rest port
    /// </summary>
    private ushort _restPort = 8080;

    /// <summary>
    /// The grpc port
    /// </summary>
    private ushort _grpcPort = 50051;

    /// <summary>
    /// The use ssl
    /// </summary>
    private bool _useSsl = false;

    /// <summary>
    /// The headers
    /// </summary>
    private Dictionary<string, string> _headers = new();

    /// <summary>
    /// The credentials
    /// </summary>
    private ICredentials? _credentials = null;

    /// <summary>
    /// The http message handler
    /// </summary>
    private HttpMessageHandler? _httpMessageHandler = null;

    /// <summary>
    /// The default timeout
    /// </summary>
    private TimeSpan? _defaultTimeout = null;

    /// <summary>
    /// The init timeout
    /// </summary>
    private TimeSpan? _initTimeout = null;

    /// <summary>
    /// The insert timeout
    /// </summary>
    private TimeSpan? _insertTimeout = null;

    /// <summary>
    /// The query timeout
    /// </summary>
    private TimeSpan? _queryTimeout = null;

    /// <summary>
    /// The retry policy
    /// </summary>
    private RetryPolicy? _retryPolicy = null;

    /// <summary>
    /// The custom handlers
    /// </summary>
    private readonly List<DelegatingHandler> _customHandlers = new();

    /// <summary>
    /// The logger factory
    /// </summary>
    private ILoggerFactory? _loggerFactory = null;

    /// <summary>
    /// Whether request logging is enabled
    /// </summary>
    private bool _logRequests = false;

    /// <summary>
    /// The log level for request/response entries
    /// </summary>
    private LogLevel _requestLoggingLevel = LogLevel.Debug;

    /// <summary>
    /// Customs the rest endpoint
    /// </summary>
    /// <param name="restEndpoint">The rest endpoint</param>
    /// <param name="restPath">The rest path</param>
    /// <param name="grpcEndpoint">The grpc endpoint</param>
    /// <param name="grpcPath">The grpc path</param>
    /// <param name="restPort">The rest port</param>
    /// <param name="grpcPort">The grpc port</param>
    /// <param name="useSsl">The use ssl</param>
    /// <param name="headers">The headers</param>
    /// <param name="credentials">The credentials</param>
    /// <param name="httpMessageHandler">The http message handler</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder Custom(
        string restEndpoint = "localhost",
        string restPath = "v1/",
        string grpcEndpoint = "localhost",
        string grpcPath = "",
        string restPort = "8080",
        string grpcPort = "50051",
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        ICredentials? credentials = null,
        HttpMessageHandler? httpMessageHandler = null
    )
    {
        return new WeaviateClientBuilder()
            .WithRestEndpoint(restEndpoint)
            .WithRestPath(restPath)
            .WithGrpcEndpoint(grpcEndpoint)
            .WithGrpcPath(grpcPath)
            .WithRestPort(Convert.ToUInt16(restPort))
            .WithGrpcPort(Convert.ToUInt16(grpcPort))
            .UseSsl(useSsl)
            .WithHeaders(headers)
            .WithCredentials(credentials)
            .WithHttpMessageHandler(httpMessageHandler);
    }

    /// <summary>
    /// Locals the credentials
    /// </summary>
    /// <param name="credentials">The credentials</param>
    /// <param name="hostname">The hostname</param>
    /// <param name="restPort">The rest port</param>
    /// <param name="grpcPort">The grpc port</param>
    /// <param name="useSsl">The use ssl</param>
    /// <param name="headers">The headers</param>
    /// <param name="httpMessageHandler">The http message handler</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder Local(
        ICredentials? credentials = null,
        string hostname = "localhost",
        ushort restPort = 8080,
        ushort grpcPort = 50051,
        bool useSsl = false,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        new WeaviateClientBuilder()
            .WithRestEndpoint(hostname)
            .WithGrpcEndpoint(hostname)
            .WithRestPort(restPort)
            .WithGrpcPort(grpcPort)
            .UseSsl(useSsl)
            .WithCredentials(credentials ?? null)
            .WithHttpMessageHandler(httpMessageHandler)
            .WithHeaders(headers);

    /// <summary>
    /// Clouds the rest endpoint
    /// </summary>
    /// <param name="restEndpoint">The rest endpoint</param>
    /// <param name="apiKey">The api key</param>
    /// <param name="headers">The headers</param>
    /// <param name="httpMessageHandler">The http message handler</param>
    /// <returns>The weaviate client builder</returns>
    public static WeaviateClientBuilder Cloud(
        string restEndpoint,
        string? apiKey = null,
        Dictionary<string, string>? headers = null,
        HttpMessageHandler? httpMessageHandler = null
    ) =>
        new WeaviateClientBuilder()
            .WithRestEndpoint(restEndpoint)
            .WithGrpcEndpoint($"grpc-{restEndpoint}")
            .WithRestPort(443)
            .WithGrpcPort(443)
            .UseSsl(true)
            .WithCredentials(string.IsNullOrEmpty(apiKey) ? null : Auth.ApiKey(apiKey))
            .WithHeaders(headers)
            .WithHttpMessageHandler(httpMessageHandler);

    /// <summary>
    /// Adds the rest endpoint using the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithRestEndpoint(string endpoint)
    {
        _restEndpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Adds the rest path using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithRestPath(string path)
    {
        _restPath = path;
        return this;
    }

    /// <summary>
    /// Adds the grpc endpoint using the specified endpoint
    /// </summary>
    /// <param name="endpoint">The endpoint</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithGrpcEndpoint(string endpoint)
    {
        _grpcEndpoint = endpoint;
        return this;
    }

    /// <summary>
    /// Adds the grpc path using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithGrpcPath(string path)
    {
        _grpcPath = path;
        return this;
    }

    /// <summary>
    /// Adds the rest port using the specified port
    /// </summary>
    /// <param name="port">The port</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithRestPort(ushort port)
    {
        _restPort = port;
        return this;
    }

    /// <summary>
    /// Adds the grpc port using the specified port
    /// </summary>
    /// <param name="port">The port</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithGrpcPort(ushort port)
    {
        _grpcPort = port;
        return this;
    }

    /// <summary>
    /// Uses the ssl using the specified use ssl
    /// </summary>
    /// <param name="useSsl">The use ssl</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder UseSsl(bool useSsl = true)
    {
        _useSsl = useSsl;
        return this;
    }

    /// <summary>
    /// Adds the headers using the specified headers
    /// </summary>
    /// <param name="headers">The headers</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithHeaders(Dictionary<string, string>? headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                _headers[header.Key] = header.Value;
            }
        }

        return this;
    }

    /// <summary>
    /// Adds the header using the specified key
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="value">The value</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithHeader(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    /// <summary>
    /// Adds the credentials using the specified credentials
    /// </summary>
    /// <param name="credentials">The credentials</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithCredentials(ICredentials? credentials)
    {
        if (credentials == null)
            return this;

        _credentials = credentials;
        return this;
    }

    /// <summary>
    /// Adds the http message handler using the specified handler
    /// </summary>
    /// <param name="handler">The handler</param>
    /// <returns>The weaviate client builder</returns>
    public WeaviateClientBuilder WithHttpMessageHandler(HttpMessageHandler? handler)
    {
        if (handler == null)
            return this;

        _httpMessageHandler = handler;
        return this;
    }

    /// <summary>
    /// Sets the request timeout for both REST and gRPC operations.
    /// If not set, defaults to WeaviateDefaults.DefaultTimeout (30 seconds).
    /// </summary>
    /// <param name="timeout">The timeout duration for requests.</param>
    /// <summary>
    /// Sets the default timeout for all requests.
    /// </summary>
    public WeaviateClientBuilder WithDefaultTimeout(TimeSpan timeout)
    {
        _defaultTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the timeout for client initialization operations (GetMeta, Live, IsReady).
    /// </summary>
    public WeaviateClientBuilder WithInitTimeout(TimeSpan timeout)
    {
        _initTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the timeout for data operations.
    /// </summary>
    public WeaviateClientBuilder WithInsertTimeout(TimeSpan timeout)
    {
        _insertTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets the timeout for query/search operations.
    /// </summary>
    public WeaviateClientBuilder WithQueryTimeout(TimeSpan timeout)
    {
        _queryTimeout = timeout;
        return this;
    }

    /// <summary>
    /// Sets all timeout values at once. Any null values are skipped.
    /// </summary>
    public WeaviateClientBuilder ApplyTimeouts(
        TimeSpan? defaultTimeout = null,
        TimeSpan? initTimeout = null,
        TimeSpan? insertTimeout = null,
        TimeSpan? queryTimeout = null
    )
    {
        if (defaultTimeout.HasValue)
            WithDefaultTimeout(defaultTimeout.Value);
        if (initTimeout.HasValue)
            WithInitTimeout(initTimeout.Value);
        if (insertTimeout.HasValue)
            WithInsertTimeout(insertTimeout.Value);
        if (queryTimeout.HasValue)
            WithQueryTimeout(queryTimeout.Value);
        return this;
    }

    /// <summary>
    /// Sets the retry policy for transient failures.
    /// </summary>
    public WeaviateClientBuilder WithRetryPolicy(RetryPolicy policy)
    {
        _retryPolicy = policy;
        return this;
    }

    /// <summary>
    /// Disables retries.
    /// </summary>
    public WeaviateClientBuilder WithoutRetries()
    {
        _retryPolicy = RetryPolicy.None;
        return this;
    }

    /// <summary>
    /// Adds a custom delegating handler that will wrap the HTTP pipeline.
    /// Handlers are applied in the order added (each new handler becomes the outer-most).
    /// The retry handler, if enabled, will still be the outer-most wrapper.
    /// </summary>
    /// <remarks>
    /// If the handler's InnerHandler is already set it will not be overwritten.
    /// </remarks>
    public WeaviateClientBuilder AddHandler(DelegatingHandler handler)
    {
        if (handler != null)
        {
            _customHandlers.Add(handler);
        }
        return this;
    }

    /// <summary>
    /// Configures the logger factory used by all internal components.
    /// When set, typed loggers are created from this factory instead of the default silent NullLoggerFactory.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use.</param>
    public WeaviateClientBuilder WithLoggerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        return this;
    }

    /// <summary>
    /// Enables request/response logging for both HTTP and gRPC transports.
    /// Logs method, URI/method name, status code, and elapsed time.
    /// Authorization header values are redacted from HTTP logs.
    /// Requires a logger factory to be configured via <see cref="WithLoggerFactory"/>.
    /// </summary>
    /// <param name="level">The log level for request/response entries. Defaults to Debug.</param>
    public WeaviateClientBuilder UseRequestLogging(LogLevel level = LogLevel.Debug)
    {
        _logRequests = true;
        _requestLoggingLevel = level;
        return this;
    }

    /// <summary>
    /// Builds a WeaviateClient asynchronously with all services properly initialized.
    /// This is the recommended way to create clients.
    /// </summary>
    public async Task<WeaviateClient> BuildAsync()
    {
        var config = new ClientConfiguration(
            _restEndpoint,
            _restPath,
            _grpcEndpoint,
            _grpcPath,
            _restPort,
            _grpcPort,
            _useSsl,
            _headers.Count > 0 ? new Dictionary<string, string>(_headers) : null,
            _credentials,
            _defaultTimeout,
            _initTimeout,
            _insertTimeout,
            _queryTimeout,
            _retryPolicy,
            _customHandlers.Count > 0 ? _customHandlers.ToArray() : null,
            _httpMessageHandler,
            _loggerFactory,
            _logRequests,
            _requestLoggingLevel
        );

        return await config.BuildAsync();
    }

    /// <summary>
    /// Implicitly converts a WeaviateClientBuilder to a Task that builds the client
    /// </summary>
    /// <param name="builder">The builder</param>
    public static implicit operator Task<WeaviateClient>(WeaviateClientBuilder builder) =>
        builder.BuildAsync();
}
