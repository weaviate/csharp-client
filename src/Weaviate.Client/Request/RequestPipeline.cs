using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weaviate.Client.Request;

/// <summary>
/// Manages the request pipeline, orchestrating interceptors and request execution.
/// Supports both composite IRequestInterceptor and individual interceptor interfaces.
/// </summary>
public class RequestPipeline
{
    private readonly List<object> _interceptors;

    public RequestPipeline(IEnumerable<IRequestInterceptor>? interceptors = null)
    {
        _interceptors = interceptors?.Cast<object>().ToList() ?? new List<object>();
    }

    /// <summary>
    /// Adds a composite interceptor to the pipeline.
    /// </summary>
    public void AddInterceptor(IRequestInterceptor interceptor)
    {
        if (interceptor == null)
            throw new ArgumentNullException(nameof(interceptor));

        _interceptors.Add(interceptor);
    }

    /// <summary>
    /// Adds a before-send interceptor to the pipeline.
    /// </summary>
    public void AddBeforeSendInterceptor(IBeforeSendInterceptor interceptor)
    {
        if (interceptor == null)
            throw new ArgumentNullException(nameof(interceptor));

        _interceptors.Add(interceptor);
    }

    /// <summary>
    /// Adds an after-receive interceptor to the pipeline.
    /// </summary>
    public void AddAfterReceiveInterceptor(IAfterReceiveInterceptor interceptor)
    {
        if (interceptor == null)
            throw new ArgumentNullException(nameof(interceptor));

        _interceptors.Add(interceptor);
    }

    /// <summary>
    /// Adds an error interceptor to the pipeline.
    /// </summary>
    public void AddErrorInterceptor(IErrorInterceptor interceptor)
    {
        if (interceptor == null)
            throw new ArgumentNullException(nameof(interceptor));

        _interceptors.Add(interceptor);
    }

    /// <summary>
    /// Removes an interceptor from the pipeline.
    /// </summary>
    public bool RemoveInterceptor(object interceptor)
    {
        return _interceptors.Remove(interceptor);
    }

    /// <summary>
    /// Clears all interceptors from the pipeline.
    /// </summary>
    public void ClearInterceptors()
    {
        _interceptors.Clear();
    }

    /// <summary>
    /// Gets the current interceptors in the pipeline.
    /// </summary>
    public IReadOnlyList<object> Interceptors => _interceptors.AsReadOnly();

    /// <summary>
    /// Executes a request through the pipeline.
    /// </summary>
    /// <typeparam name="TResponse">The response type</typeparam>
    /// <param name="context">The request context</param>
    /// <param name="executor">The function that actually executes the request</param>
    /// <returns>The response</returns>
    public async Task<TResponse> ExecuteAsync<TResponse>(
        RequestContext context,
        Func<RequestContext, Task<TResponse>> executor)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        if (executor == null)
            throw new ArgumentNullException(nameof(executor));

        try
        {
            // Run all OnBeforeSend interceptors in forward order
            var modifiedContext = context;
            foreach (var interceptor in _interceptors)
            {
                if (interceptor is IBeforeSendInterceptor beforeSend)
                {
                    modifiedContext = await beforeSend.OnBeforeSendAsync(modifiedContext);

                    // Null check: interceptor should not return null
                    if (modifiedContext == null)
                        throw new InvalidOperationException($"Interceptor {interceptor.GetType().Name} returned null from OnBeforeSendAsync");
                }
            }

            // Execute the actual request
            var response = await executor(modifiedContext);

            // Run all OnAfterReceive interceptors in reverse order (unwinding)
            var modifiedResponse = response;
            for (var i = _interceptors.Count - 1; i >= 0; i--)
            {
                if (_interceptors[i] is IAfterReceiveInterceptor afterReceive)
                {
                    modifiedResponse = await afterReceive.OnAfterReceiveAsync(modifiedContext, modifiedResponse);
                }
            }

            return modifiedResponse;
        }
        catch (Exception ex)
        {
            // Notify all error interceptors
            foreach (var interceptor in _interceptors)
            {
                if (interceptor is IErrorInterceptor errorInterceptor)
                {
                    try
                    {
                        await errorInterceptor.OnErrorAsync(context, ex);
                    }
                    catch (Exception errorHandlerEx)
                    {
                        // Error handler threw an exception - log but don't let it break the pipeline
                        // In production code, this should be logged to a proper logger
                        System.Diagnostics.Debug.WriteLine(
                            $"Error interceptor {interceptor.GetType().Name} threw exception: {errorHandlerEx}");
                    }
                }
            }

            // Re-throw the original exception
            throw;
        }
    }

    /// <summary>
    /// Executes a request through the pipeline (void return version).
    /// </summary>
    /// <param name="context">The request context</param>
    /// <param name="executor">The function that actually executes the request</param>
    public async Task ExecuteAsync(
        RequestContext context,
        Func<RequestContext, Task> executor)
    {
        if (executor == null)
            throw new ArgumentNullException(nameof(executor));

        await ExecuteAsync<object?>(context, async ctx =>
        {
            await executor(ctx);
            return null;
        });
    }
}
