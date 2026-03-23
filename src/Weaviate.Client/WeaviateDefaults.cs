namespace Weaviate.Client;

/// <summary>
/// Global default settings for Weaviate clients.
/// </summary>
public static class WeaviateDefaults
{
    /// <summary>
    /// Default timeout for all requests. Default is 30 seconds.
    /// This can be overridden per client via ClientConfiguration.
    /// </summary>
    public static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default timeout for initialization operations (GetMeta, Live, IsReady). Default is 2 seconds.
    /// This can be overridden per client via ClientConfiguration.WithInitTimeout().
    /// </summary>
    public static TimeSpan InitTimeout { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Default timeout for data operations (Insert, Delete, Update, Reference management). Default is 120 seconds.
    /// This can be overridden per client via ClientConfiguration.WithInsertTimeout().
    /// </summary>
    public static TimeSpan InsertTimeout { get; set; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Default timeout for query/search operations (FetchObjects, NearText, BM25, Hybrid, etc.). Default is 60 seconds.
    /// This can be overridden per client via ClientConfiguration.WithQueryTimeout().
    /// </summary>
    public static TimeSpan QueryTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Default retry policy applied when a client does not specify one explicitly.
    /// </summary>
    public static RetryPolicy DefaultRetryPolicy { get; set; } = RetryPolicy.Default;

    /// <summary>
    /// The HTTP header name used to identify integration libraries built on top of the core client.
    /// </summary>
    public const string IntegrationHeader = "X-Weaviate-Client-Integration";

    /// <summary>
    /// Gets or sets the default User-Agent headers sent by the client.
    /// This can be used to identify the client or provide additional metadata in requests.
    /// </summary>
    public static string UserAgent =>
        $"weaviate-client-csharp/{typeof(WeaviateClient).Assembly.GetName().Version}";

    /// <summary>
    /// Returns an integration agent header value for the given integration name.
    /// Used to populate the <c>X-Weaviate-Client-Integration</c> header so higher-level
    /// libraries built on top of this client can identify themselves to the server.
    /// Format: <c>{integrationName}/{version}</c>, e.g. <c>weaviate-client-csharp-managed/1.0.0</c>.
    /// </summary>
    /// <param name="integrationName">The integration package name.</param>
    public static string IntegrationAgent(string integrationName) =>
        $"{integrationName}/{typeof(WeaviateClient).Assembly.GetName().Version}";
}
