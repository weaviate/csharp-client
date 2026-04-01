using Microsoft.Extensions.VectorData;
using Weaviate.Client.Models;
using static Weaviate.Client.Models.VectorIndexConfig;

namespace Weaviate.Client.VectorData.Mapping;

/// <summary>
/// Converts a <see cref="VectorStoreCollectionDefinition"/> or attributed record type
/// into a Weaviate <see cref="CollectionCreateParams"/>.
/// </summary>
internal static class VectorDataSchemaBuilder
{
    /// <summary>
    /// Builds a Weaviate collection schema from a VectorData collection definition.
    /// </summary>
    public static CollectionCreateParams BuildSchema(
        string collectionName,
        VectorStoreCollectionDefinition definition
    )
    {
        var properties = new List<Property>();
        var vectorConfigs = new List<VectorConfig>();

        foreach (var prop in definition.Properties)
        {
            switch (prop)
            {
                case VectorStoreKeyProperty:
                    // Key is mapped to Weaviate's built-in UUID, not a property
                    break;

                case VectorStoreDataProperty dataProp:
                    properties.Add(MapDataProperty(dataProp));
                    break;

                case VectorStoreVectorProperty vectorProp:
                    vectorConfigs.Add(MapVectorProperty(vectorProp));
                    break;
            }
        }

        var createParams = new CollectionCreateParams
        {
            Name = collectionName,
            Properties = properties.ToArray(),
        };

        if (vectorConfigs.Count > 0)
        {
            createParams = createParams with { VectorConfig = vectorConfigs.ToArray() };
        }

        return createParams;
    }

    /// <summary>
    /// Builds a Weaviate collection schema from an attributed record type.
    /// </summary>
    public static CollectionCreateParams BuildSchema<TRecord>(string collectionName)
    {
        var model = RecordPropertyModel.GetOrCreate<TRecord>();
        return BuildSchemaFromModel(collectionName, model);
    }

    /// <summary>
    /// Builds a schema from a <see cref="RecordPropertyModel"/>.
    /// </summary>
    internal static CollectionCreateParams BuildSchemaFromModel(
        string collectionName,
        RecordPropertyModel model
    )
    {
        var properties = new List<Property>();
        var vectorConfigs = new List<VectorConfig>();

        foreach (var dataProp in model.DataProperties)
        {
            properties.Add(MapDataPropertyFromReflection(dataProp));
        }

        foreach (var vectorProp in model.VectorProperties)
        {
            vectorConfigs.Add(MapVectorPropertyFromReflection(vectorProp));
        }

        var createParams = new CollectionCreateParams
        {
            Name = collectionName,
            Properties = properties.ToArray(),
        };

        if (vectorConfigs.Count > 0)
        {
            createParams = createParams with { VectorConfig = vectorConfigs.ToArray() };
        }

        return createParams;
    }

    private static Property MapDataProperty(VectorStoreDataProperty prop)
    {
        var storageName = prop.StorageName ?? RecordPropertyModel.Decapitalize(prop.Name);
        var dataType = MapClrTypeToWeaviateDataType(prop.Type ?? typeof(string));
        var property = new Property(storageName, dataType);

        if (prop.IsIndexed)
            property = property with { IndexFilterable = true };

        if (prop.IsFullTextIndexed)
            property = property with { IndexSearchable = true };

        return property;
    }

    private static Property MapDataPropertyFromReflection(DataPropertyInfo prop)
    {
        var dataType = MapClrTypeToWeaviateDataType(prop.Property.PropertyType);
        var property = new Property(prop.StorageName, dataType);

        if (prop.IsIndexed)
            property = property with { IndexFilterable = true };

        if (prop.IsFullTextIndexed)
            property = property with { IndexSearchable = true };

        return property;
    }

    private static VectorConfig MapVectorProperty(VectorStoreVectorProperty prop)
    {
        var storageName = prop.StorageName ?? RecordPropertyModel.Decapitalize(prop.Name);
        return BuildVectorConfig(storageName, prop.DistanceFunction, prop.IndexKind);
    }

    private static VectorConfig MapVectorPropertyFromReflection(VectorPropertyInfo prop)
    {
        return BuildVectorConfig(prop.StorageName, prop.DistanceFunction, prop.IndexKind);
    }

    private static VectorConfig BuildVectorConfig(
        string name,
        string? distanceFunction,
        string? indexKind
    )
    {
        var distance = MapDistanceFunction(distanceFunction);
        var indexConfig = BuildIndexConfig(indexKind, distance);

        return Configure.Vector(name, v => v.SelfProvided(), index: indexConfig);
    }

    /// <summary>
    /// Creates the appropriate VectorIndexConfig with the specified distance metric.
    /// </summary>
    private static VectorIndexConfig BuildIndexConfig(string? indexKind, VectorDistance distance)
    {
        return indexKind switch
        {
            null => new VectorIndex.HNSW { Distance = distance },
            IndexKind.Hnsw => new VectorIndex.HNSW { Distance = distance },
            IndexKind.Flat => new VectorIndex.Flat { Distance = distance },
            _ => throw new NotSupportedException(
                $"Index kind '{indexKind}' is not supported by Weaviate. Supported: Hnsw, Flat."
            ),
        };
    }

    /// <summary>
    /// Maps a CLR type to a Weaviate <see cref="DataType"/>.
    /// </summary>
    internal static DataType MapClrTypeToWeaviateDataType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
            return DataType.Text;
        if (
            underlyingType == typeof(int)
            || underlyingType == typeof(long)
            || underlyingType == typeof(short)
            || underlyingType == typeof(byte)
        )
            return DataType.Int;
        if (
            underlyingType == typeof(float)
            || underlyingType == typeof(double)
            || underlyingType == typeof(decimal)
        )
            return DataType.Number;
        if (underlyingType == typeof(bool))
            return DataType.Bool;
        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
            return DataType.Date;
        if (underlyingType == typeof(Guid))
            return DataType.Uuid;

        // Array types
        if (
            underlyingType == typeof(string[])
            || underlyingType == typeof(List<string>)
            || underlyingType == typeof(IEnumerable<string>)
        )
            return DataType.TextArray;
        if (
            underlyingType == typeof(int[])
            || underlyingType == typeof(List<int>)
            || underlyingType == typeof(long[])
            || underlyingType == typeof(List<long>)
        )
            return DataType.IntArray;
        if (
            underlyingType == typeof(float[])
            || underlyingType == typeof(List<float>)
            || underlyingType == typeof(double[])
            || underlyingType == typeof(List<double>)
        )
            return DataType.NumberArray;
        if (underlyingType == typeof(bool[]) || underlyingType == typeof(List<bool>))
            return DataType.BoolArray;

        throw new NotSupportedException(
            $"CLR type '{type.Name}' cannot be mapped to a Weaviate data type. "
                + "Supported types: string, int, long, float, double, decimal, bool, DateTime, DateTimeOffset, Guid, and their array/list variants."
        );
    }

    /// <summary>
    /// Maps a VectorData distance function to a Weaviate <see cref="VectorDistance"/>.
    /// </summary>
    internal static VectorDistance MapDistanceFunction(string? distanceFunction)
    {
        return distanceFunction switch
        {
            null => VectorDistance.Cosine, // default
            DistanceFunction.CosineSimilarity => VectorDistance.Cosine,
            DistanceFunction.CosineDistance => VectorDistance.Cosine,
            DistanceFunction.DotProductSimilarity => VectorDistance.Dot,
            DistanceFunction.NegativeDotProductSimilarity => VectorDistance.Dot,
            DistanceFunction.EuclideanDistance => VectorDistance.L2Squared,
            DistanceFunction.EuclideanSquaredDistance => VectorDistance.L2Squared,
            DistanceFunction.ManhattanDistance => throw new NotSupportedException(
                "Manhattan distance is not supported by Weaviate. Use CosineSimilarity, DotProductSimilarity, EuclideanDistance, or HammingDistance."
            ),
            DistanceFunction.HammingDistance => VectorDistance.Hamming,
            _ => throw new NotSupportedException(
                $"Distance function '{distanceFunction}' is not supported by Weaviate. "
                    + "Supported: CosineSimilarity, CosineDistance, DotProductSimilarity, NegativeDotProductSimilarity, "
                    + "EuclideanDistance, EuclideanSquaredDistance, ManhattanDistance, HammingDistance."
            ),
        };
    }
}
