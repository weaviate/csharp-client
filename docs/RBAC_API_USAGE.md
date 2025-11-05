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
  - [Rotate User API Key](#rotate-user-api-key)
  - [Activate / Deactivate User](#activate--deactivate-user)
  - [Assign / Revoke Roles](#assign--revoke-roles)
  - [List Roles For User](#list-roles-for-user)
  - [Check Role For User](#check-role-for-user)
- [Roles](#roles)
  - [List Roles](#list-roles)
  - [Create Role With Permissions](#create-role-with-permissions)
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
client.Users;   // UsersClient
client.Roles;   // RolesClient
client.Groups;  // GroupsClient
```

- `UsersClient`: create/list/get users, rotate API keys, activate/deactivate.
- `RolesClient`: create/list/get/delete roles, manage permissions, assign/revoke roles to users or groups, query assignments.
- `GroupsClient`: list groups (e.g. OIDC) and retrieve role assignments.

## Version Requirements

| Feature | Minimum Version |
| ------- | --------------- |
| Own user info | 1.28.0 |
| User CRUD, role assignment helpers | 1.30.0 |
| Groups listing / roles | 1.32.0 |

Use `client.WeaviateVersion` or test helpers to gate functionality when targeting older servers.

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

### Own User Info

```csharp
var me = await client.Users.OwnInfo();
Console.WriteLine($"Me: {me.Username}, Active={me.Active}, Roles={string.Join(",", me.Roles.Select(r => r.Name))}");
```

### List Users

```csharp
var users = await client.Users.List();
foreach (var u in users)
    Console.WriteLine($"User: {u.UserId}, Active={u.Active}");
```

### Create & Get User

```csharp
var newUserId = $"user-{Random.Shared.Next(1, 10_000)}";
var apiKey = await client.Users.Create(newUserId);
var user = await client.Users.Get(newUserId);
Console.WriteLine($"Created {user.UserId}, Active={user.Active}");
```

### Rotate User API Key

```csharp
var rotatedKey = await client.Users.RotateApiKey(newUserId);
// Use rotatedKey to create a secondary client if needed
```

### Activate / Deactivate User

```csharp
await client.Users.Deactivate(newUserId);
await client.Users.Activate(newUserId);
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

### Create Role With Permissions

```csharp
var roleName = $"role-{Guid.NewGuid():N}";
await client.Roles.Delete(roleName); // idempotent cleanup
await client.Roles.Create(roleName, new[] { new PermissionInfo("read_roles") });
```

### Add / Remove Permissions

```csharp
await client.Roles.AddPermissions(roleName, new[] { new PermissionInfo("create_roles") });
await client.Roles.RemovePermissions(roleName, new[] { new PermissionInfo("create_roles") });
```

### Check Permission

```csharp
var has = await client.Roles.HasPermission(roleName, new PermissionInfo("read_roles"));
Console.WriteLine(has);
```

### User Assignments For Role

```csharp
var assignments = await client.Roles.UserAssignments(roleName);
foreach (var a in assignments) Console.WriteLine(a.UserId);
```

## Groups

Groups usually originate from identity providers (e.g. OIDC). They may be empty locally.

### List Groups

```csharp
var groups = await client.Groups.List("oidc");
```

### Roles For Group

```csharp
var groupId = "/example-group";
var groupRoles = await client.Groups.Roles(groupId, "oidc");
```

## Permission Action Strings

Permission actions are lowercase snake-case identifiers:

```text
read_roles
create_roles
update_roles
assign_roles
```

Use them via `new PermissionInfo("read_roles")`. If you add enums, prefer `[EnumMember(Value=...)]` + `ToEnumMemberString()`.

## Error Handling

Non-success HTTP codes throw `WeaviateUnexpectedStatusCodeException`:

```csharp
try
{
    await client.Roles.Create("existing-role", Array.Empty<PermissionInfo>());
}
catch (WeaviateUnexpectedStatusCodeException ex)
{
    Console.WriteLine($"Status={ex.StatusCode} Message={ex.Message}");
}
```

Invalid API keys can fail early during client construction (meta endpoint auth).

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
var adminKey = Environment.GetEnvironmentVariable("WEAVIATE_ADMIN_API_KEY") ?? "admin-key";
var client = WeaviateClientBuilder.Local(restPort: 8092, grpcPort: 50063)
    .WithCredentials(Auth.ApiKey(adminKey))
    .Build();

if (!await client.IsReady()) throw new Exception("Weaviate not ready");

// Create a role
var roleName = $"demo-role-{Guid.NewGuid():N}";
await client.Roles.Delete(roleName);
await client.Roles.Create(roleName, new[] { new PermissionInfo("read_roles") });

// Create a user
var userId = $"demo-user-{Guid.NewGuid():N}";
var userKey = await client.Users.Create(userId);

// Assign role
await client.Roles.AssignToUser(userId, "db", new[] { roleName });

// Verify
var has = await client.Roles.HasPermission(roleName, new PermissionInfo("read_roles"));
Console.WriteLine($"Role has read_roles? {has}");
var userRoles = await client.Roles.RolesForUser(userId, "db");
Console.WriteLine($"User roles: {string.Join(",", userRoles.Select(r => r.Name))}");

// Cleanup
await client.Roles.RevokeFromUser(userId, "db", new[] { roleName });
await client.Roles.Delete(roleName);
await client.Users.Delete(userId);
```

---
Generated automatically to accompany the RBAC client feature.
