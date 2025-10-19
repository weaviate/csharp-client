using System;
using System.Threading.Tasks;

namespace Weaviate.Client.Models
{
    /// <summary>
    /// Represents a restore operation for a backup, with status polling and cancellation.
    /// </summary>
    public class RestoreOperation : BackupOperationBase
    {
        public RestoreOperation(
            Backup initial,
            Func<Task<Backup>> statusFetcher,
            Func<Task> operationCancel
        )
            : base(initial, statusFetcher, operationCancel) { }
    }
}
