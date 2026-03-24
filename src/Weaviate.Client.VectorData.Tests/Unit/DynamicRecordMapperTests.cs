using Weaviate.Client.Models;
using Weaviate.Client.VectorData.Mapping;

namespace Weaviate.Client.VectorData.Tests.Unit;

public class DynamicRecordMapperTests
{
    [Fact]
    public void MapToWeaviate_ExtractsGuidKey()
    {
        var mapper = new DynamicRecordMapper();
        var guid = Guid.NewGuid();

        var record = new Dictionary<string, object?> { ["Key"] = guid, ["title"] = "Hello" };

        var (key, properties, _) = mapper.MapToWeaviate(record);

        Assert.Equal(guid, key);
        Assert.Equal("Hello", properties["title"]);
        Assert.False(properties.ContainsKey("Key"));
    }

    [Fact]
    public void MapToWeaviate_ExtractsStringKey()
    {
        var mapper = new DynamicRecordMapper();
        var guid = Guid.NewGuid();

        var record = new Dictionary<string, object?>
        {
            ["Key"] = guid.ToString(),
            ["title"] = "Hello",
        };

        var (key, _, _) = mapper.MapToWeaviate(record);

        Assert.Equal(guid, key);
    }

    [Fact]
    public void MapToWeaviate_SeparatesVectorsFromProperties()
    {
        var mapper = new DynamicRecordMapper();
        var floats = new float[] { 1.0f, 2.0f, 3.0f };

        var record = new Dictionary<string, object?>
        {
            ["Key"] = Guid.NewGuid(),
            ["title"] = "Hello",
            ["embedding"] = floats,
        };

        var (_, properties, vectors) = mapper.MapToWeaviate(record);

        Assert.False(properties.ContainsKey("embedding"));
        Assert.Equal("Hello", properties["title"]);
        Assert.NotNull(vectors);
        Assert.Single(vectors!);
    }

    [Fact]
    public void MapToWeaviate_NullKey_ReturnsNull()
    {
        var mapper = new DynamicRecordMapper();

        var record = new Dictionary<string, object?> { ["title"] = "Hello" };

        var (key, _, _) = mapper.MapToWeaviate(record);

        Assert.Null(key);
    }

    [Fact]
    public void MapFromWeaviate_SetsKeyAndProperties()
    {
        var mapper = new DynamicRecordMapper();
        var guid = Guid.NewGuid();
        var floats = new float[] { 1.0f, 2.0f };

        var obj = new WeaviateObject
        {
            UUID = guid,
            Properties = new Dictionary<string, object?> { ["title"] = "Hello" },
            Vectors = new Vectors(new NamedVector("embedding", (Vector)floats)),
        };

        var record = mapper.MapFromWeaviate(obj);

        Assert.Equal(guid.ToString(), record["Key"]);
        Assert.Equal("Hello", record["title"]);
        Assert.IsType<float[]>(record["embedding"]);
    }
}
