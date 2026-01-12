using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using Weaviate.Client.Internal;

namespace Weaviate.Client.Grpc;

/// <summary>
/// gRPC interceptor that implements retry logic for gRPC requests.
/// </summary>
internal class RetryInterceptor : Interceptor
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
    /// Initializes a new instance of the <see cref="RetryInterceptor"/> class
    /// </summary>
    /// <param name="policy">The policy</param>
    /// <param name="logger">The logger</param>
    public RetryInterceptor(RetryPolicy policy, ILogger? logger = null)
    {
        _policy = policy;
        _logger = logger;
    }

    /// <summary>
    /// Asyncs the unary call using the specified request
    /// </summary>
    /// <typeparam name="TRequest">The request</typeparam>
    /// <typeparam name="TResponse">The response</typeparam>
    /// <param name="request">The request</param>
    /// <param name="context">The context</param>
    /// <param name="continuation">The continuation</param>
    /// <returns>An async unary call of t response</returns>
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

    /// <summary>
    /// Retries the request
    /// </summary>
    /// <typeparam name="TRequest">The request</typeparam>
    /// <typeparam name="TResponse">The response</typeparam>
    /// <param name="request">The request</param>
    /// <param name="context">The context</param>
    /// <param name="continuation">The continuation</param>
    /// <exception cref="RpcException"></exception>
    /// <exception cref="WeaviateTimeoutException"></exception>
    /// <returns>A task containing the response</returns>
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
        if (lastException != null)
        {
            // Check if it's a timeout
            if (TimeoutHelper.IsTimeoutCancellation(lastException))
            {
                var timeout = TimeoutHelper.GetTimeout();
                var operation = TimeoutHelper.GetOperation();
                throw new WeaviateTimeoutException(timeout, operation, lastException);
            }

            if (lastException is RpcException rpcEx)
                throw rpcEx;

            throw lastException;
        }

        throw new RpcException(
            new Status(StatusCode.Internal, "Request failed after all retry attempts")
        );
    }

    /// <summary>
    /// Shoulds the retry grpc using the specified ex
    /// </summary>
    /// <param name="ex">The ex</param>
    /// <param name="attempt">The attempt</param>
    /// <returns>The bool</returns>
    private bool ShouldRetryGrpc(RpcException ex, int attempt)
    {
        if (attempt >= _policy.MaxRetries)
            return false;

        return _policy.ShouldRetryGrpcStatus(ex.StatusCode);
    }

    /// <summary>
    /// Shoulds the retry exception using the specified ex
    /// </summary>
    /// <param name="ex">The ex</param>
    /// <param name="attempt">The attempt</param>
    /// <returns>The bool</returns>
    private bool ShouldRetryException(Exception ex, int attempt)
    {
        if (attempt >= _policy.MaxRetries)
            return false;

        return _policy.ShouldRetryException(ex);
    }
}
