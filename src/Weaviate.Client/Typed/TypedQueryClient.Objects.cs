using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedQueryClient<T>
{
    /// <summary>
    /// Fetches objects with group-by aggregation.
    /// </summary>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="filters">Filters to apply to the query.</param>
    /// <param name="sort">Sorting configuration.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed grouped result.</returns>
    public async Task<GroupByResult<T>> FetchObjects(
        GroupByRequest groupBy,
        uint? limit = null,
        Filter? filters = null,
        AutoArray<Sort>? sort = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjects(
            groupBy: groupBy,
            limit: limit,
            filters: filters,
            sort: sort,
            rerank: rerank,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Fetches objects from the collection.
    /// </summary>
    /// <param name="after">Cursor for pagination.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="filters">Filters to apply to the query.</param>
    /// <param name="sort">Sorting configuration.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the fetched objects.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> FetchObjects(
        Guid? after = null,
        uint? limit = null,
        Filter? filters = null,
        AutoArray<Sort>? sort = null,
        Rerank? rerank = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjects(
            limit: limit,
            filters: filters,
            sort: sort,
            rerank: rerank,
            after: after,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Fetches a single object by its ID.
    /// </summary>
    /// <param name="uuid">The UUID of the object to fetch.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed object, or null if not found.</returns>
    public async Task<WeaviateObject<T>?> FetchObjectByID(
        Guid uuid,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjectByID(
            id: uuid,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result?.ToTyped<T>();
    }

    /// <summary>
    /// Fetches multiple objects by their IDs.
    /// </summary>
    /// <param name="uuids">The UUIDs of the objects to fetch.</param>
    /// <param name="limit">Maximum number of objects to return.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="filters">Additional filters to apply.</param>
    /// <param name="sort">Sorting configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed result containing the fetched objects.</returns>
    public async Task<Models.WeaviateResult<WeaviateObject<T>>> FetchObjectsByIDs(
        HashSet<Guid> uuids,
        uint? limit = null,
        Rerank? rerank = null,
        Filter? filters = null,
        AutoArray<Sort>? sort = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _queryClient.FetchObjectsByIDs(
            ids: uuids,
            limit: limit,
            rerank: rerank,
            filters: filters,
            sort: sort,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }
}
