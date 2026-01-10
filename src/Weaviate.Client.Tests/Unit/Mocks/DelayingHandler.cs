namespace Weaviate.Client.Tests.Unit.Mocks;

/// <summary>
/// DelegatingHandler that introduces an artificial delay for matching request paths.
/// Respects CancellationToken and can simulate slow endpoints for cancellation testing.
/// </summary>
public sealed class DelayingHandler : DelegatingHandler
{
    /// <summary>
    /// The delay
    /// </summary>
    private readonly TimeSpan _delay;

    /// <summary>
    /// The should delay
    /// </summary>
    private readonly Func<HttpRequestMessage, bool> _shouldDelay;

    /// <summary>
    /// Number of times SendAsync has been invoked.
    /// </summary>
    public int Attempts { get; private set; }

    /// <param name="delay">Delay duration per matching request.</param>
    /// <param name="shouldDelay">Predicate deciding whether to delay a request (default: delay meta/live/ready)</param>
    /// <param name="inner">Optional inner handler; if null a default HttpClientHandler is used.</param>
    public DelayingHandler(
        TimeSpan delay,
        Func<HttpRequestMessage, bool>? shouldDelay = null,
        HttpMessageHandler? inner = null
    )
    {
        _delay = delay;
        _shouldDelay =
            shouldDelay
            ?? (
                req =>
                {
                    var path = req.RequestUri?.PathAndQuery ?? string.Empty;
                    return path.Contains("/v1/meta") || path.Contains("/v1/.well-known");
                }
            );
        InnerHandler = inner ?? new HttpClientHandler();
    }

    /// <summary>
    /// Sends the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the http response message</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Attempts++;
        if (_shouldDelay(request))
        {
            await Task.Delay(_delay, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
