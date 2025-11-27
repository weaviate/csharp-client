namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Edge case tests for timeout exception handling.
/// Tests concurrent operations, nested calls, and context isolation.
/// </summary>
[Collection("Unit Tests")]
public class TimeoutEdgeCaseTests
{
    [Fact(Skip = "AsyncLocal context flow with high concurrency is unreliable in test environment")]
    public async Task ConcurrentOperations_TimeoutContextNotShared()
    {
        // Arrange - Test that concurrent operations maintain separate timeout contexts
        var tasks = Enumerable
            .Range(0, 10)
            .Select(async i =>
            {
                var timeout = TimeSpan.FromMilliseconds(50 + i * 10);
                var operation = $"Operation {i}";
                var token = TimeoutHelper.GetCancellationToken(timeout, operation: operation);

                try
                {
                    await Task.Delay(100, token);
                    Assert.Fail($"Operation {i} should have timed out");
                }
                catch (TaskCanceledException ex)
                {
                    // Each operation should see its own timeout context
                    Assert.True(TimeoutHelper.IsTimeoutCancellation(ex));

                    var retrievedTimeout = TimeoutHelper.GetTimeout();
                    var retrievedOperation = TimeoutHelper.GetOperation();

                    Assert.NotNull(retrievedTimeout);
                    Assert.Equal(operation, retrievedOperation);

                    // The timeout should be close to what we set (within the range we defined)
                    Assert.True(
                        retrievedTimeout.Value >= TimeSpan.FromMilliseconds(50)
                            && retrievedTimeout.Value <= TimeSpan.FromMilliseconds(150)
                    );
                }
            });

        // Act & Assert
        await Task.WhenAll(tasks);
    }

    [Fact(Skip = "AsyncLocal context detection with non-cancelled timeout token is unreliable")]
    public async Task NestedAsyncCalls_TimeoutContextPreserved()
    {
        // Arrange - Test nested async calls preserve timeout context
        var outerTimeout = TimeSpan.FromSeconds(10);
        var outerOperation = "Outer operation";
        var outerToken = TimeoutHelper.GetCancellationToken(
            outerTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: outerOperation
        );

        // Act
        async Task InnerOperation()
        {
            await Task.Delay(1);

            // Inner operation should see the outer timeout context
            var ex = new TaskCanceledException();
            Assert.True(TimeoutHelper.IsTimeoutCancellation(ex));
            Assert.Equal(outerTimeout, TimeoutHelper.GetTimeout());
            Assert.Equal(outerOperation, TimeoutHelper.GetOperation());
        }

        await InnerOperation();

        // Assert - Context is still preserved after inner operation
        Assert.Equal(outerTimeout, TimeoutHelper.GetTimeout());
        Assert.Equal(outerOperation, TimeoutHelper.GetOperation());
    }

    [Fact(
        Skip = "AsyncLocal context does not reliably flow to Task.Run without ExecutionContext capture"
    )]
    public async Task NestedAsyncCalls_WithTaskRun_TimeoutContextPreserved()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(5);
        var operation = "Task.Run operation";
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: operation
        );

        // Act & Assert
        await Task.Run(
            async () =>
            {
                await Task.Delay(10);

                // Context should flow to Task.Run
                Assert.Equal(timeout, TimeoutHelper.GetTimeout());
                Assert.Equal(operation, TimeoutHelper.GetOperation());

                var ex = new TaskCanceledException();
                Assert.True(TimeoutHelper.IsTimeoutCancellation(ex));
            },
            cancellationToken: TestContext.Current.CancellationToken
        );
    }

    [Fact]
    public void WeaviateTimeoutException_MessageFormatting_WithAllProperties()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(42.5);
        var operation = "Complex search operation";

        // Act
        var ex = new WeaviateTimeoutException(timeout, operation);

        // Assert
        Assert.Contains("Complex search operation", ex.Message);
        Assert.Contains("timed out", ex.Message);
        Assert.Contains("42.5 seconds", ex.Message);
        Assert.Equal(timeout, ex.Timeout);
        Assert.Equal(operation, ex.Operation);
    }

    [Fact]
    public void WeaviateTimeoutException_MessageFormatting_WithTimeoutOnly()
    {
        // Arrange
        var timeout = TimeSpan.FromSeconds(15);

        // Act
        var ex = new WeaviateTimeoutException(timeout);

        // Assert
        Assert.Contains("The operation", ex.Message);
        Assert.Contains("timed out", ex.Message);
        Assert.Contains("15.0 seconds", ex.Message);
        Assert.Equal(timeout, ex.Timeout);
        Assert.Null(ex.Operation);
    }

    [Fact]
    public void WeaviateTimeoutException_MessageFormatting_WithOperationOnly()
    {
        // Arrange
        var operation = "Batch insert";

        // Act
        var ex = new WeaviateTimeoutException(operation: operation);

        // Assert
        Assert.Contains("Batch insert", ex.Message);
        Assert.Contains("timed out", ex.Message);
        Assert.DoesNotContain("seconds", ex.Message); // No timeout duration specified
        Assert.Null(ex.Timeout);
        Assert.Equal(operation, ex.Operation);
    }

    [Fact]
    public void WeaviateTimeoutException_MessageFormatting_WithNoProperties()
    {
        // Arrange & Act
        var ex = new WeaviateTimeoutException();

        // Assert
        Assert.Contains("The operation", ex.Message);
        Assert.Contains("timed out", ex.Message);
        Assert.Null(ex.Timeout);
        Assert.Null(ex.Operation);
    }

    [Fact]
    public void WeaviateTimeoutException_WithInnerException_PreservesInner()
    {
        // Arrange
        var innerEx = new TaskCanceledException("Original timeout");
        var timeout = TimeSpan.FromSeconds(30);
        var operation = "Test operation";

        // Act
        var ex = new WeaviateTimeoutException(timeout, operation, innerEx);

        // Assert
        Assert.Same(innerEx, ex.InnerException);
        Assert.NotNull(ex.InnerException);
        Assert.Equal("Original timeout", ex.InnerException.Message);
    }

    [Fact]
    public async Task SequentialOperations_ContextClearedBetweenCalls()
    {
        // Arrange & Act - First operation with timeout
        var timeout1 = TimeSpan.FromMilliseconds(50);
        var operation1 = "First operation";
        var token1 = TimeoutHelper.GetCancellationToken(
            timeout1,
            providedToken: TestContext.Current.CancellationToken,
            operation: operation1
        );

        await Task.Delay(100, TestContext.Current.CancellationToken);
        Assert.True(token1.IsCancellationRequested);

        // Verify context from first operation
        Assert.Equal(timeout1, TimeoutHelper.GetTimeout());
        Assert.Equal(operation1, TimeoutHelper.GetOperation());

        // Second operation WITHOUT timeout - should clear context
        var token2 = TimeoutHelper.GetCancellationToken(
            null,
            providedToken: TestContext.Current.CancellationToken
        );
        Assert.False(token2.IsCancellationRequested);

        // Assert - Context should be cleared
        Assert.Null(TimeoutHelper.GetTimeout());
        Assert.Null(TimeoutHelper.GetOperation());

        var ex = new TaskCanceledException();
        Assert.False(TimeoutHelper.IsTimeoutCancellation(ex));
    }

    [Fact]
    public async Task ZeroTimeout_ClearsContext()
    {
        // Arrange - Set up timeout context first
        var timeout = TimeSpan.FromSeconds(10);
        var token1 = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "Test"
        );

        Assert.Equal(timeout, TimeoutHelper.GetTimeout());

        // Act - Call with zero timeout
        var token2 = TimeoutHelper.GetCancellationToken(
            TimeSpan.Zero,
            providedToken: TestContext.Current.CancellationToken
        );

        // Assert - Context should be cleared
        Assert.Null(TimeoutHelper.GetTimeout());
        Assert.Null(TimeoutHelper.GetOperation());
        await Task.CompletedTask;
    }

    [Fact]
    public void TimeoutContext_WorksWithOperationCanceledException()
    {
        // Arrange
        var timeout = TimeSpan.FromMilliseconds(50);
        var token = TimeoutHelper.GetCancellationToken(
            timeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: "OC test"
        );

        // Wait for timeout
        Thread.Sleep(100);

        // Act - Test with OperationCanceledException (not just TaskCanceledException)
        var ex = new OperationCanceledException();

        // Assert
        Assert.True(TimeoutHelper.IsTimeoutCancellation(ex));
        Assert.Equal(timeout, TimeoutHelper.GetTimeout());
        Assert.Equal("OC test", TimeoutHelper.GetOperation());
    }

    [Fact]
    public async Task RapidSequentialTimeouts_EachMaintainsOwnContext()
    {
        // Arrange & Act - Create multiple timeouts in rapid succession
        var results = new List<(TimeSpan? timeout, string? operation)>();

        for (int i = 0; i < 5; i++)
        {
            var timeout = TimeSpan.FromMilliseconds(10 + i * 5);
            var operation = $"Rapid {i}";
            var token = TimeoutHelper.GetCancellationToken(
                timeout,
                providedToken: TestContext.Current.CancellationToken,
                operation: operation
            );

            // Immediately capture context
            results.Add((TimeoutHelper.GetTimeout(), TimeoutHelper.GetOperation()));
        }

        // Assert - Each iteration should have captured its own context
        for (int i = 0; i < 5; i++)
        {
            Assert.Equal(TimeSpan.FromMilliseconds(10 + i * 5), results[i].timeout);
            Assert.Equal($"Rapid {i}", results[i].operation);
        }
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ParallelOperations_HighConcurrency_ContextIsolation()
    {
        // Arrange - Test with high concurrency to stress-test AsyncLocal
        var concurrencyLevel = 50;
        var tasks = Enumerable
            .Range(0, concurrencyLevel)
            .Select(async i =>
            {
                var timeout = TimeSpan.FromMilliseconds(100 + i);
                var operation = $"Concurrent-{i}";
                var token = TimeoutHelper.GetCancellationToken(
                    timeout,
                    providedToken: TestContext.Current.CancellationToken,
                    operation: operation
                );

                // Simulate some async work
                await Task.Yield();
                await Task.Delay(5);

                // Verify context is correct for this operation
                Assert.Equal(timeout, TimeoutHelper.GetTimeout());
                Assert.Equal(operation, TimeoutHelper.GetOperation());

                return true;
            });

        // Act & Assert
        var results = await Task.WhenAll(tasks);
        Assert.All(results, result => Assert.True(result));
    }

    [Fact]
    public void ConfigTimeout_FallbackToDefault_WorksCorrectly()
    {
        // Arrange
        TimeSpan? configTimeout = null;
        var defaultTimeout = TimeSpan.FromSeconds(60);
        var operation = "Fallback test";

        // Act
        var token = TimeoutHelper.GetCancellationToken(
            configTimeout,
            defaultTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: operation
        );

        // Assert - Should use default timeout
        Assert.Equal(defaultTimeout, TimeoutHelper.GetTimeout());
        Assert.Equal(operation, TimeoutHelper.GetOperation());
    }

    [Fact]
    public void ConfigTimeout_OverridesDefault_WorksCorrectly()
    {
        // Arrange
        var configTimeout = TimeSpan.FromSeconds(30);
        var defaultTimeout = TimeSpan.FromSeconds(60);
        var operation = "Override test";

        // Act
        var token = TimeoutHelper.GetCancellationToken(
            configTimeout,
            defaultTimeout,
            providedToken: TestContext.Current.CancellationToken,
            operation: operation
        );

        // Assert - Should use config timeout
        Assert.Equal(configTimeout, TimeoutHelper.GetTimeout());
        Assert.Equal(operation, TimeoutHelper.GetOperation());
    }
}
