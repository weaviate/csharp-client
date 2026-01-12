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
    /// Gets or sets the default User-Agent headers sent by the client.
    /// This can be used to identify the client or provide additional metadata in requests.
    /// </summary>
    public static string UserAgent =>
        $"weaviate-client-csharp/{typeof(WeaviateClient).Assembly.GetName().Version}";
}
