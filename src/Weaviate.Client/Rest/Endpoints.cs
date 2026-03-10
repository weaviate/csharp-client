namespace Weaviate.Client.Rest;

/// <summary>
/// The weaviate endpoints class
/// </summary>
internal static partial class WeaviateEndpoints
{
    /// <summary>
    /// Aliases the alias name
    /// </summary>
    /// <param name="aliasName">The alias name</param>
    /// <returns>The string</returns>
    internal static string Alias(string aliasName) => $"aliases/{aliasName}";

    /// <summary>
    /// Aliaseses the collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Collections
    /// </summary>
    /// <returns>The string</returns>
    internal static string Collection() => $"schema";

    /// <summary>
    /// Collections the object using the specified collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="id">The id</param>
    /// <param name="tenant">The tenant</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Collections the properties using the specified class name
    /// </summary>
    /// <param name="className">The class name</param>
    /// <returns>The string</returns>
    internal static string CollectionProperties(string className) =>
        $"schema/{className}/properties";

    /// <summary>
    /// Path for dropping a specific inverted index from a collection property.
    /// </summary>
    /// <param name="className">The class name</param>
    /// <param name="propertyName">The property name</param>
    /// <param name="indexName">The index name (filterable, searchable, rangeFilters)</param>
    /// <returns>The string</returns>
    internal static string CollectionPropertyIndex(
        string className,
        string propertyName,
        string indexName
    ) => $"schema/{className}/properties/{propertyName}/index/{indexName}";

    /// <summary>
    /// Collections the shard using the specified class name
    /// </summary>
    /// <param name="className">The class name</param>
    /// <param name="shardName">The shard name</param>
    /// <returns>The string</returns>
    internal static string CollectionShard(string className, string shardName) =>
        $"schema/{className}/shards/{shardName}";

    /// <summary>
    /// Collections the shards using the specified class name
    /// </summary>
    /// <param name="className">The class name</param>
    /// <returns>The string</returns>
    internal static string CollectionShards(string className) => $"schema/{className}/shards";

    /// <summary>
    /// Collections the tenants using the specified class name
    /// </summary>
    /// <param name="className">The class name</param>
    /// <returns>The string</returns>
    internal static string CollectionTenants(string className) => $"schema/{className}/tenants";

    /// <summary>
    /// Collections the tenant using the specified class name
    /// </summary>
    /// <param name="className">The class name</param>
    /// <param name="tenantName">The tenant name</param>
    /// <returns>The string</returns>
    internal static string CollectionTenant(string className, string tenantName) =>
        $"schema/{className}/tenants/{tenantName}";

    /// <summary>
    /// Collections the collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <returns>The string</returns>
    internal static string Collection(string collectionName) => $"schema/{collectionName}";

    /// <summary>
    /// Metas
    /// </summary>
    /// <returns>The string</returns>
    internal static string Meta() => $"meta";

    /// <summary>
    /// Nodeses the collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="verbosity">The verbosity</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Objectses
    /// </summary>
    /// <returns>The string</returns>
    internal static string Objects() => $"objects";

    /// <summary>
    /// References the collection name
    /// </summary>
    /// <param name="collectionName">The collection name</param>
    /// <param name="from">The from</param>
    /// <param name="fromProperty">The from property</param>
    /// <param name="tenant">The tenant</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Referenceses the add using the specified consistency level
    /// </summary>
    /// <param name="consistencyLevel">The consistency level</param>
    /// <returns>The path</returns>
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
    /// <summary>
    /// Wells the known live
    /// </summary>
    /// <returns>The string</returns>
    internal static string WellKnownLive() => ".well-known/live";

    /// <summary>
    /// Wells the known ready
    /// </summary>
    /// <returns>The string</returns>
    internal static string WellKnownReady() => ".well-known/ready";

    // Backups endpoints
    /// <summary>
    /// Backupses the backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <returns>The string</returns>
    internal static string Backups(string backend) => $"backups/{backend}";

    /// <summary>
    /// Backups the backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string Backup(string backend, string id)
    {
        return $"backups/{backend}/{id}";
    }

    /// <summary>
    /// Backups the restore using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string BackupRestore(string backend, string id) =>
        $"backups/{backend}/{id}/restore";

    /// <summary>
    /// Backups the status using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    /// <returns>The ep</returns>
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

    /// <summary>
    /// Backups the restore status using the specified backend
    /// </summary>
    /// <param name="backend">The backend</param>
    /// <param name="id">The id</param>
    /// <param name="bucket">The bucket</param>
    /// <param name="path">The path</param>
    /// <returns>The ep</returns>
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
    /// <summary>
    /// Userses the own info
    /// </summary>
    /// <returns>The string</returns>
    internal static string UsersOwnInfo() => "users/own-info";

    /// <summary>
    /// Userses the db using the specified include last used time
    /// </summary>
    /// <param name="includeLastUsedTime">The include last used time</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Users the db using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="includeLastUsedTime">The include last used time</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Users the db rotate key using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <returns>The string</returns>
    internal static string UserDbRotateKey(string userId) => $"users/db/{userId}/rotate-key";

    /// <summary>
    /// Users the db activate using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <returns>The string</returns>
    internal static string UserDbActivate(string userId) => $"users/db/{userId}/activate";

    /// <summary>
    /// Users the db deactivate using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <returns>The string</returns>
    internal static string UserDbDeactivate(string userId) => $"users/db/{userId}/deactivate";

    // Authz Roles & Assignments endpoints
    /// <summary>
    /// Roleses
    /// </summary>
    /// <returns>The string</returns>
    internal static string Roles() => "authz/roles";

    /// <summary>
    /// Roles the id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string Role(string id) => $"authz/roles/{id}";

    /// <summary>
    /// Roles the add permissions using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string RoleAddPermissions(string id) => $"authz/roles/{id}/add-permissions";

    /// <summary>
    /// Roles the remove permissions using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string RoleRemovePermissions(string id) =>
        $"authz/roles/{id}/remove-permissions";

    /// <summary>
    /// Roles the has permission using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string RoleHasPermission(string id) => $"authz/roles/{id}/has-permission";

    /// <summary>
    /// Roles the users deprecated using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string RoleUsersDeprecated(string id) => $"authz/roles/{id}/users"; // deprecated

    /// <summary>
    /// Roles the user assignments using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string RoleUserAssignments(string id) => $"authz/roles/{id}/user-assignments";

    /// <summary>
    /// Roles the group assignments using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string RoleGroupAssignments(string id) => $"authz/roles/{id}/group-assignments";

    // Authz Users (role assignment)
    /// <summary>
    /// Authzes the user roles using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <param name="userType">The user type</param>
    /// <param name="includeFullRoles">The include full roles</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Authzes the user assign using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <returns>The string</returns>
    internal static string AuthzUserAssign(string userId) => $"authz/users/{userId}/assign";

    /// <summary>
    /// Authzes the user revoke using the specified user id
    /// </summary>
    /// <param name="userId">The user id</param>
    /// <returns>The string</returns>
    internal static string AuthzUserRevoke(string userId) => $"authz/users/{userId}/revoke";

    // Authz Groups
    /// <summary>
    /// Authzes the group assign using the specified group id
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <returns>The string</returns>
    internal static string AuthzGroupAssign(string groupId) => $"authz/groups/{groupId}/assign";

    /// <summary>
    /// Authzes the group revoke using the specified group id
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <returns>The string</returns>
    internal static string AuthzGroupRevoke(string groupId) => $"authz/groups/{groupId}/revoke";

    /// <summary>
    /// Authzes the group roles using the specified group id
    /// </summary>
    /// <param name="groupId">The group id</param>
    /// <param name="groupType">The group type</param>
    /// <param name="includeFullRoles">The include full roles</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Authzes the groups using the specified group type
    /// </summary>
    /// <param name="groupType">The group type</param>
    /// <returns>The string</returns>
    internal static string AuthzGroups(string groupType) => $"authz/groups/{groupType}";

    // Replication endpoints
    /// <summary>
    /// Replicates
    /// </summary>
    /// <returns>The string</returns>
    internal static string Replicate() => "replication/replicate";

    /// <summary>
    /// Replications the details using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <param name="includeHistory">The include history</param>
    /// <returns>The path</returns>
    internal static string ReplicationDetails(Guid id, bool includeHistory = false)
    {
        var path = $"replication/replicate/{id}";
        if (includeHistory)
        {
            path += "?includeHistory=true";
        }
        return path;
    }

    /// <summary>
    /// Replications the list using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="shard">The shard</param>
    /// <param name="targetNode">The target node</param>
    /// <param name="includeHistory">The include history</param>
    /// <returns>The path</returns>
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

    /// <summary>
    /// Replications the cancel using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string ReplicationCancel(Guid id) => $"replication/replicate/{id}/cancel";

    /// <summary>
    /// Replications the delete using the specified id
    /// </summary>
    /// <param name="id">The id</param>
    /// <returns>The string</returns>
    internal static string ReplicationDelete(Guid id) => $"replication/replicate/{id}";
}
