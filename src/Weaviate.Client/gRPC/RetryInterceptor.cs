using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;

namespace Weaviate.Client.Grpc;

/// <summary>
/// gRPC interceptor that implements retry logic for gRPC requests.
/// </summary>
internal class RetryInterceptor : Interceptor
{
    private readonly RetryPolicy _policy;
    private readonly ILogger? _logger;

    public RetryInterceptor(RetryPolicy policy, ILogger? logger = null)
    {
        _policy = policy;
        _logger = logger;
    }

    public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
    {
        var call = RetryAsync(request, context, continuation);
        return new AsyncUnaryCall<TResponse>(
            call,
            Task.FromResult(global::Grpc.Core.Metadata.Empty),
            () => Status.DefaultSuccess,
            () => global::Grpc.Core.Metadata.Empty,
            () => { }
        );
    }

    private async Task<TResponse> RetryAsync<TRequest, TResponse>(
        TRequest request,
        ClientInterceptorContext<TRequest, TResponse> context,
        AsyncUnaryCallContinuation<TRequest, TResponse> continuation
    )
        where TRequest : class
        where TResponse : class
    {
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _policy.MaxRetries; attempt++)
        {
            try
            {
                var call = continuation(request, context);
                return await call.ResponseAsync;
            }
            catch (RpcException ex) when (ShouldRetryGrpc(ex, attempt))
            {
                lastException = ex;

                _logger?.LogWarning(
                    ex,
                    "gRPC request failed with status {StatusCode}. Retry attempt {Attempt} of {MaxRetries}",
                    ex.StatusCode,
                    attempt + 1,
                    _policy.MaxRetries
                );

                var delay = _policy.CalculateDelay(attempt);
                await Task.Delay(delay);
            }
            catch (Exception ex) when (ShouldRetryException(ex, attempt))
            {
                lastException = ex;

                _logger?.LogWarning(
                    ex,
                    "gRPC request failed with exception. Retry attempt {Attempt} of {MaxRetries}",
                    attempt + 1,
                    _policy.MaxRetries
                );

                var delay = _policy.CalculateDelay(attempt);
                await Task.Delay(delay);
            }
        }

        // All retries exhausted
        if (lastException is RpcException rpcEx)
            throw rpcEx;

        throw lastException
            ?? new RpcException(
                new Status(StatusCode.Internal, "Request failed after all retry attempts")
            );
    }

    private bool ShouldRetryGrpc(RpcException ex, int attempt)
    {
        if (attempt >= _policy.MaxRetries)
            return false;

        return _policy.ShouldRetryGrpcStatus(ex.StatusCode);
    }

    private bool ShouldRetryException(Exception ex, int attempt)
    {
        if (attempt >= _policy.MaxRetries)
            return false;

        return _policy.ShouldRetryException(ex);
    }
}
