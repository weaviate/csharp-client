using Weaviate.Client.Models;
using Xunit;

namespace Weaviate.Client.Tests.Integration
{
    public class UntypedDataClientValidationIntegrationTests : IntegrationTests
    {
        public class SimpleModel
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }

        private readonly CollectionConfig _config = new()
        {
            Name = "People",
            Properties = new[]
            {
                new Weaviate.Client.Models.Property { Name = "name", DataType = new[] { "text" } },
                new Weaviate.Client.Models.Property { Name = "age", DataType = new[] { "int" } },
            },
        };

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
