using System.Collections;
using System.Dynamic;
using Google.Protobuf.Collections;
using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    internal static Metadata BuildMetadataFromResult(MetadataResult metadata)
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
        };
    }

    internal static VectorContainer BuildVectorsFromResult(RepeatedField<Vectors> vectors)
    {
        var result = new VectorContainer();

        foreach (var vector in vectors)
        {
            var vectorData = vector.VectorBytes.FromByteString<float>();
            result.Add(vector.Name, [.. vectorData]);
        }

        return result;
    }

    internal static GroupByObject BuildGroupByObjectFromResult(
        string collection,
        string groupName,
        SearchResult obj
    )
    {
        var metadata = obj.Metadata;
        var properties = obj.Properties;

        return new GroupByObject(BuildObjectFromResult(collection, metadata, properties))
        {
            BelongsToGroup = groupName,
        };
    }

    private static ExpandoObject MakeNonRefs(Properties result)
    {
        var eoBase = new ExpandoObject();

        if (result is null)
        {
            return eoBase;
        }

        var eo = eoBase as IDictionary<string, object>;

        foreach (var r in result.Fields)
        {
            Value.KindOneofCase kind = r.Value.KindCase;
            switch (kind)
            {
                case Value.KindOneofCase.None:
                case Value.KindOneofCase.NullValue:
                    continue;
                case Value.KindOneofCase.NumberValue:
                    eo[r.Key] = r.Value.NumberValue;
                    break;
                case Value.KindOneofCase.StringValue:
#pragma warning disable CS0612 // Type or member is obsolete
                    eo[r.Key] = r.Value.StringValue;
#pragma warning restore CS0612 // Type or member is obsolete
                    break;
                case Value.KindOneofCase.BoolValue:
                    eo[r.Key] = r.Value.BoolValue;
                    break;
                case Value.KindOneofCase.ObjectValue:
                    eo[r.Key] = MakeNonRefs(r.Value.ObjectValue) ?? new object { };
                    break;
                case Value.KindOneofCase.ListValue:
                    eo[r.Key] = MakeListValue(r.Value.ListValue);
                    break;
                case Value.KindOneofCase.DateValue:
                    eo[r.Key] = r.Value.DateValue; // TODO Parse date here?
                    break;
                case Value.KindOneofCase.UuidValue:
                    eo[r.Key] = Guid.Parse(r.Value.UuidValue);
                    break;
                case Value.KindOneofCase.IntValue:
                    eo[r.Key] = r.Value.IntValue;
                    break;
                case Value.KindOneofCase.GeoValue:
                    eo[r.Key] = new Models.GeoCoordinate(
                        r.Value.GeoValue.Latitude,
                        r.Value.GeoValue.Longitude
                    );
                    break;
                case Value.KindOneofCase.BlobValue:
                    eo[r.Key] = r.Value.BlobValue;
                    break;
                case Value.KindOneofCase.PhoneValue:
                    eo[r.Key] = r.Value.PhoneValue;
                    break;
                case Value.KindOneofCase.TextValue:
                    eo[r.Key] = r.Value.TextValue;
                    break;
            }
        }

        return eoBase;
    }

    private static IList MakeListValue(ListValue list)
    {
        switch (list.KindCase)
        {
            case ListValue.KindOneofCase.BoolValues:
                return list.BoolValues.Values.ToArray();
            case ListValue.KindOneofCase.ObjectValues:
                return list.ObjectValues.Values.Select(v => MakeNonRefs(v)).ToArray();
            case ListValue.KindOneofCase.DateValues:
                return list.DateValues.Values.Select(v => DateTime.Parse(v)).ToArray();
            case ListValue.KindOneofCase.UuidValues:
                return list.UuidValues.Values.Select(v => Guid.Parse(v)).ToArray();
            case ListValue.KindOneofCase.TextValues:
                return list.TextValues.Values;
            case ListValue.KindOneofCase.IntValues:
                return list.IntValues.Values.FromByteString<long>().ToArray();
            case ListValue.KindOneofCase.NumberValues:
                return list.NumberValues.Values.FromByteString<double>().ToArray();
            case ListValue.KindOneofCase.None:
            default:
                return new List<object> { };
        }
    }

    internal static WeaviateObject BuildObjectFromResult(
        string collection,
        MetadataResult metadata,
        PropertiesResult properties
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

    internal static WeaviateResult BuildResult(string collection, SearchReply? reply)
    {
        return new WeaviateResult
        {
            Objects =
                reply?.Results?.Select(r =>
                    BuildObjectFromResult(collection, r.Metadata, r.Properties)
                ) ?? [],
        };
    }

    internal static IDictionary<string, IList<WeaviateObject>> MakeRefs(
        RepeatedField<RefPropertiesResult> refProps
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

    internal static Models.GroupByResult BuildGroupByResult(string collection, SearchReply? reply)
    {
        if (reply?.GroupByResults == null || reply.GroupByResults.Count == 0)
            return (Array.Empty<GroupByObject>(), new Dictionary<string, WeaviateGroup>());

        var groups = reply.GroupByResults.ToDictionary(
            g => g.Name,
            g => new WeaviateGroup
            {
                Name = g.Name,
                Objects = g
                    .Objects.Select(obj => BuildGroupByObjectFromResult(collection, g.Name, obj))
                    .ToArray(),
            }
        );

        var objects = groups.Values.SelectMany(g => g.Objects).ToArray();

        return (objects, groups);
    }
}
