namespace Weaviate.Client.Batch
{
    /// <summary>
    /// Represents the result of a batch operation.
    /// </summary>
    public class BatchResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation was successful.
        /// </summary>
        public bool Success { get; internal set; }

        /// <summary>
        /// Gets or sets the error message if the operation failed.
        /// </summary>
        public string? ErrorMessage { get; internal set; }

        /// <summary>
        /// Gets or sets the server response.
        /// </summary>
        public object? ServerResponse { get; internal set; }
    }
}
