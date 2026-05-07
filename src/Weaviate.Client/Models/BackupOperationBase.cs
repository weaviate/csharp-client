namespace Weaviate.Client.Models;

/// <summary>
/// Abstract base for backup/restore operations, with status polling and cancellation.
/// Implements automatic resource cleanup when the operation completes.
/// </summary>
public abstract class BackupOperationBase : IDisposable, IAsyncDisposable
{
    private readonly Func<CancellationToken, Task<Backup>> _statusFetcher;
    private readonly Func<CancellationToken, Task> _operationCancel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _backgroundRefreshTask;
    private Backup _current;
    private volatile bool _isCompleted;
    private volatile bool _isSuccessful;
    private volatile bool _isCanceled;
    private volatile bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupOperationBase"/> class.
    /// </summary>
    /// <param name="initial">The initial backup state returned by the server.</param>
    /// <param name="statusFetcher">Delegate to poll the latest status from the server.</param>
    /// <param name="operationCancel">Delegate to request cancellation on the server.</param>
    protected BackupOperationBase(
        Backup initial,
        Func<CancellationToken, Task<Backup>> statusFetcher,
        Func<CancellationToken, Task> operationCancel
    )
    {
        _current = initial;
        _statusFetcher = statusFetcher;
        _operationCancel = operationCancel;
        _isCompleted = IsTerminalStatus(initial.Status);
        _isSuccessful = initial.Status == BackupStatus.Success;
        _isCanceled = initial.Status == BackupStatus.Canceled;
        _backgroundRefreshTask = StartBackgroundRefresh();
    }

    /// <summary>
    /// Gets the latest backup state for this operation.
    /// </summary>
    public Backup Current => _current;

    /// <summary>
    /// True if the operation has completed (success, failure, or cancellation).
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// True if the operation completed successfully.
    /// </summary>
    public bool IsSuccessful => _isSuccessful;

    /// <summary>
    /// True if the operation was canceled.
    /// </summary>
    public bool IsCanceled => _isCanceled;

    private Task StartBackgroundRefresh()
    {
        if (_isCompleted)
            return Task.CompletedTask;
        return Task.Run(async () =>
        {
            while (!_isCompleted && !_cts.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(BackupClient.Config.PollInterval, _cts.Token);
                    await RefreshStatusInternal();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex) when (ex is not OutOfMemoryException)
                {
                    // Transient errors (network, server) are swallowed so the loop
                    // retries on the next poll interval rather than killing the background task.
                }
            }
        });
    }

    private async Task RefreshStatusInternal(CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
            return;
        var status = await _statusFetcher(
            cancellationToken == default ? _cts.Token : cancellationToken
        );
        _current = status;
        if (IsTerminalStatus(status.Status))
        {
            _isCompleted = true;
            _isSuccessful = status.Status == BackupStatus.Success;
            _isCanceled = status.Status == BackupStatus.Canceled;
            await _cts.CancelAsync();
            DisposeInternal();
        }
    }

    /// <summary>
    /// Waits for the operation to complete.
    /// Throws <see cref="TimeoutException"/> if it does not finish within the provided timeout.
    /// </summary>
    /// <param name="timeout">Optional timeout; if null uses <see cref="BackupClient.Config"/> default.</param>
    /// <param name="cancellationToken">Optional cancellation token to abort waiting.</param>
    public async Task<Backup> WaitForCompletion(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var effectiveTimeout = timeout ?? BackupClient.Config.Timeout;
        var start = DateTime.UtcNow;

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token,
            cancellationToken
        );
        var effectiveToken = linkedCts.Token;

        while (!_isCompleted && !effectiveToken.IsCancellationRequested)
        {
            if (DateTime.UtcNow - start > effectiveTimeout)
            {
                throw new TimeoutException(
                    $"Backup operation did not complete within {effectiveTimeout} (last status={_current.Status})."
                );
            }
            try
            {
                await Task.Delay(BackupClient.Config.PollInterval, effectiveToken);
            }
            catch (OperationCanceledException) when (_isCompleted) { }
        }
        return _current;
    }

    /// <summary>
    /// Requests cancellation of the operation.
    /// </summary>
    public async Task Cancel(CancellationToken cancellationToken = default)
    {
        await _operationCancel(cancellationToken);

        await RefreshStatusInternal(cancellationToken);
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
                _backgroundRefreshTask.Wait(BackupClient.Config.PollInterval);
            }
            catch (Exception ex) when (ex is AggregateException or OperationCanceledException) { }
            _cts.Dispose();
        }
        _disposed = true;
    }

    /// <summary>
    /// Internal disposal called from the background refresh task itself when a terminal status
    /// is observed. Must NOT Wait() on _backgroundRefreshTask — that would block the very task
    /// executing this code on its own completion (bounded only by the Wait timeout). The task
    /// is already exiting because _cts has been canceled and _isCompleted is set, so just
    /// release the CTS.
    /// </summary>
    private void DisposeInternal()
    {
        if (_disposed)
            return;
        _cts.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
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
            await _backgroundRefreshTask.ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is OperationCanceledException or AggregateException) { }

        _cts.Dispose();
        _disposed = true;
    }

    private static bool IsTerminalStatus(BackupStatus? status)
    {
        return status == BackupStatus.Success
            || status == BackupStatus.Failed
            || status == BackupStatus.Canceled;
    }
}
