namespace Weaviate.Client.Models;

/// <summary>
/// Represents a backup creation operation, with status polling and cancellation.
/// </summary>
public class BackupCreateOperation : BackupOperationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackupCreateOperation"/> class
    /// </summary>
    /// <param name="initial">The initial</param>
    /// <param name="statusFetcher">The status fetcher</param>
    /// <param name="operationCancel">The operation cancel</param>
    internal BackupCreateOperation(
        Backup initial,
        Func<CancellationToken, Task<Backup>> statusFetcher,
        Func<CancellationToken, Task> operationCancel
    )
        : base(initial, statusFetcher, operationCancel) { }
}
