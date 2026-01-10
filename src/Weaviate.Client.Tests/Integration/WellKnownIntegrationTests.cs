namespace Weaviate.Client.Tests.Integration
{
    /// <summary>
    /// The well known integration tests class
    /// </summary>
    /// <seealso cref="IntegrationTests"/>
    public class WellKnownIntegrationTests : IntegrationTests
    {
        /// <summary>
        /// Tests that server is ready should return true
        /// </summary>
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

        /// <summary>
        /// Tests that live should return true
        /// </summary>
        [Fact]
        public async Task Live_ShouldReturnTrue()
        {
            // Arrange
            var client = _weaviate;

            // Act
            var isLive = await client.IsLive(TestContext.Current.CancellationToken);

            // Assert
            Assert.True(isLive, "The server should be live.");
        }

        /// <summary>
        /// Tests that wait until ready should complete successfully
        /// </summary>
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
