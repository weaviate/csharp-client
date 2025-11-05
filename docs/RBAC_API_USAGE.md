# RBAC API Usage (Users, Roles, Groups)

This guide shows how to use the new Users, Roles, and Groups clients in the Weaviate C# SDK to manage database users and role-based access control (RBAC).

## Overview

The SDK exposes three high-level clients off `WeaviateClient`:

```csharp
var client = WeaviateClientBuilder.Local().Build();
client.Users;   // UsersClient
client.Roles;   // RolesClient
client.Groups;  // GroupsClient
```

- `UsersClient` manages database ("db") users: create, list, get, activate/deactivate, rotate API key.
- `RolesClient` manages roles and permissions, assigns/revokes roles to users or groups, queries assignments.
- `GroupsClient` lists groups and fetches roles assigned to a group.

## Users

### Create a user and capture its API key

```csharp
string apiKey = await client.Users.Create("alice");
```

Optional experimental parameters:

```csharp
string apiKey = await client.Users.Create("alice", import: false, createTime: DateTimeOffset.UtcNow);
```

### List users

```csharp
var users = await client.Users.List(includeLastUsedTime: true);
foreach (var u in users)
{
    Console.WriteLine($"User: {u.UserId}, Active: {u.Active}, Roles: {string.Join(",", u.Roles)}");
}
```

### Get a specific user

```csharp
var user = await client.Users.Get("alice", includeLastUsedTime: true);
```

### Rotate user API key

```csharp
var newKey = await client.Users.RotateApiKey("alice");
```

### Activate / Deactivate

```csharp
await client.Users.Deactivate("alice", revokeKey: true); // returns bool
await client.Users.Activate("alice");
```

### Delete user

```csharp
await client.Users.Delete("alice");
```

## Roles

A role is a name plus a set of permissions. Permission actions map to Weaviate RBAC operations (e.g. `read_roles`, `create_users`).

### Create a role

```csharp
var created = await client.Roles.Create(
    name: "data_reader",
    permissions: new [] { new PermissionInfo("read_data"), new PermissionInfo("read_collections") }
);
```

### List roles

```csharp
var roles = await client.Roles.List();
```

### Get / Delete

```csharp
var role = await client.Roles.Get("data_reader");
await client.Roles.Delete("data_reader");
```

### Add / Remove permissions

```csharp
await client.Roles.AddPermissions("data_reader", new [] { new PermissionInfo("read_nodes") });
await client.Roles.RemovePermissions("data_reader", new [] { new PermissionInfo("read_nodes") });
```

### Check permission

```csharp
bool? has = await client.Roles.HasPermission("data_reader", new PermissionInfo("read_data"));
if (has == true) { /* role allows action */ }
```

### Assign / Revoke roles on a user

```csharp
await client.Roles.AssignToUser("alice", userType: "db", roles: new [] { "data_reader" });
await client.Roles.RevokeFromUser("alice", userType: "db", roles: new [] { "data_reader" });
```

### Query roles for user

```csharp
var userRoles = await client.Roles.RolesForUser("alice", userType: "db", includeFullRoles: true);
```

### Assign / Revoke roles on a group

```csharp
await client.Roles.AssignToGroup("engineering", groupType: "oidc", roles: new [] { "data_reader" });
await client.Roles.RevokeFromGroup("engineering", groupType: "oidc", roles: new [] { "data_reader" });
```

### Query roles for group

```csharp
var rolesForGroup = await client.Roles.RolesForGroup("engineering", groupType: "oidc", includeFullRoles: true);
```

### List assignments

```csharp
var userAssignments = await client.Roles.UserAssignments("data_reader");
var groupAssignments = await client.Roles.GroupAssignments("data_reader");
```

## Groups

Groups are identified by name and type. Currently supported group type: `oidc`.

### List groups of a type

```csharp
var oidcGroups = await client.Groups.List("oidc");
```

### Roles for a group

```csharp
var rolesForGroup = await client.Groups.Roles("engineering", "oidc", includeFullRoles: true);
```

## Models Summary

| Type | Description |
|------|-------------|
| `DatabaseUser` | UserId, Active, DbUserType, timestamps, partial API key, roles |
| `RoleInfo` | Role name + permissions list |
| `PermissionInfo` | Single action string (wire format) |
| `UserRoleAssignment` | userId + userType for a role |
| `GroupRoleAssignment` | groupId + groupType for a role |

## Error Handling

All methods throw `WeaviateRestClientException` or more specific exceptions if unexpected status codes occur. Null return values indicate not-found scenarios for some GET operations (e.g. `Get` returning `null`).

## Notes & Best Practices

- Use canonical action strings exactly as defined by the server (`read_data`, `create_users`, etc.).
- Keep role names stable; renaming requires delete + recreate.
- Avoid storing raw API keys; only use them for client auth configuration when provisioning secondary clients.
- For large assignment workflows, batch by role to reduce round trips.

## Quick Setup Snippet

```csharp
var client = WeaviateClientBuilder.Local().Build();
await client.Users.Create("alice");
await client.Roles.Create("data_reader", new [] { new PermissionInfo("read_data") });
await client.Roles.AssignToUser("alice", "db", new [] { "data_reader" });
var rolesForAlice = await client.Roles.RolesForUser("alice", "db", includeFullRoles: true);
```

## Future Extensions

- Expand `PermissionInfo` to include resource scoping (collections, aliases, etc.).
- Provide higher-level convenience methods for common role templates.
- Add integration tests that spin up a container with RBAC enabled.

---
Generated automatically to accompany the RBAC client feature.
