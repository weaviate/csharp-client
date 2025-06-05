using System.Diagnostics;
using System.Text.Json;
using Weaviate.Client.Models;
using Xunit.v3;

namespace Weaviate.Client.Tests.Integration;

public partial class BasicTests
{
    [Theory]
    [ClassData(typeof(DatasetBatchInsertMany))]
    public async Task InsertMany(string key)
    {
        (
            int expectedObjects,
            int expectedErrors,
            int expectedReferences,
            int expectedReferencedObjects,
            Action<DataClient<dynamic>.InsertDelegate>[] batcher
        ) = DatasetBatchInsertMany.Cases[key];

        var client = await CollectionFactory(
            description: "Testing Batch InsertMany",
            properties:
            [
                Property.For<string>("Name"),
                Property.For<int>("Size"),
                Property.For<double>("Price"),
                Property.For<bool>("IsAvailable"),
                Property.For<DateTime>("AvailableSince"),
            ]
        );

        await client.AddReference(Property.Reference("ref", client.Name));

        var result = await client.Data.InsertMany(batcher);

        var data = await client.Query.List(references: [new("ref")]);

        Assert.Equal(expectedObjects, data.Count());
        Assert.Equal(expectedErrors, result.Count(r => r.Error != null));
        Assert.Equal(expectedReferences, data.Count(r => r.References.Any()));
        Assert.Equal(
            expectedReferencedObjects,
            data.Select(d => d.References.Sum(r => r.Value.Count)).Sum()
        );
    }
}
