using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;

namespace Weaviate.Client.Tests.Integration;

[Collection("TestQueries")]
public class TestQueries : IntegrationTests
{
    [Theory()]
    [InlineData("testText")]
    [InlineData("testInt")]
    [InlineData("testNumber")]
    [InlineData("testDate")]
    public async Task Test_Sorting(string propertyName)
    {
        // Arrange
        var collection = await this.CollectionFactory<TestProperties>(
            properties: Property.FromClass<TestProperties>(),
            vectorConfig: new VectorConfig("default", new Vectorizer.SelfProvided())
        );

        var testData = new[]
        {
            new TestProperties
            {
                TestText = "Alice",
                TestInt = 1,
                TestNumber = 1.1,
                TestDate = DateTime.Parse("2023-01-01"),
            },
            new TestProperties
            {
                TestText = "Bob",
                TestInt = 2,
                TestNumber = 2.2,
                TestDate = DateTime.Parse("2023-01-02"),
            },
            new TestProperties
            {
                TestText = "Charlie",
                TestInt = 3,
                TestNumber = 3.3,
                TestDate = DateTime.Parse("2023-01-03"),
            },
        };

        await collection.Data.InsertMany(BatchInsertRequest.Create(testData));

        // Act
        var dataDesc = await collection.Query.FetchObjects(
            sort: Sort.ByProperty(propertyName).Descending()
        );
        var dataAsc = await collection.Query.FetchObjects(
            sort: Sort.ByProperty(propertyName).Ascending()
        );

        var namesDesc = dataDesc.Select(d => d.Properties["testText"]);
        var namesAsc = dataAsc.Select(d => d.Properties["testText"]);

        // Assert
        Assert.Equal(new List<string> { "Charlie", "Bob", "Alice" }, namesDesc);
        Assert.Equal(new List<string> { "Alice", "Bob", "Charlie" }, namesAsc);
    }
}
