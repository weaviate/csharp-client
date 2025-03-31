namespace Weaviate.Client.Rest;

internal static class WeaviateEndpoints
{
    public static string Schema() => $"/schema";
    public static string Schema(string className) => $"schema/{className}";
    public static string SchemaProperties(string className) => $"schema/{className}/properties";
    public static string SchemaShards(string className) => $"schema/{className}/shards";
    public static string SchemaShard(string className, string shardName) => $"schema/{className}/shards/{shardName}";
    public static string SchemaTenants(string className) => $"schema/{className}/tenants";
    public static string SchemaTenant(string className, string tenantName) => $"schema/{className}/tenants/{tenantName}";

}