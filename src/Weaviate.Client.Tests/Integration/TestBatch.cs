using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

[Collection("BatchTests")]
public partial class BatchTests : IntegrationTests
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
                Property<string>.New("Name"),
                Property<int>.New("Size"),
                Property<double>.New("Price"),
                Property<bool>.New("IsAvailable"),
                Property<DateTime>.New("AvailableSince"),
            ]
        );

        await client.Config.AddReference(new Reference("ref", client.Name));
        await client.Config.AddReference(new Reference("ref2", client.Name));

        var result = await client.Data.InsertMany(batcher);

        var data = await client.Query.FetchObjects(returnReferences: [new("ref"), new("ref2")]);

        Assert.Equal(expectedObjects, data.Count());
        Assert.Equal(expectedErrors, result.Count(r => r.Error != null));
        Assert.Equal(expectedReferences, data.Count(r => r.References.Any()));
        Assert.Equal(
            expectedReferencedObjects,
            data.Select(d => d.References.Sum(r => r.Value.Count)).Sum()
        );
    }

    [Fact]
    public async Task Test_Batch_ReferenceAddMany()
    {
        // Setup referenced collection ("To")
        var refCollection = await CollectionFactory(
            name: "To",
            vectorConfig: new VectorConfig("default"),
            properties: [Property.Int("number")]
        );
        int numObjects = 10;

        // Insert objects into the referenced collection and get their UUIDs
        var refInsertResult = await refCollection.Data.InsertMany(add =>
            Enumerable.Range(0, numObjects).ToList().ForEach(i => add(new { Number = i }))
        );

        Guid[] uuidsTo = [.. refInsertResult.Select(r => r.ID!.Value)];

        // Setup main collection ("From") with a reference property
        var collection = await CollectionFactory(
            name: "From",
            properties: Property.Int("num"),
            references: new Reference("ref", refCollection.Name),
            vectorConfig: new VectorConfig("default")
        );

        // Insert objects into the main collection and get their UUIDs
        var fromInsertResult = await collection.Data.InsertMany(add =>
            Enumerable.Range(0, numObjects).ToList().ForEach(i => add(new { Num = i }))
        );

        Guid[] uuidsFrom = [.. fromInsertResult.Select(r => r.ID!.Value)];

        // First batch: each "From" object references the "To" object with the same index
        var batchReturn1 = await collection.Data.ReferenceAddMany(
            Enumerable
                .Range(0, numObjects)
                .Select(i => new DataReference(uuidsFrom[i], "ref", uuidsTo[i]))
                .ToArray()
        );
        Assert.False(batchReturn1.HasErrors);

        // Second batch: each "From" object references the first 3 "To" objects
        var batchReturn2 = await collection.Data.ReferenceAddMany(
            Enumerable
                .Range(0, numObjects)
                .Select(i => new DataReference(uuidsFrom[i], "ref", uuidsTo.Take(3).ToArray()))
                .ToArray()
        );
        Assert.False(batchReturn2.HasErrors);

        // Fetch objects with references
        var objects = await collection.Query.FetchObjects(
            returnProperties: ["num"],
            returnReferences: [new QueryReference(linkOn: "ref")]
        );

        foreach (var obj in objects)
        {
            var num = (long)obj.Properties["num"]!;
            var refObjects = obj.References["ref"];

            // The first reference should match the corresponding "To" object's "number"
            Assert.Equal(num, (long)refObjects[0].Properties["number"]!);
            Assert.Contains(refObjects[0].ID!.Value, uuidsTo);

            // There should be 4 references: 1 from the first batch, 3 from the second
            Assert.Equal(4, refObjects.Count);

            // The next 3 references should have "number" properties 0, 1, 2 (order sorted)
            var refs = refObjects
                .Skip(1)
                .Take(3)
                .Select(r => (long)r.Properties["number"]!)
                .OrderBy(x => x)
                .ToList();
            Assert.Equal(new List<long> { 0, 1, 2 }, refs);
        }
    }
}
