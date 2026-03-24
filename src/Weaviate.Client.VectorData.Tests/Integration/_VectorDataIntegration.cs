using Weaviate.Client.Models;

namespace Weaviate.Client.VectorData.Tests.Integration;

/// <summary>
/// Base class for VectorData integration tests.
/// Connects to a local Weaviate instance and cleans up collections after each test.
/// </summary>
public abstract class VectorDataIntegrationTests : IAsyncLifetime, IAsyncDisposable
{
    protected WeaviateClient _weaviate = null!;
    private readonly List<string> _collectionsToCleanup = [];

    /// <summary>
    /// Shorthand for the xunit test cancellation token.
    /// </summary>
    protected static CancellationToken CT => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        _weaviate = await WeaviateClientBuilder.Local().BuildAsync();

        var ready = false;
        try
        {
            ready = await _weaviate.IsReady();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Weaviate readiness check failed during VectorData integration test initialization.",
                ex
            );
        }

        if (!ready)
        {
            throw new InvalidOperationException(
                "Weaviate not ready on localhost:8080. Expected a running instance for integration tests."
            );
        }
    }

    /// <summary>
    /// Registers a collection name for cleanup after the test.
    /// </summary>
    protected void TrackCollection(string name)
    {
        _collectionsToCleanup.Add(name);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var name in _collectionsToCleanup)
        {
            try
            {
                await _weaviate.Collections.Delete(name);
            }
            catch
            {
                // Best effort cleanup
            }
        }

        _weaviate.Dispose();
    }
}
