using System;
using System.Threading;
using System.Threading.Tasks;

namespace Weaviate.Client.Models
{
    /// <summary>
    /// Abstract base for backup/restore operations, with status polling and cancellation.
    /// </summary>
    public abstract class BackupOperationBase : IDisposable
    {
        private readonly Func<Task<Backup>> _statusFetcher;
        private readonly Func<Task> _operationCancel;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _backgroundRefreshTask;
        private Backup _current;
        private bool _isCompleted;
        private bool _isSuccessful;
        private bool _isCanceled;
        private bool _disposed;

        protected BackupOperationBase(
            Backup initial,
            Func<Task<Backup>> statusFetcher,
            Func<Task> operationCancel
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
                    catch
                    {
                        // Swallow errors, optionally log
                    }
                }
            });
        }

        private async Task RefreshStatusInternal()
        {
            if (_isCompleted)
                return;
            var status = await _statusFetcher();
            _current = status;
            if (IsTerminalStatus(status.Status))
            {
                _isCompleted = true;
                _isSuccessful = status.Status == BackupStatus.Success;
                _isCanceled = status.Status == BackupStatus.Canceled;
                await _cts.CancelAsync(); // Stop background polling
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

            while (!_isCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (DateTime.UtcNow - start > effectiveTimeout)
                {
                    throw new TimeoutException(
                        $"Backup operation did not complete within {effectiveTimeout} (last status={_current.Status})."
                    );
                }
                await Task.Delay(BackupClient.Config.PollInterval, _cts.Token);
            }
            return _current;
        }

        /// <summary>
        /// Requests cancellation of the operation.
        /// </summary>
        public async Task Cancel()
        {
            await _cts.CancelAsync();

            // Call server-side Cancel
            await _operationCancel();

            await RefreshStatusInternal();
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
                catch (Exception)
                {
                    // Ignore exceptions from waiting on background task, as disposal is best-effort
                }
                _cts.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private static bool IsTerminalStatus(BackupStatus? status)
        {
            return status == BackupStatus.Success
                || status == BackupStatus.Failed
                || status == BackupStatus.Canceled;
        }
    }
}
