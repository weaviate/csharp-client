using Weaviate.Client.Internal;
using Weaviate.Client.Models;

namespace Weaviate.Client;

/// <summary>
/// The generate client class
/// </summary>
public partial class GenerateClient
{
    /// <summary>
    /// Search near vector with generative AI capabilities.
    /// </summary>
    /// <param name="vectors">The vector or named vectors to search near.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Certainty threshold for the search: the minimum similarity a result must reach. If not specified, no threshold is applied.</param>
    /// <param name="distance">Distance threshold for the search: the maximum distance a result may have from the query vector. If not specified, no threshold is applied.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task<GenerativeWeaviateResult> NearVector(
        VectorSearchInput vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _client.GrpcClient.SearchNearVector(
            _collectionClient.Name,
            vectors,
            distance: distance,
            certainty: certainty,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            limit: limit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            boost: boost,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Search near vector with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="vectors">The vector or named vectors to search near.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Certainty threshold for the search: the minimum similarity a result must reach. If not specified, no threshold is applied.</param>
    /// <param name="distance">Distance threshold for the search: the maximum distance a result may have from the query vector. If not specified, no threshold is applied.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task<GenerativeGroupByResult> NearVector(
        VectorSearchInput vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _client.GrpcClient.SearchNearVector(
            _collectionClient.Name,
            vectors,
            groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            offset: offset,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            limit: limit,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            boost: boost,
            singlePrompt: EnrichPrompt(singlePrompt, provider) as SinglePrompt,
            groupedTask: EnrichPrompt(groupedTask, provider) as GroupedTask,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    /// <summary>
    /// Search near vector with generative AI capabilities using lambda builder.
    /// </summary>
    /// <param name="vectors">Lambda builder for the vector input to search near.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Certainty threshold for the search: the minimum similarity a result must reach. If not specified, no threshold is applied.</param>
    /// <param name="distance">Distance threshold for the search: the maximum distance a result may have from the query vector. If not specified, no threshold is applied.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task<GenerativeWeaviateResult> NearVector(
        VectorSearchInput.FactoryFn vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            filters,
            certainty,
            distance,
            diversitySelection,
            autoLimit,
            limit,
            offset,
            rerank,
            boost,
            singlePrompt,
            groupedTask,
            provider,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Search near vector with generative AI capabilities and grouping using lambda builder.
    /// </summary>
    /// <param name="vectors">Lambda builder for the vector input to search near.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="certainty">Certainty threshold for the search: the minimum similarity a result must reach. If not specified, no threshold is applied.</param>
    /// <param name="distance">Distance threshold for the search: the maximum distance a result may have from the query vector. If not specified, no threshold is applied.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    public async Task<GenerativeGroupByResult> NearVector(
        VectorSearchInput.FactoryFn vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors(new VectorSearchInput.Builder()),
            groupBy,
            filters,
            certainty,
            distance,
            diversitySelection,
            autoLimit,
            limit,
            offset,
            rerank,
            boost,
            singlePrompt,
            groupedTask,
            provider,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Search near vector with generative AI capabilities using a NearVectorInput record.
    /// </summary>
    /// <param name="query">Near-vector input containing vector, certainty, and distance.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Generative search results.</returns>
    public async Task<GenerativeWeaviateResult> NearVector(
        NearVectorInput query,
        Filter? filters = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors: query.Vector,
            filters: filters,
            certainty: query.Certainty,
            distance: query.Distance,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            boost: boost,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Search near vector with generative AI capabilities and grouping using a NearVectorInput record.
    /// </summary>
    /// <param name="query">Near-vector input containing vector, certainty, and distance.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Generative grouped search results.</returns>
    public async Task<GenerativeGroupByResult> NearVector(
        NearVectorInput query,
        GroupByRequest groupBy,
        Filter? filters = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors: query.Vector,
            groupBy: groupBy,
            filters: filters,
            certainty: query.Certainty,
            distance: query.Distance,
            diversitySelection: diversitySelection,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            boost: boost,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Search near vector with generative AI capabilities using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="vectors"></param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Generative search results.</returns>
    public async Task<GenerativeWeaviateResult> NearVector(
        NearVectorInput.FactoryFn vectors,
        Filter? filters = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            filters,
            diversitySelection,
            autoLimit,
            limit,
            offset,
            rerank,
            boost,
            singlePrompt,
            groupedTask,
            provider,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );

    /// <summary>
    /// Search near vector with generative AI capabilities and grouping using a lambda builder for NearVectorInput.
    /// </summary>
    /// <param name="vectors"></param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="diversitySelection">Diversity selection (MMR) to apply to the results. If not specified, no diversification is applied.</param>
    /// <param name="autoLimit">Automatic result cutoff (autocut): results stop after this many jumps in score or distance. If not specified, no cutoff is applied.</param>
    /// <param name="limit">Maximum number of results to return. If not specified, the server default limit is used.</param>
    /// <param name="offset">Number of results to skip. If not specified, results start from the first object.</param>
    /// <param name="rerank">Re-ranking configuration. Requires a reranker model integration on the collection.</param>
    /// <param name="boost">Soft-ranking to apply to the results: promotes or demotes objects in the pool of candidates the search fetches, re-scoring them rather than excluding them the way a filter does. If not specified, no boost is applied. Preview feature: requires Weaviate 1.38 or later; older servers silently ignore it.</param>
    /// <param name="singlePrompt">Prompt run separately for each returned object. If not specified, no per-object generation is performed.</param>
    /// <param name="groupedTask">Prompt run once over the whole result set. If not specified, no grouped generation is performed.</param>
    /// <param name="provider">Generative provider applied to prompts that do not carry one. Throws if a prompt already has a provider.</param>
    /// <param name="returnProperties">Properties to return in the response. If not specified, all non-blob properties are returned.</param>
    /// <param name="returnReferences">Cross-references to return. If not specified, no references are returned.</param>
    /// <param name="returnMetadata">Metadata to include in the response. If not specified, no metadata is returned.</param>
    /// <param name="includeVectors">Vector configuration for returned objects. If not specified, no vectors are returned.</param>
    /// <param name="cancellationToken">The cancellation token to monitor for cancellation requests.</param>
    /// <returns>Generative grouped search results.</returns>
    public async Task<GenerativeGroupByResult> NearVector(
        NearVectorInput.FactoryFn vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        Diversity? diversitySelection = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        Boost? boost = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            groupBy,
            filters,
            diversitySelection,
            autoLimit,
            limit,
            offset,
            rerank,
            boost,
            singlePrompt,
            groupedTask,
            provider,
            returnProperties,
            returnReferences,
            returnMetadata,
            includeVectors,
            cancellationToken
        );
}
