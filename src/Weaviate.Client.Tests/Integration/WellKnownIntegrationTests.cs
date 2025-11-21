namespace Weaviate.Client.Tests.Integration
{
    public class WellKnownIntegrationTests : IntegrationTests
    {
        [Fact]
        public async Task ServerIsReady_ShouldReturnTrue()
        {
            // Arrange
            var client = _weaviate;

            // Act
            var isReady = await client.IsReady(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(isReady, "The server should be ready.");
        }

        [Fact]
        public async Task Live_ShouldReturnTrue()
        {
            // Arrange
            var client = _weaviate;

            // Act
            var isLive = await client.Live(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(isLive, "The server should be live.");
        }

        [Fact]
        public async Task WaitUntilReady_ShouldCompleteSuccessfully()
        {
            // Arrange
            var client = _weaviate;

            // Act
            await client.WaitUntilReady(
                TimeSpan.FromSeconds(10),
                TestContext.Current.CancellationToken
            );

            // Assert
            // If no exception is thrown, the test passes.
        }
    }
}
