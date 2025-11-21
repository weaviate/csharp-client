namespace Weaviate.Client.Models;

/// <summary>
/// Represents a replication operation with status polling and cancellation.
/// Implements automatic resource cleanup when the operation completes.
/// </summary>
public class ReplicationOperationTracker : IDisposable, IAsyncDisposable
{
    private readonly Func<CancellationToken, Task<ReplicationOperation>> _statusFetcher;
    private readonly Func<CancellationToken, Task> _operationCancel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _backgroundRefreshTask;
    private ReplicationOperation _current;
    private bool _disposed;

    internal ReplicationOperationTracker(
        ReplicationOperation initial,
        Func<CancellationToken, Task<ReplicationOperation>> statusFetcher,
        Func<CancellationToken, Task> operationCancel
    )
    {
        _current = initial;
        _statusFetcher = statusFetcher;
        _operationCancel = operationCancel;
        _backgroundRefreshTask = StartBackgroundRefresh();
    }

    /// <summary>
    /// Gets the latest replication operation state.
    /// </summary>
    public ReplicationOperation Current => _current;

    /// <summary>
    /// True if the operation has completed (success or cancellation).
    /// </summary>
    public bool IsCompleted => _current.IsCompleted;

    /// <summary>
    /// True if the operation completed successfully.
    /// </summary>
    public bool IsSuccessful => _current.IsSuccessful;

    /// <summary>
    /// True if the operation was cancelled.
    /// </summary>
    public bool IsCancelled => _current.IsCancelled;

    private Task StartBackgroundRefresh()
    {
        return Task.Run(async () =>
        {
            while (!_current.IsCompleted && !_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(ReplicationClientConfig.Default.PollInterval, _cts.Token);
                    await RefreshStatusInternal(_cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    // Swallow errors, optionally log
                }
            }
        });
    }

    private async Task RefreshStatusInternal(CancellationToken cancellationToken = default)
    {
        if (_current.IsCompleted)
            return;

        var status = await _statusFetcher(cancellationToken);
        _current = status;

        if (_current.IsCompleted)
        {
            await _cts.CancelAsync(); // Stop background polling

            // Auto-dispose resources now that operation is complete
            // This prevents resource leaks if caller forgets to dispose
            DisposeInternal();
        }
    }

    /// <summary>
    /// Manually refresh the operation status from the server.
    /// </summary>
    public async Task RefreshStatus(CancellationToken cancellationToken = default)
    {
        await RefreshStatusInternal(cancellationToken);
    }

    /// <summary>
    /// Waits for the operation to complete.
    /// Throws <see cref="TimeoutException"/> if it does not finish within the provided timeout.
    /// </summary>
    /// <param name="timeout">Optional timeout; if null uses <see cref="ReplicationClientConfig.Default"/> timeout.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort waiting.</param>
    public async Task<ReplicationOperation> WaitForCompletion(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var effectiveTimeout = timeout ?? ReplicationClientConfig.Default.Timeout;
        var start = DateTime.UtcNow;

        var effectiveToken = CancellationTokenSource
            .CreateLinkedTokenSource(_cts.Token, cancellationToken)
            .Token;

        while (!_current.IsCompleted && !effectiveToken.IsCancellationRequested)
        {
            effectiveToken.ThrowIfCancellationRequested();
            if (DateTime.UtcNow - start > effectiveTimeout)
            {
                throw new TimeoutException(
                    $"Replication operation did not complete within {effectiveTimeout} (last status={_current.Status.State})."
                );
            }
            try
            {
                await Task.Delay(ReplicationClientConfig.Default.PollInterval, effectiveToken);
            }
            catch (OperationCanceledException) when (_current.IsCompleted)
            {
                // Operation completed while waiting
            }
        }
        return _current;
    }

    /// <summary>
    /// Requests cancellation of the operation.
    /// </summary>
    public async Task Cancel(CancellationToken cancellationToken = default)
    {
        // Call server-side Cancel
        await _operationCancel(cancellationToken);

        await RefreshStatusInternal(cancellationToken);
    }

    /// <summary>
    /// Cancels the operation and waits synchronously for cancellation to complete.
    /// This method blocks until the operation reaches a terminal state (cancelled or ready).
    /// </summary>
    /// <param name="timeout">Optional timeout; if null uses default of 10 seconds.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort waiting.</param>
    /// <returns>The final operation state after cancellation.</returns>
    public async Task<ReplicationOperation> CancelSync(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        await Cancel(cancellationToken);
        return await WaitForCompletion(timeout ?? TimeSpan.FromSeconds(10), cancellationToken);
    }

    /// <summary>
    /// Dispose the operation and cancel background polling.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            _cts.Cancel();
            try
            {
                _backgroundRefreshTask.Wait(ReplicationClientConfig.Default.PollInterval);
            }
            catch (Exception)
            {
                // Ignore exceptions from waiting on background task, as disposal is best-effort
            }
            _cts.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Internal disposal logic that can be called from background task without re-entrancy issues.
    /// </summary>
    private void DisposeInternal()
    {
        if (_disposed)
            return;

        try
        {
            // Wait for background task to complete (should be immediate since we just canceled)
            _backgroundRefreshTask.Wait(ReplicationClientConfig.Default.PollInterval);
        }
        catch (Exception)
        {
            // Ignore exceptions, disposal is best-effort
        }

        _cts.Dispose();
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously dispose the operation and cancel background polling.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _cts.Cancel();

        try
        {
            // Await the background task gracefully
            await _backgroundRefreshTask.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Ignore exceptions, disposal is best-effort
        }

        _cts.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
