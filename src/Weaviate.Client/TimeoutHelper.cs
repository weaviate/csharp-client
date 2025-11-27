namespace Weaviate.Client;

/// <summary>
/// Helper utility for managing timeouts through CancellationToken.
/// Converts TimeSpan? to CancellationToken by creating a CancellationTokenSource with CancelAfter.
/// Uses AsyncLocal to track timeout context for distinguishing timeouts from user cancellations.
/// </summary>
internal static class TimeoutHelper
{
    // Thread-safe storage for tracking timeout cancellations
    private static readonly AsyncLocal<TimeoutContext?> _timeoutContext = new();

    private class TimeoutContext
    {
        public TimeSpan Timeout { get; init; }
        public CancellationToken TimeoutToken { get; init; }
        public string? Operation { get; init; }
    }

    /// <summary>
    /// Creates a CancellationToken that will be cancelled after the specified timeout.
    /// If no timeout is provided, returns the provided cancellation token (or default if none provided).
    /// </summary>
    /// <param name="timeout">The timeout duration. If null or zero, no timeout is applied.</param>
    /// <param name="providedToken">An optional cancellation token to link with the timeout token.</param>
    /// <param name="operation">Optional description of the operation for better error messages.</param>
    /// <returns>A CancellationToken that will be cancelled after the timeout expires.</returns>
    internal static CancellationToken GetCancellationToken(
        TimeSpan? timeout,
        CancellationToken providedToken = default,
        string? operation = null
    )
    {
        // If no timeout specified, clear context and use the provided token
        if (!timeout.HasValue || timeout.Value == TimeSpan.Zero)
        {
            _timeoutContext.Value = null;
            return providedToken;
        }

        // Create a CancellationTokenSource that will cancel after the timeout
        var cts = CancellationTokenSource.CreateLinkedTokenSource(providedToken);
        cts.CancelAfter(timeout.Value);

        // Store context so we can identify timeout cancellations later
        _timeoutContext.Value = new TimeoutContext
        {
            Timeout = timeout.Value,
            TimeoutToken = cts.Token,
            Operation = operation,
        };

        return cts.Token;
    }

    /// <summary>
    /// Creates a CancellationToken from a configuration timeout, falling back to a default timeout if needed.
    /// </summary>
    /// <param name="configTimeout">The specific timeout from configuration (e.g., InsertTimeout, QueryTimeout).</param>
    /// <param name="defaultTimeout">The default timeout to use if configTimeout is null.</param>
    /// <param name="providedToken">An optional cancellation token to link with the timeout token.</param>
    /// <param name="operation">Optional description of the operation for better error messages.</param>
    /// <returns>A CancellationToken that will be cancelled after the effective timeout expires.</returns>
    internal static CancellationToken GetCancellationToken(
        TimeSpan? configTimeout,
        TimeSpan? defaultTimeout,
        CancellationToken providedToken = default,
        string? operation = null
    )
    {
        // Use the specific timeout if provided, otherwise fall back to default
        var effectiveTimeout = configTimeout ?? defaultTimeout;
        return GetCancellationToken(effectiveTimeout, providedToken, operation);
    }

    /// <summary>
    /// Checks if an exception is due to a timeout (vs user cancellation).
    /// </summary>
    internal static bool IsTimeoutCancellation(Exception ex)
    {
        // Check if it's a cancellation exception
        if (ex is not (TaskCanceledException or OperationCanceledException))
            return false;

        // Check if we have timeout context and the timeout token was cancelled
        var context = _timeoutContext.Value;
        if (context == null)
            return false;

        return context.TimeoutToken.IsCancellationRequested;
    }

    /// <summary>
    /// Gets the timeout duration if the exception was due to a timeout.
    /// </summary>
    internal static TimeSpan? GetTimeout()
    {
        return _timeoutContext.Value?.Timeout;
    }

    /// <summary>
    /// Gets the operation description if the exception was due to a timeout.
    /// </summary>
    internal static string? GetOperation()
    {
        return _timeoutContext.Value?.Operation;
    }
}
