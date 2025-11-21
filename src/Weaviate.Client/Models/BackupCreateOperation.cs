namespace Weaviate.Client.Models;

/// <summary>
/// Represents a backup creation operation, with status polling and cancellation.
/// </summary>
public class BackupCreateOperation : BackupOperationBase
{
    internal BackupCreateOperation(
        Backup initial,
        Func<CancellationToken, Task<Backup>> statusFetcher,
        Func<CancellationToken, Task> operationCancel
    )
        : base(initial, statusFetcher, operationCancel) { }
}
