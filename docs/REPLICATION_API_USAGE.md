# Replication API Usage Guide

This guide covers the Weaviate C# client's cluster replication functionality. It provides examples and best practices for managing shard replica movements across cluster nodes.

## Table of Contents

- [Overview](#overview)
- [Replication Types](#replication-types)
- [Starting Replication Operations](#starting-replication-operations)
- [Monitoring Operations](#monitoring-operations)
- [Managing Operations](#managing-operations)
- [Querying and Filtering](#querying-and-filtering)
- [Advanced Usage](#advanced-usage)

## Overview

The Replication API allows you to move or copy shard replicas between nodes in your Weaviate cluster. This is useful for load balancing, node maintenance, or scaling operations.

Minimum server version: **1.32.0** (earlier versions do not expose replication endpoints).

Key features include:

- **Type-safe operations**: `ReplicationOperationTracker` for async operations with progress tracking
- **Background polling**: Automatic status updates without manual intervention
- **Flexible patterns**: Async tracking, sync blocking, or fire-and-forget
- **Operation management**: Query, cancel, and delete operations
- **Filtering**: Find operations by collection, shard, or target node
- **Configurable timeouts**: Global defaults with per-call overrides

## Replication Types

Weaviate supports two types of replication operations:

### COPY (Default)

Creates a new replica on the target node while keeping the source replica intact. Use this for:

- Adding redundancy
- Load balancing without reducing capacity
- Creating temporary replicas for testing

```csharp
var request = new ReplicateRequest(
    Collection: "MyCollection",
    Shard: "shard-abc123",
    SourceNode: "node-1",
    TargetNode: "node-2",
    Type: ReplicationType.Copy  // Default
);
```

### MOVE

Creates a new replica on the target node and removes the source replica after successful transfer. Use this for:

- Migrating data off a node before decommissioning
- Rebalancing replicas across the cluster
- Node maintenance operations

```csharp
var request = new ReplicateRequest(
    Collection: "MyCollection",
    Shard: "shard-abc123",
    SourceNode: "node-1",
    TargetNode: "node-2",
    Type: ReplicationType.Move
);
```

## Starting Replication Operations

### Async Pattern with Tracking

Start a replication operation and get a tracker for monitoring progress:

```csharp
var operation = await client.Cluster.Replicate(
    new ReplicateRequest(
        Collection: "Articles",
        Shard: "shard-xyz789",
        SourceNode: "node-1",
        TargetNode: "node-2",
        Type: ReplicationType.Copy
    )
);

Console.WriteLine($"Operation ID: {operation.Current.Id}");
Console.WriteLine($"Status: {operation.Current.Status.State}");
```

### Synchronous Pattern (Wait for Completion)

Block until the replication completes:

```csharp
var result = await client.Cluster.ReplicateSync(
    new ReplicateRequest(
        Collection: "Articles",
        Shard: "shard-xyz789",
        SourceNode: "node-1",
        TargetNode: "node-2"
    ),
    timeout: TimeSpan.FromMinutes(10)
);

Console.WriteLine($"Final Status: {result.Status.State}");
Console.WriteLine($"Successful: {result.IsSuccessful}");
```

## Monitoring Operations

The `ReplicationOperationTracker` provides real-time status updates through background polling:

### Track Progress

```csharp
var operation = await client.Cluster.Replicate(request);

while (!operation.IsCompleted)
{
    var current = operation.Current;
    Console.WriteLine($"State: {current.Status.State}");
    
    if (current.WhenStarted.HasValue)
    {
        var elapsed = DateTimeOffset.UtcNow - current.WhenStarted.Value;
        Console.WriteLine($"Running for: {elapsed.TotalSeconds:F1}s");
    }
    
    await Task.Delay(1000);
}

if (operation.IsSuccessful)
{
    Console.WriteLine("Replication completed successfully!");
}
else if (operation.IsCancelled)
{
    Console.WriteLine("Replication was cancelled.");
}
```

### Cancel an Operation

There are two ways to cancel a replication operation:

#### Fire and Forget Cancel

```csharp
var operation = await client.Cluster.Replicate(request);

// Request cancellation without waiting
await operation.Cancel();

// Later, check if cancelled
await operation.RefreshStatus();
Console.WriteLine($"Cancelled: {operation.IsCancelled}");
```

#### Synchronous Cancel (Cancel and Wait)

Cancel the operation and wait for it to reach a terminal state:

```csharp
var operation = await client.Cluster.Replicate(request);

// Cancel and wait for completion (default 10s timeout)
var result = await operation.CancelSync();

Console.WriteLine($"Final State: {result.Status.State}");
Console.WriteLine($"Cancelled: {result.IsCancelled}");
```

With custom timeout:

```csharp
var result = await operation.CancelSync(
    timeout: TimeSpan.FromSeconds(30),
    cancellationToken: cancellationToken
);
```

### Replication States

Operations progress through these states:

- **REGISTERED**: Operation created, not yet started
- **HYDRATING**: Copying data to target node
- **FINALIZING**: Completing the replication
- **DEHYDRATING**: Removing source replica (MOVE only)
- **READY**: Successfully completed
- **CANCELLED**: Operation was cancelled

## Managing Operations

### Get Operation Details

Retrieve a specific operation by ID:

```csharp
var operation = await client.Cluster.Replications.Get(operationId);

if (operation != null)
{
    Console.WriteLine($"Collection: {operation.Collection}");
    Console.WriteLine($"Shard: {operation.Shard}");
    Console.WriteLine($"Type: {operation.Type}");
    Console.WriteLine($"Status: {operation.Status.State}");
}
```

### Get Operation with History

Include the full status transition history:

```csharp
var operation = await client.Cluster.Replications.Get(
    operationId,
    includeHistory: true
);

if (operation?.StatusHistory != null)
{
    Console.WriteLine("Status History:");
    foreach (var status in operation.StatusHistory)
    {
        Console.WriteLine($"  - {status.State} at {status.WhenStarted}");
        if (status.Errors?.Any() == true)
        {
            foreach (var error in status.Errors)
            {
                Console.WriteLine($"    Error: {error.Message}");
            }
        }
    }
}
```

### Delete an Operation

Remove an operation from tracking (operation must be completed or cancelled first):

```csharp
// Using the tracker's CancelSync for clean cancellation + wait
var operation = await client.Cluster.Replicate(request);
await operation.CancelSync();

// Then delete via the tracker or directly
await client.Cluster.Replications.Delete(operation.Current.Id);

// Or if you have just the ID
await client.Cluster.Replications.Cancel(operationId);
await client.Cluster.Replications.Delete(operationId);

// Verify deletion
var deleted = await client.Cluster.Replications.Get(operationId);
Console.WriteLine($"Deleted: {deleted == null}");
```

### Delete All Operations

Clean up all tracked operations:

```csharp
await client.Cluster.Replications.DeleteAll();

// Verify
var operations = await client.Cluster.Replications.ListAll();
Console.WriteLine($"Operations remaining: {operations.Count()}");
```

## Querying and Filtering

### List All Operations

Get all replication operations with full history:

```csharp
var allOperations = await client.Cluster.Replications.ListAll();

foreach (var op in allOperations)
{
    Console.WriteLine($"{op.Id}: {op.Collection}/{op.Shard} - {op.Status.State}");
}
```

### Filter by Collection

Find all replication operations for a specific collection:

```csharp
var operations = await client.Cluster.Replications.List(
    collection: "Articles"
);

Console.WriteLine($"Found {operations.Count()} operations for Articles collection");
```

### Filter by Shard

Find operations for a specific shard:

```csharp
var operations = await client.Cluster.Replications.List(
    collection: "Articles",
    shard: "shard-xyz789"
);
```

### Filter by Target Node

Find all operations targeting a specific node:

```csharp
var operations = await client.Cluster.Replications.List(
    targetNode: "node-3"
);

Console.WriteLine($"Operations targeting node-3: {operations.Count()}");
```

### Combined Filters

Use multiple filters together:

```csharp
var operations = await client.Cluster.Replications.List(
    collection: "Articles",
    shard: "shard-xyz789",
    targetNode: "node-2",
    includeHistory: true
);
```

## Advanced Usage

### Configurable Timeouts

Configure global defaults for polling intervals and timeouts:

```csharp
// Adjust polling frequency (default: 500ms)
ReplicationClientConfig.Default.PollInterval = TimeSpan.FromMilliseconds(250);

// Adjust default timeout for WaitForCompletion (default: 10 minutes)
ReplicationClientConfig.Default.Timeout = TimeSpan.FromMinutes(15);
```

Per-operation timeouts:

```csharp
// Wait for completion with custom timeout
var result = await operation.WaitForCompletion(
    timeout: TimeSpan.FromMinutes(5),
    cancellationToken: cancellationToken
);

// CancelSync also accepts custom timeout (default: 10s)
var result = await operation.CancelSync(
    timeout: TimeSpan.FromSeconds(30)
);
```

### Finding Shards for Replication

Get cluster nodes and their shards to determine replication targets:

```csharp
var nodes = await client.Cluster.Nodes.NodesVerbose(collection: "Articles");

foreach (var node in nodes)
{
    Console.WriteLine($"Node: {node.Name}");
    foreach (var shard in node.Shards ?? Array.Empty<Shard>())
    {
        Console.WriteLine($"  Shard: {shard.Name}");
        Console.WriteLine($"    Objects: {shard.ObjectCount}");
        Console.WriteLine($"    Status: {shard.VectorIndexingStatus}");
    }
}
```

### Batch Operations

Replicate multiple shards sequentially:

```csharp
var shardIds = new[] { "shard-1", "shard-2", "shard-3" };

foreach (var shardId in shardIds)
{
    var operation = await client.Cluster.Replicate(
        new ReplicateRequest(
            Collection: "Articles",
            Shard: shardId,
            SourceNode: "node-1",
            TargetNode: "node-2"
        )
    );
    
    await operation.WaitForCompletion(timeout: TimeSpan.FromMinutes(5));
    Console.WriteLine($"Completed replication of {shardId}");
}
```

### Error Handling

Handle replication errors gracefully:

```csharp
try
{
    var result = await client.Cluster.ReplicateSync(request);
    
    if (result.Status.Errors?.Any() == true)
    {
        Console.WriteLine("Replication completed with errors:");
        foreach (var error in result.Status.Errors)
        {
            Console.WriteLine($"  - {error.Message} at {error.WhenErrored}");
        }
    }
}
catch (TimeoutException)
{
    Console.WriteLine("Replication timed out");
}
catch (Exception ex)
{
    Console.WriteLine($"Replication failed: {ex.Message}");
}
```

### Resource Management

The `ReplicationOperationTracker` implements `IDisposable` and `IAsyncDisposable`:

```csharp
// Automatic cleanup when operation completes
await using var operation = await client.Cluster.Replicate(request);
await operation.WaitForCompletion();
// Tracker resources are automatically disposed

// Or manual disposal
var operation = await client.Cluster.Replicate(request);
try
{
    await operation.WaitForCompletion();
}
finally
{
    await operation.DisposeAsync();
}
```

## Best Practices

1. **Monitor cluster health**: Check node status before initiating replications
2. **Use COPY for redundancy**: Prefer COPY operations to maintain cluster capacity
3. **Use MOVE for migrations**: Use MOVE when decommissioning nodes
4. **Clean up operations**: Delete completed operations to keep tracking clean
5. **Handle timeouts**: Set appropriate timeouts based on shard size
6. **Check for errors**: Always inspect the `Status.Errors` collection
7. **Sequential operations**: Don't start multiple replications of the same shard simultaneously
8. **Cancel gracefully**: Use `CancelSync()` instead of `Cancel()` + manual waiting when you need to ensure cancellation completes before deletion
9. **Background refresh**: The tracker automatically polls for status updates every 500ms - no manual refresh needed for most cases
10. **External cancellation**: If an operation is cancelled via REST API by another thread/process, the tracker will automatically detect it during the next background refresh
