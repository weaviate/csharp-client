using System.Collections.Concurrent;
using System.Threading.Channels;
using Weaviate.Client.Grpc;
using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client.Batch;

/// <summary>
/// Represents the state of a batch operation.
/// </summary>
public enum BatchState
{
    /// <summary>
    /// Batch is open and ready to accept objects.
    /// </summary>
    Open,

    /// <summary>
    /// Batch is in flight, sending/receiving objects.
    /// </summary>
    InFlight,

    /// <summary>
    /// Batch is closed and no longer accepting objects.
    /// </summary>
    Closed,
}

/// <summary>
/// Represents a server-side batch context for sending objects to Weaviate.
/// </summary>
public class BatchContext : IAsyncDisposable
{
    /// <summary>
    /// Gets the current state of the batch.
    /// </summary>
    public BatchState State { get; private set; } = BatchState.Open;

    private readonly BatchOptions? _options;
    private readonly Weaviate.Client.Grpc.WeaviateGrpcClient _grpcClient;
    private readonly BatchStreamContext _streamContext;
    private Weaviate.Client.Grpc.BatchStreamWrapper? _stream;

    private readonly ConcurrentDictionary<string, TaskHandle> _pendingTasks = new();
    private readonly Channel<BatchInsertRequest> _sendQueue;
    private Task? _readerTask;
    private Task? _writerTask;
    private readonly CancellationTokenSource _cts = new();
    private readonly TaskCompletionSource _startedTcs = new();
    private readonly SemaphoreSlim _flowControlSemaphore = new(1, 1);
    private bool _isDisposed;
    private int _batchSize = BatchOptions.DefaultBatchSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="BatchContext"/> class.
    /// </summary>
    /// <param name="grpcClient">The gRPC client</param>
    /// <param name="collectionName">The collection name</param>
    /// <param name="tenant">The tenant name</param>
    /// <param name="options">The batch options</param>
    internal BatchContext(
        Weaviate.Client.Grpc.WeaviateGrpcClient grpcClient,
        string collectionName,
        string? tenant = null,
        BatchOptions? options = null
    )
    {
        _grpcClient = grpcClient;
        _options = options;
        _batchSize = options?.BatchSize ?? BatchOptions.DefaultBatchSize;
        _sendQueue = Channel.CreateUnbounded<BatchInsertRequest>();
        _streamContext = new BatchStreamContext { Collection = collectionName, Tenant = tenant };

        // Validate options if provided
        options?.Validate();
    }

    /// <summary>
    /// Starts the batch stream and waits for the server to signal readiness.
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the async operation</returns>
    public async Task Start(CancellationToken ct = default)
    {
        if (_stream != null)
            throw new InvalidOperationException("Batch stream already started");

        // Start the gRPC batch stream with consistency level from options
        _stream = await _grpcClient.StartBatchStream(_options?.ConsistencyLevel, ct);
        State = BatchState.InFlight;

        // Start background tasks for reading responses and writing requests
        _readerTask = Task.Run(() => ReadResponsesAsync(_cts.Token), _cts.Token);
        _writerTask = Task.Run(() => WriteRequestsAsync(_cts.Token), _cts.Token);

        // Wait for the Started message from the server with a timeout
        var timeout = _options?.Timeout ?? TimeSpan.FromSeconds(30);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        linkedCts.CancelAfter(timeout);

        try
        {
            await _startedTcs.Task.WaitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new TimeoutException(
                $"Server did not respond within {timeout.TotalSeconds} seconds"
            );
        }
    }

    private async Task ReadResponsesAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (_stream == null)
                    return;

                while (true)
                {
                    if (ct.IsCancellationRequested)
                        return;

                    var message = await _stream.ReadNextAsync(ct);
                    if (message == null)
                    {
                        // Stream ended - exit the reader
                        return;
                    }

                    switch (message)
                    {
                        case BatchStreamStarted:
                            _startedTcs.TrySetResult();
                            break;
                        case BatchStreamResults results:
                            HandleResults(results);
                            break;
                        case BatchStreamAcks acks:
                            HandleAcks(acks);
                            break;
                        case BatchStreamBackoff backoff:
                            await HandleBackoff(backoff);
                            break;
                        case BatchStreamOutOfMemory oom:
                            await HandleOutOfMemory(oom);
                            break;
                        case BatchStreamShuttingDown:
                            HandleShuttingDown();
                            return;
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Attempt recovery on stream error if still open (not closed by user) and has pending work
                if (!ct.IsCancellationRequested && State == BatchState.InFlight)
                {
                    var hasPendingWork = _pendingTasks.Values.Any(h =>
                        h.Status == BatchObjectStatus.Pending
                        || h.Status == BatchObjectStatus.Acked
                        || h.Status == BatchObjectStatus.Retried
                    );
                    if (hasPendingWork)
                    {
                        await RecoverAndResumeAsync(ct);
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }
    }

    private async Task RecoverAndResumeAsync(CancellationToken ct)
    {
        // Wait a moment before attempting to recover
        await Task.Delay(TimeSpan.FromSeconds(1), ct);

        // Attempt to re-establish the stream
        _stream = await _grpcClient.StartBatchStream(_options?.ConsistencyLevel, ct);
        State = BatchState.InFlight;

        // Restart the writer task if not running
        if (_writerTask == null || _writerTask.IsCompleted)
        {
            _writerTask = Task.Run(() => WriteRequestsAsync(_cts.Token), _cts.Token);
        }

        // Resend all objects that are not Acked
        foreach (var kvp in _pendingTasks)
        {
            var handle = kvp.Value;
            if (
                handle.Status == BatchObjectStatus.Pending
                || handle.Status == BatchObjectStatus.Failed
                || handle.Status == BatchObjectStatus.Retried
            )
            {
                if (handle.OriginalRequest != null)
                {
                    await _sendQueue.Writer.WriteAsync(handle.OriginalRequest, ct);
                }
            }
        }
    }

    private async Task WriteRequestsAsync(CancellationToken ct)
    {
        if (_stream == null)
            return;

        try
        {
            var batch = new List<BatchInsertRequest>();
            var flushDelay = TimeSpan.FromMilliseconds(50);

            while (!ct.IsCancellationRequested)
            {
                // Try to read with a timeout for auto-flush of partial batches
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(flushDelay);

                try
                {
                    // Try to read the next item
                    if (await _sendQueue.Reader.WaitToReadAsync(timeoutCts.Token))
                    {
                        while (_sendQueue.Reader.TryRead(out var obj))
                        {
                            // Only send objects that are Pending or Retried
                            var uuid = obj.UUID?.ToString();
                            if (
                                !string.IsNullOrEmpty(uuid)
                                && _pendingTasks.TryGetValue(uuid, out var handle)
                            )
                            {
                                if (
                                    handle.Status != BatchObjectStatus.Pending
                                    && handle.Status != BatchObjectStatus.Retried
                                )
                                    continue;
                            }

                            batch.Add(obj);

                            if (batch.Count >= _batchSize)
                            {
                                await SendBatchAsync(batch, ct);
                                batch.Clear();
                            }
                        }
                    }
                    else
                    {
                        // Channel completed, exit loop
                        break;
                    }
                }
                catch (OperationCanceledException)
                    when (timeoutCts.IsCancellationRequested && !ct.IsCancellationRequested)
                {
                    // Timeout - flush partial batch if we have items
                    if (batch.Count > 0)
                    {
                        await SendBatchAsync(batch, ct);
                        batch.Clear();
                    }
                }
            }

            // Send any remaining objects
            if (batch.Count > 0)
            {
                await SendBatchAsync(batch, ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Log or handle writer errors
        }
    }

    private async Task SendBatchAsync(List<BatchInsertRequest> batch, CancellationToken ct)
    {
        if (_stream == null)
            return;

        await _stream.SendObjectsAsync(batch, _streamContext, ct);
    }

    private void HandleResults(BatchStreamResults results)
    {
        // Process errors
        foreach (var error in results.Errors)
        {
            var uuid = error.UUID;
            if (_pendingTasks.TryGetValue(uuid, out var handle))
            {
                handle.SetResult(
                    new BatchResult
                    {
                        Success = false,
                        ErrorMessage = error.Error,
                        ServerResponse = error,
                    }
                );
            }
        }

        // Process successes
        foreach (var success in results.Successes)
        {
            var uuid = success.UUID;
            if (_pendingTasks.TryGetValue(uuid, out var handle))
            {
                handle.SetResult(
                    new BatchResult
                    {
                        Success = true,
                        ErrorMessage = null,
                        ServerResponse = success,
                    }
                );
                _pendingTasks.TryRemove(uuid, out _);
            }
        }
    }

    private void HandleAcks(BatchStreamAcks acks)
    {
        // Mark all acked UUIDs as acknowledged
        foreach (var uuid in acks.UUIDs)
        {
            if (_pendingTasks.TryGetValue(uuid, out var handle))
            {
                handle.SetAcked();
            }
        }
    }

    private async Task HandleBackoff(BatchStreamBackoff backoff)
    {
        // Server requests backoff - adjust batch size and pause briefly
        var newBatchSize = Math.Max(backoff.BatchSize, 100);
        if (newBatchSize < _batchSize)
        {
            _batchSize = newBatchSize;

            // Pause sending briefly to allow server to catch up
            await _flowControlSemaphore.WaitAsync();
            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
            finally
            {
                _flowControlSemaphore.Release();
            }
        }
    }

    private async Task HandleOutOfMemory(BatchStreamOutOfMemory outOfMemory)
    {
        // Server is out of memory - pause sending for the specified duration
        await _flowControlSemaphore.WaitAsync();
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(outOfMemory.WaitTimeSeconds));
        }
        finally
        {
            _flowControlSemaphore.Release();
        }
    }

    private void HandleShuttingDown()
    {
        // Server is shutting down - close the stream gracefully
        State = BatchState.Closed;
        _cts.Cancel();
    }

    /// <summary>
    /// Adds an object to the batch with optional UUID, vectors, and references.
    /// </summary>
    /// <param name="obj">The object to add</param>
    /// <param name="uuid">Optional UUID for the object</param>
    /// <param name="vectors">Optional vectors for the object</param>
    /// <param name="references">Optional references for the object</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task handle for tracking the object's status</returns>
    public async ValueTask<TaskHandle> Add(
        object obj,
        Guid? uuid = null,
        Vectors? vectors = null,
        IEnumerable<ObjectReference>? references = null,
        CancellationToken ct = default
    )
    {
        if (obj is BatchInsertRequest bir)
        {
            return await Add(bir, ct);
        }

        return await Add(BatchInsertRequest.Create(obj, uuid, vectors, references), ct);
    }

    /// <summary>
    /// Adds a BatchInsertRequest to the batch.
    /// </summary>
    /// <param name="request">The batch insert request to add</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task handle for tracking the object's status</returns>
    public async ValueTask<TaskHandle> Add(
        BatchInsertRequest request,
        CancellationToken ct = default
    )
    {
        if (State == BatchState.Closed)
            throw new InvalidOperationException("Batch is closed");

        // Ensure UUID is always set
        var finalUuid = request.UUID ?? Guid.NewGuid();
        var requestWithUuid = request with { UUID = finalUuid };

        var handle = new TaskHandle { TimesRetried = 0, OriginalRequest = requestWithUuid };

        _pendingTasks[finalUuid.ToString()] = handle;
        await _sendQueue.Writer.WriteAsync(requestWithUuid, ct);
        return handle;
    }

    /// <summary>
    /// Retries a failed batch operation.
    /// </summary>
    /// <param name="handle">The task handle to retry</param>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A new task handle for the retried operation</returns>
    public async ValueTask<TaskHandle> Retry(TaskHandle handle, CancellationToken ct = default)
    {
        if (State == BatchState.Closed)
            throw new InvalidOperationException("Batch is closed");

        if (handle.OriginalRequest == null)
            throw new InvalidOperationException(
                "Cannot retry: original request not available in handle"
            );

        // Reuse the original UUID for the retry (true upsert/replace semantics)
        var originalUuid = handle.OriginalRequest.UUID ?? Guid.NewGuid();
        var requestWithSameUuid = handle.OriginalRequest with { UUID = originalUuid };

        // Mark as retried (this resets the result TCS)
        handle.SetRetried();

        // Reuse the same handle for retry, update the request
        handle.OriginalRequest = requestWithSameUuid;
        handle.TimesRetried++;

        // Re-add the handle to pending tasks so results can be matched
        _pendingTasks[originalUuid.ToString()] = handle;

        await _sendQueue.Writer.WriteAsync(requestWithSameUuid, ct);

        return handle;
    }

    /// <summary>
    /// Disposes the batch context asynchronously.
    /// </summary>
    /// <returns>A task representing the async dispose operation</returns>
    public async ValueTask DisposeAsync()
    {
        await Close();
    }

    /// <summary>
    /// Closes the batch context and waits for all pending operations to complete.
    /// </summary>
    /// <param name="ct">The cancellation token</param>
    /// <returns>A task representing the async close operation</returns>
    public async Task Close(CancellationToken ct = default)
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        State = BatchState.Closed;

        // Complete the send queue to signal no more objects
        _sendQueue.Writer.Complete();

        // Wait for writer to finish sending all pending objects
        if (_writerTask != null)
        {
            try
            {
                await _writerTask.WaitAsync(ct);
            }
            catch (OperationCanceledException)
            {
                // Writer was cancelled, continue with cleanup
            }
        }

        // Send Stop message and complete the request stream
        if (_stream != null)
        {
            await _stream.SendStopAsync(ct);
        }

        // Wait for reader to finish processing all responses (with a timeout)
        if (_readerTask != null)
        {
            try
            {
                // Give the reader time to process remaining results
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
                await _readerTask.WaitAsync(timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                // Reader timed out or was cancelled, cancel cts and mark remaining tasks as failed
                _cts.Cancel();
                foreach (var kvp in _pendingTasks)
                {
                    kvp.Value.SetFailed("Batch closed before result was received");
                }
            }
        }

        // Now cancel and dispose
        _cts.Cancel();
        if (_stream != null)
            await _stream.DisposeAsync();
        _cts.Dispose();
        _flowControlSemaphore.Dispose();
    }
}
