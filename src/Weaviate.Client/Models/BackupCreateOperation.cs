namespace Weaviate.Client.Models;

/// <summary>
/// Represents a backup creation operation, with status polling and cancellation.
/// </summary>
public class BackupCreateOperation : BackupOperationBase
{
    internal BackupCreateOperation(
        Backup initial,
        Func<Task<Backup>> statusFetcher,
        Func<Task> operationCancel
    )
        : base(initial, statusFetcher, operationCancel) { }
}
