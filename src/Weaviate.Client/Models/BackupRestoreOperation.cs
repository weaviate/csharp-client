namespace Weaviate.Client.Models;

/// <summary>
/// Represents a backup restore operation, with status polling and cancellation.
/// </summary>
public class BackupRestoreOperation : BackupOperationBase
{
    internal BackupRestoreOperation(
        Backup initial,
        Func<CancellationToken, Task<Backup>> statusFetcher,
        Func<CancellationToken, Task> operationCancel
    )
        : base(initial, statusFetcher, operationCancel) { }
}
