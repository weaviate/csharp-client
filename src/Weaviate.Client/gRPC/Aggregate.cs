using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public partial class WeaviateGrpcClient
{
    private AggregateRequest BaseAggregateRequest(
        string collection,
        Filter? filter = null,
        Aggregate.GroupBy? groupBy = null,
        uint? limit = null,
        bool? totalCount = false,
        params MetricRequest[] metrics
    )
    {
        var r = new AggregateRequest
        {
            Collection = collection,
            Filters = filter?.InternalFilter, // No filter by default
            ObjectsCount = totalCount ?? false,
        };

        if (groupBy != null)
        {
            r.GroupBy = new AggregateRequest.Types.GroupBy
            {
                Collection = "",
                Property = groupBy.Property,
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

        if (metrics.Length > 0)
        {
            r.Aggregations.AddRange(
                metrics.Select(m =>
                {
                    var a = new AggregateRequest.Types.Aggregation { Property = m.Name };
                    switch (m)
                    {
                        case MetricRequest.Text text:
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
                        case MetricRequest.Integer integer:
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
                        case MetricRequest.Number number:
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
                        case MetricRequest.Boolean boolean:
                            a.Boolean = new AggregateRequest.Types.Aggregation.Types.Boolean
                            {
                                Count = boolean.Count,
                                PercentageFalse = boolean.PercentageFalse,
                                PercentageTrue = boolean.PercentageTrue,
                                TotalFalse = boolean.TotalFalse,
                                TotalTrue = boolean.TotalTrue,
                            };
                            break;
                        case MetricRequest.Date date:
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

    private async Task<AggregateReply> Aggregate(AggregateRequest request)
    {
        return await _grpcClient.AggregateAsync(request);
    }

    internal async Task<AggregateReply> AggregateOverAll(
        string collection,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        bool totalCount,
        params MetricRequest[] metrics
    )
    {
        var request = BaseAggregateRequest(collection, filter, groupBy, null, totalCount, metrics);

        return await Aggregate(request);
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
        string[]? targetVector,
        bool totalCount,
        MetricRequest[] metrics
    )
    {
        var request = BaseAggregateRequest(collection, filter, groupBy, limit, totalCount, metrics);

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
            var uuids = moveTo.Objects is null
                ? []
                : (new Guid?[] { moveTo.Objects }).Select(x => x.ToString());
            var concepts = moveTo.Concepts is null ? new string[] { } : [moveTo.Concepts];
            request.NearText.MoveTo = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveTo.Force,
            };
        }
        if (moveAway is not null)
        {
            var uuids = moveAway.Objects is null
                ? []
                : (new Guid?[] { moveAway.Objects }).Select(x => x.ToString());
            var concepts = moveAway.Concepts is null ? new string[] { } : [moveAway.Concepts];
            request.NearText.MoveAway = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveAway.Force,
            };
        }

        return await Aggregate(request);
    }

    internal async Task<AggregateReply> AggregateNearVector(
        string collection,
        float[] vector,
        double? certainty,
        double? distance,
        uint? limit,
        Filter? filter,
        Aggregate.GroupBy? groupBy,
        string[]? targetVector,
        bool totalCount,
        MetricRequest[] metrics
    )
    {
        var request = BaseAggregateRequest(collection, filter, groupBy, limit, totalCount, metrics);

        request.NearVector = new()
        {
            Vectors =
            {
                new Vectors
                {
                    Name = "default",
                    Type = Vectors.Types.VectorType.SingleFp32,
                    VectorBytes = vector.ToByteString(),
                },
            },
            // Targets = null,
            // VectorForTargets = { },
        };

        if (certainty.HasValue)
        {
            request.NearVector.Certainty = certainty.Value;
        }
        if (distance.HasValue)
        {
            request.NearVector.Distance = distance.Value;
        }

        return await Aggregate(request);
    }
}
