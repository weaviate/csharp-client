# Backup API Usage Guide

This guide covers the Weaviate C# client's backup and restore functionality. It provides examples and best practices for using the modern, idiomatic backup API.

## Table of Contents

- [Overview](#overview)
- [Backend Configuration](#backend-configuration)
- [Creating Backups](#creating-backups)
- [Restoring Backups](#restoring-backups)
- [Monitoring Operations](#monitoring-operations)
- [Concurrency and Coordination](#concurrency-and-coordination)
- [Advanced Usage](#advanced-usage)

## Overview

The Backup API allows you to create and restore backups of your Weaviate collections. Backups can be stored on various backend storage providers, including the local filesystem, S3, GCS, and Azure Blob Storage.

Key features include:

- **Type-safe operations**: Separate `BackupCreateOperation` and `BackupRestoreOperation` types.
- **Type-safe backends**: Separate `FilesystemBackend` and `ObjectStorageBackend` types.
- **Automatic cleanup**: Resources are automatically disposed of when operations complete.
- **Flexible patterns**: Async tracking, sync blocking, or fire-and-forget.
- **Configurable timeouts**: Global defaults with per-call overrides.

## Backend Configuration

Backends are configured using the static factory methods on the `BackupBackend` class. Supported backends include:

### Filesystem Backend

```csharp
var backend = BackupBackend.Filesystem(path: "/backups");
Console.WriteLine(backend.Provider); // BackupStorageProvider.Filesystem
```

### Object Storage Backends

#### S3

```csharp
var backend = BackupBackend.S3(bucket: "my-backup-bucket", path: "backups/");
Console.WriteLine(backend.Provider); // BackupStorageProvider.S3
```

#### Google Cloud Storage

```csharp
var backend = BackupBackend.GCS(bucket: "my-gcs-bucket", path: "weaviate-backups");
Console.WriteLine(backend.Provider); // BackupStorageProvider.GCS
```

#### Azure Blob Storage

```csharp
var backend = BackupBackend.Azure(bucket: "my-container", path: "backups");
Console.WriteLine(backend.Provider); // BackupStorageProvider.Azure
```

## Creating Backups

To create a backup, use the `BackupClient.Create` method. This returns a `BackupCreateOperation` object that can be used to track the operation's status.

### Creating Backup Example

```csharp
var operation = await client.Backups.Create(new BackupCreateRequest(
    id: "my-backup-id",
    backend: BackupBackend.Filesystem(path: "/backups")
));

await operation.WaitForCompletion();
Console.WriteLine(operation.Current.Status); // BackupStatus.Success
```

## Restoring Backups

To restore a backup, use the `BackupClient.Restore` method. This returns a `BackupRestoreOperation` object.

### Restoring Backup Example

```csharp
var operation = await client.Backups.Restore(new BackupRestoreRequest(
    id: "my-backup-id",
    backend: BackupBackend.Filesystem(path: "/backups")
));

await operation.WaitForCompletion();
Console.WriteLine(operation.Current.Status); // BackupStatus.Success
```

## Monitoring Operations

Both `BackupCreateOperation` and `BackupRestoreOperation` support:

- **Status polling**: Use the `Current` property to get the latest status.
- **Cancellation**: Call the `Cancel` method to abort the operation.

### Monitoring Operation Example

```csharp
var operation = await client.Backups.Create(new BackupCreateRequest(
    id: "my-backup-id",
    backend: BackupBackend.Filesystem(path: "/backups")
));

while (!operation.IsCompleted)
{
    Console.WriteLine(operation.Current.Status);
    await Task.Delay(1000);
}
```

## Concurrency and Coordination

**Important:** Concurrent backup or restore operations are not allowed. Attempting to start a new operation while another is in progress will throw a `WeaviateBackupConflictException`.

To perform multiple operations, ensure each one is awaited to completion before starting the next.

### Sequential Operations Example

```csharp
// Create the first backup
var operation1 = await client.Backups.Create(new BackupCreateRequest(
    id: "backup-1",
    backend: BackupBackend.Filesystem(path: "/backups")
));
await operation1.WaitForCompletion();

// Create the second backup after the first completes
var operation2 = await client.Backups.Create(new BackupCreateRequest(
    id: "backup-2",
    backend: BackupBackend.Filesystem(path: "/backups")
));
await operation2.WaitForCompletion();
```

## Advanced Usage

### Configurable Timeouts

You can configure global defaults for polling intervals and timeouts using `BackupClient.Config`:

```csharp
BackupClient.Config = new BackupClientConfig
{
    PollInterval = TimeSpan.FromMilliseconds(500),
    Timeout = TimeSpan.FromMinutes(5)
};
```

### Selective Backup/Restore

Include or exclude specific collections:

```csharp
var operation = await client.Backups.Create(new BackupCreateRequest(
    id: "my-backup-id",
    backend: BackupBackend.Filesystem(path: "/backups"),
    Include = new[] { "Collection1", "Collection2" }
));
await operation.WaitForCompletion();
```
