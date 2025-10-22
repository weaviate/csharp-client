namespace Weaviate.Client.Models;

public struct MetaInfo
{
    public string Hostname { get; set; }
    public System.Version Version { get; set; }
    public Dictionary<string, object> Modules { get; set; }
    public ulong? GrpcMaxMessageSize { get; set; }

    public static Version? ParseWeaviateVersion(string versionString)
    {
        if (string.IsNullOrEmpty(versionString))
            return null;

        // Remove 'v' or 'V' prefix if present
        var cleaned = versionString.TrimStart('v', 'V');

        // Split on common pre-release separators and take the first part
        cleaned = cleaned.Split('-', '+')[0];

        return Version.TryParse(cleaned, out var version) ? version : null;
    }
}
