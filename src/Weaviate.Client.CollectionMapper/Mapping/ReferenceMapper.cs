using System.Reflection;
using Weaviate.Client.CollectionMapper.Attributes;
using Weaviate.Client.CollectionMapper.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Mapping;

/// <summary>
/// Maps reference properties to and from Weaviate's References dictionary.
/// Handles automatic extraction and injection of cross-references.
/// </summary>
internal static class ReferenceMapper
{
    /// <summary>
    /// Extracts references from an object's properties and returns a References dictionary.
    /// Supports three reference types:
    /// - Single reference (T?)
    /// - ID-only reference (Guid?)
    /// - Multi-reference (List&lt;T&gt;)
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="obj">The object to extract references from.</param>
    /// <returns>A dictionary of references, or null if no references found.</returns>
    public static IDictionary<string, IList<WeaviateObject>>? ExtractReferences<T>(T obj)
        where T : class
    {
        var type = typeof(T);
        var refProps = type.GetProperties()
            .Where(p => p.GetCustomAttribute<ReferenceAttribute>() != null)
            .ToList();

        if (refProps.Count == 0)
            return null;

        var references = new Dictionary<string, IList<WeaviateObject>>();

        foreach (var prop in refProps)
        {
            var refName = PropertyHelper.ToCamelCase(prop.Name);
            var value = prop.GetValue(obj);

            if (value == null)
                continue;

            var refAttr = prop.GetCustomAttribute<ReferenceAttribute>()!;

            // Handle Guid? (ID-only reference)
            if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
            {
                var id = value is Guid guid ? guid : (Guid?)value;
                if (id.HasValue)
                {
                    references[refName] = new List<WeaviateObject>
                    {
                        new WeaviateObject { ID = id.Value, Collection = refAttr.TargetCollection },
                    };
                }
            }
            // Handle List<Guid> (multi-ID reference)
            else if (IsGenericList(prop.PropertyType, typeof(Guid)))
            {
                var ids = value as IEnumerable<Guid>;
                if (ids != null)
                {
                    var refList = ids.Select(id => new WeaviateObject
                        {
                            ID = id,
                            Collection = refAttr.TargetCollection,
                        })
                        .ToList();

                    if (refList.Count > 0)
                    {
                        references[refName] = refList;
                    }
                }
            }
            // Handle single object reference (T?)
            else if (prop.PropertyType.IsClass && !IsGenericList(prop.PropertyType))
            {
                // For single object references, we need the object to have an ID property
                var idProp = prop.PropertyType.GetProperty(
                    "Id",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                );
                if (
                    idProp != null
                    && (idProp.PropertyType == typeof(Guid) || idProp.PropertyType == typeof(Guid?))
                )
                {
                    var id = idProp.GetValue(value) as Guid?;
                    if (id.HasValue)
                    {
                        references[refName] = new List<WeaviateObject>
                        {
                            new WeaviateObject
                            {
                                ID = id.Value,
                                Collection = refAttr.TargetCollection,
                            },
                        };
                    }
                }
            }
            // Handle List<T> (multi-object reference)
            else if (IsGenericList(prop.PropertyType))
            {
                var elementType = prop.PropertyType.GetGenericArguments()[0];
                var idProp = elementType.GetProperty(
                    "Id",
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase
                );

                if (
                    idProp != null
                    && (idProp.PropertyType == typeof(Guid) || idProp.PropertyType == typeof(Guid?))
                )
                {
                    var list = value as System.Collections.IEnumerable;
                    if (list != null)
                    {
                        var refList = new List<WeaviateObject>();
                        foreach (var item in list)
                        {
                            var id = idProp.GetValue(item) as Guid?;
                            if (id.HasValue)
                            {
                                refList.Add(
                                    new WeaviateObject
                                    {
                                        ID = id.Value,
                                        Collection = refAttr.TargetCollection,
                                    }
                                );
                            }
                        }

                        if (refList.Count > 0)
                        {
                            references[refName] = refList;
                        }
                    }
                }
            }
        }

        return references.Count > 0 ? references : null;
    }

    /// <summary>
    /// Injects references from a References dictionary into an object's properties.
    /// Note: This only populates ID fields. Full object hydration requires additional queries.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="obj">The object to inject references into.</param>
    /// <param name="references">The References dictionary from Weaviate.</param>
    public static void InjectReferences<T>(
        T obj,
        IDictionary<string, IList<WeaviateObject>>? references
    )
        where T : class
    {
        if (references == null || references.Count == 0)
            return;

        var type = typeof(T);
        var refProps = type.GetProperties()
            .Where(p => p.GetCustomAttribute<ReferenceAttribute>() != null && p.CanWrite)
            .ToList();

        foreach (var prop in refProps)
        {
            var refName = PropertyHelper.ToCamelCase(prop.Name);

            if (!references.TryGetValue(refName, out var refList) || refList.Count == 0)
                continue;

            // Handle Guid? (ID-only reference)
            if (prop.PropertyType == typeof(Guid) || prop.PropertyType == typeof(Guid?))
            {
                var firstRef = refList.FirstOrDefault();
                if (firstRef?.ID != null)
                {
                    prop.SetValue(obj, firstRef.ID);
                }
            }
            // Handle List<Guid> (multi-ID reference)
            else if (IsGenericList(prop.PropertyType, typeof(Guid)))
            {
                var ids = refList.Where(r => r.ID.HasValue).Select(r => r.ID!.Value).ToList();
                prop.SetValue(obj, ids);
            }
            // Handle single object reference (T?)
            else if (prop.PropertyType.IsClass && !IsGenericList(prop.PropertyType))
            {
                var firstRef = refList.FirstOrDefault();
                if (firstRef != null)
                {
                    // Try to create instance and set ID
                    try
                    {
                        var instance = Activator.CreateInstance(prop.PropertyType);
                        if (instance != null)
                        {
                            var idProp = prop.PropertyType.GetProperty(
                                "Id",
                                BindingFlags.Public
                                    | BindingFlags.Instance
                                    | BindingFlags.IgnoreCase
                            );
                            if (idProp != null && idProp.CanWrite && firstRef.ID.HasValue)
                            {
                                idProp.SetValue(instance, firstRef.ID.Value);
                            }

                            // Note: Full object hydration from Properties would require ObjectHelper which is internal
                            // For now, we only populate the ID. Full reference expansion would need to be done
                            // via explicit queries using WithReferences() in the query builder.

                            prop.SetValue(obj, instance);
                        }
                    }
                    catch
                    {
                        // If we can't create instance, just skip
                    }
                }
            }
            // Handle List<T> (multi-object reference)
            else if (IsGenericList(prop.PropertyType))
            {
                var elementType = prop.PropertyType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = Activator.CreateInstance(listType) as System.Collections.IList;

                if (list != null)
                {
                    foreach (var refObj in refList)
                    {
                        try
                        {
                            var instance = Activator.CreateInstance(elementType);
                            if (instance != null)
                            {
                                var idProp = elementType.GetProperty(
                                    "Id",
                                    BindingFlags.Public
                                        | BindingFlags.Instance
                                        | BindingFlags.IgnoreCase
                                );
                                if (idProp != null && idProp.CanWrite && refObj.ID.HasValue)
                                {
                                    idProp.SetValue(instance, refObj.ID.Value);
                                }

                                // Note: Full object hydration from Properties would require ObjectHelper which is internal
                                // For now, we only populate the ID. Full reference expansion would need to be done
                                // via explicit queries using WithReferences() in the query builder.

                                list.Add(instance);
                            }
                        }
                        catch
                        {
                            // If we can't create instance, just skip
                        }
                    }

                    if (list.Count > 0)
                    {
                        prop.SetValue(obj, list);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Gets the names of all reference properties on a type (in camelCase).
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <returns>List of reference property names in camelCase.</returns>
    public static List<string> GetReferencePropertyNames<T>()
        where T : class
    {
        var type = typeof(T);
        return type.GetProperties()
            .Where(p => p.GetCustomAttribute<ReferenceAttribute>() != null)
            .Select(p => PropertyHelper.ToCamelCase(p.Name))
            .ToList();
    }

    /// <summary>
    /// Checks if a type is a generic List.
    /// </summary>
    private static bool IsGenericList(Type type, Type? elementType = null)
    {
        if (!type.IsGenericType)
            return false;

        var genericDef = type.GetGenericTypeDefinition();
        if (
            genericDef != typeof(List<>)
            && genericDef != typeof(IList<>)
            && genericDef != typeof(IEnumerable<>)
            && genericDef != typeof(ICollection<>)
        )
            return false;

        if (elementType != null)
        {
            var args = type.GetGenericArguments();
            return args.Length == 1 && args[0] == elementType;
        }

        return true;
    }
}
