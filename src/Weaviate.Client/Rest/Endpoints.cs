namespace Weaviate.Client.Rest;

internal static class WeaviateEndpoints
{
    public static string Collection() => $"schema";

    public static string Collection(string collectionName) => $"schema/{collectionName}";

    public static string CollectionProperties(string className) => $"schema/{className}/properties";

    public static string CollectionShards(string className) => $"schema/{className}/shards";

    public static string CollectionShard(string className, string shardName) =>
        $"schema/{className}/shards/{shardName}";

    public static string CollectionTenants(string className) => $"schema/{className}/tenants";

    public static string CollectionTenant(string className, string tenantName) =>
        $"schema/{className}/tenants/{tenantName}";

    public static string Meta() => $"meta";

    public static string Nodes() => $"nodes";

    public static string Objects() => $"objects";

    internal static string? CollectionObject(string collectionName, Guid id) =>
        $"objects/{collectionName}/{id}";

    internal static string? Reference(string collectionName, Guid from, string fromProperty) =>
        $"objects/{collectionName}/{from}/references/{fromProperty}";

    internal static string? ReferencesAdd() => "batch/references";
}
