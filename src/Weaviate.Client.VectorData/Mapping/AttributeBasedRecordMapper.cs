using Microsoft.Extensions.VectorData;
using Weaviate.Client.Models;

namespace Weaviate.Client.VectorData.Mapping;

/// <summary>
/// Maps between a VectorData-attributed record type and Weaviate's storage format.
/// Uses reflection (cached) to discover key, data, and vector properties.
/// </summary>
/// <typeparam name="TRecord">The user's record type.</typeparam>
internal sealed class AttributeBasedRecordMapper<TRecord> : IWeaviateRecordMapper<TRecord>
    where TRecord : class
{
    private readonly RecordPropertyModel _model;

    public AttributeBasedRecordMapper(VectorStoreCollectionDefinition? definition = null)
    {
        _model =
            definition != null
                ? RecordPropertyModel.FromDefinition<TRecord>(definition)
                : RecordPropertyModel.GetOrCreate<TRecord>();
    }

    public (Guid? Key, IDictionary<string, object?> Properties, Vectors? Vectors) MapToWeaviate(
        TRecord record
    )
    {
        // Extract key
        var keyValue = _model.KeyProperty.GetValue(record);
        Guid? key = keyValue switch
        {
            Guid g => g,
            string s => Guid.Parse(s),
            null => null,
            _ => throw new VectorStoreException(
                $"Key property '{_model.KeyProperty.Name}' must be Guid or string, got {keyValue.GetType().Name}."
            ),
        };

        // Extract data properties
        var properties = new Dictionary<string, object?>(_model.DataProperties.Count);
        foreach (var dataProp in _model.DataProperties)
        {
            var value = dataProp.Property.GetValue(record);
            properties[dataProp.StorageName] = value;
        }

        // Extract vectors
        Vectors? vectors = null;
        if (_model.VectorProperties.Count > 0)
        {
            vectors = new Vectors();
            foreach (var vectorProp in _model.VectorProperties)
            {
                var vectorValue = vectorProp.Property.GetValue(record);
                if (vectorValue == null)
                    continue;

                var floatArray = ConvertToFloatArray(vectorValue, vectorProp.Property.Name);
                if (floatArray != null)
                {
                    vectors.Add(new NamedVector(vectorProp.StorageName, (Vector)floatArray));
                }
            }
        }

        return (key, properties, vectors);
    }

    public TRecord MapFromWeaviate(WeaviateObject weaviateObject)
    {
        TRecord record;
        try
        {
            record = Activator.CreateInstance<TRecord>();
        }
        catch (Exception ex)
        {
            throw new VectorStoreException(
                $"Failed to create instance of '{typeof(TRecord).Name}'. Ensure it has a parameterless constructor.",
                ex
            );
        }

        // Set key
        if (weaviateObject.UUID.HasValue)
        {
            var keyType = _model.KeyProperty.PropertyType;
            object keyValue =
                keyType == typeof(string)
                    ? weaviateObject.UUID.Value.ToString()
                    : (object)weaviateObject.UUID.Value;
            _model.KeyProperty.SetValue(record, keyValue);
        }

        // Set data properties
        foreach (var dataProp in _model.DataProperties)
        {
            if (weaviateObject.Properties.TryGetValue(dataProp.StorageName, out var value))
            {
                var converted = ConvertPropertyValue(value, dataProp.Property.PropertyType);
                dataProp.Property.SetValue(record, converted);
            }
        }

        // Set vector properties
        foreach (var vectorProp in _model.VectorProperties)
        {
            if (weaviateObject.Vectors.TryGetValue(vectorProp.StorageName, out var vector))
            {
                float[] floats = vector;
                var convertedVector = ConvertFromFloatArray(
                    floats,
                    vectorProp.Property.PropertyType
                );
                vectorProp.Property.SetValue(record, convertedVector);
            }
        }

        return record;
    }

    public IReadOnlyList<string> GetStoragePropertyNames()
    {
        return _model.DataProperties.Select(p => p.StorageName).ToList();
    }

    public IReadOnlyList<string> GetVectorPropertyNames()
    {
        return _model.VectorProperties.Select(p => p.StorageName).ToList();
    }

    /// <summary>
    /// Converts a vector property value to a float array for Weaviate storage.
    /// </summary>
    internal static float[]? ConvertToFloatArray(object? value, string propertyName)
    {
        return value switch
        {
            null => null,
            float[] arr => arr,
            ReadOnlyMemory<float> rom => rom.ToArray(),
            double[] darr => Array.ConvertAll(darr, d => (float)d),
            _ => throw new VectorStoreException(
                $"Vector property '{propertyName}' has unsupported type '{value.GetType().Name}'. "
                    + "Supported types: float[], ReadOnlyMemory<float>, double[]."
            ),
        };
    }

    /// <summary>
    /// Converts a float array from Weaviate back to the target vector property type.
    /// </summary>
    private static object? ConvertFromFloatArray(float[]? floats, Type targetType)
    {
        if (floats == null)
            return null;

        if (targetType == typeof(float[]))
            return floats;

        if (
            targetType == typeof(ReadOnlyMemory<float>)
            || targetType == typeof(ReadOnlyMemory<float>?)
        )
            return new ReadOnlyMemory<float>(floats);

        if (targetType == typeof(double[]))
            return Array.ConvertAll(floats, f => (double)f);

        throw new VectorStoreException(
            $"Cannot convert float[] to target type '{targetType.Name}'. "
                + "Supported types: float[], ReadOnlyMemory<float>, double[]."
        );
    }

    /// <summary>
    /// Converts a property value from Weaviate's format to the target CLR type.
    /// Weaviate returns JSON-deserialized values which may need type coercion.
    /// </summary>
    internal static object? ConvertPropertyValue(object? value, Type targetType)
    {
        if (value == null)
            return null;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType.IsInstanceOfType(value))
            return value;

        try
        {
            return Convert.ChangeType(value, underlyingType);
        }
        catch (Exception ex)
        {
            throw new VectorStoreException(
                $"Cannot convert value of type '{value.GetType().Name}' to target type '{targetType.Name}'.",
                ex
            );
        }
    }
}
