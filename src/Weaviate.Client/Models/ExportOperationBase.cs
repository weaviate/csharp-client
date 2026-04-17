namespace Weaviate.Client.Models;

/// <summary>
/// Abstract base for export operations, with status polling and cancellation.
/// </summary>
public abstract class ExportOperationBase : IDisposable, IAsyncDisposable
{
    private readonly Func<CancellationToken, Task<Export>> _statusFetcher;
    private readonly Func<CancellationToken, Task> _operationCancel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _backgroundRefreshTask;
    private Export _current;
    private volatile bool _isCompleted;
    private volatile bool _isSuccessful;
    private volatile bool _isCanceled;
    private bool _disposed;

    protected ExportOperationBase(
        Export initial,
        Func<CancellationToken, Task<Export>> statusFetcher,
        Func<CancellationToken, Task> operationCancel
    )
    {
        _current = initial;
        _statusFetcher = statusFetcher;
        _operationCancel = operationCancel;
        _isCompleted = IsTerminalStatus(initial.Status);
        _isSuccessful = initial.Status == ExportStatus.Success;
        _isCanceled = initial.Status == ExportStatus.Canceled;
        _backgroundRefreshTask = StartBackgroundRefresh();
    }

    /// <summary>
    /// Gets the current export status and metadata.
    /// </summary>
    public Export Current => _current;

    /// <summary>
    /// Gets a value indicating whether the export operation has completed (success, failure, or canceled).
    /// </summary>
    public bool IsCompleted => _isCompleted;

    /// <summary>
    /// Gets a value indicating whether the export operation completed successfully.
    /// </summary>
    public bool IsSuccessful => _isSuccessful;

    /// <summary>
    /// Gets a value indicating whether the export operation was canceled.
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
                    await Task.Delay(ExportClient.Config.PollInterval, _cts.Token);
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
            _isSuccessful = status.Status == ExportStatus.Success;
            _isCanceled = status.Status == ExportStatus.Canceled;
            await _cts.CancelAsync();
            DisposeInternal();
        }
    }

    /// <summary>
    /// Waits asynchronously for the export operation to complete or timeout.
    /// </summary>
    /// <param name="timeout">Optional timeout for the operation. If not specified, uses the default export timeout.</param>
    /// <param name="cancellationToken">A cancellation token to observe while waiting.</param>
    /// <returns>The final <see cref="Export"/> result.</returns>
    public async Task<Export> WaitForCompletion(
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        var effectiveTimeout = timeout ?? ExportClient.Config.Timeout;
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
                    $"Export operation did not complete within {effectiveTimeout} (last status={_current.Status})."
                );
            }
            try
            {
                await Task.Delay(ExportClient.Config.PollInterval, effectiveToken);
            }
            catch (OperationCanceledException) when (_isCompleted) { }
        }
        return _current;
    }

    /// <summary>
    /// Cancels the export operation asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to observe while canceling.</param>
    public async Task Cancel(CancellationToken cancellationToken = default)
    {
        await _operationCancel(cancellationToken);
        await RefreshStatusInternal(cancellationToken);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        if (disposing)
        {
            _cts.Cancel();
            try
            {
                _backgroundRefreshTask.Wait(ExportClient.Config.PollInterval);
            }
            catch (Exception) { }
            _cts.Dispose();
        }
        _disposed = true;
    }

    private void DisposeInternal()
    {
        if (_disposed)
            return;
        try
        {
            _backgroundRefreshTask.Wait(ExportClient.Config.PollInterval);
        }
        catch (Exception)
        {
            // Best-effort disposal
        }
        _cts.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Disposes the export operation and releases resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
    }

    /// <summary>
    /// Disposes the export operation asynchronously and releases resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _cts.Cancel();
        try
        {
            await _backgroundRefreshTask.ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Best-effort disposal
        }
        _cts.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private static bool IsTerminalStatus(ExportStatus? status)
    {
        return status == ExportStatus.Success
            || status == ExportStatus.Failed
            || status == ExportStatus.Canceled;
    }
}
