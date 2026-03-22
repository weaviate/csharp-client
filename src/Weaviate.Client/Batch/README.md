# Weaviate.Client.Batch

This folder contains the core types for server-side batching support in the Weaviate C# client.

## Core Types

### `BatchManager`

High-level API for batch operations. Accessed via `collection.Batch`.

**Methods:**
- `StartBatch(BatchOptions?, CancellationToken)` - Start a low-level batch context
- `InsertMany(IEnumerable<BatchInsertRequest>, ...)` - Insert from enumerable
- `InsertMany<T>(IEnumerable<T>, ...)` - Insert typed objects from enumerable
- `InsertMany(IAsyncEnumerable<BatchInsertRequest>, ...)` - Insert from async enumerable
- `InsertMany<T>(IAsyncEnumerable<T>, ...)` - Insert typed objects from async enumerable
- `InsertMany(ChannelReader<BatchInsertRequest>, ...)` - Insert from channel (accumulating)
- `InsertMany<T>(ChannelReader<T>, ...)` - Insert typed objects from channel (accumulating)
- `InsertMany(ChannelReader<BatchInsertRequest>, ChannelWriter<BatchInsertResponseEntry>, ...)` - Streaming results
- `InsertMany<T>(ChannelReader<T>, ChannelWriter<BatchInsertResponseEntry>, ...)` - Streaming results with typed input

### `BatchContext`

Manages the lifecycle and state of a batch, including adding objects, retrying, and closing the batch. Implements `IAsyncDisposable`.

**Methods:**
- `Add(object, Guid?, ...)` - Add an object with optional UUID, vectors, and references
- `Add(BatchInsertRequest, CancellationToken)` - Add a batch insert request
- `Retry(TaskHandle, CancellationToken)` - Retry a failed operation
- `Close(CancellationToken)` - Close the batch and wait for all results

**Properties:**
- `State` - Current `BatchState` (Open, InFlight, Closed, Aborted)

### `TaskHandle`

Represents a submitted batch task, tracks status, result, and retries.

**Properties:**
- `Uuid` - The UUID assigned to this object (from the original request)
- `IsAcked` - Task that completes when acknowledged by the server
- `Result` - Task that completes with the final `BatchResult`
- `TimesRetried` - Number of retry attempts


### `BatchResult`

Contains the result of a batch operation.

**Properties:**
- `Success` - Whether the operation succeeded
- `ErrorMessage` - Error message if failed
- `ServerResponse` - Raw server response (optional)

### `BatchOptions`

Configuration for batch operations.

**Properties:**
- `BatchSize` - Objects per request (1-1000, default 100)
- `ConsistencyLevel` - Write consistency level
- `MaxRetries` - Maximum retry attempts (default 3)
- `RetryDelay` - Initial retry delay (exponential backoff)

## Usage Examples

### High-Level API (Recommended)

```csharp
// Insert many objects at once
var response = await collection.Batch.InsertMany(objects);

// Check results
foreach (var entry in response.Entries)
{
    if (entry.Exception != null)
    {
        Console.WriteLine($"Failed: {entry.Exception.Message}");
    }
}
```

### Streaming with Channels (High-Volume)

```csharp
var inputChannel = Channel.CreateUnbounded<BatchInsertRequest>();
var outputChannel = Channel.CreateUnbounded<BatchInsertResponseEntry>();

// Producer: write objects to input channel
var produceTask = Task.Run(async () => {
    foreach (var obj in GetObjects())
    {
        await inputChannel.Writer.WriteAsync(new BatchInsertRequest(obj));
    }
    inputChannel.Writer.Complete();
});

// Consumer: read results as they complete
var consumeTask = Task.Run(async () => {
    await foreach (var entry in outputChannel.Reader.ReadAllAsync())
    {
        // Process result immediately
    }
});

// Process the batch with streaming results
await collection.Batch.InsertMany(
    inputChannel.Reader,
    outputChannel.Writer
);
outputChannel.Writer.Complete();

await Task.WhenAll(produceTask, consumeTask);
```

### Low-Level API (Fine-Grained Control)

```csharp
await using var batch = await collection.Batch.StartBatch();

var handles = new List<TaskHandle>();
foreach (var obj in objects)
{
    var handle = await batch.Add(obj);
    handles.Add(handle);
}

await batch.Close();

// Inspect results and retry if needed
foreach (var handle in handles)
{
    var result = await handle.Result;
    if (!result.Success && handle.TimesRetried < 3)
    {
        await batch.Retry(handle);
    }
}
```
