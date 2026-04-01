using Microsoft.Extensions.VectorData;
using Weaviate.Client.Models;
using Weaviate.Client.VectorData.Mapping;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.VectorData.Tests.Unit;

public class VectorDataSchemaBuilderTests
{
    private class TestRecord
    {
        [VectorStoreKey]
        public Guid Id { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Name { get; set; } = "";

        [VectorStoreData(IsFullTextIndexed = true)]
        public string Description { get; set; } = "";

        [VectorStoreData]
        public int Count { get; set; }

        [VectorStoreVector(
            384,
            DistanceFunction = DistanceFunction.CosineSimilarity,
            IndexKind = IndexKind.Hnsw
        )]
        public float[] Embedding { get; set; } = [];
    }

    [Fact]
    public void BuildSchema_FromType_CreatesCorrectProperties()
    {
        var schema = VectorDataSchemaBuilder.BuildSchema<TestRecord>("TestCollection");

        Assert.Equal("TestCollection", schema.Name);
        Assert.Equal(3, schema.Properties.Length);

        var nameProp = schema.Properties.First(p => p.Name == "name");
        Assert.Equal(DataType.Text, nameProp.DataType);
        Assert.True(nameProp.IndexFilterable);

        var descProp = schema.Properties.First(p => p.Name == "description");
        Assert.Equal(DataType.Text, descProp.DataType);
        Assert.True(descProp.IndexSearchable);

        var countProp = schema.Properties.First(p => p.Name == "count");
        Assert.Equal(DataType.Int, countProp.DataType);
    }

    [Fact]
    public void BuildSchema_FromType_CreatesVectorConfig()
    {
        var schema = VectorDataSchemaBuilder.BuildSchema<TestRecord>("TestCollection");

        Assert.NotNull(schema.VectorConfig);
        Assert.Single(schema.VectorConfig!);

        var vc = schema.VectorConfig!["embedding"];
        Assert.Equal("embedding", vc.Name);
        Assert.IsType<VectorIndex.HNSW>(vc.VectorIndexConfig);

        var hnsw = (VectorIndex.HNSW)vc.VectorIndexConfig!;
        Assert.Equal(VectorDistance.Cosine, hnsw.Distance);
    }

    [Fact]
    public void MapClrTypeToWeaviateDataType_MapsAllSupportedTypes()
    {
        Assert.Equal(
            DataType.Text,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(string))
        );
        Assert.Equal(
            DataType.Int,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(int))
        );
        Assert.Equal(
            DataType.Int,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(long))
        );
        Assert.Equal(
            DataType.Number,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(float))
        );
        Assert.Equal(
            DataType.Number,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(double))
        );
        Assert.Equal(
            DataType.Bool,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(bool))
        );
        Assert.Equal(
            DataType.Date,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(DateTime))
        );
        Assert.Equal(
            DataType.Uuid,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(Guid))
        );
    }

    [Fact]
    public void MapClrTypeToWeaviateDataType_HandlesNullable()
    {
        Assert.Equal(
            DataType.Int,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(int?))
        );
        Assert.Equal(
            DataType.Bool,
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(bool?))
        );
    }

    [Fact]
    public void MapClrTypeToWeaviateDataType_ThrowsForUnsupported()
    {
        Assert.Throws<NotSupportedException>(() =>
            VectorDataSchemaBuilder.MapClrTypeToWeaviateDataType(typeof(object))
        );
    }

    [Fact]
    public void MapDistanceFunction_MapsCorrectly()
    {
        Assert.Equal(
            VectorDistance.Cosine,
            VectorDataSchemaBuilder.MapDistanceFunction(DistanceFunction.CosineSimilarity)
        );
        Assert.Equal(
            VectorDistance.Cosine,
            VectorDataSchemaBuilder.MapDistanceFunction(DistanceFunction.CosineDistance)
        );
        Assert.Equal(
            VectorDistance.Dot,
            VectorDataSchemaBuilder.MapDistanceFunction(DistanceFunction.DotProductSimilarity)
        );
        Assert.Equal(
            VectorDistance.L2Squared,
            VectorDataSchemaBuilder.MapDistanceFunction(DistanceFunction.EuclideanDistance)
        );
        Assert.Equal(
            VectorDistance.Hamming,
            VectorDataSchemaBuilder.MapDistanceFunction(DistanceFunction.HammingDistance)
        );
    }

    [Fact]
    public void MapDistanceFunction_DefaultsToCosine()
    {
        Assert.Equal(VectorDistance.Cosine, VectorDataSchemaBuilder.MapDistanceFunction(null));
    }

    [Fact]
    public void MapDistanceFunction_ThrowsForManhattan()
    {
        Assert.Throws<NotSupportedException>(() =>
            VectorDataSchemaBuilder.MapDistanceFunction(DistanceFunction.ManhattanDistance)
        );
    }
}
