using Weaviate.Client.Models;

namespace Weaviate.Client.Batch
{
    /// <summary>
    /// Represents a handle for tracking the status of a batch operation.
    /// </summary>
    /// <summary>
    /// Represents the status of a batch object in the stream.
    /// </summary>
    /// <summary>
    /// Status of a batch object in the streaming batch API.
    /// </summary>
    public enum BatchObjectStatus
    {
        /// <summary>
        /// The object is pending and has not yet been sent or acknowledged.
        /// </summary>
        Pending,

        /// <summary>
        /// The object has been acknowledged by the server.
        /// </summary>
        Acked,

        /// <summary>
        /// The object failed to be processed by the server.
        /// </summary>
        Failed,

        /// <summary>
        /// The object is being retried after a failure or interruption.
        /// </summary>
        Retried,
    }

    /// <summary>
    /// Represents a handle for tracking the status and result of a batch operation object.
    /// </summary>
    public class TaskHandle
    {
        private TaskCompletionSource<bool> _isAckedTcs = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        private TaskCompletionSource<BatchResult> _resultTcs = new(
            TaskCreationOptions.RunContinuationsAsynchronously
        );

        /// <summary>
        /// Gets the current status of this batch object in the batch stream.
        /// </summary>
        public BatchObjectStatus Status { get; internal set; } = BatchObjectStatus.Pending;

        /// <summary>
        /// Gets the UUID assigned to this object (either user-specified or auto-generated).
        /// </summary>
        public Guid? Uuid => OriginalRequest?.UUID;

        /// <summary>
        /// Gets a task that completes when the object is acknowledged by the server.
        /// </summary>
        public Task<bool> IsAcked => _isAckedTcs.Task;

        /// <summary>
        /// Gets a task that completes with the final result of the batch operation.
        /// </summary>
        public Task<BatchResult> Result => _resultTcs.Task;

        /// <summary>
        /// Gets the number of times this operation has been retried.
        /// </summary>
        public int TimesRetried { get; internal set; }

        /// <summary>
        /// Internal storage of the original batch request for retry purposes.
        /// </summary>
        internal BatchInsertRequest? OriginalRequest { get; set; }

        internal void SetAcked()
        {
            Status = BatchObjectStatus.Acked;
            _isAckedTcs.TrySetResult(true);
        }

        internal void SetResult(BatchResult result)
        {
            Status = result.Success ? BatchObjectStatus.Acked : BatchObjectStatus.Failed;
            _resultTcs.TrySetResult(result);
        }

        internal void SetFailed(string errorMessage)
        {
            Status = BatchObjectStatus.Failed;
            _resultTcs.TrySetResult(
                new BatchResult { Success = false, ErrorMessage = errorMessage }
            );
        }

        internal void SetRetried()
        {
            Status = BatchObjectStatus.Retried;
            // Reset the result TCS so that awaiting Result will wait for the new result
            _resultTcs = new TaskCompletionSource<BatchResult>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            _isAckedTcs = new TaskCompletionSource<bool>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
        }
    }
}
