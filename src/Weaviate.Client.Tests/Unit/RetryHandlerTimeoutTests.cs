using System.Net;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests for RetryHandler timeout exception wrapping after retry exhaustion.
/// </summary>
[Collection("Unit Tests")]
public class RetryHandlerTimeoutTests
{
    /// <summary>
    /// Mock HTTP handler that always throws TaskCanceledException to simulate timeout.
    /// </summary>
    private class TimeoutMockHttpHandler : HttpMessageHandler
    {
        private readonly TimeSpan _timeout;
        private readonly string? _operation;

        public TimeoutMockHttpHandler(TimeSpan timeout, string? operation = null)
        {
            _timeout = timeout;
            _operation = operation;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            // Set up timeout context
            var timeoutToken = TimeoutHelper.GetCancellationToken(
                _timeout,
                cancellationToken,
                _operation
            );

            // Wait for the timeout to actually expire (timeout + buffer)
            // We use Thread.Sleep instead of Task.Delay to ensure it blocks and the timeout fires
            Thread.Sleep(_timeout + TimeSpan.FromMilliseconds(50));

            // Now the timeout token should be cancelled, throw to simulate timeout
            throw new TaskCanceledException("Simulated timeout");
        }
    }

    /// <summary>
    /// Mock HTTP handler that succeeds after a specified number of failures.
    /// </summary>
    private class FlakeyHttpHandler : HttpMessageHandler
    {
        private readonly int _failuresBeforeSuccess;
        private int _attemptCount = 0;

        public FlakeyHttpHandler(int failuresBeforeSuccess)
        {
            _failuresBeforeSuccess = failuresBeforeSuccess;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            _attemptCount++;

            if (_attemptCount <= _failuresBeforeSuccess)
            {
                // Throw HttpRequestException with a SocketException inner to simulate network error
                var innerEx = new System.Net.Sockets.SocketException(
                    (int)System.Net.Sockets.SocketError.HostUnreachable
                );
                throw new HttpRequestException("Simulated network failure", innerEx);
            }

            return Task.FromResult(
                new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("Success"),
                }
            );
        }
    }

    [Fact]
    public async Task RetryHandler_TimeoutAfterAllRetries_ThrowsTimeoutException()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxRetries = 2,
            RetryOn = RetryOn.Timeout,
            InitialDelay = TimeSpan.FromMilliseconds(1), // Fast retries for testing
        };

        var timeout = TimeSpan.FromMilliseconds(10); // Short timeout for testing
        var operation = "Test operation";

        var retryHandler = new RetryHandler(policy)
        {
            InnerHandler = new TimeoutMockHttpHandler(timeout, operation),
        };

        using var client = new HttpClient(retryHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<WeaviateTimeoutException>(async () =>
        {
            await client.SendAsync(
                request,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });

        Assert.Equal(timeout, ex.Timeout);
        Assert.Equal(operation, ex.Operation);
        Assert.Contains("0.0 seconds", ex.Message); // 10ms rounds to 0.0
        Assert.Contains("Test operation", ex.Message);
    }

    [Fact]
    public async Task RetryHandler_TimeoutWithNoOperation_UsesDefaultMessage()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxRetries = 1,
            RetryOn = RetryOn.Timeout,
            InitialDelay = TimeSpan.FromMilliseconds(1), // Fast retries for testing
        };

        var timeout = TimeSpan.FromMilliseconds(10); // Short timeout for testing

        var retryHandler = new RetryHandler(policy)
        {
            InnerHandler = new TimeoutMockHttpHandler(timeout), // No operation specified
        };

        using var client = new HttpClient(retryHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<WeaviateTimeoutException>(async () =>
        {
            await client.SendAsync(
                request,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });

        Assert.Equal(timeout, ex.Timeout);
        Assert.Null(ex.Operation);
        Assert.Contains("The operation timed out", ex.Message);
    }

    [Fact]
    public async Task RetryHandler_NonTimeoutException_DoesNotWrapInTimeoutException()
    {
        // Arrange
        var policy = new RetryPolicy { MaxRetries = 1, RetryOn = RetryOn.NetworkError };

        var retryHandler = new RetryHandler(policy) { InnerHandler = new FlakeyHttpHandler(5) };

        using var client = new HttpClient(retryHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act & Assert - Should throw HttpRequestException, not WeaviateTimeoutException
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
        {
            await client.SendAsync(
                request,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });
    }

    [Fact]
    public async Task RetryHandler_SuccessAfterRetries_DoesNotThrowTimeout()
    {
        // Arrange
        var policy = new RetryPolicy { MaxRetries = 3, RetryOn = RetryOn.NetworkError };

        var retryHandler = new RetryHandler(policy)
        {
            InnerHandler = new FlakeyHttpHandler(2), // Fails 2 times, succeeds on 3rd
        };

        using var client = new HttpClient(retryHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act
        var response = await client.SendAsync(
            request,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RetryHandler_TimeoutDuringRetry_PreservesOriginalTimeout()
    {
        // Arrange
        var policy = new RetryPolicy
        {
            MaxRetries = 3,
            RetryOn = RetryOn.Timeout,
            InitialDelay = TimeSpan.FromMilliseconds(1),
        };

        var originalTimeout = TimeSpan.FromMilliseconds(10); // Short timeout for testing
        var operation = "Retry test operation";

        var retryHandler = new RetryHandler(policy)
        {
            InnerHandler = new TimeoutMockHttpHandler(originalTimeout, operation),
        };

        using var client = new HttpClient(retryHandler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<WeaviateTimeoutException>(async () =>
        {
            await client.SendAsync(
                request,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });

        // The timeout information should be preserved from the original timeout
        Assert.Equal(originalTimeout, ex.Timeout);
        Assert.Equal(operation, ex.Operation);
    }

    [Fact]
    public async Task RetryHandler_MultipleRequests_TimeoutContextIsolated()
    {
        // Arrange - Test that timeout context from one request doesn't leak to another
        var policy = new RetryPolicy
        {
            MaxRetries = 1,
            RetryOn = RetryOn.Timeout,
            InitialDelay = TimeSpan.FromMilliseconds(1), // Fast retries for testing
        };

        // First request with timeout
        var timeout1 = TimeSpan.FromMilliseconds(10); // Short timeout for testing
        var retryHandler1 = new RetryHandler(policy)
        {
            InnerHandler = new TimeoutMockHttpHandler(timeout1, "Operation 1"),
        };

        using var client1 = new HttpClient(retryHandler1);
        var request1 = new HttpRequestMessage(HttpMethod.Get, "http://test.com/1");

        // Act & Assert - First request
        var ex1 = await Assert.ThrowsAsync<WeaviateTimeoutException>(async () =>
        {
            await client1.SendAsync(
                request1,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });

        Assert.Equal(timeout1, ex1.Timeout);
        Assert.Equal("Operation 1", ex1.Operation);

        // Second request with different timeout - create new handler instance
        var timeout2 = TimeSpan.FromMilliseconds(15); // Different short timeout
        var retryHandler2 = new RetryHandler(policy)
        {
            InnerHandler = new TimeoutMockHttpHandler(timeout2, "Operation 2"),
        };

        using var client2 = new HttpClient(retryHandler2);
        var request2 = new HttpRequestMessage(HttpMethod.Get, "http://test.com/2");

        // Act & Assert - Second request
        var ex2 = await Assert.ThrowsAsync<WeaviateTimeoutException>(async () =>
        {
            await client2.SendAsync(
                request2,
                cancellationToken: TestContext.Current.CancellationToken
            );
        });

        Assert.Equal(timeout2, ex2.Timeout);
        Assert.Equal("Operation 2", ex2.Operation);

        // Verify contexts didn't leak
        Assert.NotEqual(ex1.Timeout, ex2.Timeout);
        Assert.NotEqual(ex1.Operation, ex2.Operation);
    }
}
