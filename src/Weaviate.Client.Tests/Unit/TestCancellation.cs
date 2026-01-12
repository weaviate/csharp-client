using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The test cancellation class
/// </summary>
public class TestCancellation
{
    // Helper creates a client with a delaying mock handler that respects cancellation.
    /// <summary>
    /// Creates the client with delay using the specified delay
    /// </summary>
    /// <param name="delay">The delay</param>
    /// <returns>The client</returns>
    private static WeaviateClient CreateClientWithDelay(TimeSpan delay)
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(mockLeaf => new DelayingHandler(
            delay,
            inner: mockLeaf
        ));
        return client;
    }

    /// <summary>
    /// Tests that get meta pre cancelled token throws
    /// </summary>
    [Fact]
    public async Task GetMeta_PreCancelledToken_Throws()
    {
        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(50));
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetMeta(cts.Token));
    }

    /// <summary>
    /// Tests that wait until ready cancels before timeout
    /// </summary>
    [Fact]
    public async Task WaitUntilReady_CancelsBeforeTimeout()
    {
        // Delay long enough that cancellation triggers before readiness loop completes.
        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(200));
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // cancel before the simulated 200ms delay finishes

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.WaitUntilReady(TimeSpan.FromSeconds(2), null, cts.Token)
        );
    }

    /// <summary>
    /// Tests that live pre cancelled token throws or returns false
    /// </summary>
    [Fact]
    public async Task Live_PreCancelledToken_ThrowsOrReturnsFalse()
    {
        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(100));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Live wraps exceptions and returns false on failure; ensure cancellation doesn't masquerade as success.
        var result = await client.IsLive(cts.Token);
        Assert.False(result);
    }

    /// <summary>
    /// Tests that get meta cancellation during delay throws
    /// </summary>
    [Fact]
    public async Task GetMeta_CancellationDuringDelay_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(1000));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetMeta(cts.Token));
    }
}
