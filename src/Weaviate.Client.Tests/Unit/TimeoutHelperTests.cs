namespace Weaviate.Client.Tests.Unit;

[Collection("Unit Tests")]
public class TimeoutHelperTests
{
    [Fact]
    public async Task GetCancellationToken_WithTimeout_CancelsAfterTimeout()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(100);

        // Act
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken
        );
        await Task.Delay(200, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(token.IsCancellationRequested);
    }

    [Fact]
    public void GetCancellationToken_NoTimeout_ReturnsProvidedToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var token = TimeoutHelper.GetCancellationToken(null, cts.Token);

        // Assert
        Assert.Equal(cts.Token, token);
    }

    [Fact]
    public void GetCancellationToken_ZeroTimeout_ReturnsProvidedToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();

        // Act
        var token = TimeoutHelper.GetCancellationToken(TimeSpan.Zero, cts.Token);

        // Assert
        Assert.Equal(cts.Token, token);
    }

    [Fact]
    public async Task IsTimeoutCancellation_WithTimeoutException_ReturnsTrue()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(50);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await Task.Delay(100, token);
        });

        Assert.True(TimeoutHelper.IsTimeoutCancellation(exception));
        Assert.Equal(timeout, TimeoutHelper.GetTimeout());
    }

    [Fact]
    public async Task IsTimeoutCancellation_WithUserCancellation_ReturnsFalse()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await Task.Delay(1000, cts.Token);
        });

        Assert.False(TimeoutHelper.IsTimeoutCancellation(exception));
    }

    [Fact]
    public async Task TimeoutContext_ClearedBetweenOperations()
    {
        // Arrange & Act - First operation with timeout
        var token1 = TimeoutHelper.GetCancellationToken(
            TimeSpan.FromMilliseconds(50),
            providedToken: TestContext.Current.CancellationToken
        );
        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.True(token1.IsCancellationRequested);

        // Second operation without timeout
        var token2 = TimeoutHelper.GetCancellationToken(
            null,
            providedToken: TestContext.Current.CancellationToken
        );
        Assert.False(token2.IsCancellationRequested);

        // Assert - Verify context was cleared
        var ex = new TaskCanceledException();
        Assert.False(TimeoutHelper.IsTimeoutCancellation(ex));
    }

    [Fact]
    public async Task TimeoutContext_PreservedAcrossAsyncBoundaries()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(50);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken
        );

        // Act & Assert
        await Task.Run(
            async () =>
            {
                await Task.Delay(100, TestContext.Current.CancellationToken);

                var exception = new TaskCanceledException();
                Assert.True(TimeoutHelper.IsTimeoutCancellation(exception));
                Assert.Equal(timeout, TimeoutHelper.GetTimeout());
            },
            TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public void GetCancellationToken_WithOperationDescription_StoresOperation()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(30);
        var operation = "Search operation";

        // Act
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: operation
        );

        // Assert
        Assert.Equal(operation, TimeoutHelper.GetOperation());
    }

    [Fact]
    public void GetCancellationToken_WithConfigAndDefault_UsesConfig()
    {
        // Arrange
        var configTimeout = TimeSpan.FromSeconds(10);
        var defaultTimeout = TimeSpan.FromSeconds(30);

        // Act
        var token = TimeoutHelper.GetCancellationToken(
            configTimeout,
            defaultTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Test operation"
        );

        // Assert
        Assert.Equal(configTimeout, TimeoutHelper.GetTimeout());
    }

    [Fact]
    public void GetCancellationToken_WithNullConfig_UsesDefault()
    {
        // Arrange
        TimeSpan? configTimeout = null;
        var defaultTimeout = TimeSpan.FromSeconds(30);

        // Act
        var token = TimeoutHelper.GetCancellationToken(
            configTimeout,
            defaultTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Test operation"
        );

        // Assert
        Assert.Equal(defaultTimeout, TimeoutHelper.GetTimeout());
    }

    [Fact]
    public void IsTimeoutCancellation_WithNonCancellationException_ReturnsFalse()
    {
        // Arrange
        var exception = new InvalidOperationException("Not a cancellation");

        // Act
        var result = TimeoutHelper.IsTimeoutCancellation(exception);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsTimeoutCancellation_WithOperationCanceledException_ChecksContext()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(50);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken
        );

        // Wait for timeout
        Thread.Sleep(100);

        var exception = new OperationCanceledException();

        // Act
        var result = TimeoutHelper.IsTimeoutCancellation(exception);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetOperation_WithNoContext_ReturnsNull()
    {
        // Arrange
        TimeoutHelper.GetCancellationToken(
            null,
            providedToken: TestContext.Current.CancellationToken
        );

        // Act
        var operation = TimeoutHelper.GetOperation();

        // Assert
        Assert.Null(operation);
    }

    [Fact]
    public void GetTimeout_WithNoContext_ReturnsNull()
    {
        // Arrange
        TimeoutHelper.GetCancellationToken(
            null,
            providedToken: TestContext.Current.CancellationToken
        );

        // Act
        var timeout = TimeoutHelper.GetTimeout();

        // Assert
        Assert.Null(timeout);
    }
}
