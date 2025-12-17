using System.Linq.Expressions;
using Weaviate.Client;
using Weaviate.Client.CollectionMapper.Mapping;
using Weaviate.Client.CollectionMapper.Query;
using Weaviate.Client.Models;

namespace Weaviate.Client.CollectionMapper.Extensions;

/// <summary>
/// Extension methods for DataClient to enable ORM-style data operations with automatic vector and reference handling.
/// </summary>
public static class DataClientExtensions
{
    /// <summary>
    /// Inserts a single object into the collection with automatic vector and reference extraction.
    /// Vectors and references are automatically extracted from properties decorated with [Vector] and [Reference] attributes.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="dataClient">The DataClient instance.</param>
    /// <param name="obj">The object to insert.</param>
    /// <param name="id">Optional ID for the object. If not provided, Weaviate generates one.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ID of the inserted object.</returns>
    public static async Task<Guid> Insert<T>(
        this DataClient dataClient,
        T obj,
        Guid? id = null,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        // Extract vectors
        var vectors = VectorMapper.ExtractVectors(obj);

        // Extract references and convert to ObjectReference format
        var references = ReferenceMapper.ExtractReferences(obj);
        AutoArray<ObjectReference>? refParam = null;
        if (references != null && references.Count > 0)
        {
            var refList = new List<ObjectReference>();
            foreach (var (refName, refObjects) in references)
            {
                foreach (var refObj in refObjects)
                {
                    if (refObj.UUID.HasValue)
                    {
                        refList.Add(new ObjectReference(refName, refObj.UUID.Value));
                    }
                }
            }
            if (refList.Count > 0)
            {
                refParam = refList.ToArray();
            }
        }

        // Insert using existing DataClient - it will handle object serialization
        var result = await dataClient.Insert(
            properties: obj,
            uuid: id,
            vectors: vectors,
            references: refParam,
            cancellationToken: cancellationToken
        );

        return result;
    }

    /// <summary>
    /// Inserts multiple objects into the collection with automatic vector and reference extraction.
    /// This is more efficient than calling Insert multiple times as it uses batch operations.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="dataClient">The DataClient instance.</param>
    /// <param name="objects">The objects to insert.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch insert response with IDs and any errors.</returns>
    public static async Task<BatchInsertResponse> InsertMany<T>(
        this DataClient dataClient,
        IEnumerable<T> objects,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(objects);

        // Create batch insert requests with vectors extracted
        var requests = objects.Select(obj =>
        {
            var vectors = VectorMapper.ExtractVectors(obj);
            return (obj as object, vectors!);
        });

        // Insert using existing DataClient batch operation
        return await dataClient.InsertMany(requests, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Replaces an entire object with a new version (upsert behavior).
    /// All properties, vectors, and references are replaced.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="dataClient">The DataClient instance.</param>
    /// <param name="obj">The object to replace.</param>
    /// <param name="id">The ID of the object.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public static async Task Replace<T>(
        this DataClient dataClient,
        T obj,
        Guid id,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        // Extract vectors
        var vectors = VectorMapper.ExtractVectors(obj);

        // Extract references and convert to ObjectReference format
        var references = ReferenceMapper.ExtractReferences(obj);
        IEnumerable<ObjectReference>? refParam = null;
        if (references != null && references.Count > 0)
        {
            var refList = new List<ObjectReference>();
            foreach (var (refName, refObjects) in references)
            {
                foreach (var refObj in refObjects)
                {
                    if (refObj.UUID.HasValue)
                    {
                        refList.Add(new ObjectReference(refName, refObj.UUID.Value));
                    }
                }
            }
            if (refList.Count > 0)
            {
                refParam = refList;
            }
        }

        // Replace using existing DataClient
        await dataClient.Replace(
            uuid: id,
            properties: obj,
            vectors: vectors,
            references: refParam,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Updates specific properties of an object without replacing the entire object.
    /// Only the provided properties, vectors, and references are updated.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="dataClient">The DataClient instance.</param>
    /// <param name="obj">The object with properties to update.</param>
    /// <param name="id">The ID of the object to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public static async Task Update<T>(
        this DataClient dataClient,
        T obj,
        Guid id,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(obj);

        // Extract vectors
        var vectors = VectorMapper.ExtractVectors(obj);

        // Extract references and convert to ObjectReference format
        var references = ReferenceMapper.ExtractReferences(obj);
        IEnumerable<ObjectReference>? refParam = null;
        if (references != null && references.Count > 0)
        {
            var refList = new List<ObjectReference>();
            foreach (var (refName, refObjects) in references)
            {
                foreach (var refObj in refObjects)
                {
                    if (refObj.UUID.HasValue)
                    {
                        refList.Add(new ObjectReference(refName, refObj.UUID.Value));
                    }
                }
            }
            if (refList.Count > 0)
            {
                refParam = refList;
            }
        }

        // Update using existing DataClient
        await dataClient.Update(
            uuid: id,
            properties: obj,
            vectors: vectors,
            references: refParam,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Deletes a single object by its ID.
    /// </summary>
    /// <param name="dataClient">The DataClient instance.</param>
    /// <param name="id">The ID of the object to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public static async Task DeleteByID(
        this DataClient dataClient,
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        await dataClient.DeleteByID(id, cancellationToken);
    }

    /// <summary>
    /// Deletes multiple objects matching a filter expression.
    /// Uses type-safe LINQ-style filtering to identify objects to delete.
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="dataClient">The DataClient instance.</param>
    /// <param name="where">Filter expression to identify objects to delete.</param>
    /// <param name="dryRun">If true, returns what would be deleted without actually deleting.</param>
    /// <param name="verbose">If true, includes detailed information in the response.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>DeleteManyResult with information about deleted objects.</returns>
    public static async Task<DeleteManyResult> DeleteMany<T>(
        this DataClient dataClient,
        Expression<Func<T, bool>> where,
        bool dryRun = false,
        bool verbose = false,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        ArgumentNullException.ThrowIfNull(where);

        // Convert expression to Filter using the ORM expression converter
        var filter = ExpressionToFilterConverter.Convert(where);

        // Delete using existing DataClient
        return await dataClient.DeleteMany(
            where: filter,
            dryRun: dryRun,
            verbose: verbose,
            cancellationToken: cancellationToken
        );
    }
}
