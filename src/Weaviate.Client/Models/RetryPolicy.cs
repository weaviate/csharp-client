namespace Weaviate.Client.Models;

/// <summary>
/// Defines what types of failures should trigger a retry.
/// </summary>
[Flags]
public enum RetryOn
{
    /// <summary>
    /// No retries.
    /// </summary>
    None = 0,

    /// <summary>
    /// Retry on request timeout.
    /// </summary>
    Timeout = 1 << 0,

    /// <summary>
    /// Retry on rate limiting (HTTP 429, gRPC ResourceExhausted).
    /// </summary>
    RateLimited = 1 << 1,

    /// <summary>
    /// Retry on service unavailable (HTTP 503/504, gRPC Unavailable).
    /// </summary>
    ServiceUnavailable = 1 << 2,

    /// <summary>
    /// Retry on network errors (connection failures, DNS issues).
    /// </summary>
    NetworkError = 1 << 3,

    /// <summary>
    /// All transient errors (default: Timeout + RateLimited + ServiceUnavailable + NetworkError).
    /// </summary>
    TransientErrors = Timeout | RateLimited | ServiceUnavailable | NetworkError,
}

/// <summary>
/// Configuration for retry behavior on transient failures.
/// </summary>
public record RetryPolicy
{
    /// <summary>
    /// Maximum number of retry attempts. Default is 3.
    /// Set to 0 to disable retries.
    /// </summary>
    public int MaxRetries { get; init; } = 3;

    /// <summary>
    /// Initial delay before the first retry. Default is 100ms.
    /// </summary>
    public TimeSpan InitialDelay { get; init; } = TimeSpan.FromMilliseconds(100);

    /// <summary>
    /// Multiplier for exponential backoff. Default is 2.0.
    /// Each retry will wait InitialDelay * (BackoffMultiplier ^ attemptNumber).
    /// </summary>
    public double BackoffMultiplier { get; init; } = 2.0;

    /// <summary>
    /// Maximum delay between retries. Default is 10 seconds.
    /// </summary>
    public TimeSpan MaxDelay { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// What types of failures should trigger a retry. Default is all transient errors.
    /// </summary>
    public RetryOn RetryOn { get; init; } = RetryOn.TransientErrors;

    /// <summary>
    /// Creates a RetryPolicy with no retries (retries disabled).
    /// </summary>
    public static RetryPolicy None => new() { MaxRetries = 0 };

    /// <summary>
    /// Creates the default retry policy (3 retries with exponential backoff on all transient errors).
    /// </summary>
    public static RetryPolicy Default => new();

    /// <summary>
    /// Calculates the delay for a given retry attempt using exponential backoff.
    /// </summary>
    /// <param name="attemptNumber">The current attempt number (0-based).</param>
    /// <returns>The delay to wait before the next retry.</returns>
    public TimeSpan CalculateDelay(int attemptNumber)
    {
        var delay = InitialDelay.TotalMilliseconds * Math.Pow(BackoffMultiplier, attemptNumber);
        var cappedDelay = Math.Min(delay, MaxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(cappedDelay);
    }

    /// <summary>
    /// Determines if the given HTTP status code should trigger a retry based on the RetryOn flags.
    /// </summary>
    internal bool ShouldRetryHttpStatus(int statusCode)
    {
        return statusCode switch
        {
            429 => RetryOn.HasFlag(RetryOn.RateLimited),
            503 or 504 => RetryOn.HasFlag(RetryOn.ServiceUnavailable),
            408 => RetryOn.HasFlag(RetryOn.Timeout),
            _ => false,
        };
    }

    /// <summary>
    /// Determines if the given gRPC status code should trigger a retry based on the RetryOn flags.
    /// </summary>
    internal bool ShouldRetryGrpcStatus(global::Grpc.Core.StatusCode statusCode)
    {
        return statusCode switch
        {
            global::Grpc.Core.StatusCode.ResourceExhausted => RetryOn.HasFlag(RetryOn.RateLimited),
            global::Grpc.Core.StatusCode.Unavailable => RetryOn.HasFlag(RetryOn.ServiceUnavailable),
            global::Grpc.Core.StatusCode.DeadlineExceeded => RetryOn.HasFlag(RetryOn.Timeout),
            _ => false,
        };
    }

    /// <summary>
    /// Determines if the given exception should trigger a retry based on the RetryOn flags.
    /// </summary>
    internal bool ShouldRetryException(Exception ex)
    {
        return ex switch
        {
            TimeoutException => RetryOn.HasFlag(RetryOn.Timeout),
            TaskCanceledException => RetryOn.HasFlag(RetryOn.Timeout),
            HttpRequestException httpEx when IsNetworkError(httpEx) => RetryOn.HasFlag(
                RetryOn.NetworkError
            ),
            System.Net.Sockets.SocketException => RetryOn.HasFlag(RetryOn.NetworkError),
            System.Net.WebException => RetryOn.HasFlag(RetryOn.NetworkError),
            _ => false,
        };
    }

    private static bool IsNetworkError(HttpRequestException ex)
    {
        // Check for common network-related inner exceptions
        return ex.InnerException is System.Net.Sockets.SocketException
            || ex.InnerException is System.Net.WebException
            || ex.InnerException is System.IO.IOException;
    }
}
