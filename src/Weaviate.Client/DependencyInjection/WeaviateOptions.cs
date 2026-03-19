using Microsoft.Extensions.Logging;

namespace Weaviate.Client.DependencyInjection;

/// <summary>
/// Configuration options for Weaviate client when using dependency injection.
/// </summary>
public class WeaviateOptions
{
    /// <summary>
    /// REST endpoint address. Default is "localhost".
    /// </summary>
    public string RestEndpoint { get; set; } = "localhost";

    /// <summary>
    /// REST API path. Default is "v1/".
    /// </summary>
    public string RestPath { get; set; } = "v1/";

    /// <summary>
    /// gRPC endpoint address. Default is "localhost".
    /// </summary>
    public string GrpcEndpoint { get; set; } = "localhost";

    /// <summary>
    /// gRPC path. Default is empty.
    /// </summary>
    public string GrpcPath { get; set; } = "";

    /// <summary>
    /// REST port. Default is 8080.
    /// </summary>
    public ushort RestPort { get; set; } = 8080;

    /// <summary>
    /// gRPC port. Default is 50051.
    /// </summary>
    public ushort GrpcPort { get; set; } = 50051;

    /// <summary>
    /// Whether to use SSL/TLS. Default is false.
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Additional HTTP headers to include in requests.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Adds or appends a single HTTP header. If the key already exists the value is overwritten.
    /// </summary>
    /// <param name="key">The header name.</param>
    /// <param name="value">The header value.</param>
    /// <returns>This instance for method chaining.</returns>
    public WeaviateOptions AddHeader(string key, string value)
    {
        Headers ??= new Dictionary<string, string>();
        Headers[key] = value;
        return this;
    }

    /// <summary>
    /// Appends an integration identifier to the <c>X-Weaviate-Client-Integration</c> header.
    /// Use this to identify higher-level libraries built on top of the core client.
    /// Multiple calls append space-separated tokens, e.g.
    /// <c>weaviate-client-csharp-managed/1.0.0 my-framework/2.3.0</c>.
    /// </summary>
    /// <param name="integrationValue">
    /// An integration identifier, typically in <c>name/version</c> format.
    /// </param>
    /// <returns>This instance for method chaining.</returns>
    public WeaviateOptions AddIntegration(string integrationValue)
    {
        if (integrationValue.Any(char.IsWhiteSpace))
            throw new ArgumentException(
                "Integration value must not contain whitespace.",
                nameof(integrationValue)
            );
        Headers ??= new Dictionary<string, string>();
        if (Headers.TryGetValue(WeaviateDefaults.IntegrationHeader, out var existing))
            Headers[WeaviateDefaults.IntegrationHeader] = $"{existing} {integrationValue}";
        else
            Headers[WeaviateDefaults.IntegrationHeader] = integrationValue;
        return this;
    }

    /// <summary>
    /// Authentication credentials.
    /// </summary>
    public ICredentials? Credentials { get; set; }

    /// <summary>
    /// Default timeout for all operations.
    /// </summary>
    public TimeSpan? DefaultTimeout { get; set; }

    /// <summary>
    /// Timeout for initialization operations (GetMeta, Live, IsReady).
    /// </summary>
    public TimeSpan? InitTimeout { get; set; }

    /// <summary>
    /// Timeout for data operations (Insert, Delete, Update, Reference management).
    /// </summary>
    public TimeSpan? InsertTimeout { get; set; }

    /// <summary>
    /// Timeout for query/search operations (FetchObjects, NearText, BM25, Hybrid, etc.).
    /// </summary>
    public TimeSpan? QueryTimeout { get; set; }

    /// <summary>
    /// Retry policy for failed requests.
    /// </summary>
    public RetryPolicy? RetryPolicy { get; set; }

    /// <summary>
    /// When true, HTTP requests/responses and gRPC calls are logged.
    /// Requires the host to provide an ILoggerFactory via DI. Defaults to false.
    /// </summary>
    public bool LogRequests { get; set; } = false;

    /// <summary>
    /// The log level used for request/response log entries. Defaults to Debug.
    /// </summary>
    public LogLevel RequestLoggingLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Converts these options to a ClientConfiguration.
    /// The ILoggerFactory is not set here — it is injected by the DI constructor.
    /// </summary>
    internal ClientConfiguration ToClientConfiguration()
    {
        return new ClientConfiguration(
            RestAddress: RestEndpoint,
            RestPath: RestPath,
            GrpcAddress: GrpcEndpoint,
            GrpcPath: GrpcPath,
            RestPort: RestPort,
            GrpcPort: GrpcPort,
            UseSsl: UseSsl,
            Headers: Headers,
            Credentials: Credentials,
            DefaultTimeout: DefaultTimeout,
            InitTimeout: InitTimeout,
            InsertTimeout: InsertTimeout,
            QueryTimeout: QueryTimeout,
            RetryPolicy: RetryPolicy,
            LogRequests: LogRequests,
            RequestLoggingLevel: RequestLoggingLevel
        );
    }
}
