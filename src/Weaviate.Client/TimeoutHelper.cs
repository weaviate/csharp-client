namespace Weaviate.Client;

/// <summary>
/// Helper utility for managing timeouts through CancellationToken.
/// Converts TimeSpan? to CancellationToken by creating a CancellationTokenSource with CancelAfter.
/// </summary>
internal static class TimeoutHelper
{
    /// <summary>
    /// Creates a CancellationToken that will be cancelled after the specified timeout.
    /// If no timeout is provided, returns the provided cancellation token (or default if none provided).
    /// </summary>
    /// <param name="timeout">The timeout duration. If null or zero, no timeout is applied.</param>
    /// <param name="providedToken">An optional cancellation token to link with the timeout token.</param>
    /// <returns>A CancellationToken that will be cancelled after the timeout expires.</returns>
    internal static CancellationToken GetCancellationToken(
        TimeSpan? timeout,
        CancellationToken providedToken = default
    )
    {
        // If no timeout specified, use the provided token
        if (!timeout.HasValue || timeout.Value == TimeSpan.Zero)
        {
            return providedToken;
        }

        // Create a CancellationTokenSource that will cancel after the timeout
        var cts = CancellationTokenSource.CreateLinkedTokenSource(providedToken);
        cts.CancelAfter(timeout.Value);

        return cts.Token;
    }

    /// <summary>
    /// Creates a CancellationToken from a configuration timeout, falling back to a default timeout if needed.
    /// </summary>
    /// <param name="configTimeout">The specific timeout from configuration (e.g., DataTimeout, QueryTimeout).</param>
    /// <param name="defaultTimeout">The default timeout to use if configTimeout is null.</param>
    /// <param name="providedToken">An optional cancellation token to link with the timeout token.</param>
    /// <returns>A CancellationToken that will be cancelled after the effective timeout expires.</returns>
    internal static CancellationToken GetCancellationToken(
        TimeSpan? configTimeout,
        TimeSpan? defaultTimeout,
        CancellationToken providedToken = default
    )
    {
        // Use the specific timeout if provided, otherwise fall back to default
        var effectiveTimeout = configTimeout ?? defaultTimeout;
        return GetCancellationToken(effectiveTimeout, providedToken);
    }
}
