using Weaviate.Client.Grpc.Protobuf.V1;
using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

/// <summary>
/// The weaviate grpc client class
/// </summary>
internal partial class WeaviateGrpcClient
{
    /// <summary>
    /// Bases the aggregate request using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="filter">The filter</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="limit">The limit</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <exception cref="NotSupportedException">Metric type {m.GetType()} is not supported.</exception>
    /// <returns>The </returns>
    private static AggregateRequest BaseAggregateRequest(
        string collection,
        Filter? filter = null,
        Aggregate.GroupBy? groupBy = null,
        uint? limit = null,
        bool? totalCount = false,
        string? tenant = null,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var r = new AggregateRequest
        {
            Collection = collection,
            Filters = filter?.InternalFilter,
            ObjectsCount = totalCount ?? false,
            Tenant = tenant ?? string.Empty,
        };

        if (groupBy != null)
        {
            r.GroupBy = new AggregateRequest.Types.GroupBy
            {
                Collection = "",
                Property = groupBy.Property.Decapitalize(),
            };

            if (groupBy.Limit.HasValue)
            {
                r.Limit = groupBy.Limit!.Value;
            }
        }

        if (limit.HasValue)
        {
            r.ObjectLimit = limit.Value;
        }

        if (metrics?.Any() ?? false)
        {
            r.Aggregations.AddRange(
                metrics!.Select(m =>
                {
                    var a = new AggregateRequest.Types.Aggregation { Property = m.Name };
                    switch (m)
                    {
                        case Aggregate.Metric.Text text:
                            a.Text = new AggregateRequest.Types.Aggregation.Types.Text
                            {
                                Count = text.Count,
                            };
                            if (text.MinOccurrences.HasValue)
                            {
                                a.Text.TopOccurencesLimit = text.MinOccurrences.Value;
                            }
                            a.Text.TopOccurences =
                                text.TopOccurrencesCount || text.TopOccurrencesValue;
                            break;
                        case Aggregate.Metric.Integer integer:
                            a.Int = new AggregateRequest.Types.Aggregation.Types.Integer
                            {
                                Count = integer.Count,
                                Maximum = integer.Maximum,
                                Mean = integer.Mean,
                                Median = integer.Median,
                                Minimum = integer.Minimum,
                                Mode = integer.Mode,
                                Sum = integer.Sum,
                            };
                            break;
                        case Aggregate.Metric.Number number:
                            a.Number = new AggregateRequest.Types.Aggregation.Types.Number
                            {
                                Count = number.Count,
                                Maximum = number.Maximum,
                                Mean = number.Mean,
                                Median = number.Median,
                                Minimum = number.Minimum,
                                Mode = number.Mode,
                                Sum = number.Sum,
                            };
                            break;
                        case Aggregate.Metric.Boolean boolean:
                            a.Boolean = new AggregateRequest.Types.Aggregation.Types.Boolean
                            {
                                Count = boolean.Count,
                                PercentageFalse = boolean.PercentageFalse,
                                PercentageTrue = boolean.PercentageTrue,
                                TotalFalse = boolean.TotalFalse,
                                TotalTrue = boolean.TotalTrue,
                            };
                            break;
                        case Aggregate.Metric.Date date:
                            a.Date = new AggregateRequest.Types.Aggregation.Types.Date
                            {
                                Count = date.Count,
                                Maximum = date.Maximum,
                                Median = date.Median,
                                Minimum = date.Minimum,
                                Mode = date.Mode,
                            };
                            break;
                        default:
                            throw new NotSupportedException(
                                $"Metric type {m.GetType()} is not supported."
                            );
                    }

                    return a;
                })
            );
        }

        return r;
    }

    /// <summary>
    /// Aggregates the request
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the aggregate reply</returns>
    private async Task<AggregateReply> Aggregate(
        AggregateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var reply = await _grpcClient.AggregateAsync(
                request,
                CreateCallOptions(cancellationToken)
            );
            reply.Collection = request.Collection;
            return reply;
        }
        catch (global::Grpc.Core.RpcException ex)
        {
            // Use centralized exception mapping helper
            throw Internal.ExceptionHelper.MapGrpcException(ex, "Aggregate request failed");
        }
    }

    /// <summary>
    /// Aggregates the over all using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="filter">The filter</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the aggregate reply</returns>
    internal async Task<AggregateReply> AggregateOverAll(
        string collection,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        bool totalCount,
        string? tenant,
        IEnumerable<Aggregate.Metric>? metrics = null,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseAggregateRequest(
            collection,
            filter,
            groupBy,
            null,
            totalCount,
            tenant,
            metrics
        );

        return await Aggregate(request, cancellationToken);
    }

    /// <summary>
    /// Aggregates the near text using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="query">The query</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="moveTo">The move to</param>
    /// <param name="moveAway">The move away</param>
    /// <param name="filter">The filter</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="targetVector">The target vector</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the aggregate reply</returns>
    internal async Task<AggregateReply> AggregateNearText(
        string collection,
        string[] query,
        double? certainty,
        double? distance,
        uint? limit,
        Move? moveTo,
        Move? moveAway,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        TargetVectors? targetVector,
        bool totalCount,
        string? tenant,
        IEnumerable<Aggregate.Metric>? metrics,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseAggregateRequest(
            collection,
            filter,
            groupBy,
            limit,
            totalCount,
            tenant,
            metrics
        );

        request.NearText = BuildNearText(
            query,
            distance,
            certainty,
            moveTo,
            moveAway,
            targetVector
        );

        return await Aggregate(request, cancellationToken);
    }

    /// <summary>
    /// Aggregates the near vector using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="filter">The filter</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the aggregate reply</returns>
    internal async Task<AggregateReply> AggregateNearVector(
        string collection,
        VectorSearchInput vectors,
        double? certainty,
        double? distance,
        uint? limit,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        bool totalCount,
        string? tenant,
        IEnumerable<Aggregate.Metric>? metrics,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseAggregateRequest(
            collection,
            filter,
            groupBy,
            limit,
            totalCount,
            tenant,
            metrics
        );

        request.NearVector = BuildNearVector(vectors, certainty, distance);

        return await Aggregate(request, cancellationToken);
    }

    /// <summary>
    /// Aggregates the hybrid using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="query">The query</param>
    /// <param name="alpha">The alpha</param>
    /// <param name="vectors">The vectors</param>
    /// <param name="queryProperties">The query properties</param>
    /// <param name="bm25Operator">The bm 25 operator</param>
    /// <param name="maxVectorDistance">The max vector distance</param>
    /// <param name="filter">The filter</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="objectLimit">The object limit</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the aggregate reply</returns>
    internal async Task<AggregateReply> AggregateHybrid(
        string collection,
        string? query,
        float alpha,
        HybridVectorInput? vectors,
        string[]? queryProperties,
        BM25Operator? bm25Operator,
        float? maxVectorDistance,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        uint? objectLimit,
        bool totalCount,
        string? tenant,
        IEnumerable<Aggregate.Metric>? metrics,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseAggregateRequest(
            collection,
            filter,
            groupBy,
            objectLimit,
            totalCount,
            tenant,
            metrics
        );

        request.Hybrid = BuildHybrid(
            query,
            alpha,
            vectors,
            queryProperties,
            null,
            maxVectorDistance,
            bm25Operator
        );

        return await Aggregate(request, cancellationToken);
    }

    /// <summary>
    /// Aggregates the near object using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="objectID">The object id</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="filters">The filters</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="targetVector">The target vector</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task containing the aggregate reply</returns>
    internal async Task<AggregateReply> AggregateNearObject(
        string collection,
        Guid objectID,
        double? certainty,
        double? distance,
        uint? limit,
        Filter? filters,
        Aggregate.GroupBy? groupBy,
        TargetVectors? targetVector,
        bool totalCount,
        string? tenant,
        IEnumerable<Aggregate.Metric>? metrics,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseAggregateRequest(
            collection,
            filters,
            groupBy,
            limit,
            totalCount,
            tenant,
            metrics
        );

        request.NearObject = new()
        {
            Id = objectID.ToString(),
            Targets = BuildTargetVector(targetVector).targets,
        };

        if (certainty.HasValue)
        {
            request.NearObject.Certainty = certainty.Value;
        }
        if (distance.HasValue)
        {
            request.NearObject.Distance = distance.Value;
        }

        return await Aggregate(request, cancellationToken);
    }

    /// <summary>
    /// Aggregates the near media using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="media">The media</param>
    /// <param name="mediaType">The media type</param>
    /// <param name="certainty">The certainty</param>
    /// <param name="distance">The distance</param>
    /// <param name="limit">The limit</param>
    /// <param name="filter">The filter</param>
    /// <param name="groupBy">The group by</param>
    /// <param name="targetVector">The target vector</param>
    /// <param name="totalCount">The total count</param>
    /// <param name="tenant">The tenant</param>
    /// <param name="metrics">The metrics</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <exception cref="ArgumentException">Unsupported media type for aggregate near media.</exception>
    /// <returns>A task containing the aggregate reply</returns>
    internal async Task<AggregateReply> AggregateNearMedia(
        string collection,
        byte[] media,
        NearMediaType mediaType,
        double? certainty,
        double? distance,
        uint? limit,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        TargetVectors? targetVector,
        bool totalCount,
        string? tenant,
        IEnumerable<Aggregate.Metric>? metrics,
        CancellationToken cancellationToken = default
    )
    {
        var request = BaseAggregateRequest(
            collection,
            filter,
            groupBy,
            limit,
            totalCount,
            tenant,
            metrics
        );

        switch (mediaType)
        {
            case NearMediaType.Image:
                request.NearImage = new NearImageSearch { Image = Convert.ToBase64String(media) };
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
                request.NearVideo = new NearVideoSearch { Video = Convert.ToBase64String(media) };
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
                request.NearAudio = new NearAudioSearch { Audio = Convert.ToBase64String(media) };
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
                request.NearDepth = new NearDepthSearch { Depth = Convert.ToBase64String(media) };
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
                request.NearThermal = new NearThermalSearch
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
                request.NearImu = new NearIMUSearch { Imu = Convert.ToBase64String(media) };
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
                throw new ArgumentException("Unsupported media type for aggregate near media.");
        }

        return await Aggregate(request, cancellationToken);
    }
}
