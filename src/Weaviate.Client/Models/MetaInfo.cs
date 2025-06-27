namespace Weaviate.Client.Models;

public class WeaviateVersion
{
    public string OriginalString { get; set; }
    public Version? ParsedVersion { get; set; }

    public WeaviateVersion(string versionString)
    {
        OriginalString = versionString;
        ParsedVersion = ParseVersionIgnoringPrerelease(versionString);
    }

    private static Version? ParseVersionIgnoringPrerelease(string versionString)
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

public struct MetaInfo
{
    public string Hostname { get; set; }
    public WeaviateVersion Version { get; set; }
    public Dictionary<string, object> Modules { get; set; }
    public int GrpcMaxMessageSize { get; set; }
}
