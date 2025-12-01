using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Weaviate.Client.Request;

/// <summary>
/// Manages the request pipeline, orchestrating interceptors and request execution.
/// </summary>
public class RequestPipeline
{
    private readonly List<IRequestInterceptor> _interceptors;

    public RequestPipeline(IEnumerable<IRequestInterceptor>? interceptors = null)
    {
        _interceptors = interceptors?.ToList() ?? new List<IRequestInterceptor>();
    }

    /// <summary>
    /// Adds an interceptor to the pipeline.
    /// </summary>
    public void AddInterceptor(IRequestInterceptor interceptor)
    {
        if (interceptor == null)
            throw new ArgumentNullException(nameof(interceptor));

        _interceptors.Add(interceptor);
    }

    /// <summary>
    /// Removes an interceptor from the pipeline.
    /// </summary>
    public bool RemoveInterceptor(IRequestInterceptor interceptor)
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
    public IReadOnlyList<IRequestInterceptor> Interceptors => _interceptors.AsReadOnly();

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
            // Run all OnBeforeSend interceptors
            var modifiedContext = context;
            foreach (var interceptor in _interceptors)
            {
                modifiedContext = await interceptor.OnBeforeSendAsync(modifiedContext);
            }

            // Execute the actual request
            var response = await executor(modifiedContext);

            // Run all OnAfterReceive interceptors (in reverse order)
            var modifiedResponse = response;
            for (var i = _interceptors.Count - 1; i >= 0; i--)
            {
                modifiedResponse = await _interceptors[i].OnAfterReceiveAsync(modifiedContext, modifiedResponse);
            }

            return modifiedResponse;
        }
        catch (Exception ex)
        {
            // Notify all interceptors of the error
            foreach (var interceptor in _interceptors)
            {
                await interceptor.OnErrorAsync(context, ex);
            }

            // Re-throw the exception
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
        await ExecuteAsync<object?>(context, async ctx =>
        {
            await executor(ctx);
            return null;
        });
    }
}
