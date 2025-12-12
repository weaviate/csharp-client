using System.Collections;
using System.Dynamic;
using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private static Grpc.Protobuf.V1.PropertiesRequest? MakePropsRequest(
        string[]? fields,
        IList<QueryReference>? reference
    )
    {
        if (fields is null && reference is null)
            return null;

        var req = new Grpc.Protobuf.V1.PropertiesRequest();

        if (fields is not null)
        {
            req.NonRefProperties.AddRange(fields);
        }
        else
        {
            req.ReturnAllNonrefProperties = true;
        }

        foreach (var r in reference ?? [])
        {
            if (reference is not null)
            {
                req.RefProperties.Add(MakeRefPropsRequest(r));
            }
        }

        return req;
    }

    private static Grpc.Protobuf.V1.RefPropertiesRequest? MakeRefPropsRequest(
        QueryReference? reference
    )
    {
        if (reference is null)
            return null;

        return new Grpc.Protobuf.V1.RefPropertiesRequest()
        {
            Metadata = new Grpc.Protobuf.V1.MetadataRequest()
            {
                Uuid = true,
                LastUpdateTimeUnix = reference.Metadata?.LastUpdateTime ?? false,
                CreationTimeUnix = reference.Metadata?.CreationTime ?? false,
                Certainty = reference.Metadata?.Certainty ?? false,
                Distance = reference.Metadata?.Distance ?? false,
                Score = reference.Metadata?.Score ?? false,
                ExplainScore = reference.Metadata?.ExplainScore ?? false,
                IsConsistent = reference.Metadata?.IsConsistent ?? false,
            },
            Properties = MakePropsRequest(reference?.Fields, reference?.References),
            ReferenceProperty = reference?.LinkOn ?? string.Empty,
        };
    }

    internal static Metadata BuildMetadataFromResult(Grpc.Protobuf.V1.MetadataResult metadata)
    {
        return new Metadata
        {
            LastUpdateTime = metadata.LastUpdateTimeUnixPresent
                ? DateTimeOffset.FromUnixTimeMilliseconds(metadata.LastUpdateTimeUnix).UtcDateTime
                : null,
            CreationTime = metadata.CreationTimeUnixPresent
                ? DateTimeOffset.FromUnixTimeMilliseconds(metadata.CreationTimeUnix).UtcDateTime
                : null,
            Certainty = metadata.CertaintyPresent ? metadata.Certainty : null,
            Distance = metadata.DistancePresent ? metadata.Distance : null,
            Score = metadata.ScorePresent ? metadata.Score : null,
            ExplainScore = metadata.ExplainScorePresent ? metadata.ExplainScore : null,
            IsConsistent = metadata.IsConsistentPresent ? metadata.IsConsistent : null,
            RerankScore = metadata.RerankScorePresent ? metadata.RerankScore : null,
        };
    }

    internal static Vectors BuildVectorsFromResult(Grpc.Protobuf.V1.MetadataResult metadataResult)
    {
        if (metadataResult.VectorBytes != null && metadataResult.VectorBytes.Length > 0)
        {
            return Vector.Create<float>([.. metadataResult.VectorBytes.FromByteString<float>()]);
        }

        var vectors = metadataResult.Vectors;

        var result = new Vectors();

        foreach (var vector in vectors)
        {
            result.Add(vector.FromByteString<float>());
        }

        return result;
    }

    internal static GroupByObject BuildGroupByObjectFromResult(
        string collection,
        string groupName,
        Grpc.Protobuf.V1.SearchResult obj
    )
    {
        var metadata = obj.Metadata;
        var properties = obj.Properties;

        return new GroupByObject(BuildObjectFromResult(collection, metadata, properties))
        {
            BelongsToGroup = groupName,
        };
    }

    private static ExpandoObject MakeNonRefs(Grpc.Protobuf.V1.Properties result)
    {
        var eoBase = new ExpandoObject();

        if (result is null)
        {
            return eoBase;
        }

        var eo = eoBase as IDictionary<string, object>;

        foreach (var r in result.Fields)
        {
            Grpc.Protobuf.V1.Value.KindOneofCase kind = r.Value.KindCase;
            switch (kind)
            {
                case Grpc.Protobuf.V1.Value.KindOneofCase.None:
                case Grpc.Protobuf.V1.Value.KindOneofCase.NullValue:
                    continue;
                case Grpc.Protobuf.V1.Value.KindOneofCase.NumberValue:
                    eo[r.Key] = r.Value.NumberValue;
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.BoolValue:
                    eo[r.Key] = r.Value.BoolValue;
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.ObjectValue:
                    eo[r.Key] = MakeNonRefs(r.Value.ObjectValue) ?? new object { };
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.ListValue:
                    eo[r.Key] = MakeListValue(r.Value.ListValue);
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.DateValue:
                    eo[r.Key] = DateTime.SpecifyKind(
                        DateTime.Parse(r.Value.DateValue),
                        DateTimeKind.Utc
                    );
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.UuidValue:
                    eo[r.Key] = Guid.Parse(r.Value.UuidValue);
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.IntValue:
                    eo[r.Key] = r.Value.IntValue;
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.GeoValue:
                    eo[r.Key] = new Models.GeoCoordinate(
                        r.Value.GeoValue.Latitude,
                        r.Value.GeoValue.Longitude
                    );
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.BlobValue:
                    eo[r.Key] = r.Value.BlobValue;
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.PhoneValue:
                    eo[r.Key] = new Models.PhoneNumber
                    {
                        Input = r.Value.PhoneValue.Input,
                        CountryCode = r.Value.PhoneValue.CountryCode,
                        DefaultCountry = r.Value.PhoneValue.DefaultCountry,
                        InternationalFormatted = r.Value.PhoneValue.InternationalFormatted,
                        National = r.Value.PhoneValue.National,
                        NationalFormatted = r.Value.PhoneValue.NationalFormatted,
                        Valid = r.Value.PhoneValue.Valid,
                    };
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.TextValue:
                    eo[r.Key] = r.Value.TextValue;
                    break;
            }
        }

        return eoBase;
    }

    private static IList MakeListValue(Grpc.Protobuf.V1.ListValue list)
    {
        switch (list.KindCase)
        {
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.BoolValues:
                return list.BoolValues.Values.ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.ObjectValues:
                return list.ObjectValues.Values.Select(v => MakeNonRefs(v)).ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.DateValues:
                return list
                    .DateValues.Values.Select(v =>
                        DateTime.SpecifyKind(DateTime.Parse(v), DateTimeKind.Utc)
                    )
                    .ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.UuidValues:
                return list.UuidValues.Values.Select(v => Guid.Parse(v)).ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.TextValues:
                return list.TextValues.Values.ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.IntValues:
                return list.IntValues.Values.FromByteString<long>().ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.NumberValues:
                return list.NumberValues.Values.FromByteString<double>().ToArray();
            case Grpc.Protobuf.V1.ListValue.KindOneofCase.None:
            default:
                return new List<object> { };
        }
    }

    internal static WeaviateObject BuildObjectFromResult(
        string collection,
        Grpc.Protobuf.V1.MetadataResult metadata,
        Grpc.Protobuf.V1.PropertiesResult properties
    )
    {
        return new WeaviateObject
        {
            UUID = !string.IsNullOrEmpty(metadata.Id) ? Guid.Parse(metadata.Id) : Guid.Empty,
            Collection = collection,
            Vectors = BuildVectorsFromResult(metadata),
            Properties = MakeNonRefs(properties.NonRefProps),
            References = properties.RefPropsRequested
                ? MakeRefs(properties.RefProps)
                : new Dictionary<string, IList<WeaviateObject>>(),
            Metadata = BuildMetadataFromResult(metadata),
        };
    }

    internal static IList<Models.GenerativeReply> BuildGenerativeReplyFromResult(
        IEnumerable<Grpc.Protobuf.V1.GenerativeReply>? generative
    )
    {
        return generative
                ?.Select(g => new Models.GenerativeReply(
                    Result: g.Result,
                    Debug: g.Debug is null ? null : new GenerativeDebug(g.Debug.FullPrompt),
                    Metadata: g.Metadata
                ))
                .ToList() ?? [];
    }

    internal static GenerativeWeaviateObject BuildGenerativeObjectFromResult(
        string collection,
        Grpc.Protobuf.V1.MetadataResult metadata,
        Grpc.Protobuf.V1.PropertiesResult properties,
        Grpc.Protobuf.V1.GenerativeResult generative
    )
    {
        var obj = BuildObjectFromResult(collection, metadata, properties);

        return new GenerativeWeaviateObject
        {
            UUID = obj.UUID,
            Collection = obj.Collection,
            Vectors = obj.Vectors,
            Properties = obj.Properties,
            References = obj.References,
            Metadata = obj.Metadata,
            Generative = BuildGenerativeResult(generative),
        };
    }

    private static GenerativeResult BuildGenerativeResult(
        Grpc.Protobuf.V1.GenerativeResult? generative
    )
    {
        return new GenerativeResult(BuildGenerativeReplyFromResult(generative?.Values));
    }

    internal static IDictionary<string, IList<WeaviateObject>> MakeRefs(
        Google.Protobuf.Collections.RepeatedField<Grpc.Protobuf.V1.RefPropertiesResult> refProps
    )
    {
        var result = new Dictionary<string, IList<WeaviateObject>>();

        foreach (var refProp in refProps)
        {
            result[refProp.PropName] = refProp
                .Properties.Select(p => BuildObjectFromResult(p.TargetCollection, p.Metadata, p))
                .ToList();
        }

        return result;
    }

    internal static WeaviateResult BuildResult(Protobuf.V1.SearchReply? reply)
    {
        if (reply?.Results == null || reply.Results.Count == 0)
            return WeaviateResult.Empty;

        return new WeaviateResult
        {
            Objects =
                reply
                    ?.Results?.Select(r =>
                        BuildObjectFromResult(reply.Collection, r.Metadata, r.Properties)
                    )
                    .ToList() ?? [],
        };
    }

    internal static Models.GroupByResult BuildGroupByResult(Protobuf.V1.SearchReply? reply)
    {
        if (reply?.GroupByResults == null || reply.GroupByResults.Count == 0)
            return Models.GroupByResult.Empty;

        var groups = reply.GroupByResults.ToDictionary(
            g => g.Name,
            g => new WeaviateGroup
            {
                Name = g.Name,
                Objects = g
                    .Objects.Select(obj =>
                        BuildGroupByObjectFromResult(reply.Collection, g.Name, obj)
                    )
                    .ToArray(),
                MinDistance = g.MinDistance,
                MaxDistance = g.MaxDistance,
            }
        );

        var objects = groups.Values.SelectMany(g => g.Objects).ToArray();

        return new Models.GroupByResult(objects, groups);
    }

    internal static GenerativeWeaviateResult BuildGenerativeResult(Protobuf.V1.SearchReply? reply)
    {
        if (reply == null || reply?.Results == null || reply.Results.Count == 0)
            return GenerativeWeaviateResult.Empty;

        return new GenerativeWeaviateResult
        {
            Objects =
                reply
                    .Results?.Select(r =>
                        BuildGenerativeObjectFromResult(
                            reply.Collection,
                            r.Metadata,
                            r.Properties,
                            r.Generative
                        )
                    )
                    .ToList() ?? [],
            Generative = BuildGenerativeResult(reply.GenerativeGroupedResults),
        };
    }

    internal static GenerativeGroupByObject BuildGenerativeGroupByObjectFromResult(
        string collection,
        string groupName,
        Grpc.Protobuf.V1.SearchResult obj
    )
    {
        var result = BuildGenerativeObjectFromResult(
            collection,
            obj.Metadata,
            obj.Properties,
            obj.Generative
        );

        return new GenerativeGroupByObject()
        {
            UUID = result.UUID,
            Collection = result.Collection,
            Vectors = result.Vectors,
            Properties = result.Properties,
            References = result.References,
            Metadata = result.Metadata,
            Generative = result.Generative,
            BelongsToGroup = groupName,
        };
    }

    internal static Models.GenerativeGroupByResult BuildGenerativeGroupByResult(
        Protobuf.V1.SearchReply? reply
    )
    {
        if (reply?.GroupByResults == null || reply.GroupByResults.Count == 0)
            return Models.GenerativeGroupByResult.Empty;

        var groups = new Dictionary<string, GenerativeWeaviateGroup>();
        foreach (var g in reply.GroupByResults)
        {
            var generative = BuildGenerativeResult(g.GenerativeResult);

#pragma warning disable CS0612 // Member Generative is obsolete
            // Fallback for Weaviate versions that still populate deprecated fields and leave the new fields empty.
            if (generative is { Values.Count: 0 } && g.Generative?.Result is not null)
            {
                generative = new GenerativeResult(BuildGenerativeReplyFromResult([g.Generative]));
            }
#pragma warning restore CS0612 // Type or member is obsolete

            var groupObjects = g
                .Objects.Select(obj =>
                    BuildGenerativeGroupByObjectFromResult(reply.Collection, g.Name, obj)
                )
                .ToArray();

            var group = new GenerativeWeaviateGroup
            {
                Name = g.Name,
                Objects = groupObjects,
                Generative = generative,
                MinDistance = g.MinDistance,
                MaxDistance = g.MaxDistance,
            };
            // You can add a breakpoint or debug here for each group 'g' or 'group'
            groups[g.Name] = group;
        }

        var objects = groups.Values.SelectMany(g => g.Objects).ToArray();

        GenerativeResult gs = BuildGenerativeResult(reply.GenerativeGroupedResults);

#pragma warning disable CS0612 // Members HasGenerativeGroupedResult and GenerativeGroupedResult are obsolete
        // Fallback for Weaviate versions that still populate deprecated fields and leave the new fields empty.
        if (
            gs is { Values.Count: 0 }
            && reply is { HasGenerativeGroupedResult: true, GenerativeGroupedResult: not null }
        )
        {
            gs = new GenerativeResult(
                [
                    new GenerativeReply(
                        Result: reply.GenerativeGroupedResult,
                        Debug: null,
                        Metadata: null
                    ),
                ]
            );
        }
#pragma warning restore CS0612 // Type or member is obsolete

        var result = new Models.GenerativeGroupByResult(objects, groups, gs);

        return result;
    }
}
