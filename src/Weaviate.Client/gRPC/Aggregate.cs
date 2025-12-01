using Weaviate.Client.Grpc.Protobuf.V1;
using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private Grpc.Protobuf.V1.AggregateRequest BaseAggregateRequest(
        string collection,
        Filter? filter = null,
        Aggregate.GroupBy? groupBy = null,
        uint? limit = null,
        bool? totalCount = false,
        string? tenant = null,
        IEnumerable<Aggregate.Metric>? metrics = null
    )
    {
        var r = new Grpc.Protobuf.V1.AggregateRequest
        {
            Collection = collection,
            Filters = filter?.InternalFilter,
            ObjectsCount = totalCount ?? false,
            Tenant = tenant ?? string.Empty,
        };

        if (groupBy != null)
        {
            r.GroupBy = new Grpc.Protobuf.V1.AggregateRequest.Types.GroupBy
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
                    var a = new Grpc.Protobuf.V1.AggregateRequest.Types.Aggregation
                    {
                        Property = m.Name,
                    };
                    switch (m)
                    {
                        case Aggregate.Metric.Text text:
                            a.Text =
                                new Grpc.Protobuf.V1.AggregateRequest.Types.Aggregation.Types.Text
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
                            a.Int =
                                new Grpc.Protobuf.V1.AggregateRequest.Types.Aggregation.Types.Integer
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
                            a.Number =
                                new Grpc.Protobuf.V1.AggregateRequest.Types.Aggregation.Types.Number
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
                            a.Boolean =
                                new Grpc.Protobuf.V1.AggregateRequest.Types.Aggregation.Types.Boolean
                                {
                                    Count = boolean.Count,
                                    PercentageFalse = boolean.PercentageFalse,
                                    PercentageTrue = boolean.PercentageTrue,
                                    TotalFalse = boolean.TotalFalse,
                                    TotalTrue = boolean.TotalTrue,
                                };
                            break;
                        case Aggregate.Metric.Date date:
                            a.Date =
                                new Grpc.Protobuf.V1.AggregateRequest.Types.Aggregation.Types.Date
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

    private async Task<AggregateReply> Aggregate(
        Grpc.Protobuf.V1.AggregateRequest request,
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
            // Provide clearer diagnostics if the server does not implement the Aggregate RPC yet.
            if (ex.StatusCode == global::Grpc.Core.StatusCode.Unimplemented)
            {
                throw new WeaviateFeatureNotSupportedException(
                    "gRPC Aggregate method is not implemented on the connected Weaviate server.",
                    ex
                );
            }

            throw new WeaviateServerException("Aggregate request failed", ex);
        }
    }

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

        request.NearText = new() { Query = { query } };

        if (certainty.HasValue)
        {
            request.NearText.Certainty = certainty.Value;
        }
        if (distance.HasValue)
        {
            request.NearText.Distance = distance.Value;
        }
        if (moveTo is not null)
        {
            var uuids = moveTo.Objects is null ? [] : moveTo.Objects.Select(x => x.ToString());
            var concepts = moveTo.Concepts is null ? new string[] { } : moveTo.Concepts;
            request.NearText.MoveTo = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveTo.Force,
            };
        }
        if (moveAway is not null)
        {
            var uuids = moveAway.Objects is null ? [] : moveAway.Objects.Select(x => x.ToString());
            var concepts = moveAway.Concepts is null ? new string[] { } : moveAway.Concepts;
            request.NearText.MoveAway = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveAway.Force,
            };
        }

        return await Aggregate(request, cancellationToken);
    }

    internal async Task<AggregateReply> AggregateNearVector(
        string collection,
        Models.Vectors vector,
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

        request.NearVector = BuildNearVector(vector, certainty, distance, targetVector);

        return await Aggregate(request, cancellationToken);
    }

    internal async Task<AggregateReply> AggregateHybrid(
        string collection,
        string? query,
        float alpha,
        Models.Vectors? vectors,
        string[]? queryProperties,
        BM25Operator? bm25Operator,
        string? targetVector,
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

        request.Hybrid = new() { Query = query, Alpha = alpha };

        if (queryProperties is not null && queryProperties.Length > 0)
        {
            request.Hybrid.Properties.AddRange(queryProperties);
        }

        var (targets, _, vector) = BuildTargetVector(
            targetVector is null ? null : [targetVector],
            vectors
        );

        request.Hybrid.Vectors.AddRange(vector ?? []);
        request.Hybrid.Targets = targets;

        if (maxVectorDistance.HasValue)
        {
            request.Hybrid.VectorDistance = maxVectorDistance.Value;
        }

        if (bm25Operator != null)
        {
            request.Hybrid.Bm25SearchOperator = new()
            {
                Operator = bm25Operator switch
                {
                    BM25Operator.And => Protobuf.V1.SearchOperatorOptions.Types.Operator.And,
                    BM25Operator.Or => Protobuf.V1.SearchOperatorOptions.Types.Operator.Or,
                    _ => Protobuf.V1.SearchOperatorOptions.Types.Operator.Unspecified,
                },
                MinimumOrTokensMatch = (bm25Operator as BM25Operator.Or)?.MinimumMatch ?? 1,
            };
        }

        return await Aggregate(request, cancellationToken);
    }

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
