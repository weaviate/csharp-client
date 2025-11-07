using Microsoft.Extensions.Logging;

namespace Weaviate.Client;

/// <summary>
/// HTTP delegating handler that implements retry logic for REST requests.
/// </summary>
internal class RetryHandler : DelegatingHandler
{
    private readonly RetryPolicy _policy;
    private readonly ILogger? _logger;

    public RetryHandler(RetryPolicy policy, ILogger? logger = null)
    {
        _policy = policy;
        _logger = logger;
    }

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
            catch (Exception ex) when (ShouldRetry(ex, attempt))
            {
                lastException = ex;

                _logger?.LogWarning(
                    ex,
                    "HTTP request failed with exception. Retry attempt {Attempt} of {MaxRetries}",
                    attempt + 1,
                    _policy.MaxRetries
                );

                var delay = _policy.CalculateDelay(attempt);
                await Task.Delay(delay, cancellationToken);
            }
        }

        // All retries exhausted, throw the last exception
        throw lastException ?? new HttpRequestException("Request failed after all retry attempts");
    }

    private bool ShouldRetry(Exception ex, int attempt)
    {
        if (attempt >= _policy.MaxRetries)
            return false;

        return _policy.ShouldRetryException(ex);
    }
}
