namespace Weaviate.Client.Internal;

/// <summary>
/// Normalizes the cluster endpoint passed to the Weaviate Cloud connection helpers.
/// </summary>
internal static class CloudEndpoint
{
    private const string ExpectedForms =
        "Expected a Weaviate Cloud cluster URL such as 'https://my-cluster.weaviate.cloud' or a bare hostname such as 'my-cluster.weaviate.cloud'.";

    /// <summary>
    /// Returns the host of a Weaviate Cloud cluster URL or bare hostname. The scheme, path, query
    /// and fragment are dropped, because Weaviate Cloud is always reached over TLS on port 443.
    /// </summary>
    /// <param name="endpoint">A cluster URL such as <c>https://my-cluster.weaviate.cloud</c>, or a bare hostname.</param>
    /// <param name="paramName">The caller's parameter name, reported in the exception.</param>
    /// <returns>The bare, lowercased host, e.g. <c>my-cluster.weaviate.cloud</c>.</returns>
    /// <exception cref="ArgumentException">
    /// The endpoint is empty, uses a scheme other than http or https, contains user credentials,
    /// specifies a port other than 443, or does not have a valid DNS hostname.
    /// </exception>
    internal static string NormalizeHost(string? endpoint, string paramName)
    {
        var value = endpoint?.Trim();
        if (string.IsNullOrEmpty(value))
            throw Invalid("The Weaviate Cloud cluster endpoint is empty.", paramName);

        var hostAndRest = value;
        var schemeEnd = value.IndexOf("://", StringComparison.Ordinal);
        if (schemeEnd > 0 && Uri.CheckSchemeName(value[..schemeEnd]))
        {
            var scheme = value[..schemeEnd];
            if (
                !scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)
                && !scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)
            )
            {
                throw Invalid("Only the http and https schemes are supported.", paramName);
            }
            hostAndRest = value[(schemeEnd + 3)..];
        }

        // Parsed as https whatever the given scheme, so an absent port means 443, never 80.
        if (!Uri.TryCreate("https://" + hostAndRest, UriKind.Absolute, out var uri))
            throw Invalid(
                "The Weaviate Cloud cluster endpoint is not a valid URL or hostname.",
                paramName
            );

        if (uri.UserInfo.Length > 0)
            throw Invalid(
                "The Weaviate Cloud cluster endpoint must not contain user credentials; pass the API key separately.",
                paramName
            );

        if (uri.Port != 443)
            throw Invalid(
                "Weaviate Cloud is served on port 443; other ports are not supported.",
                paramName
            );

        if (Uri.CheckHostName(uri.Host) != UriHostNameType.Dns)
            throw Invalid(
                "The Weaviate Cloud cluster endpoint does not have a valid DNS hostname.",
                paramName
            );

        // A mistyped scheme ("https:/x", "https://https://x") parses as the host "https".
        if (uri.Host is "http" or "https")
            throw Invalid(
                "The Weaviate Cloud cluster endpoint is not a valid URL or hostname.",
                paramName
            );

        return uri.Host;
    }

    // Never echo the input: it may be a misplaced API key or carry a password.
    private static ArgumentException Invalid(string reason, string paramName) =>
        new($"{reason} {ExpectedForms}", paramName);
}
