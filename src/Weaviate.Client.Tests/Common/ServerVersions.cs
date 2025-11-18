namespace Weaviate.Client.Tests.Common;

/// <summary>
/// Centralized server version constants for integration tests.
/// </summary>
internal static class ServerVersions
{
    /// <summary>
    /// Minimum Weaviate server version officially supported by this C# client for integration scenarios.
    /// Tests will skip if the running server reports a lower version.
    /// </summary>
    public const string MinSupported = "1.31.0";
}
