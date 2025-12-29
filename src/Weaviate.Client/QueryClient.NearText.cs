using Weaviate.Client.Models;

namespace Weaviate.Client;

public partial class QueryClient
{
    public async Task<WeaviateResult> NearText(
        AutoArray<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearText(
            _collectionClient.Name,
            text.ToArray(),
            distance: distance,
            certainty: certainty,
            limit: limit,
            moveTo: moveTo,
            moveAway: moveAway,
            offset: offset,
            autoLimit: autoLimit,
            targetVector: targets,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );

    public async Task<GroupByResult> NearText(
        AutoArray<string> text,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await _grpc.SearchNearText(
            _collectionClient.Name,
            text.ToArray(),
            groupBy: groupBy,
            distance: distance,
            certainty: certainty,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            tenant: _collectionClient.Tenant,
            rerank: rerank,
            targetVector: targets,
            consistencyLevel: _collectionClient.ConsistencyLevel,
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: CreateTimeoutCancellationToken(cancellationToken)
        );
}

/// <summary>
/// Extension methods for QueryClient NearText search with lambda target vector builders.
/// </summary>
public static class QueryClientNearTextExtensions
{
    /// <summary>
    /// Performs a near-text search using a lambda to build target vectors.
    /// </summary>
    public static async Task<WeaviateResult> NearText(
        this QueryClient client,
        AutoArray<string> text,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await client.NearText(
            text: text,
            certainty: certainty,
            distance: distance,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targets: targets?.Invoke(new TargetVectors.Builder()),
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );

    /// <summary>
    /// Performs a near-text search with group-by using a lambda to build target vectors.
    /// </summary>
    public static async Task<GroupByResult> NearText(
        this QueryClient client,
        AutoArray<string> text,
        GroupByRequest groupBy,
        float? certainty = null,
        float? distance = null,
        Move? moveTo = null,
        Move? moveAway = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Rerank? rerank = null,
        TargetVectors.FactoryFn? targets = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    ) =>
        await client.NearText(
            text: text,
            groupBy: groupBy,
            certainty: certainty,
            distance: distance,
            moveTo: moveTo,
            moveAway: moveAway,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            filters: filters,
            rerank: rerank,
            targets: targets?.Invoke(new TargetVectors.Builder()),
            returnProperties: returnProperties,
            returnReferences: returnReferences,
            returnMetadata: returnMetadata,
            includeVectors: includeVectors,
            cancellationToken: cancellationToken
        );
}
