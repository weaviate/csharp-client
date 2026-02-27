using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client.Grpc;

/// <summary>
/// gRPC interceptor that logs request method names, response status codes, and elapsed time.
/// Applied as the outermost interceptor so one log entry is emitted per logical operation,
/// regardless of internal retry attempts.
/// </summary>
internal class LoggingInterceptor : Interceptor
{
    private readonly ILogger<LoggingInterceptor> _logger;
    private readonly LogLevel _level;

    /// <summary>
    /// Initializes a new instance of <see cref="LoggingInterceptor"/>.
    /// </summary>
    /// <param name="loggerFactory">Factory used to create the typed logger.</param>
    /// <param name="level">Log level for gRPC call entries. Defaults to Debug.</param>
    public LoggingInterceptor(ILoggerFactory loggerFactory, LogLevel level = LogLevel.Debug)
    {
        _logger = loggerFactory.CreateLogger<LoggingInterceptor>();
        _level = level;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
    {
        var call = LogAsync(request, context, continuation);
        return new AsyncUnaryCall<TResponse>(
            call,
            Task.FromResult(Metadata.Empty),
            () => Status.DefaultSuccess,
            () => Metadata.Empty,
            () => { }
        );
    }

    private async Task<TResponse> LogAsync<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
        where TRequest : class
        where TResponse : class
    {
        var method = context.Method.FullName;

        if (_logger.IsEnabled(_level))
        {
            _logger.Log(_level, "gRPC {Method}", method);
        }

        var sw = Stopwatch.StartNew();
        try
        {
            var innerCall = continuation(request, context);
            var response = await innerCall.ResponseAsync;
            sw.Stop();

            if (_logger.IsEnabled(_level))
            {
                _logger.Log(_level, "-> OK in {ElapsedMs}ms", sw.ElapsedMilliseconds);
            }

            return response;
        }
        catch (RpcException ex)
        {
            sw.Stop();
            _logger.LogWarning(
                "gRPC {Method} failed: {StatusCode} in {ElapsedMs}ms — {Detail}",
                method,
                ex.StatusCode,
                sw.ElapsedMilliseconds,
                ex.Status.Detail
            );
            throw;
        }
    }
}
