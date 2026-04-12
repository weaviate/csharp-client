namespace Weaviate.Client.Models;

/// <summary>
/// The meta info
/// </summary>
public struct MetaInfo
{
    /// <summary>
    /// Gets or sets the value of the hostname
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    /// Gets or sets the value of the version
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// Gets or sets the value of the modules
    /// </summary>
    public Dictionary<string, object> Modules { get; set; }

    /// <summary>
    /// Gets or sets the value of the grpc max message size
    /// </summary>
    public ulong? GrpcMaxMessageSize { get; set; }

    /// <summary>
    /// Parses the weaviate version using the specified version string
    /// </summary>
    /// <param name="versionString">The version string</param>
    /// <returns>The version</returns>
    public static Version? ParseWeaviateVersion(string versionString)
    {
        if (string.IsNullOrEmpty(versionString))
            return null;

        // Remove 'v' or 'V' prefix if present
        var cleaned = versionString.TrimStart('v', 'V');

        // Take only the numeric version portion (digits and dots)
        var end = 0;
        while (end < cleaned.Length && (char.IsDigit(cleaned[end]) || cleaned[end] == '.'))
            end++;
        cleaned = cleaned[..end];

        return Version.TryParse(cleaned, out var version) ? version : null;
    }
}
