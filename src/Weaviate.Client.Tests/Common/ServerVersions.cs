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
    public const string MinSupported = "1.32.0";

    /// <summary>
    /// First Weaviate server version that applies a server-side default for
    /// <c>vectorIndexType</c> when the client omits it. On this version and later,
    /// the C# client must leave an unset <c>VectorIndexType</c> empty rather than
    /// injecting <c>"hnsw"</c>.
    /// </summary>
    public const string DefaultVectorIndexTypeServerSide = "1.37.5";
}
