namespace Weaviate.Client.Internal;

/// <summary>
/// Represents a message received from the batch stream.
/// </summary>
internal abstract class BatchStreamMessage { }

/// <summary>
/// Server has started and is ready to receive objects.
/// </summary>
internal class BatchStreamStarted : BatchStreamMessage { }

/// <summary>
/// Server is shutting down.
/// </summary>
internal class BatchStreamShuttingDown : BatchStreamMessage { }

/// <summary>
/// Server requests a backoff with a new batch size.
/// </summary>
internal class BatchStreamBackoff : BatchStreamMessage
{
    public int BatchSize { get; set; }
}

/// <summary>
/// Server is out of memory and requests a wait.
/// </summary>
internal class BatchStreamOutOfMemory : BatchStreamMessage
{
    public IReadOnlyList<string> UUIDs { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Beacons { get; set; } = Array.Empty<string>();
    public int WaitTimeSeconds { get; set; }
}

/// <summary>
/// Server acknowledges receipt of objects.
/// </summary>
internal class BatchStreamAcks : BatchStreamMessage
{
    public IReadOnlyList<string> UUIDs { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string> Beacons { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Server returns results for objects.
/// </summary>
internal class BatchStreamResults : BatchStreamMessage
{
    public IReadOnlyList<BatchStreamError> Errors { get; set; } = Array.Empty<BatchStreamError>();
    public IReadOnlyList<BatchStreamSuccess> Successes { get; set; } =
        Array.Empty<BatchStreamSuccess>();
}

/// <summary>
/// An error result for a batch object.
/// </summary>
internal class BatchStreamError
{
    public string UUID { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

/// <summary>
/// A success result for a batch object.
/// </summary>
internal class BatchStreamSuccess
{
    public string UUID { get; set; } = string.Empty;
}
