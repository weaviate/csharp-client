using System;
using System.Threading.Tasks;

namespace Weaviate.Client.Request;

/// <summary>
/// Interface for request interceptors that can inspect and modify requests
/// before they are sent and responses after they are received.
/// </summary>
public interface IRequestInterceptor
{
    /// <summary>
    /// Called before a request is sent over the wire.
    /// Interceptors can inspect, modify, or even replace the request.
    /// </summary>
    /// <param name="context">The request context</param>
    /// <returns>The potentially modified request context</returns>
    Task<RequestContext> OnBeforeSendAsync(RequestContext context);

    /// <summary>
    /// Called after a response is received.
    /// Interceptors can inspect or modify the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="context">The request context</param>
    /// <param name="response">The received response</param>
    /// <returns>The potentially modified response</returns>
    Task<TResponse> OnAfterReceiveAsync<TResponse>(RequestContext context, TResponse response);

    /// <summary>
    /// Called when an error occurs during request execution.
    /// Interceptors can log, modify, or handle the error.
    /// </summary>
    /// <param name="context">The request context</param>
    /// <param name="exception">The exception that occurred</param>
    Task OnErrorAsync(RequestContext context, Exception exception);
}

/// <summary>
/// Base class for request interceptors with default no-op implementations.
/// </summary>
public abstract class RequestInterceptorBase : IRequestInterceptor
{
    /// <inheritdoc/>
    public virtual Task<RequestContext> OnBeforeSendAsync(RequestContext context)
    {
        return Task.FromResult(context);
    }

    /// <inheritdoc/>
    public virtual Task<TResponse> OnAfterReceiveAsync<TResponse>(RequestContext context, TResponse response)
    {
        return Task.FromResult(response);
    }

    /// <inheritdoc/>
    public virtual Task OnErrorAsync(RequestContext context, Exception exception)
    {
        return Task.CompletedTask;
    }
}
