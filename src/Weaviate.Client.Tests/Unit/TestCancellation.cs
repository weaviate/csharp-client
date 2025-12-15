using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

public class TestCancellation
{
    // Helper creates a client with a delaying mock handler that respects cancellation.
    private static WeaviateClient CreateClientWithDelay(TimeSpan delay)
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(mockLeaf =>
        {
            // Chain: DelayingHandler -> MockLeaf
            return new DelayingHandler(delay, inner: mockLeaf);
        });
        return client;
    }

    [Fact]
    public async Task GetMeta_PreCancelledToken_Throws()
    {
        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(50));
        var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetMeta(cts.Token));
    }

    [Fact]
    public async Task WaitUntilReady_CancelsBeforeTimeout()
    {
        // Delay long enough that cancellation triggers before readiness loop completes.
        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(200));
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(50); // cancel before the simulated 200ms delay finishes

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            client.WaitUntilReady(TimeSpan.FromSeconds(2), cts.Token)
        );
    }

    [Fact]
    public async Task Live_PreCancelledToken_ThrowsOrReturnsFalse()
    {
        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(100));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Live wraps exceptions and returns false on failure; ensure cancellation doesn't masquerade as success.
        var result = await client.IsLive(cts.Token);
        Assert.False(result);
    }

    [Fact]
    public async Task GetMeta_CancellationDuringDelay_Throws()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        using var client = CreateClientWithDelay(TimeSpan.FromMilliseconds(1000));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetMeta(cts.Token));
    }
}
