using System.Net;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Internal;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Minimal ILogger implementation that captures log entries for assertions.
/// </summary>
internal sealed class TestLogger<T> : ILogger<T>
{
    public List<(LogLevel Level, string Message)> Entries { get; } = [];

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Entries.Add((logLevel, formatter(state, exception)));
    }
}

public class TestHttpLoggingHandler
{
    private static HttpLoggingHandler BuildHandler(ILogger logger, LogLevel level = LogLevel.Debug)
    {
        var loggerFactory = LoggerFactory.Create(_ => { });
        return new HttpLoggingHandler(loggerFactory, level)
        {
            InnerHandler = new TestDelegateHandler(_ =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK))
            ),
        };
    }

    // Helper that accepts a custom inner handler so tests can control the response.
    private static (HttpLoggingHandler Handler, TestLogger<HttpLoggingHandler> Logger) Build(
        Func<HttpRequestMessage, Task<HttpResponseMessage>>? inner = null
    )
    {
        var logger = new TestLogger<HttpLoggingHandler>();
        var loggerFactory = new TestLoggerFactory<HttpLoggingHandler>(logger);
        var handler = new HttpLoggingHandler(loggerFactory, LogLevel.Debug)
        {
            InnerHandler = new TestDelegateHandler(
                inner ?? (_ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)))
            ),
        };
        return (handler, logger);
    }

    [Fact]
    public async Task LogsRequestMethodAndUri()
    {
        var (handler, logger) = Build();
        using var client = new HttpClient(handler);

        await client.GetAsync("http://localhost/v1/meta", TestContext.Current.CancellationToken);

        Assert.Contains(
            logger.Entries,
            e => e.Message.Contains("GET") && e.Message.Contains("/v1/meta")
        );
    }

    [Fact]
    public async Task LogsResponseStatusCode()
    {
        var (handler, logger) = Build(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound))
        );
        using var client = new HttpClient(handler);

        await client.GetAsync("http://localhost/v1/meta", TestContext.Current.CancellationToken);

        Assert.Contains(logger.Entries, e => e.Message.Contains("404"));
    }

    [Fact]
    public async Task RedactsAuthorizationHeader()
    {
        var (handler, logger) = Build();
        using var client = new HttpClient(handler);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "super-secret-token");

        await client.GetAsync("http://localhost/v1/meta", TestContext.Current.CancellationToken);

        // The token value must NOT appear in any log entry
        Assert.DoesNotContain(logger.Entries, e => e.Message.Contains("super-secret-token"));
        // But there should be a log entry that mentions Authorization
        Assert.Contains(logger.Entries, e => e.Message.Contains("Authorization"));
    }

    [Fact]
    public async Task LogsElapsedTime()
    {
        var (handler, logger) = Build();
        using var client = new HttpClient(handler);

        await client.GetAsync("http://localhost/v1/objects", TestContext.Current.CancellationToken);

        // Response log entry should contain timing info (e.g. "ms")
        Assert.Contains(logger.Entries, e => e.Message.Contains("ms"));
    }
}

/// <summary>
/// Simple delegating handler backed by a lambda — used in tests only.
/// </summary>
internal sealed class TestDelegateHandler(
    Func<HttpRequestMessage, Task<HttpResponseMessage>> handler
) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    ) => handler(request);
}

/// <summary>
/// Minimal ILoggerFactory that returns a pre-built typed logger.
/// </summary>
internal sealed class TestLoggerFactory<T> : ILoggerFactory
{
    private readonly ILogger<T> _logger;

    public TestLoggerFactory(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void AddProvider(ILoggerProvider provider) { }

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose() { }
}
