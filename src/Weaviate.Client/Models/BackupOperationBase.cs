namespace Weaviate.Client.Models;

/// <summary>
/// Abstract base for backup/restore operations, with status polling and cancellation.
/// Implements automatic resource cleanup when the operation completes.
/// </summary>
public abstract class BackupOperationBase : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// The status fetcher
    /// </summary>
    private readonly Func<CancellationToken, Task<Backup>> _statusFetcher;

    /// <summary>
    /// The operation cancel
    /// </summary>
    private readonly Func<CancellationToken, Task> _operationCancel;

    /// <summary>
    /// The cts
    /// </summary>
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// The background refresh task
    /// </summary>
    private readonly Task _backgroundRefreshTask;

    /// <summary>
    /// The current
    /// </summary>
    private Backup _current;

    /// <summary>
    /// The is completed
    /// </summary>
    private volatile bool _isCompleted;

    /// <summary>
    /// The is successful
    /// </summary>
    private volatile bool _isSuccessful;

    /// <summary>
    /// The is canceled
    /// </summary>
    private volatile bool _isCanceled;

    /// <summary>
    /// The disposed
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="BackupOperationBase"/> class
    /// </summary>
    /// <param name="initial">The initial</param>
    /// <param name="statusFetcher">The status fetcher</param>
    /// <param name="operationCancel">The operation cancel</param>
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

    /// <summary>
    /// Starts the background refresh
    /// </summary>
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
                catch { }
            }
        });
    }

    /// <summary>
    /// Refreshes the status internal using the specified cancellation token
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
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
            catch (Exception) { }
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
            _backgroundRefreshTask.Wait(BackupClient.Config.PollInterval);
        }
        catch (Exception) { }

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
        catch (Exception) { }

        _cts.Dispose();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Ises the terminal status using the specified status
    /// </summary>
    /// <param name="status">The status</param>
    /// <returns>The bool</returns>
    private static bool IsTerminalStatus(BackupStatus? status)
    {
        return status == BackupStatus.Success
            || status == BackupStatus.Failed
            || status == BackupStatus.Canceled;
    }
}
