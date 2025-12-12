using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class GenerateClient
{
    #region Objects
    /// <summary>
    /// Fetch objects with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="sort">Sort configuration</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative group-by result</returns>
    public async Task<GenerativeGroupByResult> FetchObjects(
        Models.GroupByRequest groupBy,
        Guid? after = null,
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _client.GrpcClient.FetchObjects(
            _collectionName,
            after: after,
            limit: limit,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            filters: filters,
            sort: sort,
            groupBy: groupBy,
            tenant: _collectionClient.Tenant,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Fetch objects with generative AI capabilities.
    /// </summary>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="sort">Sort configuration</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult?> FetchObjects(
        Guid? after = null,
        uint? limit = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _client.GrpcClient.FetchObjects(
            _collectionName,
            after: after,
            limit: limit,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            filters: filters,
            sort: sort,
            tenant: _collectionClient.Tenant,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Fetch a single object by ID with generative AI capabilities.
    /// </summary>
    /// <param name="id">Object ID</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult?> FetchObjectByID(
        Guid id,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _client.GrpcClient.FetchObjects(
            _collectionName,
            returnProperties: returnProperties,
            filters: Filter.ID.IsEqual(id),
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            tenant: _collectionClient.Tenant,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
        return result;
    }

    /// <summary>
    /// Fetch multiple objects by IDs with generative AI capabilities.
    /// </summary>
    /// <param name="ids">Set of object IDs</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="sort">Sort configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider to enrich prompts that don't have a provider set. If the prompt already has a provider, it will not be overridden.</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Generative result</returns>
    public async Task<GenerativeWeaviateResult> FetchObjectsByIDs(
        HashSet<Guid> ids,
        uint? limit = null,
        Rerank? rerank = null,
        Filter? filters = null,
        OneOrManyOf<Sort>? sort = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        OneOrManyOf<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        if (ids == null)
            throw new ArgumentNullException(nameof(ids));

        if (ids.Count == 0)
            return GenerativeWeaviateResult.Empty;

        Filter idFilter =
            ids.Count == 1
                ? Filter.ID.IsEqual(ids.First())
                : Filter.AnyOf([.. ids.Select(id => Filter.ID.IsEqual(id))]);

        if (filters is not null)
            idFilter = filters & idFilter;

        return await _client.GrpcClient.FetchObjects(
            _collectionName,
            limit: limit,
            filters: idFilter,
            sort: sort,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
    }
    #endregion
}
