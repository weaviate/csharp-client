# Error Handling Guide

This guide describes the exception hierarchy and error handling patterns in the Weaviate C# client.

## Table of Contents

- [Exception Hierarchy](#exception-hierarchy)
- [Common Exceptions](#common-exceptions)
  - [WeaviateException](#weaviateexception)
  - [WeaviateClientException](#weaviateclientexception)
  - [WeaviateServerException](#weaviateserverexception)
  - [WeaviateUnexpectedStatusCodeException](#weaviateunexpectedstatuscodeexception)
  - [WeaviateNotFoundException](#weaviatenotfoundexception)
  - [WeaviateConflictException](#weaviateconflictexception)
  - [WeaviateFeatureNotSupportedException](#weaviatefeaturenotsupportedexception)
  - [WeaviateBackupConflictException](#weaviatebackupconflictexception)
- [Protocol-Specific Exceptions](#protocol-specific-exceptions)
  - [REST Exceptions](#rest-exceptions)
  - [gRPC Exceptions](#grpc-exceptions)
- [Error Handling Patterns](#error-handling-patterns)
  - [Standard Error Handling](#standard-error-handling)
  - [Specific Status Codes](#specific-status-codes)
  - [Retry Logic](#retry-logic)
  - [Version Compatibility](#version-compatibility)
- [API-Specific Behaviors](#api-specific-behaviors)
  - [Idempotent Operations](#idempotent-operations)
  - [Lenient Validation](#lenient-validation)
- [Best Practices](#best-practices)

## Exception Hierarchy

```plaintext
Exception (System)
└── WeaviateException
    ├── WeaviateClientException
    │   ├── WeaviateRestClientException
    │   └── (other client-side errors)
    └── WeaviateServerException
        ├── WeaviateRestServerException
        ├── WeaviateGrpcServerException
        ├── WeaviateUnexpectedStatusCodeException
        ├── WeaviateNotFoundException
        ├── WeaviateConflictException
        ├── WeaviateFeatureNotSupportedException
        └── WeaviateBackupConflictException
```

## Common Exceptions

### WeaviateException

**Base class** for all Weaviate-related exceptions.

```csharp
public class WeaviateException : Exception
```

**When thrown:** Never directly; serves as base for all other Weaviate exceptions.

**How to handle:** Catch this to handle any Weaviate-related error generically.

```csharp
try
{
    await client.Collections.Get("MyCollection");
}
catch (WeaviateException ex)
{
    Console.WriteLine($"Weaviate error: {ex.Message}");
}
```

### WeaviateClientException

**Client-side errors** such as invalid configuration, network issues, or serialization problems.

```csharp
public class WeaviateClientException : WeaviateException
```

**When thrown:**

- Invalid client configuration
- Network connectivity issues
- Request serialization failures
- Client-side validation errors

**How to handle:** These typically indicate issues with your application code or environment.

```csharp
try
{
    var client = WeaviateClientBuilder.Local(hostname: "invalid-host").Build();
    await client.IsReady();
}
catch (WeaviateClientException ex)
{
    Console.WriteLine($"Client error: {ex.Message}");
    // Check configuration, network connectivity, etc.
}
```

### WeaviateServerException

**Server-side errors** returned by the Weaviate instance.

```csharp
public class WeaviateServerException : WeaviateException
```

**When thrown:**

- Server returns an error response
- Server is unavailable or misconfigured
- Server-side validation failures
- Resource constraints or internal errors

**How to handle:** These indicate issues with the Weaviate server or your request.

```csharp
try
{
    await collection.Data.Insert(invalidObject);
}
catch (WeaviateServerException ex)
{
    Console.WriteLine($"Server error: {ex.Message}");
    // Check request validity, server logs, etc.
}
```

### WeaviateUnexpectedStatusCodeException

**Unexpected HTTP status code** returned by the REST API.

```csharp
internal class WeaviateUnexpectedStatusCodeException : WeaviateServerException
{
    public HttpStatusCode StatusCode { get; }
    public ISet<HttpStatusCode> ExpectedStatusCodes { get; }
}
```

**When thrown:** REST endpoint returns a status code not in the expected set for that operation.

**Properties:**

- `StatusCode`: The actual HTTP status code received
- `ExpectedStatusCodes`: Set of status codes the client expected
- `Message`: Response body content (often contains server error details)

**Example:**

```csharp
try
{
    await client.Roles.Create("existing-role", permissions);
}
catch (WeaviateUnexpectedStatusCodeException ex)
{
    Console.WriteLine($"Unexpected status: {ex.StatusCode}");
    Console.WriteLine($"Expected: {string.Join(", ", ex.ExpectedStatusCodes)}");
    Console.WriteLine($"Server message: {ex.Message}");
}
```

**Note:** This is an **internal** exception type. Higher-level methods often catch this and throw more specific exceptions (like `WeaviateConflictException` or `WeaviateNotFoundException`).

### WeaviateNotFoundException

**Resource not found** (HTTP 404 or gRPC NotFound).

```csharp
public class WeaviateNotFoundException : WeaviateServerException
{
    public ResourceType? ResourceType { get; }
}
```

**When thrown:**

- Collection doesn't exist
- Object ID not found
- Role or user doesn't exist
- Backup not found

**Example:**

```csharp
try
{
    var collection = await client.Collections.Get("NonExistentCollection");
}
catch (WeaviateNotFoundException ex)
{
    Console.WriteLine($"Not found: {ex.ResourceType}");
    // Create the resource or handle the missing case
}
```

### WeaviateConflictException

**Resource already exists** (HTTP 409 Conflict).

```csharp
public class WeaviateConflictException : WeaviateServerException
```

**When thrown:**

- Attempting to create a collection that already exists
- Creating a role with a duplicate name
- Creating an alias that conflicts with existing resources

**Example:**

```csharp
try
{
    await client.Roles.Create("admin", permissions);
}
catch (WeaviateConflictException ex)
{
    Console.WriteLine($"Conflict: {ex.Message}");
    // Resource already exists; decide whether to update or skip
}
```

### WeaviateFeatureNotSupportedException

**Feature not supported** by the connected Weaviate server version.

```csharp
public class WeaviateFeatureNotSupportedException : WeaviateServerException
```

**When thrown:**

- Using gRPC aggregate on servers without the aggregate RPC method
- Accessing features added in newer Weaviate versions
- Server built without optional modules

**Example:**

```csharp
try
{
    var results = await collection.Aggregate.OverAll();
}
catch (WeaviateFeatureNotSupportedException ex)
{
    Console.WriteLine($"Feature not supported: {ex.Message}");
    // Fall back to alternative approach or upgrade server
}
```

**Best practice:** Check server version before using version-specific features:

```csharp
if (client.WeaviateVersion >= new Version("1.25.0"))
{
    // Use newer feature
}
else
{
    // Use fallback or skip
}
```

### WeaviateBackupConflictException

**Backup or restore operation conflict** - another backup/restore is already in progress.

```csharp
public class WeaviateBackupConflictException : WeaviateServerException
```

**When thrown:**

- Starting a backup while another backup is running
- Starting a restore while another restore is running
- Starting either operation while any backup/restore is in progress

**Example:**

```csharp
try
{
    await client.Backup.Create("backup-1", BackupStorage.Filesystem);
}
catch (WeaviateBackupConflictException ex)
{
    Console.WriteLine("Another backup/restore is in progress");
    // Wait and retry, or cancel the existing operation
}
```

## Protocol-Specific Exceptions

### REST Exceptions

**WeaviateRestClientException** - Client-side REST errors

```csharp
public class WeaviateRestClientException : WeaviateClientException
```

Issues with REST request construction or HTTP communication.

**WeaviateRestServerException** - Server-side REST errors

```csharp
public class WeaviateRestServerException : WeaviateServerException
```

Errors returned by REST endpoints.

### gRPC Exceptions

**WeaviateGrpcServerException** - gRPC protocol errors

```csharp
public class WeaviateGrpcServerException : WeaviateServerException
```

Wraps `Grpc.Core.RpcException` with additional context.

## Error Handling Patterns

### Standard Error Handling

Most operations follow this pattern:

```csharp
try
{
    var result = await client.SomeOperation();
    // Process result
}
catch (WeaviateNotFoundException ex)
{
    // Handle missing resource
    Console.WriteLine("Resource not found");
}
catch (WeaviateConflictException ex)
{
    // Handle duplicate resource
    Console.WriteLine("Resource already exists");
}
catch (WeaviateServerException ex)
{
    // Handle other server errors
    Console.WriteLine($"Server error: {ex.Message}");
}
catch (WeaviateClientException ex)
{
    // Handle client errors
    Console.WriteLine($"Client error: {ex.Message}");
}
```

### Specific Status Codes

When you need to handle specific HTTP status codes:

```csharp
try
{
    await client.SomeOperation();
}
catch (WeaviateUnexpectedStatusCodeException ex)
{
    switch (ex.StatusCode)
    {
        case HttpStatusCode.BadRequest:
            Console.WriteLine("Invalid request");
            break;
        case HttpStatusCode.Unauthorized:
            Console.WriteLine("Authentication required");
            break;
        case HttpStatusCode.Forbidden:
            Console.WriteLine("Permission denied");
            break;
        default:
            Console.WriteLine($"Unexpected error: {ex.StatusCode}");
            break;
    }
}
```

### Retry Logic

Example with exponential backoff:

```csharp
async Task<T> RetryOperation<T>(Func<Task<T>> operation, int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            return await operation();
        }
        catch (WeaviateServerException ex) when (i < maxRetries - 1)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, i));
            Console.WriteLine($"Retry {i + 1}/{maxRetries} after {delay.TotalSeconds}s");
            await Task.Delay(delay);
        }
    }
    throw new Exception("Max retries exceeded");
}
```

### Version Compatibility

Check server capabilities before using features:

```csharp
try
{
    if (client.WeaviateVersion < new Version("1.28.0"))
    {
        Console.WriteLine("RBAC requires Weaviate >= 1.28.0");
        return;
    }
    
    await client.Users.Database.Create("newuser");
}
catch (WeaviateFeatureNotSupportedException ex)
{
    Console.WriteLine("Server doesn't support this feature");
}
```

## API-Specific Behaviors

### Idempotent Operations

Some operations are designed to succeed regardless of resource existence:

**Roles.Delete()** - Returns success even if the role doesn't exist:

```csharp
// No exception thrown even if role doesn't exist
await client.Roles.Delete("non-existent-role");  // ✅ Succeeds
```

This allows safe cleanup without existence checks:

```csharp
// Cleanup pattern - no try/catch needed
await client.Roles.Delete(tempRoleName);
await client.Users.Database.Delete(tempUserId);
```

### Lenient Validation

Some endpoints return semantic results instead of errors:

**Roles.HasPermission()** - Returns `false` for non-existent roles:

```csharp
// Returns false instead of throwing NotFoundException
var hasPermission = await client.Roles.HasPermission(
    "non-existent-role", 
    new PermissionInfo("read_roles")
);
// hasPermission == false (not an exception)
```

**When to expect exceptions vs. semantic results:**

| Operation | Non-existent resource behavior |
|-----------|-------------------------------|
| `Roles.Get()` | Throws `WeaviateNotFoundException` |
| `Roles.HasPermission()` | Returns `false` |
| `Roles.Delete()` | Succeeds (idempotent) |
| `Collections.Get()` | Throws `WeaviateNotFoundException` |
| `Collections.Exists()` | Returns `false` |

## Best Practices

### 1. Catch Specific Exceptions First

```csharp
// ✅ Good - specific to general
try
{
    await operation();
}
catch (WeaviateNotFoundException) { /* handle not found */ }
catch (WeaviateConflictException) { /* handle conflict */ }
catch (WeaviateServerException) { /* handle other server errors */ }

// ❌ Bad - general exception hides specific cases
try
{
    await operation();
}
catch (WeaviateException) { /* can't distinguish error types */ }
```

### 2. Check Server Version for New Features

```csharp
// ✅ Good - proactive check
if (client.WeaviateVersion >= new Version("1.25.0"))
{
    await UseNewFeature();
}

// ❌ Bad - reactive exception handling
try
{
    await UseNewFeature();
}
catch (WeaviateFeatureNotSupportedException) { }
```

### 3. Use Idempotent Operations for Cleanup

```csharp
// ✅ Good - cleanup always succeeds
try
{
    await CreateTestResources();
    await RunTests();
}
finally
{
    // These succeed even if resources don't exist
    await client.Roles.Delete(testRole);
    await client.Users.Database.Delete(testUser);
}

// ❌ Bad - cleanup requires extra error handling
try
{
    await client.Roles.Get(testRole);
    await client.Roles.Delete(testRole);
}
catch (WeaviateNotFoundException) { }
```

### 4. Provide Context in Error Messages

```csharp
try
{
    await client.Collections.Get(collectionName);
}
catch (WeaviateNotFoundException)
{
    throw new ApplicationException(
        $"Collection '{collectionName}' not found. " +
        "Ensure the collection was created before use."
    );
}
```

### 5. Don't Swallow Exceptions

```csharp
// ❌ Bad - silently swallows errors
try
{
    await CriticalOperation();
}
catch (WeaviateException) { }

// ✅ Good - log and/or rethrow
try
{
    await CriticalOperation();
}
catch (WeaviateException ex)
{
    _logger.LogError(ex, "Critical operation failed");
    throw;
}
```

### 6. Use Semantic Checks When Available

```csharp
// ✅ Good - uses semantic check
if (await client.Collections.Exists(name))
{
    await client.Collections.Delete(name);
}

// ❌ Bad - uses exceptions for flow control
try
{
    await client.Collections.Get(name);
    await client.Collections.Delete(name);
}
catch (WeaviateNotFoundException) { }
```

---

**Related Documentation:**

- [RBAC API Usage](RBAC_API_USAGE.md)
- [Backup API Usage](BACKUP_API_USAGE.md)
- [Nodes API Usage](NODES_API_USAGE.md)
