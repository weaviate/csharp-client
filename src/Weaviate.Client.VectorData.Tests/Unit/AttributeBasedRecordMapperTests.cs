using Microsoft.Extensions.VectorData;
using Weaviate.Client.Models;
using Weaviate.Client.VectorData.Mapping;

namespace Weaviate.Client.VectorData.Tests.Unit;

public class AttributeBasedRecordMapperTests
{
    private class TestRecord
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Name { get; set; } = "";

        [VectorStoreData]
        public int Age { get; set; }

        [VectorStoreVector(4, DistanceFunction = DistanceFunction.CosineSimilarity)]
        public float[] Embedding { get; set; } = [];
    }

    private class StringKeyRecord
    {
        [VectorStoreKey]
        public string Id { get; set; } = "";

        [VectorStoreData]
        public string Title { get; set; } = "";
    }

    [Fact]
    public void MapToWeaviate_ExtractsKeyPropertiesAndVectors()
    {
        var mapper = new AttributeBasedRecordMapper<TestRecord>();
        var id = Guid.NewGuid();
        var embedding = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };

        var record = new TestRecord
        {
            Id = id,
            Name = "Alice",
            Age = 30,
            Embedding = embedding,
        };

        var (key, properties, vectors) = mapper.MapToWeaviate(record);

        Assert.Equal(id, key);
        Assert.Equal("Alice", properties["name"]);
        Assert.Equal(30, properties["age"]);
        Assert.NotNull(vectors);
        Assert.Single(vectors);
    }

    [Fact]
    public void MapFromWeaviate_SetsAllProperties()
    {
        var mapper = new AttributeBasedRecordMapper<TestRecord>();
        var id = Guid.NewGuid();
        var floats = new float[] { 1.0f, 2.0f, 3.0f, 4.0f };

        var obj = new WeaviateObject
        {
            UUID = id,
            Properties = new Dictionary<string, object?> { ["name"] = "Bob", ["age"] = 25 },
            Vectors = new Vectors(new NamedVector("embedding", (Vector)floats)),
        };

        var record = mapper.MapFromWeaviate(obj);

        Assert.Equal(id, record.Id);
        Assert.Equal("Bob", record.Name);
        Assert.Equal(25, record.Age);
        Assert.Equal(floats, record.Embedding);
    }

    [Fact]
    public void MapToWeaviate_StringKey_ParsesAsGuid()
    {
        var mapper = new AttributeBasedRecordMapper<StringKeyRecord>();
        var guid = Guid.NewGuid();

        var record = new StringKeyRecord { Id = guid.ToString(), Title = "Test" };

        var (key, properties, _) = mapper.MapToWeaviate(record);

        Assert.Equal(guid, key);
        Assert.Equal("Test", properties["title"]);
    }

    [Fact]
    public void MapFromWeaviate_StringKey_ReturnsGuidString()
    {
        var mapper = new AttributeBasedRecordMapper<StringKeyRecord>();
        var guid = Guid.NewGuid();

        var obj = new WeaviateObject
        {
            UUID = guid,
            Properties = new Dictionary<string, object?> { ["title"] = "Test" },
        };

        var record = mapper.MapFromWeaviate(obj);

        Assert.Equal(guid.ToString(), record.Id);
        Assert.Equal("Test", record.Title);
    }

    [Fact]
    public void GetStoragePropertyNames_ReturnsDecapitalizedNames()
    {
        var mapper = new AttributeBasedRecordMapper<TestRecord>();
        var names = mapper.GetStoragePropertyNames();

        Assert.Contains("name", names);
        Assert.Contains("age", names);
        Assert.Equal(2, names.Count);
    }

    [Fact]
    public void GetVectorPropertyNames_ReturnsDecapitalizedNames()
    {
        var mapper = new AttributeBasedRecordMapper<TestRecord>();
        var names = mapper.GetVectorPropertyNames();

        Assert.Contains("embedding", names);
        Assert.Single(names);
    }
}
