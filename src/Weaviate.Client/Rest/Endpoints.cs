namespace Weaviate.Client.Rest;

internal static partial class WeaviateEndpoints
{
    internal static string Alias(string aliasName) => $"aliases/{aliasName}";

    internal static string Aliases(string? collectionName = null)
    {
        var path = $"aliases";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (collectionName is not null)
        {
            query["class"] = collectionName;
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string Collection() => $"schema";

    internal static string CollectionObject(string collectionName, Guid id, string? tenant = null)
    {
        var path = $"objects/{collectionName}/{id}";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (tenant is not null)
        {
            query["tenant"] = tenant;
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string CollectionProperties(string className) =>
        $"schema/{className}/properties";

    internal static string CollectionShard(string className, string shardName) =>
        $"schema/{className}/shards/{shardName}";

    internal static string CollectionShards(string className) => $"schema/{className}/shards";

    internal static string CollectionTenants(string className) => $"schema/{className}/tenants";

    internal static string CollectionTenant(string className, string tenantName) =>
        $"schema/{className}/tenants/{tenantName}";

    internal static string Collection(string collectionName) => $"schema/{collectionName}";

    internal static string Meta() => $"meta";

    internal static string Nodes(string? collection, string verbosity)
    {
        var path = $"nodes";
        if (collection is not null)
        {
            path += $"/{collection}";
        }
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        query["output"] = verbosity.ToLowerInvariant();
        path += $"?{query}";
        return path;
    }

    internal static string Objects() => $"objects";

    internal static string? Reference(
        string collectionName,
        Guid from,
        string fromProperty,
        string? tenant = null
    )
    {
        var path = $"objects/{collectionName}/{from}/references/{fromProperty}";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (tenant is not null)
        {
            query["tenant"] = tenant;
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string? ReferencesAdd(ConsistencyLevels? consistencyLevel = null)
    {
        var path = $"batch/references";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (consistencyLevel is not null)
        {
            query["consistency_level"] = consistencyLevel.Value.ToString().ToLower();
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    // Well-known endpoints
    internal static string WellKnownLive() => ".well-known/live";

    internal static string WellKnownReady() => ".well-known/ready";

    // Backups endpoints
    internal static string Backups(string backend) => $"backups/{backend}";

    internal static string Backup(string backend, string id)
    {
        return $"backups/{backend}/{id}";
    }

    internal static string BackupRestore(string backend, string id) =>
        $"backups/{backend}/{id}/restore";

    internal static string BackupStatus(
        string backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var ep = Backup(backend, id);
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrWhiteSpace(bucket))
        {
            query["bucket"] = bucket;
        }
        if (!string.IsNullOrWhiteSpace(path))
        {
            query["path"] = path;
        }
        if (query.Count > 0)
        {
            ep += $"?{query}";
        }
        return ep;
    }

    internal static string BackupRestoreStatus(
        string backend,
        string id,
        string? bucket = null,
        string? path = null
    )
    {
        var ep = BackupRestore(backend, id);
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (!string.IsNullOrWhiteSpace(bucket))
        {
            query["bucket"] = bucket;
        }
        if (!string.IsNullOrWhiteSpace(path))
        {
            query["path"] = path;
        }
        if (query.Count > 0)
        {
            ep += $"?{query}";
        }
        return ep;
    }

    // Users endpoints
    internal static string UsersOwnInfo() => "users/own-info";

    internal static string UsersDb(bool? includeLastUsedTime = null)
    {
        var path = "users/db";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (includeLastUsedTime == true)
        {
            query["includeLastUsedTime"] = "true";
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string UserDb(string userId, bool? includeLastUsedTime = null)
    {
        var path = $"users/db/{userId}";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (includeLastUsedTime == true)
        {
            query["includeLastUsedTime"] = "true";
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string UserDbRotateKey(string userId) => $"users/db/{userId}/rotate-key";

    internal static string UserDbActivate(string userId) => $"users/db/{userId}/activate";

    internal static string UserDbDeactivate(string userId) => $"users/db/{userId}/deactivate";

    // Authz Roles & Assignments endpoints
    internal static string Roles() => "authz/roles";

    internal static string Role(string id) => $"authz/roles/{id}";

    internal static string RoleAddPermissions(string id) => $"authz/roles/{id}/add-permissions";

    internal static string RoleRemovePermissions(string id) =>
        $"authz/roles/{id}/remove-permissions";

    internal static string RoleHasPermission(string id) => $"authz/roles/{id}/has-permission";

    internal static string RoleUsersDeprecated(string id) => $"authz/roles/{id}/users"; // deprecated

    internal static string RoleUserAssignments(string id) => $"authz/roles/{id}/user-assignments";

    internal static string RoleGroupAssignments(string id) => $"authz/roles/{id}/group-assignments";

    // Authz Users (role assignment)
    internal static string AuthzUserRoles(
        string userId,
        string userType,
        bool? includeFullRoles = null
    )
    {
        var path = $"authz/users/{userId}/roles/{userType}";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (includeFullRoles == true)
        {
            query["includeFullRoles"] = "true";
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string AuthzUserAssign(string userId) => $"authz/users/{userId}/assign";

    internal static string AuthzUserRevoke(string userId) => $"authz/users/{userId}/revoke";

    // Authz Groups
    internal static string AuthzGroupAssign(string groupId) => $"authz/groups/{groupId}/assign";

    internal static string AuthzGroupRevoke(string groupId) => $"authz/groups/{groupId}/revoke";

    internal static string AuthzGroupRoles(
        string groupId,
        string groupType,
        bool? includeFullRoles = null
    )
    {
        var path = $"authz/groups/{groupId}/roles/{groupType}";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);
        if (includeFullRoles == true)
        {
            query["includeFullRoles"] = "true";
        }
        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string AuthzGroups(string groupType) => $"authz/groups/{groupType}";

    // Replication endpoints
    internal static string Replicate() => "replication/replicate";

    internal static string ReplicationDetails(Guid id, bool includeHistory = false)
    {
        var path = $"replication/replicate/{id}";
        if (includeHistory)
        {
            path += "?includeHistory=true";
        }
        return path;
    }

    internal static string ReplicationList(
        string? collection = null,
        string? shard = null,
        string? targetNode = null,
        bool includeHistory = false
    )
    {
        var path = "replication/replicate/list";
        var query = System.Web.HttpUtility.ParseQueryString(string.Empty);

        if (collection is not null)
        {
            query["collection"] = collection;
        }
        if (shard is not null)
        {
            query["shard"] = shard;
        }
        if (targetNode is not null)
        {
            query["targetNode"] = targetNode;
        }
        if (includeHistory)
        {
            query["includeHistory"] = "true";
        }

        if (query.Count > 0)
        {
            path += $"?{query}";
        }
        return path;
    }

    internal static string ReplicationCancel(Guid id) => $"replication/replicate/{id}/cancel";

    internal static string ReplicationDelete(Guid id) => $"replication/replicate/{id}";
}
