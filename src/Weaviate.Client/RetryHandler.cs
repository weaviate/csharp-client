using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

/// <summary>
/// HTTP delegating handler that implements retry logic for REST requests.
/// </summary>
internal class RetryHandler : DelegatingHandler
{
    /// <summary>
    /// The policy
    /// </summary>
    private readonly RetryPolicy _policy;

    /// <summary>
    /// The logger
    /// </summary>
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetryHandler"/> class
    /// </summary>
    /// <param name="policy">The policy</param>
    /// <param name="logger">The logger</param>
    public RetryHandler(RetryPolicy policy, ILogger? logger = null)
    {
        _policy = policy;
        _logger = logger;
    }

    /// <summary>
    /// Sends the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="WeaviateTimeoutException"></exception>
    /// <exception cref="WeaviateClientException">Request failed after all retry attempts</exception>
    /// <returns>A task containing the http response message</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _policy.MaxRetries; attempt++)
        {
            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                // Check if status code indicates retry
                if (response.IsSuccessStatusCode || attempt == _policy.MaxRetries)
                {
                    return response;
                }

                if (_policy.ShouldRetryHttpStatus((int)response.StatusCode))
                {
                    _logger?.LogWarning(
                        "HTTP request failed with status {StatusCode}. Retry attempt {Attempt} of {MaxRetries}",
                        response.StatusCode,
                        attempt + 1,
                        _policy.MaxRetries
                    );

                    var delay = _policy.CalculateDelay(attempt);
                    await Task.Delay(delay, cancellationToken);
                    continue;
                }

                // Non-retriable status code
                return response;
            }
            catch (Exception ex)
            {
                lastException = ex;

                // Check if we should retry
                if (ShouldRetry(ex, attempt))
                {
                    _logger?.LogWarning(
                        ex,
                        "HTTP request failed with exception. Retry attempt {Attempt} of {MaxRetries}",
                        attempt + 1,
                        _policy.MaxRetries
                    );

                    var delay = _policy.CalculateDelay(attempt);
                    await Task.Delay(delay, cancellationToken);
                }
                else
                {
                    // Last attempt or non-retriable exception
                    // Check if it's a timeout
                    if (TimeoutHelper.IsTimeoutCancellation(ex))
                    {
                        var timeout = TimeoutHelper.GetTimeout();
                        var operation = TimeoutHelper.GetOperation();
                        throw new WeaviateTimeoutException(timeout, operation, ex);
                    }

                    throw;
                }
            }
        }

        // This should never be reached as exceptions are thrown in the catch block
        throw new WeaviateClientException("Request failed after all retry attempts");
    }

    /// <summary>
    /// Shoulds the retry using the specified ex
    /// </summary>
    /// <param name="ex">The ex</param>
    /// <param name="attempt">The attempt</param>
    /// <returns>The bool</returns>
    private bool ShouldRetry(Exception ex, int attempt)
    {
        if (attempt >= _policy.MaxRetries)
            return false;

        return _policy.ShouldRetryException(ex);
    }
}
