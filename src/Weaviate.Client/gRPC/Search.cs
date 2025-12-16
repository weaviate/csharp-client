using Weaviate.Client.Models;
using Rerank = Weaviate.Client.Models.Rerank;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private async Task<V1.SearchReply> Search(
        V1.SearchRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reply = await _grpcClient.SearchAsync(
                request,
                CreateCallOptions(cancellationToken)
            );
            reply.Collection = request.Collection;
            return reply;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw ExceptionHelper.MapGrpcException(ex, "Search request failed");
        }
    }

    internal async Task<V1.SearchReply> FetchObjects(
        string collection,
        Filter? filters = null,
        IEnumerable<Sort>? sort = null,
        uint? limit = null,
        GroupByRequest? groupBy = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var req = BaseSearchRequest(
            collection,
            filters: filters,
            sort: sort,
            limit: limit,
            groupBy: groupBy,
            after: after,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            includeVectors: includeVectors
        );

        return await Search(req, cancellationToken);
    }

    internal async Task<V1.SearchReply> SearchNearVector(
        string collection,
        Models.Vectors vector,
        GroupByRequest? groupBy = null,
        float? distance = null,
        float? certainty = null,
        TargetVectors? targetVector = null,
        uint? limit = null,
        uint? autoLimit = null,
        uint? offset = null,
        Filter? filters = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        MetadataQuery? returnMetadata = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseSearchRequest(
            collection,
            filters: filters,
            sort: null,
            autoLimit: autoLimit,
            offset: offset,
            limit: limit,
            groupBy: groupBy,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            includeVectors: includeVectors
        );

        request.NearVector = BuildNearVector(vector, certainty, distance, targetVector);

        return await Search(request, cancellationToken);
    }

    internal async Task<V1.SearchReply> SearchNearText(
        string collection,
        string[] query,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        uint? offset = null,
        uint? autoLimit = null,
        Filter? filters = null,
        Move? moveTo = null,
        Move? moveAway = null,
        GroupByRequest? groupBy = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        AutoArray<string>? returnProperties = null,
        IList<QueryReference>? returnReferences = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseSearchRequest(
            collection,
            filters: filters,
            sort: null,
            limit: limit,
            offset: offset,
            autoLimit: autoLimit,
            groupBy: groupBy,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            includeVectors: includeVectors
        );

        request.NearText = BuildNearText(
            query,
            distance,
            certainty,
            moveTo,
            moveAway,
            targetVector
        );

        return await Search(request, cancellationToken);
    }

    internal async Task<V1.SearchReply> SearchBM25(
        string collection,
        string query,
        string[]? searchFields,
        Filter? filters = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? searchOperator = null,
        GroupByRequest? groupBy = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        AutoArray<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseSearchRequest(
            collection,
            filters: filters,
            sort: null,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            after: after,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            returnProperties: returnProperties,
            includeVectors: includeVectors
        );

        BuildBM25(request, query, properties: searchFields, searchOperator);

        return await Search(request, cancellationToken);
    }

    internal async Task<V1.SearchReply> SearchHybrid(
        string collection,
        string? query = null,
        float? alpha = null,
        Models.Vectors? vector = null,
        HybridNearVector? nearVector = null,
        HybridNearText? nearText = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        BM25Operator? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        GroupByRequest? groupBy = null,
        Rerank? rerank = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        TargetVectors? targetVector = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        AutoArray<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !(vector is not null || nearVector is not null || nearText is not null)
            && string.IsNullOrEmpty(query)
        )
        {
            throw new ArgumentException(
                "Either vector or query must be provided for hybrid search."
            );
        }

        var request = BaseSearchRequest(
            collection,
            filters: filters,
            sort: null,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            includeVectors: includeVectors
        );

        BuildHybrid(
            request,
            query,
            alpha,
            vector,
            nearVector,
            nearText,
            queryProperties,
            fusionType,
            maxVectorDistance,
            bm25Operator,
            targetVector
        );

        return await Search(request, cancellationToken);
    }

    internal async Task<V1.SearchReply> SearchNearObject(
        string collection,
        Guid objectID,
        double? certainty,
        double? distance,
        uint? limit,
        uint? offset,
        uint? autoLimit,
        Filter? filters,
        GroupByRequest? groupBy,
        Rerank? rerank,
        SinglePrompt? singlePrompt,
        GroupedTask? groupedTask,
        TargetVectors? targetVector,
        MetadataQuery? returnMetadata,
        AutoArray<string>? returnProperties,
        IList<QueryReference>? returnReferences,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseSearchRequest(
            collection,
            filters: filters,
            sort: null,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            includeVectors: includeVectors
        );

        BuildNearObject(request, objectID, certainty, distance, targetVector);

        return await Search(request, cancellationToken);
    }

    internal async Task<V1.SearchReply> SearchNearMedia(
        string collection,
        byte[] media,
        NearMediaType mediaType,
        double? certainty,
        double? distance,
        uint? limit,
        uint? offset,
        uint? autoLimit,
        Filter? filters,
        GroupByRequest? groupBy,
        Rerank? rerank,
        SinglePrompt? singlePrompt,
        GroupedTask? groupedTask,
        string? tenant,
        TargetVectors? targetVector,
        ConsistencyLevels? consistencyLevel,
        AutoArray<string>? returnProperties,
        MetadataQuery? returnMetadata,
        IList<QueryReference>? returnReferences,
        VectorQuery? includeVectors = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseSearchRequest(
            collection,
            filters: filters,
            sort: null,
            autoLimit: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            tenant: tenant,
            consistencyLevel: consistencyLevel,
            rerank: rerank,
            singlePrompt: singlePrompt,
            groupedTask: groupedTask,
            returnProperties: returnProperties,
            returnMetadata: returnMetadata,
            returnReferences: returnReferences,
            includeVectors: includeVectors
        );

        switch (mediaType)
        {
            case NearMediaType.Image:
                request.NearImage = new V1.NearImageSearch
                {
                    Image = Convert.ToBase64String(media),
                };
                if (certainty.HasValue)
                {
                    request.NearImage.Certainty = certainty.Value;
                }

                if (distance.HasValue)
                {
                    request.NearImage.Distance = distance.Value;
                }

                request.NearImage.Targets = BuildTargetVector(targetVector).targets;

                break;
            case NearMediaType.Video:
                request.NearVideo = new V1.NearVideoSearch
                {
                    Video = Convert.ToBase64String(media),
                };
                if (certainty.HasValue)
                {
                    request.NearVideo.Certainty = certainty.Value;
                }

                if (distance.HasValue)
                {
                    request.NearVideo.Distance = distance.Value;
                }

                request.NearVideo.Targets = BuildTargetVector(targetVector).targets;
                break;
            case NearMediaType.Audio:
                request.NearAudio = new V1.NearAudioSearch
                {
                    Audio = Convert.ToBase64String(media),
                };
                if (certainty.HasValue)
                {
                    request.NearAudio.Certainty = certainty.Value;
                }

                if (distance.HasValue)
                {
                    request.NearAudio.Distance = distance.Value;
                }

                request.NearAudio.Targets = BuildTargetVector(targetVector).targets;
                break;
            case NearMediaType.Depth:
                request.NearDepth = new V1.NearDepthSearch
                {
                    Depth = Convert.ToBase64String(media),
                };
                if (certainty.HasValue)
                {
                    request.NearDepth.Certainty = certainty.Value;
                }

                if (distance.HasValue)
                {
                    request.NearDepth.Distance = distance.Value;
                }

                request.NearDepth.Targets = BuildTargetVector(targetVector).targets;
                break;
            case NearMediaType.Thermal:
                request.NearThermal = new V1.NearThermalSearch
                {
                    Thermal = Convert.ToBase64String(media),
                };
                if (certainty.HasValue)
                {
                    request.NearThermal.Certainty = certainty.Value;
                }

                if (distance.HasValue)
                {
                    request.NearThermal.Distance = distance.Value;
                }

                request.NearThermal.Targets = BuildTargetVector(targetVector).targets;
                break;
            case NearMediaType.IMU:
                request.NearImu = new V1.NearIMUSearch { Imu = Convert.ToBase64String(media) };
                if (certainty.HasValue)
                {
                    request.NearImu.Certainty = certainty.Value;
                }

                if (distance.HasValue)
                {
                    request.NearImu.Distance = distance.Value;
                }

                request.NearImu.Targets = BuildTargetVector(targetVector).targets;
                break;
            default:
                throw new ArgumentException("Unsupported media type for near media search.");
        }

        return await Search(request, cancellationToken);
    }
}
