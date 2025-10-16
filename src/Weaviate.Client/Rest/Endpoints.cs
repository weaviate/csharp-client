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
}
