using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.VectorData;

namespace Weaviate.Client.VectorData.Mapping;

/// <summary>
/// Cached metadata about a record type's VectorData-attributed properties.
/// </summary>
internal sealed class RecordPropertyModel
{
    private static readonly ConcurrentDictionary<Type, RecordPropertyModel> s_cache = new();

    public PropertyInfo KeyProperty { get; }
    public string KeyStorageName { get; }
    public IReadOnlyList<DataPropertyInfo> DataProperties { get; }
    public IReadOnlyList<VectorPropertyInfo> VectorProperties { get; }

    private RecordPropertyModel(
        PropertyInfo keyProperty,
        string keyStorageName,
        IReadOnlyList<DataPropertyInfo> dataProperties,
        IReadOnlyList<VectorPropertyInfo> vectorProperties
    )
    {
        KeyProperty = keyProperty;
        KeyStorageName = keyStorageName;
        DataProperties = dataProperties;
        VectorProperties = vectorProperties;
    }

    /// <summary>
    /// Gets or creates a cached property model for the given record type.
    /// </summary>
    public static RecordPropertyModel GetOrCreate<TRecord>()
    {
        return s_cache.GetOrAdd(typeof(TRecord), static type => Build(type));
    }

    /// <summary>
    /// Creates a property model from a <see cref="VectorStoreCollectionDefinition"/>.
    /// </summary>
    public static RecordPropertyModel FromDefinition<TRecord>(
        VectorStoreCollectionDefinition definition
    )
    {
        var type = typeof(TRecord);
        var dataProps = new List<DataPropertyInfo>();
        var vectorProps = new List<VectorPropertyInfo>();
        PropertyInfo? keyProp = null;
        string? keyStorageName = null;

        foreach (var prop in definition.Properties)
        {
            var clrProp = type.GetProperty(prop.Name);

            switch (prop)
            {
                case VectorStoreKeyProperty keyDef:
                    keyProp =
                        clrProp
                        ?? throw new InvalidOperationException(
                            $"Key property '{prop.Name}' not found on type '{type.Name}'."
                        );
                    keyStorageName = keyDef.StorageName ?? prop.Name;
                    break;

                case VectorStoreDataProperty dataDef:
                    if (clrProp == null)
                        throw new InvalidOperationException(
                            $"Data property '{prop.Name}' not found on type '{type.Name}'."
                        );
                    dataProps.Add(
                        new DataPropertyInfo(
                            clrProp,
                            dataDef.StorageName ?? Decapitalize(prop.Name),
                            dataDef.IsIndexed,
                            dataDef.IsFullTextIndexed
                        )
                    );
                    break;

                case VectorStoreVectorProperty vectorDef:
                    if (clrProp == null)
                        throw new InvalidOperationException(
                            $"Vector property '{prop.Name}' not found on type '{type.Name}'."
                        );
                    vectorProps.Add(
                        new VectorPropertyInfo(
                            clrProp,
                            vectorDef.StorageName ?? Decapitalize(prop.Name),
                            vectorDef.Dimensions,
                            vectorDef.DistanceFunction,
                            vectorDef.IndexKind
                        )
                    );
                    break;
            }
        }

        if (keyProp == null)
            throw new InvalidOperationException(
                $"No key property found in definition for type '{type.Name}'."
            );

        return new RecordPropertyModel(keyProp, keyStorageName!, dataProps, vectorProps);
    }

    private static RecordPropertyModel Build(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        PropertyInfo? keyProperty = null;
        string? keyStorageName = null;
        var dataProperties = new List<DataPropertyInfo>();
        var vectorProperties = new List<VectorPropertyInfo>();

        foreach (var prop in properties)
        {
            var keyAttr = prop.GetCustomAttribute<VectorStoreKeyAttribute>();
            if (keyAttr != null)
            {
                if (keyProperty != null)
                    throw new InvalidOperationException(
                        $"Type '{type.Name}' has multiple properties with [VectorStoreKey]."
                    );
                keyProperty = prop;
                keyStorageName = keyAttr.StorageName ?? prop.Name;
                continue;
            }

            var dataAttr = prop.GetCustomAttribute<VectorStoreDataAttribute>();
            if (dataAttr != null)
            {
                dataProperties.Add(
                    new DataPropertyInfo(
                        prop,
                        dataAttr.StorageName ?? Decapitalize(prop.Name),
                        dataAttr.IsIndexed,
                        dataAttr.IsFullTextIndexed
                    )
                );
                continue;
            }

            var vectorAttr = prop.GetCustomAttribute<VectorStoreVectorAttribute>();
            if (vectorAttr != null)
            {
                vectorProperties.Add(
                    new VectorPropertyInfo(
                        prop,
                        vectorAttr.StorageName ?? Decapitalize(prop.Name),
                        vectorAttr.Dimensions,
                        vectorAttr.DistanceFunction,
                        vectorAttr.IndexKind
                    )
                );
            }
        }

        if (keyProperty == null)
            throw new InvalidOperationException(
                $"Type '{type.Name}' must have a property decorated with [VectorStoreKey]."
            );

        return new RecordPropertyModel(
            keyProperty,
            keyStorageName!,
            dataProperties,
            vectorProperties
        );
    }

    internal static string Decapitalize(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}

/// <summary>
/// Metadata about a data property.
/// </summary>
internal sealed record DataPropertyInfo(
    PropertyInfo Property,
    string StorageName,
    bool IsIndexed,
    bool IsFullTextIndexed
);

/// <summary>
/// Metadata about a vector property.
/// </summary>
internal sealed record VectorPropertyInfo(
    PropertyInfo Property,
    string StorageName,
    int Dimensions,
    string? DistanceFunction,
    string? IndexKind
);
