using System.Collections;
using System.Dynamic;
using Google.Protobuf.Collections;
using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private static V1.PropertiesRequest? MakePropsRequest(
        string[]? fields,
        IList<QueryReference>? reference
    )
    {
        if (fields is null && reference is null)
            return null;

        var req = new V1.PropertiesRequest();

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

    private static V1.RefPropertiesRequest? MakeRefPropsRequest(QueryReference? reference)
    {
        if (reference is null)
            return null;

        return new V1.RefPropertiesRequest()
        {
            Metadata = new V1.MetadataRequest()
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

    internal static Metadata BuildMetadataFromResult(V1.MetadataResult metadata)
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

    internal static Vectors BuildVectorsFromResult(RepeatedField<V1.Vectors> vectors)
    {
        var result = new Vectors();

        foreach (var vector in vectors)
        {
            // TODO Handle multi-vectors and other types
            var vectorData = vector.VectorBytes.FromByteString<float>();
            result.Add(vector.Name, [.. vectorData]);
        }

        return result;
    }

    internal static GroupByObject BuildGroupByObjectFromResult(
        string collection,
        string groupName,
        V1.SearchResult obj
    )
    {
        var metadata = obj.Metadata;
        var properties = obj.Properties;

        return new GroupByObject(BuildObjectFromResult(collection, metadata, properties))
        {
            BelongsToGroup = groupName,
        };
    }

    private static ExpandoObject MakeNonRefs(V1.Properties result)
    {
        var eoBase = new ExpandoObject();

        if (result is null)
        {
            return eoBase;
        }

        var eo = eoBase as IDictionary<string, object>;

        foreach (var r in result.Fields)
        {
            V1.Value.KindOneofCase kind = r.Value.KindCase;
            switch (kind)
            {
                case V1.Value.KindOneofCase.None:
                case V1.Value.KindOneofCase.NullValue:
                    continue;
                case V1.Value.KindOneofCase.NumberValue:
                    eo[r.Key] = r.Value.NumberValue;
                    break;
                case V1.Value.KindOneofCase.BoolValue:
                    eo[r.Key] = r.Value.BoolValue;
                    break;
                case V1.Value.KindOneofCase.ObjectValue:
                    eo[r.Key] = MakeNonRefs(r.Value.ObjectValue) ?? new object { };
                    break;
                case V1.Value.KindOneofCase.ListValue:
                    eo[r.Key] = MakeListValue(r.Value.ListValue);
                    break;
                case V1.Value.KindOneofCase.DateValue:
                    eo[r.Key] = r.Value.DateValue; // TODO Parse date here?
                    break;
                case V1.Value.KindOneofCase.UuidValue:
                    eo[r.Key] = Guid.Parse(r.Value.UuidValue);
                    break;
                case V1.Value.KindOneofCase.IntValue:
                    eo[r.Key] = r.Value.IntValue;
                    break;
                case V1.Value.KindOneofCase.GeoValue:
                    eo[r.Key] = new Models.GeoCoordinate(
                        r.Value.GeoValue.Latitude,
                        r.Value.GeoValue.Longitude
                    );
                    break;
                case V1.Value.KindOneofCase.BlobValue:
                    eo[r.Key] = r.Value.BlobValue;
                    break;
                case V1.Value.KindOneofCase.PhoneValue:
                    eo[r.Key] = r.Value.PhoneValue;
                    break;
                case V1.Value.KindOneofCase.TextValue:
                    eo[r.Key] = r.Value.TextValue;
                    break;
            }
        }

        return eoBase;
    }

    private static IList MakeListValue(V1.ListValue list)
    {
        switch (list.KindCase)
        {
            case V1.ListValue.KindOneofCase.BoolValues:
                return list.BoolValues.Values.ToArray();
            case V1.ListValue.KindOneofCase.ObjectValues:
                return list.ObjectValues.Values.Select(v => MakeNonRefs(v)).ToArray();
            case V1.ListValue.KindOneofCase.DateValues:
                return list.DateValues.Values.Select(v => DateTime.Parse(v)).ToArray();
            case V1.ListValue.KindOneofCase.UuidValues:
                return list.UuidValues.Values.Select(v => Guid.Parse(v)).ToArray();
            case V1.ListValue.KindOneofCase.TextValues:
                return list.TextValues.Values;
            case V1.ListValue.KindOneofCase.IntValues:
                return list.IntValues.Values.FromByteString<long>().ToArray();
            case V1.ListValue.KindOneofCase.NumberValues:
                return list.NumberValues.Values.FromByteString<double>().ToArray();
            case V1.ListValue.KindOneofCase.None:
            default:
                return new List<object> { };
        }
    }

    internal static WeaviateObject BuildObjectFromResult(
        string collection,
        V1.MetadataResult metadata,
        V1.PropertiesResult properties
    )
    {
        return new WeaviateObject
        {
            ID = !string.IsNullOrEmpty(metadata.Id) ? Guid.Parse(metadata.Id) : Guid.Empty,
            Collection = collection,
            Vectors = BuildVectorsFromResult(metadata.Vectors),
            Properties = MakeNonRefs(properties.NonRefProps),
            References = properties.RefPropsRequested
                ? MakeRefs(properties.RefProps)
                : new Dictionary<string, IList<WeaviateObject>>(),
            Metadata = BuildMetadataFromResult(metadata),
        };
    }

    internal static IList<GenerativeReply> BuildGenerativeReplyFromResult(
        IEnumerable<V1.GenerativeReply> generative
    )
    {
        return generative
            .Select(g => new GenerativeReply(
                Result: g.Result,
                Debug: g.Debug is null ? null : new GenerativeDebug(g.Debug.FullPrompt),
                Metadata: g.Metadata
            ))
            .ToList();
    }

    internal static GenerativeWeaviateObject BuildGenerativeObjectFromResult(
        string collection,
        V1.MetadataResult metadata,
        V1.PropertiesResult properties,
        V1.GenerativeResult generative
    )
    {
        var obj = BuildObjectFromResult(collection, metadata, properties);

        return new GenerativeWeaviateObject
        {
            ID = obj.ID,
            Collection = obj.Collection,
            Vectors = obj.Vectors,
            Properties = obj.Properties,
            References = obj.References,
            Metadata = obj.Metadata,
            Generative = BuildGenerativeResult(generative),
        };
    }

    private static GenerativeResult BuildGenerativeResult(V1.GenerativeResult generative)
    {
        return new GenerativeResult(BuildGenerativeReplyFromResult(generative.Values));
    }

    internal static IDictionary<string, IList<WeaviateObject>> MakeRefs(
        RepeatedField<V1.RefPropertiesResult> refProps
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

    internal static WeaviateResult BuildResult(V1.SearchReply? reply)
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

    internal static Models.GroupByResult BuildGroupByResult(V1.SearchReply? reply)
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
            }
        );

        var objects = groups.Values.SelectMany(g => g.Objects).ToArray();

        return new Models.GroupByResult(objects, groups);
    }

    internal static GenerativeWeaviateResult BuildGenerativeResult(V1.SearchReply? reply)
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
        V1.SearchResult obj
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
            ID = result.ID,
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
        V1.SearchReply? reply
    )
    {
        if (reply?.GroupByResults == null || reply.GroupByResults.Count == 0)
            return Models.GenerativeGroupByResult.Empty;

        var groups = reply.GroupByResults.ToDictionary(
            g => g.Name,
            g => new GenerativeWeaviateGroup
            {
                Name = g.Name,
                Objects = g
                    .Objects.Select(obj =>
                        BuildGenerativeGroupByObjectFromResult(reply.Collection, g.Name, obj)
                    )
                    .ToArray(),
            }
        );

        var objects = groups.Values.SelectMany(g => g.Objects).ToArray();

        var result = new Models.GenerativeGroupByResult(
            objects,
            groups,
            BuildGenerativeResult(reply.GenerativeGroupedResults)
        );

        return result;
    }
}
