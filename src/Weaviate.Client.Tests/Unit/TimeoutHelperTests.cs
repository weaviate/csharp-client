using Weaviate.Client.Internal;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The timeout helper tests class
/// </summary>
[Collection("Unit Tests")]
public class TimeoutHelperTests
{
    /// <summary>
    /// Tests that get cancellation token with timeout cancels after timeout
    /// </summary>
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

    /// <summary>
    /// Tests that get cancellation token no timeout returns provided token
    /// </summary>
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

    /// <summary>
    /// Tests that get cancellation token zero timeout returns provided token
    /// </summary>
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

    /// <summary>
    /// Tests that is timeout cancellation with timeout exception returns true
    /// </summary>
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

    /// <summary>
    /// Tests that is timeout cancellation with user cancellation returns false
    /// </summary>
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

    /// <summary>
    /// Tests that timeout context cleared between operations
    /// </summary>
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

    /// <summary>
    /// Tests that timeout context preserved across async boundaries
    /// </summary>
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

    /// <summary>
    /// Tests that get cancellation token with operation description stores operation
    /// </summary>
    [Fact]
    public void GetCancellationToken_WithOperationDescription_StoresOperation()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(30);
        var operation = "Search operation";

        // Act
        _ = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: operation
        );

        // Assert
        Assert.Equal(operation, TimeoutHelper.GetOperation());
    }

    /// <summary>
    /// Tests that get cancellation token with config and default uses config
    /// </summary>
    [Fact]
    public void GetCancellationToken_WithConfigAndDefault_UsesConfig()
    {
        // Arrange
        var configTimeout = TimeSpan.FromSeconds(10);
        var defaultTimeout = TimeSpan.FromSeconds(30);

        // Act
        _ = TimeoutHelper.GetCancellationToken(
            configTimeout,
            defaultTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Test operation"
        );

        // Assert
        Assert.Equal(configTimeout, TimeoutHelper.GetTimeout());
    }

    /// <summary>
    /// Tests that get cancellation token with null config uses default
    /// </summary>
    [Fact]
    public void GetCancellationToken_WithNullConfig_UsesDefault()
    {
        // Arrange
        TimeSpan? configTimeout = null;
        var defaultTimeout = TimeSpan.FromSeconds(30);

        // Act
        _ = TimeoutHelper.GetCancellationToken(
            configTimeout,
            defaultTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Test operation"
        );

        // Assert
        Assert.Equal(defaultTimeout, TimeoutHelper.GetTimeout());
    }

    /// <summary>
    /// Tests that is timeout cancellation with non cancellation exception returns false
    /// </summary>
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

    /// <summary>
    /// Tests that is timeout cancellation with operation canceled exception checks context
    /// </summary>
    [Fact]
    public void IsTimeoutCancellation_WithOperationCanceledException_ChecksContext()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(50);
        _ = TimeoutHelper.GetCancellationToken(
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

    /// <summary>
    /// Tests that get operation with no context returns null
    /// </summary>
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

    /// <summary>
    /// Tests that get timeout with no context returns null
    /// </summary>
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
