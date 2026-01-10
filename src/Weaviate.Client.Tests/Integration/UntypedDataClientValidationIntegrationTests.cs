using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration
{
    /// <summary>
    /// The untyped data client validation integration tests class
    /// </summary>
    /// <seealso cref="IntegrationTests"/>
    public class UntypedDataClientValidationIntegrationTests : IntegrationTests
    {
        /// <summary>
        /// The simple model class
        /// </summary>
        public class SimpleModel
        {
            /// <summary>
            /// Gets or sets the value of the name
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the value of the age
            /// </summary>
            public int Age { get; set; }
        }

        /// <summary>
        /// The int
        /// </summary>
        private readonly CollectionCreateParams _config = new()
        {
            Name = "People",
            Properties = new[]
            {
                new Weaviate.Client.Models.Property
                {
                    Name = "name",
                    DataType = Weaviate.Client.Models.DataType.Text,
                },
                new Weaviate.Client.Models.Property
                {
                    Name = "age",
                    DataType = Weaviate.Client.Models.DataType.Int,
                },
            },
        };

        /// <summary>
        /// Tests that insert with invalid type throws validation exception
        /// </summary>
        [Fact]
        public async Task Insert_WithInvalidType_ThrowsValidationException()
        {
            var collection = await CollectionFactory<object>(_config);

            var invalidObj = new { Name = "Alice", Age = "notAnInt" }; // Age is string, should be int
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await collection.Data.Insert(
                    invalidObj,
                    validate: true,
                    cancellationToken: TestContext.Current.CancellationToken
                );
            });

            await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            {
                await collection.Data.Insert(
                    invalidObj,
                    validate: false,
                    cancellationToken: TestContext.Current.CancellationToken
                );
            });
        }

        /// <summary>
        /// Tests that insert many with invalid type throws validation exception
        /// </summary>
        [Fact]
        public async Task InsertMany_WithInvalidType_ThrowsValidationException()
        {
            var collection = await CollectionFactory<object>(_config);

            var validObj = new SimpleModel { Name = "Bob", Age = 30 };
            var invalidObj = new { Name = "Eve", Age = "notAnInt" };
            var objects = new object[] { validObj, invalidObj };

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await collection.Data.InsertMany(
                    objects,
                    validate: true,
                    cancellationToken: TestContext.Current.CancellationToken
                );
            });

            var insertResponse = await collection.Data.InsertMany(
                objects,
                validate: false,
                cancellationToken: TestContext.Current.CancellationToken
            );

            Assert.Single(insertResponse.Errors);
        }

        /// <summary>
        /// Tests that insert with valid type succeeds
        /// </summary>
        [Fact]
        public async Task Insert_WithValidType_Succeeds()
        {
            var collection = await CollectionFactory<object>(_config);

            var validObj = new SimpleModel { Name = "Charlie", Age = 25 };
            var id = await collection.Data.Insert(
                validObj,
                validate: true,
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.NotEqual(Guid.Empty, id);
        }
    }
}
