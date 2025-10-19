
# Backup API Usage Guide

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
Dispose operations to avoid leaks:
```csharp
using (BackupOperation operation = await client.Backups.Create(...))
{
    var backup = await operation.WaitForCompletion();
}
```

## Status Checking
```csharp
if (operation.IsCompleted) { }
if (operation.IsSuccessful) { }
if (operation.IsCanceled) { }
var current = operation.Current;
Console.WriteLine($"ID: {current.Id}, Status: {current.Status}");
```
