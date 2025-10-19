using System;
using System.Threading.Tasks;

namespace Weaviate.Client.Models
{
    /// <summary>
    /// Represents a backup operation, with status polling and cancellation.
    /// </summary>
    public class BackupOperation : BackupOperationBase
    {
        public BackupOperation(
            Backup initial,
            Func<Task<Backup>> statusFetcher,
            Func<Task> operationCancel
        )
            : base(initial, statusFetcher, operationCancel) { }
    }
}
