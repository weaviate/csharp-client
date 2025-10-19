
# Backup API Usage Guide

This guide covers the modern, idiomatic backup and restore API with automatic resource management.

## Key Features

- **Automatic cleanup**: Resources are automatically disposed when operations complete
- **Type-safe operations**: Separate `BackupOperation` and `RestoreOperation` types
- **Flexible patterns**: Async tracking, sync blocking, or fire-and-forget
- **Background polling**: Status updates automatically in the background
- **Configurable timeouts**: Global defaults with per-call overrides

## Configuration

Set global polling and timeout configuration for all backup operations:

```csharp
BackupClient.Config = new BackupClientConfig
{
    PollInterval = TimeSpan.FromSeconds(2),
    Timeout = TimeSpan.FromMinutes(30)
};
```

## Creating a Backup

### Async (track status)
```csharp
BackupOperation operation = await client.Backups.Create(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup-id")
);

// Status auto-refreshes in background
Console.WriteLine(operation.Current.Status);

// Wait for completion
var backup = await operation.WaitForCompletion();
```


### Sync (block until complete)
```csharp
// Uses global timeout
var backup = await client.Backups.CreateSync(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup-id")
);
Assert.Equal(BackupStatus.Success, backup.Status);

// Or override timeout per call
var backupWithTimeout = await client.Backups.CreateSync(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup-id"),
    timeout: TimeSpan.FromMinutes(5)
);
```

## Restoring a Backup

### Async (track status)
```csharp
RestoreOperation operation = await client.Backups.Restore(
    BackupStorage.Filesystem,
    "my-backup-id",
    new BackupRestoreRequest(Include: new[] { "Article" })
);

// Status auto-refreshes in background
Console.WriteLine(operation.Current.Status);

// Wait for completion
var restore = await operation.WaitForCompletion();
```


### Sync (block until complete)
```csharp
// Uses global timeout
var restore = await client.Backups.RestoreSync(
    BackupStorage.Filesystem,
    "my-backup-id",
    new BackupRestoreRequest(Include: new[] { "Article" })
);
Assert.Equal(BackupStatus.Success, restore.Status);

// Or override timeout per call
var restoreWithTimeout = await client.Backups.RestoreSync(
    BackupStorage.Filesystem,
    "my-backup-id",
    new BackupRestoreRequest(Include: new[] { "Article" }),
    timeout: TimeSpan.FromMinutes(10)
);
```

## Canceling an Operation
```csharp
await operation.Cancel();
var result = await operation.WaitForCompletion();
Assert.Equal(BackupStatus.Canceled, result.Status);
```

## Custom Timeout
```csharp
BackupOperation operation = await client.Backups.Create(...);
try
{
    var result = await operation.WaitForCompletion(TimeSpan.FromMinutes(5));
}
catch (TimeoutException)
{
    Console.WriteLine("Backup did not finish in 5 minutes");
}
```

## Resource Management

### Automatic Cleanup (Recommended)
Operations **automatically dispose their resources** when they complete (Success, Failed, or Canceled). You don't need to explicitly dispose unless you want to cancel early:

```csharp
// Fire and forget - resources cleaned up automatically when complete
var operation = await client.Backups.Create(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup-id")
);
// Resources will be freed automatically when backup completes

// Or wait for completion without disposing
var backup = await operation.WaitForCompletion();
// Still automatically cleaned up
```

### Explicit Disposal (Optional)
You can still use traditional disposal patterns if preferred:

```csharp
// Synchronous disposal
using (BackupOperation operation = await client.Backups.Create(...))
{
    var backup = await operation.WaitForCompletion();
}

// Asynchronous disposal (recommended for async code)
await using (BackupOperation operation = await client.Backups.Create(...))
{
    var backup = await operation.WaitForCompletion();
}

// Or with declaration pattern
await using var operation = await client.Backups.Create(...);
var backup = await operation.WaitForCompletion();
```

### Early Cancellation
Explicit disposal is useful for early cancellation:

```csharp
await using var operation = await client.Backups.Create(...);

// Cancel if taking too long
if (!operation.IsCompleted)
{
    await operation.Cancel();
}
// Disposal ensures background polling stops immediately
```

## Status Checking
```csharp
if (operation.IsCompleted) { }
if (operation.IsSuccessful) { }
if (operation.IsCanceled) { }
var current = operation.Current;
Console.WriteLine($"ID: {current.Id}, Status: {current.Status}");
```

## Usage Patterns

### Pattern 1: Fire and Forget
Best for background backups where you don't need to wait:
```csharp
// Start backup and continue - resources auto-cleanup
var operation = await client.Backups.Create(
    BackupStorage.Filesystem,
    new BackupCreateRequest("nightly-backup")
);
// Operation will complete in background and clean up automatically
```

### Pattern 2: Simple Blocking
Best for scripts and simple workflows:
```csharp
// Block until complete (uses global timeout)
var backup = await client.Backups.CreateSync(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup")
);

if (backup.Status == BackupStatus.Success)
{
    Console.WriteLine("Backup completed successfully");
}
```

### Pattern 3: Progress Monitoring
Best for UI applications or detailed logging:
```csharp
await using var operation = await client.Backups.Create(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup")
);

// Monitor progress
while (!operation.IsCompleted)
{
    Console.WriteLine($"Status: {operation.Current.Status}");
    await Task.Delay(TimeSpan.FromSeconds(1));
}

var result = operation.Current;
Console.WriteLine($"Final: {result.Status}");
```

### Pattern 4: Timeout with Fallback
Best for robust production code:
```csharp
BackupOperation operation = await client.Backups.Create(
    BackupStorage.Filesystem,
    new BackupCreateRequest("my-backup")
);

try
{
    var backup = await operation.WaitForCompletion(TimeSpan.FromMinutes(5));
    Console.WriteLine("Backup succeeded");
}
catch (TimeoutException)
{
    // Backup still running - decide whether to cancel
    await operation.Cancel();
    Console.WriteLine("Backup canceled due to timeout");
}
// Auto-cleanup happens regardless
```

### Pattern 5: Multiple Operations
Best for batch operations:
```csharp
var backupIds = new[] { "backup-1", "backup-2", "backup-3" };

// Start all backups concurrently
var operations = await Task.WhenAll(
    backupIds.Select(id => 
        client.Backups.Create(
            BackupStorage.Filesystem,
            new BackupCreateRequest(id)
        )
    )
);

// Wait for all to complete
var results = await Task.WhenAll(
    operations.Select(op => op.WaitForCompletion())
);

// Check results
foreach (var result in results)
{
    Console.WriteLine($"{result.Id}: {result.Status}");
}
// All operations auto-cleaned up
```
