using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client.Internal;

/// <summary>
/// A delegating handler that logs HTTP requests and responses.
/// Placed as the outermost handler so one log entry is emitted per logical operation,
/// regardless of internal retries. Authorization header values are redacted.
/// </summary>
internal class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger<HttpLoggingHandler> _logger;
    private readonly LogLevel _level;

    /// <summary>
    /// Initializes a new instance of <see cref="HttpLoggingHandler"/>.
    /// </summary>
    /// <param name="loggerFactory">Factory used to create the typed logger.</param>
    /// <param name="level">Log level for request/response entries. Defaults to Debug.</param>
    public HttpLoggingHandler(ILoggerFactory loggerFactory, LogLevel level = LogLevel.Debug)
    {
        _logger = loggerFactory.CreateLogger<HttpLoggingHandler>();
        _level = level;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_logger.IsEnabled(_level))
        {
            LogRequest(request);
        }

        var sw = Stopwatch.StartNew();
        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.Log(
                LogLevel.Warning,
                "HTTP {Method} {Uri} failed after {ElapsedMs}ms: {Error}",
                request.Method,
                request.RequestUri,
                sw.ElapsedMilliseconds,
                ex.Message
            );
            throw;
        }

        sw.Stop();

        if (_logger.IsEnabled(_level))
        {
            _logger.Log(
                _level,
                "-> {StatusCode} in {ElapsedMs}ms",
                (int)response.StatusCode,
                sw.ElapsedMilliseconds
            );
        }

        return response;
    }

    private void LogRequest(HttpRequestMessage request)
    {
        // Log method and URI first
        _logger.Log(_level, "HTTP {Method} {Uri}", request.Method, request.RequestUri);

        // Log request headers, redacting Authorization values
        foreach (var header in request.Headers)
        {
            var value = header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase)
                ? "[redacted]"
                : string.Join(", ", header.Value);

            _logger.Log(_level, "  {HeaderName}: {HeaderValue}", header.Key, value);
        }
    }
}
