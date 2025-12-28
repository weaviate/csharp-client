using Weaviate.Client;
using Xunit;

namespace Weaviate.Client.CollectionMapper.Tests.Integration;

/// <summary>
/// Base class for integration tests that require a real Weaviate instance.
///
/// Prerequisites:
/// - Docker must be installed and running
/// - Run: docker-compose -f docker-compose.integration.yml up -d
/// - Wait for Weaviate to be ready (http://localhost:8080/v1/.well-known/ready)
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected WeaviateClient Client { get; private set; } = null!;
    protected string TestCollectionPrefix { get; }

    protected IntegrationTestBase()
    {
        // Use unique prefix for each test to avoid conflicts
        TestCollectionPrefix = $"Test_{Guid.NewGuid():N}_";
    }

    /// <summary>
    /// Initialize the test by creating and verifying the Weaviate client connection.
    /// Called automatically by xUnit before each test.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Build client using WeaviateClientBuilder
        Client = await WeaviateClientBuilder.Local().BuildAsync();

        // Verify connection
        var ready = await Client.IsReady();
        if (!ready)
        {
            throw new InvalidOperationException(
                "Weaviate is not ready. "
                    + "Please ensure Docker is running and execute: "
                    + "docker-compose -f docker-compose.integration.yml up -d"
            );
        }

        // Verify minimum version requirement
        var minVersion = new Version(1, 32, 0);
        if (Client.WeaviateVersion != null && Client.WeaviateVersion < minVersion)
        {
            throw new InvalidOperationException(
                $"Weaviate version {Client.WeaviateVersion} is not supported. "
                    + $"Minimum required version is {minVersion}. "
                    + "Please upgrade your Weaviate instance."
            );
        }
    }

    /// <summary>
    /// Clean up test collections and dispose the client.
    /// Called automatically by xUnit after each test.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Clean up test collections
        try
        {
            var collections = await Client.Collections.List().ToListAsync();
            var testCollections = collections
                .Where(c => c.Name.StartsWith(TestCollectionPrefix))
                .Select(c => c.Name)
                .ToList();

            foreach (var collection in testCollections)
            {
                try
                {
                    await Client.Collections.Delete(collection);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }

        Client.Dispose();
    }

    /// <summary>
    /// Generate a unique collection name for this test.
    /// </summary>
    protected string GetTestCollectionName(string baseName) => $"{TestCollectionPrefix}{baseName}";
}
