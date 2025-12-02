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
    /// Converts these options to a ClientConfiguration.
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
            RetryPolicy: RetryPolicy
        );
    }
}
