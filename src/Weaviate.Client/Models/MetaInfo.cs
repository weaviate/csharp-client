namespace Weaviate.Client.Models;

public class WeaviateVersion : IComparable<WeaviateVersion>, IEquatable<WeaviateVersion>
{
    private string OriginalString { get; set; }
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

    public int CompareTo(WeaviateVersion? other)
    {
        if (other == null)
            return 1;

        // If both have null ParsedVersion, compare original strings
        if (ParsedVersion == null && other.ParsedVersion == null)
            return string.Compare(
                OriginalString,
                other.OriginalString,
                StringComparison.OrdinalIgnoreCase
            );

        // Null ParsedVersion is considered "less than" any valid version
        if (ParsedVersion == null)
            return -1;
        if (other.ParsedVersion == null)
            return 1;

        return ParsedVersion.CompareTo(other.ParsedVersion);
    }

    // IEquatable<WeaviateVersion> implementation
    public bool Equals(WeaviateVersion? other)
    {
        if (other == null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        // If both have null ParsedVersion, compare original strings
        if (ParsedVersion == null && other.ParsedVersion == null)
            return string.Equals(
                OriginalString,
                other.OriginalString,
                StringComparison.OrdinalIgnoreCase
            );

        // If one is null and the other isn't, they're not equal
        if (ParsedVersion == null || other.ParsedVersion == null)
            return false;

        return ParsedVersion.Equals(other.ParsedVersion);
    }

    // Override Object.Equals
    public override bool Equals(object? obj)
    {
        return Equals(obj as WeaviateVersion);
    }

    // Override GetHashCode
    public override int GetHashCode()
    {
        return ParsedVersion?.GetHashCode() ?? OriginalString.GetHashCode();
    }

    // Comparison operators
    public static bool operator ==(WeaviateVersion? left, WeaviateVersion? right)
    {
        if (ReferenceEquals(left, right))
            return true;
        if (left is null || right is null)
            return false;
        return left.Equals(right);
    }

    public static bool operator !=(WeaviateVersion? left, WeaviateVersion? right)
    {
        return !(left == right);
    }

    public static bool operator <(WeaviateVersion? left, WeaviateVersion? right)
    {
        if (left is null)
            return right is not null;
        return left.CompareTo(right) < 0;
    }

    public static bool operator <=(WeaviateVersion? left, WeaviateVersion? right)
    {
        if (left is null)
            return true;
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >(WeaviateVersion? left, WeaviateVersion? right)
    {
        if (left is null)
            return false;
        return left.CompareTo(right) > 0;
    }

    public static bool operator >=(WeaviateVersion? left, WeaviateVersion? right)
    {
        if (left is null)
            return right is null;
        return left.CompareTo(right) >= 0;
    }
}

public struct MetaInfo
{
    public string Hostname { get; set; }
    public WeaviateVersion Version { get; set; }
    public Dictionary<string, object> Modules { get; set; }
    public int GrpcMaxMessageSize { get; set; }
}
