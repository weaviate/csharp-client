# RBAC API Usage Guide

This guide shows how to use the Weaviate C# client's Role-Based Access Control (RBAC) APIs for managing database users, roles, permissions, assignments, and groups.

## Table of Contents

- [Overview](#overview)
- [Version Requirements](#version-requirements)
- [Client Initialization](#client-initialization)
- [Users](#users)
  - [Own User Info](#own-user-info)
  - [List Users](#list-users)
  - [Create & Get User](#create--get-user)
  - [Delete User](#delete-user)
  - [Rotate User API Key](#rotate-user-api-key)
  - [Activate / Deactivate User](#activate--deactivate-user)
  - [Assign / Revoke Roles](#assign--revoke-roles)
  - [List Roles For User](#list-roles-for-user)
  - [Check Role For User](#check-role-for-user-indirect)
- [Roles](#roles)
  - [List Roles](#list-roles)
  - [Get Role](#get-role)
  - [Create Role With Permissions](#create-role-with-permissions)
  - [Delete Role](#delete-role)
  - [Add / Remove Permissions](#add--remove-permissions)
  - [Check Permission](#check-permission)
  - [User Assignments For Role](#user-assignments-for-role)
- [Groups](#groups)
  - [List Groups](#list-groups)
  - [Roles For Group](#roles-for-group)
- [Permission Action Strings](#permission-action-strings)
- [Error Handling](#error-handling)
- [Readiness & Test Infrastructure](#readiness--test-infrastructure)
- [Best Practices](#best-practices)
- [Complete Example](#complete-example)

## Overview

The RBAC surface provides high-level async operations:

```csharp
client.Users;   // UsersClient - manages database users
client.Roles;   // RolesClient - manages roles, permissions, and assignments
client.Groups;  // GroupsClient - queries OIDC groups
```

- `UsersClient`: Provides specialized sub-clients for database and OIDC user management:
  - `Users.Database`: Create, list, get, delete database users; rotate API keys; activate/deactivate
  - `Users.Oidc`: List OIDC users and retrieve user details
- `RolesClient`: Create/list/get/delete roles, manage permissions, assign/revoke roles to users or groups, query assignments
- `GroupsClient`: Provides specialized sub-client for OIDC group management:
  - `Groups.Oidc`: List OIDC groups and retrieve role assignments

## Version Requirements

| Feature | Minimum Version |
| ------- | --------------- |
| RBAC APIs (users, roles, groups) | 1.28.0 |
| C# Client Integration Tests | 1.31.0 |

The C# client library targets Weaviate >= 1.31.0 for integration testing. Core RBAC features are available from Weaviate 1.28.0, but the client enforces 1.31.0 as the minimum supported version.

Use `client.WeaviateVersion` to check the server version at runtime if needed.

## Client Initialization

Example for an RBAC-enabled local deployment (REST 8092 / gRPC 50063):

```csharp
var adminKey = Environment.GetEnvironmentVariable("WEAVIATE_ADMIN_API_KEY") ?? "admin-key";
var client = WeaviateClientBuilder
    .Local(hostname: "localhost", restPort: 8092, grpcPort: 50063)
    .WithCredentials(Auth.ApiKey(adminKey))
    .Build();

if (!await client.IsReady()) throw new Exception("Weaviate not ready");
```

## Users

The `UsersClient` provides specialized sub-clients for managing database and OIDC users:

- `client.Users.Database`: Database user management (create, delete, activate, deactivate, rotate keys)
- `client.Users.Oidc`: OIDC user queries (list, get details)

### Own User Info

Get information about the currently authenticated user:

```csharp
var me = await client.Users.OwnInfo();
Console.WriteLine($"Me: {me.Username}, Active={me.Active}, Roles={string.Join(",", me.Roles.Select(r => r.Name))}");
```

### List Users

List all database users:

```csharp
var users = await client.Users.Database.List();
foreach (var u in users)
    Console.WriteLine($"User: {u.UserId}, Active={u.Active}");
```

List all OIDC users:

```csharp
var oidcUsers = await client.Users.Oidc.List();
foreach (var u in oidcUsers)
    Console.WriteLine($"OIDC User: {u.Username}");
```

### Create & Get User

Database users only (OIDC users are managed by the identity provider):

```csharp
var newUserId = $"user-{Random.Shared.Next(1, 10_000)}";
var apiKey = await client.Users.Database.Create(newUserId);
var user = await client.Users.Database.Get(newUserId);
Console.WriteLine($"Created {user.UserId}, Active={user.Active}");
```

### Delete User

```csharp
await client.Users.Database.Delete(newUserId);
```

### Rotate User API Key

Database users only:

```csharp
var rotatedKey = await client.Users.Database.RotateApiKey(newUserId);
// Use rotatedKey to create a secondary client if needed
```

### Activate / Deactivate User

Database users only:

```csharp
await client.Users.Database.Deactivate(newUserId);
await client.Users.Database.Activate(newUserId);
```

### Assign / Revoke Roles

```csharp
await client.Roles.AssignToUser(newUserId, scope: "db", roles: new[] { "viewer" });
await client.Roles.RevokeFromUser(newUserId, scope: "db", roles: new[] { "viewer" });
```

### List Roles For User

```csharp
var rolesForUser = await client.Roles.RolesForUser(newUserId, scope: "db");
foreach (var r in rolesForUser) Console.WriteLine(r.Name);
```

### Check Role For User (Indirect)

```csharp
var hasViewer = (await client.Roles.RolesForUser(newUserId, "db")).Any(r => r.Name == "viewer");
```

## Roles

### List Roles

```csharp
var roles = await client.Roles.List();
foreach (var r in roles) Console.WriteLine(r.Name);
```

### Get Role

```csharp
var role = await client.Roles.Get("viewer");
Console.WriteLine($"Role: {role.Name}, Permissions: {role.Permissions.Count}");
```

### Create Role With Permissions

Creates a new role with optional initial permissions. Returns the created role details:

```csharp
var roleName = $"role-{Guid.NewGuid():N}";
var createdRole = await client.Roles.Create(roleName, new[] { new PermissionInfo("read_roles") });
Console.WriteLine($"Created role: {createdRole.Name}");
```

### Delete Role

Idempotent—returns success even if the role doesn't exist:

```csharp
await client.Roles.Delete(roleName);
```

### Add / Remove Permissions

```csharp
await client.Roles.AddPermissions(roleName, new[] { new PermissionInfo("create_roles") });
await client.Roles.RemovePermissions(roleName, new[] { new PermissionInfo("create_roles") });
```

### Check Permission

Returns `true` if the role has the specified permission, `false` otherwise (including when the role doesn't exist):

```csharp
var has = await client.Roles.HasPermission(roleName, new PermissionInfo("read_roles"));
Console.WriteLine($"Has permission: {has}");
```

**Note:** This endpoint returns a boolean result with HTTP 200 status, even for non-existent roles. It does not throw exceptions for missing roles.

### User Assignments For Role

```csharp
var assignments = await client.Roles.UserAssignments(roleName);
foreach (var a in assignments) Console.WriteLine(a.UserId);
```

## Groups

The `GroupsClient` provides specialized sub-clients for managing groups by type. Currently, only OIDC groups are supported.

Groups originate from identity providers (e.g., OIDC) and cannot be created via the client—they're synchronized from the external identity provider.

### List Groups

```csharp
var groups = await client.Groups.Oidc.List();
foreach (var g in groups)
    Console.WriteLine($"Group: {g.Name}");
```

### Roles For Group

```csharp
var groupId = "/example-group";
var groupRoles = await client.Groups.Oidc.Roles(groupId);
foreach (var r in groupRoles)
    Console.WriteLine($"Role: {r.Name}");
```

## Permission Action Strings

Permission actions are lowercase snake-case identifiers, but the C# client provides strongly-typed permission classes for each resource. You should construct permissions using these types, not raw strings.

**Supported Permission Types:**

| Type                | Properties (actions)                | Example Usage |
|---------------------|-------------------------------------|---------------|
| `Permissions.Alias` | `Create`, `Read`, `Update`, `Delete`| `new Permissions.Alias { Read = true }` |
| `Permissions.Data`  | `Create`, `Read`, `Update`, `Delete`| `new Permissions.Data { Read = true }` |
| `Permissions.Backups` | `Manage`                           | `new Permissions.Backups { Manage = true }` |
| `Permissions.Cluster` | `Read`                             | `new Permissions.Cluster { Read = true }` |
| `Permissions.Nodes` | `Read`                              | `new Permissions.Nodes { Read = true }` |
| `Permissions.Roles` | `Create`, `Read`, `Update`, `Delete`| `new Permissions.Roles { Read = true }` |
| `Permissions.Users` | `Create`, `Read`, `Update`, `Delete`, `AssignAndRevoke` | `new Permissions.Users { Create = true }` |
| `Permissions.Tenants` | `Create`, `Read`, `Update`, `Delete` | `new Permissions.Tenants { Read = true }` |
| `Permissions.Groups` | `AssignAndRevoke`, `Read`           | `new Permissions.Groups { Read = true }` |
| `Permissions.Replicate` | `Create`, `Read`, `Update`, `Delete` | `new Permissions.Replicate { Read = true }` |
| `Permissions.Collections` | `Create`, `Read`, `Update`, `Delete` | `new Permissions.Collections { Read = true }` |

**Example:**

```csharp
// Grant read access to roles and collections
var permissions = new[] {
    new Permissions.Roles { Read = true },
    new Permissions.Collections { Read = true }
};
var createdRole = await client.Roles.Create(roleName, permissions);
```

**Wire Format Handling:**
PermissionScope objects are converted directly to DTOs (`Rest.Dto.Permission`) by the client

## Error Handling

RBAC operations follow standard Weaviate error handling patterns. For comprehensive information about exception types and error handling strategies, see the **[Error Handling Guide](ERRORS.md)**.

### Common Patterns in RBAC

**Idempotent Deletes:**

```csharp
await client.Roles.Delete("role-name");
await client.Roles.Delete("role-name");
// Succeeds even if role doesn't exist
```

**Lenient Permission Checks:**

```csharp
// Returns false instead of throwing for non-existent roles
var hasPermission = await client.Roles.HasPermission("unknown-role-name", permission);
```

**Conflict Handling:**

```csharp
try
{
    await client.Roles.Create("existing-role", permissions);
}
catch (WeaviateConflictException ex)
{
    Console.WriteLine($"Role already exists: {ex.Message}");
}
```

## Readiness & Test Infrastructure

Integration tests invoke `_weaviate.IsReady()` in a centralized `InitializeAsync` to fail fast if the server is down. Applications can apply the same readiness probe.

## Best Practices

- Clean up transient roles/users with `try/finally`.
- Keep permissions minimal (least privilege).
- Rotate keys on credential lifecycle events per security policy.
- Gate RBAC-only flows behind version checks when supporting older deployments.
- Avoid caching permissions aggressively—revalidate on security-sensitive paths.

## Complete Example

```csharp
using Weaviate.Client;
using Weaviate.Client.Models;

var adminKey = Environment.GetEnvironmentVariable("WEAVIATE_ADMIN_API_KEY") ?? "admin-key";
var client = WeaviateClientBuilder.Local(restPort: 8092, grpcPort: 50063)
    .WithCredentials(Auth.ApiKey(adminKey))
    .Build();

if (!await client.IsReady()) throw new Exception("Weaviate not ready");


// Create a role with permissions
var roleName = $"demo-role-{Guid.NewGuid():N}";
var role = await client.Roles.Create(roleName, new[]
{
  new Permissions.Roles { Read = true },
  new Permissions.Collections { Read = true }
});
Console.WriteLine($"Created role: {role.Name}");

// Create a database user
var userId = $"demo-user-{Guid.NewGuid():N}";
var userKey = await client.Users.Database.Create(userId);
Console.WriteLine($"Created user {userId} with API key");

// Assign role to user
await client.Roles.AssignToUser(userId, "db", new[] { roleName });
Console.WriteLine($"Assigned role {roleName} to user {userId}");

// Verify role assignment
var userRoles = await client.Roles.RolesForUser(userId, "db");
Console.WriteLine($"User roles: {string.Join(", ", userRoles.Select(r => r.Name))}");

// Check specific permission
var hasPermission = await client.Roles.HasPermission(roleName, new Permissions.Roles { Read = true });
Console.WriteLine($"Role has read_roles permission: {hasPermission}");

// List all role assignments for this user
var assignments = await client.Roles.UserAssignments(roleName);
Console.WriteLine($"Users with role {roleName}: {assignments.Count}");

// Cleanup
await client.Roles.RevokeFromUser(userId, "db", new[] { roleName });
await client.Roles.Delete(roleName);
await client.Users.Database.Delete(userId);
Console.WriteLine("Cleanup complete");
```

---
Generated automatically to accompany the RBAC client feature.
