namespace Weaviate.Client;

using System.Net.Http;

public partial class WeaviateClientBuilder
{
    private string _restEndpoint = "localhost";
    private string _restPath = "v1/";
    private string _grpcEndpoint = "localhost";
    private string _grpcPath = "";
    private ushort _restPort = 8080;
    private ushort _grpcPort = 50051;
    private bool _useSsl = false;
    private Dictionary<string, string> _headers = new();
    private ICredentials? _credentials = null;
    private HttpMessageHandler? _httpMessageHandler = null;
    private TimeSpan? _requestTimeout = null;
    private RetryPolicy? _retryPolicy = null;
    private readonly List<DelegatingHandler> _customHandlers = new();

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

    public static WeaviateClient Cloud(
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

    public WeaviateClientBuilder WithRestEndpoint(string endpoint)
    {
        _restEndpoint = endpoint;
        return this;
    }

    public WeaviateClientBuilder WithRestPath(string path)
    {
        _restPath = path;
        return this;
    }

    public WeaviateClientBuilder WithGrpcEndpoint(string endpoint)
    {
        _grpcEndpoint = endpoint;
        return this;
    }

    public WeaviateClientBuilder WithGrpcPath(string path)
    {
        _grpcPath = path;
        return this;
    }

    public WeaviateClientBuilder WithRestPort(ushort port)
    {
        _restPort = port;
        return this;
    }

    public WeaviateClientBuilder WithGrpcPort(ushort port)
    {
        _grpcPort = port;
        return this;
    }

    public WeaviateClientBuilder UseSsl(bool useSsl = true)
    {
        _useSsl = useSsl;
        return this;
    }

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

    public WeaviateClientBuilder WithHeader(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public WeaviateClientBuilder WithCredentials(ICredentials? credentials)
    {
        if (credentials == null)
            return this;

        _credentials = credentials;
        return this;
    }

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
    public WeaviateClientBuilder WithTimeout(TimeSpan timeout)
    {
        _requestTimeout = timeout;
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

    public WeaviateClient Build()
    {
        return new ClientConfiguration(
            _restEndpoint,
            _restPath,
            _grpcEndpoint,
            _grpcPath,
            _restPort,
            _grpcPort,
            _useSsl,
            _headers.Count > 0 ? new Dictionary<string, string>(_headers) : null,
            _credentials,
            _requestTimeout,
            _retryPolicy,
            _customHandlers.Count > 0 ? _customHandlers.ToArray() : null
        ).Client(_httpMessageHandler);
    }

    public static implicit operator WeaviateClient(WeaviateClientBuilder builder) =>
        builder.Build();
}
