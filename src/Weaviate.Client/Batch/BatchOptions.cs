namespace Weaviate.Client.Batch
{
    /// <summary>
    /// Options for configuring batch operations.
    /// </summary>
    public class BatchOptions
    {
        /// <summary>
        /// Default batch size (number of objects sent per request).
        /// </summary>
        public const int DefaultBatchSize = 100;

        /// <summary>
        /// Minimum batch size allowed by the server.
        /// </summary>
        public const int MinBatchSize = 1;

        /// <summary>
        /// Maximum batch size allowed by the server.
        /// </summary>
        public const int MaxBatchSize = 1000;

        /// <summary>
        /// Default maximum retry attempts.
        /// </summary>
        public const int DefaultMaxRetries = 3;

        /// <summary>
        /// Gets or sets the batch size (number of objects to send per request).
        /// Must be between 1 and 1000. Defaults to 100.
        /// </summary>
        public int BatchSize { get; set; } = DefaultBatchSize;

        /// <summary>
        /// Gets or sets the consistency level for write operations.
        /// When null, the server default is used.
        /// </summary>
        public ConsistencyLevels? ConsistencyLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts for failed objects.
        /// Set to 0 to disable automatic retries. Defaults to 3.
        /// </summary>
        /// <remarks>Reserved for future use. Automatic retries are not yet implemented;
        /// use <see cref="Weaviate.Client.Batch.BatchContext.Retry"/> for manual retries.</remarks>
        public int MaxRetries { get; set; } = DefaultMaxRetries;

        /// <summary>
        /// Gets or sets the initial delay between retry attempts.
        /// The delay is increased exponentially on subsequent retries.
        /// Defaults to 100 milliseconds.
        /// </summary>
        /// <remarks>Reserved for future use. Not yet applied by the batch engine.</remarks>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// Gets or sets the maximum delay between retry attempts.
        /// Defaults to 10 seconds.
        /// </summary>
        /// <remarks>Reserved for future use. Not yet applied by the batch engine.</remarks>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>
        /// Gets or sets the timeout for the batch operation.
        /// When null, no timeout is applied.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to stop processing on the first error.
        /// When false, continues processing remaining objects after failures. Defaults to false.
        /// </summary>
        /// <remarks>Reserved for future use. Not yet enforced by the batch engine.</remarks>
        public bool StopOnFirstError { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to skip objects that fail validation.
        /// When true, invalid objects are silently skipped. When false, they cause an error. Defaults to false.
        /// </summary>
        /// <remarks>Reserved for future use. Not yet enforced by the batch engine.</remarks>
        public bool SkipInvalidObjects { get; set; } = false;

        /// <summary>
        /// Validates the options and throws if invalid.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when options are invalid.</exception>
        public void Validate()
        {
            if (BatchSize < MinBatchSize || BatchSize > MaxBatchSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(BatchSize),
                    BatchSize,
                    $"BatchSize must be between {MinBatchSize} and {MaxBatchSize}."
                );
            }

            if (MaxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MaxRetries),
                    MaxRetries,
                    "MaxRetries cannot be negative."
                );
            }

            if (RetryDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(RetryDelay),
                    RetryDelay,
                    "RetryDelay cannot be negative."
                );
            }

            if (MaxRetryDelay < RetryDelay)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MaxRetryDelay),
                    MaxRetryDelay,
                    "MaxRetryDelay must be greater than or equal to RetryDelay."
                );
            }

            if (Timeout.HasValue && Timeout.Value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Timeout),
                    Timeout,
                    "Timeout must be positive."
                );
            }
        }

        /// <summary>
        /// Creates a new BatchOptions with default values.
        /// </summary>
        /// <returns>A new BatchOptions instance with default values.</returns>
        public static BatchOptions Default => new();

        /// <summary>
        /// Creates a new BatchOptions optimized for high throughput.
        /// Uses larger batch sizes and fewer retries.
        /// </summary>
        /// <returns>A new BatchOptions instance optimized for throughput.</returns>
        public static BatchOptions HighThroughput =>
            new()
            {
                BatchSize = MaxBatchSize,
                MaxRetries = 1,
                StopOnFirstError = false,
            };

        /// <summary>
        /// Creates a new BatchOptions optimized for reliability.
        /// Uses smaller batch sizes and more retries with longer delays.
        /// </summary>
        /// <returns>A new BatchOptions instance optimized for reliability.</returns>
        public static BatchOptions HighReliability =>
            new()
            {
                BatchSize = 50,
                MaxRetries = 5,
                RetryDelay = TimeSpan.FromMilliseconds(500),
                MaxRetryDelay = TimeSpan.FromSeconds(30),
                StopOnFirstError = false,
            };
    }
}
