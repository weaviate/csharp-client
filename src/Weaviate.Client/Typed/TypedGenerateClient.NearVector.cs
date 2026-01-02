using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Typed;

public partial class TypedGenerateClient<T>
{
    /// <summary>
    /// Search near vector with generative AI capabilities.
    /// </summary>
    /// <param name="vectors">Vector to search near</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative result</returns>
    public async Task<GenerativeWeaviateResult<T>> NearVector(
        VectorSearchInput vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.NearVector(
            vectors: vectors,
            filters: filters,
            certainty: certainty,
            distance: distance,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Search near vector with generative AI capabilities and grouping.
    /// </summary>
    /// <param name="vectors">Vector to search near</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="targetVector">Target vector name</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative group-by result</returns>
    public async Task<GenerativeGroupByResult<T>> NearVector(
        VectorSearchInput vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? distance = null,
        float? certainty = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var result = await _generateClient.NearVector(
            vectors: vectors,
            groupBy: groupBy,
            filters: filters,
            distance: distance,
            certainty: certainty,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            provider: provider,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
        return result.ToTyped<T>();
    }

    /// <summary>
    /// Search near vector with generative AI capabilities using lambda builder.
    /// </summary>
    /// <param name="vectors">Lambda function to build the vectors</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative result</returns>
    public Task<GenerativeWeaviateResult<T>> NearVector(
        VectorSearchInput.FactoryFn vectors,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors(new VectorSearchInput.Builder()),
            filters,
            certainty,
            distance,
            autoLimit,
            limit,
            offset,
            rerank,
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
    /// <param name="vectors">Lambda function to build the vectors</param>
    /// <param name="groupBy">Group by configuration</param>
    /// <param name="filters">Filters to apply</param>
    /// <param name="certainty">Certainty threshold</param>
    /// <param name="distance">Distance threshold</param>
    /// <param name="autoLimit">Auto-cut threshold</param>
    /// <param name="limit">Maximum number of results</param>
    /// <param name="offset">Offset for pagination</param>
    /// <param name="rerank">Rerank configuration</param>
    /// <param name="singlePrompt">Single prompt for generation</param>
    /// <param name="groupedTask">Grouped prompt for generation</param>
    /// <param name="provider">Optional generative provider</param>
    /// <param name="returnProperties">Properties to return</param>
    /// <param name="returnReferences">References to return</param>
    /// <param name="returnMetadata">Metadata to return</param>
    /// <param name="includeVectors">Vectors to include</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Strongly-typed generative group-by result</returns>
    public Task<GenerativeGroupByResult<T>> NearVector(
        VectorSearchInput.FactoryFn vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        float? certainty = null,
        float? distance = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors(new VectorSearchInput.Builder()),
            groupBy,
            filters,
            certainty,
            distance,
            autoLimit,
            limit,
            offset,
            rerank,
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
    /// <param name="vectors">Near-vector input containing vector, certainty, and distance.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed generative search result.</returns>
    public Task<GenerativeWeaviateResult<T>> NearVector(
        NearVectorInput vectors,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors: vectors.Vector,
            certainty: vectors.Certainty,
            distance: vectors.Distance,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
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
    /// <param name="vectors">Near-vector input containing vector, certainty, and distance.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed generative grouped search result.</returns>
    public Task<GenerativeGroupByResult<T>> NearVector(
        NearVectorInput vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors: vectors.Vector,
            certainty: vectors.Certainty,
            distance: vectors.Distance,
            groupBy: groupBy,
            filters: filters,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            rerank: rerank,
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
    /// <param name="vectors">Lambda builder for creating NearVectorInput with target vectors.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed generative search result.</returns>
    public Task<GenerativeWeaviateResult<T>> NearVector(
        NearVectorInput.FactoryFn vectors,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            filters,
            autoLimit,
            limit,
            offset,
            rerank,
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
    /// <param name="vectors">Lambda builder for creating NearVectorInput with target vectors.</param>
    /// <param name="groupBy">Group-by configuration.</param>
    /// <param name="filters">Filters to apply to the search.</param>
    /// <param name="autoLimit">Automatic result cutoff threshold.</param>
    /// <param name="limit">Maximum number of results to return.</param>
    /// <param name="offset">Number of results to skip.</param>
    /// <param name="rerank">Re-ranking configuration.</param>
    /// <param name="singlePrompt">Single prompt for generative AI.</param>
    /// <param name="groupedTask">Grouped task for generative AI.</param>
    /// <param name="provider">Generative AI provider configuration.</param>
    /// <param name="returnProperties">Properties to return in the response.</param>
    /// <param name="returnReferences">Cross-references to return.</param>
    /// <param name="returnMetadata">Metadata to include in the response.</param>
    /// <param name="includeVectors">Vector configuration for returned objects.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A strongly-typed generative grouped search result.</returns>
    public Task<GenerativeGroupByResult<T>> NearVector(
        NearVectorInput.FactoryFn vectors,
        GroupByRequest groupBy,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        GenerativeProvider? provider = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        NearVector(
            vectors(VectorInputBuilderFactories.CreateNearVectorBuilder()),
            groupBy,
            filters,
            autoLimit,
            limit,
            offset,
            rerank,
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
