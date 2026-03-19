namespace Weaviate.Client;

/// <summary>
/// Extension methods for <see cref="ClientConfiguration"/>.
/// </summary>
public static class ClientConfigurationExtensions
{
    /// <summary>
    /// Returns a new <see cref="ClientConfiguration"/> with the given integration identifier
    /// appended to the <c>X-Weaviate-Client-Integration</c> header.
    /// Multiple calls append space-separated tokens, e.g.
    /// <c>weaviate-client-csharp-managed/1.0.0 my-framework/2.3.0</c>.
    /// </summary>
    /// <param name="config">The base configuration.</param>
    /// <param name="integrationValue">
    /// An integration identifier in <c>name/version</c> format,
    /// e.g. <c>weaviate-client-csharp-managed/1.0.0</c>.
    /// </param>
    /// <returns>A new <see cref="ClientConfiguration"/> with the header set.</returns>
    public static ClientConfiguration WithIntegration(
        this ClientConfiguration config,
        string integrationValue
    )
    {
        if (integrationValue.Any(char.IsWhiteSpace))
            throw new ArgumentException(
                "Integration value must not contain whitespace.",
                nameof(integrationValue)
            );
        var headers = config.Headers is not null
            ? new Dictionary<string, string>(config.Headers)
            : new Dictionary<string, string>();

        if (headers.TryGetValue(WeaviateDefaults.IntegrationHeader, out var existing))
            headers[WeaviateDefaults.IntegrationHeader] = $"{existing} {integrationValue}";
        else
            headers[WeaviateDefaults.IntegrationHeader] = integrationValue;

        return config with
        {
            Headers = headers,
        };
    }
}
