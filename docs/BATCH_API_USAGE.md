# Batch API Usage Guide


> **Version Requirement:**
> Server-side batch streaming requires Weaviate v1.36.0 or newer (or v1.35 with the experimental flag enabled). Earlier versions are not supported and will fail.

This guide covers the Weaviate C# client's server-side batching functionality. It provides examples and best practices for efficiently inserting large numbers of objects.

## Table of Contents

- [Overview](#overview)
- [High-Level API](#high-level-api)
- [Streaming with Channels](#streaming-with-channels)
- [Async Enumerable Support](#async-enumerable-support)
- [Low-Level API](#low-level-api)
- [Reference Batching](#reference-batching)
- [Configuration](#configuration)
- [Error Handling](#error-handling)
- [Best Practices](#best-practices)

## Overview

The Batch API uses gRPC bidirectional streaming to efficiently insert objects into Weaviate. Key features include:

- **Automatic batching**: Objects are batched for optimal throughput.
- **Streaming results**: Results are available as soon as each object is processed.
- **Retry support**: Failed operations can be retried with built-in backoff.
- **Multiple input patterns**: Support for `IEnumerable`, `IAsyncEnumerable`, and `Channel<T>`.

## High-Level API

The high-level API is the simplest way to insert many objects. Access it via `collection.Batch`:

### Basic Insert

```csharp
// Insert many objects at once
var objects = new[]
{
    new { Title = "Document 1", Content = "..." },
    new { Title = "Document 2", Content = "..." },
    new { Title = "Document 3", Content = "..." }
};

var response = await collection.Batch.InsertMany(objects);

// Check results
foreach (var entry in response.Entries)
{
    if (entry.Exception != null)
    {
        Console.WriteLine($"Index {entry.Index} failed: {entry.Exception.Message}");
    }
    else
    {
        Console.WriteLine($"Index {entry.Index} inserted with UUID: {entry.UUID}");
    }
}
```

### Using BatchInsertRequest for Control

Use `BatchInsertRequest` when you need to specify UUIDs, vectors, or references:

```csharp
var requests = new[]
{
    new BatchInsertRequest(new { Title = "Doc 1" })
    {
        UUID = Guid.NewGuid(),
        Vectors = new Dictionary<string, float[]>
        {
            ["default"] = new float[] { 0.1f, 0.2f, 0.3f }
        }
    },
    new BatchInsertRequest(new { Title = "Doc 2" })
    {
        UUID = Guid.NewGuid()
    }
};

var response = await collection.Batch.InsertMany(requests);
```

## Streaming with Channels

For high-volume scenarios, use `Channel<T>` to stream objects and receive results as they complete:

### Streaming Results

```csharp
var inputChannel = Channel.CreateUnbounded<BatchInsertRequest>();
var outputChannel = Channel.CreateUnbounded<BatchInsertResponseEntry>();

// Producer: feed objects into the batch
var produceTask = Task.Run(async () =>
{
    foreach (var obj in GetLargeDataSet())
    {
        await inputChannel.Writer.WriteAsync(new BatchInsertRequest(obj));
    }
    inputChannel.Writer.Complete();
});

// Consumer: process results as they arrive
var consumeTask = Task.Run(async () =>
{
    await foreach (var entry in outputChannel.Reader.ReadAllAsync())
    {
        if (entry.Exception != null)
        {
            Console.WriteLine($"Failed: {entry.Exception.Message}");
        }
    }
});

// Process the batch
await collection.Batch.InsertMany(
    inputChannel.Reader,
    outputChannel.Writer
);
outputChannel.Writer.Complete();

await Task.WhenAll(produceTask, consumeTask);
```

### Accumulating Results

If you prefer to get all results at once, omit the output channel:

```csharp
var inputChannel = Channel.CreateUnbounded<BatchInsertRequest>();

// Producer task...
var produceTask = Task.Run(async () =>
{
    foreach (var obj in GetLargeDataSet())
    {
        await inputChannel.Writer.WriteAsync(new BatchInsertRequest(obj));
    }
    inputChannel.Writer.Complete();
});

// Process and accumulate results
var response = await collection.Batch.InsertMany(inputChannel.Reader);
await produceTask;

Console.WriteLine($"Inserted {response.Entries.Count} objects");
```

## Async Enumerable Support

The batch API supports `IAsyncEnumerable<T>` for lazy data loading:

```csharp
async IAsyncEnumerable<BatchInsertRequest> LoadFromDatabaseAsync()
{
    await foreach (var record in database.StreamRecordsAsync())
    {
        yield return new BatchInsertRequest(new
        {
            Title = record.Title,
            Content = record.Content
        });
    }
}

var response = await collection.Batch.InsertMany(LoadFromDatabaseAsync());
```

## Low-Level API

For fine-grained control over the batch lifecycle, use the low-level `BatchContext`:

```csharp
await using var batch = await collection.Batch.StartBatch();

var handles = new List<TaskHandle>();

foreach (var obj in objects)
{
    var handle = await batch.Add(obj);
    handles.Add(handle);
}

// Close the batch to signal completion
await batch.Close();

// Process results
foreach (var handle in handles)
{
    var result = await handle.Result;

    if (!result.Success)
    {
        Console.WriteLine($"Failed: {result.ErrorMessage}");

        // Optionally retry
        if (handle.TimesRetried < 3)
        {
            await batch.Retry(handle);
        }
    }
}
```

### TaskHandle Properties

- `Uuid` - The UUID assigned to this object
- `IsAcked` - Task that completes when acknowledged by server
- `Result` - Task that completes with the final `BatchResult`
- `TimesRetried` - Number of retry attempts
- `Error` - Exception if operation failed

### BatchContext State

The `BatchContext.State` property indicates the current state:

- `Open` - Batch is accepting objects
- `InFlight` - Processing in progress
- `Closed` - Batch completed normally
- `Aborted` - Batch failed with error

## Reference Batching

Cross-references can be inserted via the same SSB stream using `BatchContext.AddReference`. This sends references as `BatchStreamRequest.Data.References` proto messages, tracked by source beacon.

### Basic usage

```csharp
await using var batch = await sourceCollection.Batch.StartBatch();

var objHandle = await batch.Add(
    BatchInsertRequest.Create(new { Name = "Article 1" }, uuid: sourceId)
);
await batch.Close();
await objHandle.Result; // ensure the source object is committed first

await using var refBatch = await sourceCollection.Batch.StartBatch();

var refHandle = await refBatch.AddReference(
    new DataReference(sourceId, "hasAuthor", authorId)
);

await refBatch.Close();

var result = await refHandle.Result;
if (!result.Success)
    Console.WriteLine($"Reference failed: {result.ErrorMessage}");
```

> **Note:** Insert and commit source objects before adding references that point from them — the server validates that the source object exists when the reference batch is processed.

### DataReference

`DataReference` describes a set of outgoing references from one object to one or more targets:

```csharp
// Same-collection reference (FromCollection inferred from stream context)
new DataReference(sourceId, "hasTag", tagId)

// Multiple targets inline
new DataReference(sourceId, "hasTag", tagId1, tagId2, tagId3)

// Explicit FromCollection (required when using DataClient.ReferenceAddMany
// with objects from a different collection than the DataClient's own)
new DataReference(sourceId, "hasAuthor", authorId) { FromCollection = "Articles" }

// Cross-collection reference
new DataReference(sourceId, "hasAuthor", authorId)
{
    FromCollection = "Articles",
    ToCollection = "Authors",
}
```

| Property | Type | Description |
| --- | --- | --- |
| `From` | `Guid` | UUID of the source object |
| `FromProperty` | `string` | Name of the reference property on the source |
| `To` | `IEnumerable<Guid>` | Target object UUIDs |
| `FromCollection` | `string?` | Source collection name. When not set, `DataClient` infers it from the collection context and `BatchContext` infers it from the stream context. |
| `ToCollection` | `string?` | Target collection name. Only needed for cross-collection references. |
| `Beacon` | `string?` | Computed source beacon (`weaviate://localhost/{FromCollection}/{From}/{FromProperty}`). `null` when `FromCollection` is not set. |

### REST batch references

`DataClient.ReferenceAddMany` also accepts `DataReference`. `FromCollection` is optional — `DataClient` automatically sets it to the collection's own name before sending, so you only need to set it when the source objects belong to a different collection:

```csharp
// FromCollection omitted — DataClient fills it in from the collection context
await sourceCollection.Data.ReferenceAddMany(
[
    new DataReference(sourceId, "hasAuthor", authorId1),
    new DataReference(sourceId, "hasAuthor", authorId2),
]);

// FromCollection set explicitly — only needed for cross-collection scenarios
await sourceCollection.Data.ReferenceAddMany(
[
    new DataReference(sourceId, "hasAuthor", authorId)
    {
        FromCollection = "Articles",
        ToCollection = "Authors",
    },
]);
```

## Configuration

Configure batch behavior with `BatchOptions`:

```csharp
var options = new BatchOptions
{
    BatchSize = 200,      // Objects per request (1-1000, default 100)
    MaxRetries = 5,       // Maximum retry attempts (default 3)
    ConsistencyLevel = ConsistencyLevels.Quorum
};

var response = await collection.Batch.InsertMany(objects, options);
```

### Option Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `BatchSize` | `int` | 100 | Number of objects per request (1-1000) |
| `MaxRetries` | `int` | 3 | Maximum retry attempts for failed objects |
| `ConsistencyLevel` | `ConsistencyLevels?` | null | Write consistency level |

## Error Handling

### Checking Individual Results

```csharp
var response = await collection.Batch.InsertMany(objects);

var succeeded = response.Entries.Count(e => e.Exception == null);
var failed = response.Entries.Where(e => e.Exception != null);

Console.WriteLine($"{succeeded} succeeded, {failed.Count()} failed");

foreach (var entry in failed)
{
    Console.WriteLine($"Index {entry.Index}: {entry.Exception?.Message}");
}
```

### Low-Level Error Inspection

```csharp
await using var batch = await collection.Batch.StartBatch();
var handle = await batch.Add(obj);
await batch.Close();

var result = await handle.Result;
if (!result.Success)
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
    Console.WriteLine($"Server response: {result.ServerResponse}");
}
```

## Best Practices

1. **Use the high-level API** for most scenarios - it handles batching, retries, and cleanup automatically.

2. **Use streaming for large datasets** - `Channel<T>` or `IAsyncEnumerable<T>` avoids loading everything into memory.

3. **Configure appropriate batch sizes** - Larger batches have better throughput but use more memory. Start with the default (100) and adjust based on your object sizes.

4. **Handle failures gracefully** - Check each result entry and implement appropriate error handling or retry logic.

5. **Use cancellation tokens** - Pass `CancellationToken` to allow graceful cancellation of long-running operations.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
var response = await collection.Batch.InsertMany(objects, ct: cts.Token);
```

6. **Dispose resources** - When using the low-level API, always use `await using` to ensure proper cleanup.
