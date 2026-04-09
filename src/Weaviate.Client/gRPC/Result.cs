using System.Collections;
using System.Dynamic;
using Weaviate.Client.Models;

namespace Weaviate.Client.Grpc;

/// <summary>
/// The weaviate grpc client class
/// </summary>
internal partial class WeaviateGrpcClient
{
    /// <summary>
    /// Makes the props request using the specified fields
    /// </summary>
    /// <param name="fields">The fields</param>
    /// <param name="reference">The reference</param>
    /// <returns>The req</returns>
    private static Protobuf.V1.PropertiesRequest? MakePropsRequest(
        string[]? fields,
        IList<QueryReference>? reference
    )
    {
        if (fields is null && reference is null)
            return null;

        var req = new Protobuf.V1.PropertiesRequest();

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

    /// <summary>
    /// Makes the ref props request using the specified reference
    /// </summary>
    /// <param name="reference">The reference</param>
    /// <returns>The grpc protobuf ref properties request</returns>
    private static Protobuf.V1.RefPropertiesRequest? MakeRefPropsRequest(QueryReference? reference)
    {
        if (reference is null)
            return null;

        return new Protobuf.V1.RefPropertiesRequest()
        {
            Metadata = new Protobuf.V1.MetadataRequest()
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

    /// <summary>
    /// Builds the metadata from result using the specified metadata
    /// </summary>
    /// <param name="metadata">The metadata</param>
    /// <returns>The metadata</returns>
    internal static Metadata BuildMetadataFromResult(Protobuf.V1.MetadataResult metadata)
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

    /// <summary>
    /// Builds the vectors from result using the specified metadata result
    /// </summary>
    /// <param name="metadataResult">The metadata result</param>
    /// <returns>The vectors</returns>
    internal static Vectors BuildVectorsFromResult(Protobuf.V1.MetadataResult metadataResult)
    {
        if (metadataResult.VectorBytes != null && metadataResult.VectorBytes.Length > 0)
        {
            return metadataResult.VectorBytes.FromByteString<float>().ToArray();
        }

        var vectors = metadataResult.Vectors;

        return new Vectors(vectors.Select(vector => vector.FromByteString<float>()));
    }

    /// <summary>
    /// Builds the group by object from result using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="groupName">The group name</param>
    /// <param name="obj">The obj</param>
    /// <returns>The group by object</returns>
    internal static GroupByObject BuildGroupByObjectFromResult(
        string collection,
        string groupName,
        Protobuf.V1.SearchResult obj
    )
    {
        var metadata = obj.Metadata;
        var properties = obj.Properties;

        return new GroupByObject(BuildObjectFromResult(collection, metadata, properties))
        {
            BelongsToGroup = groupName,
        };
    }

    /// <summary>
    /// Makes the non refs using the specified result
    /// </summary>
    /// <param name="result">The result</param>
    /// <returns>The eo base</returns>
    private static ExpandoObject MakeNonRefs(Protobuf.V1.Properties result)
    {
        var eoBase = new ExpandoObject();

        if (result is null)
        {
            return eoBase;
        }

        var eo = eoBase as IDictionary<string, object>;

        foreach (var r in result.Fields)
        {
            Protobuf.V1.Value.KindOneofCase kind = r.Value.KindCase;
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
                    eo[r.Key] = new GeoCoordinate(
                        r.Value.GeoValue.Latitude,
                        r.Value.GeoValue.Longitude
                    );
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.BlobValue:
                    eo[r.Key] = r.Value.BlobValue;
                    break;
                case Grpc.Protobuf.V1.Value.KindOneofCase.PhoneValue:
                    eo[r.Key] = new PhoneNumber
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

    /// <summary>
    /// Makes the list value using the specified list
    /// </summary>
    /// <param name="list">The list</param>
    /// <returns>The list</returns>
    private static IList MakeListValue(Protobuf.V1.ListValue list)
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

    /// <summary>
    /// Builds the object from result using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="metadata">The metadata</param>
    /// <param name="properties">The properties</param>
    /// <returns>The weaviate object</returns>
    internal static WeaviateObject BuildObjectFromResult(
        string collection,
        Protobuf.V1.MetadataResult metadata,
        Protobuf.V1.PropertiesResult properties
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

    /// <summary>
    /// Builds the generative reply from result using the specified generative
    /// </summary>
    /// <param name="generative">The generative</param>
    /// <returns>A list of models generative reply</returns>
    internal static IList<GenerativeReply> BuildGenerativeReplyFromResult(
        IEnumerable<Protobuf.V1.GenerativeReply>? generative
    )
    {
        return generative
                ?.Select(g => new GenerativeReply(
                    Text: g.Result
                //Debug: g.Debug is null ? null : new GenerativeDebug(g.Debug.FullPrompt),
                //Metadata: g.Metadata
                ))
                .ToList()
            ?? [];
    }

    /// <summary>
    /// Builds the generative object from result using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="metadata">The metadata</param>
    /// <param name="properties">The properties</param>
    /// <param name="generative">The generative</param>
    /// <returns>The generative weaviate object</returns>
    internal static GenerativeWeaviateObject BuildGenerativeObjectFromResult(
        string collection,
        Protobuf.V1.MetadataResult metadata,
        Protobuf.V1.PropertiesResult properties,
        Protobuf.V1.GenerativeResult generative
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

    /// <summary>
    /// Builds the generative result using the specified generative
    /// </summary>
    /// <param name="generative">The generative</param>
    /// <returns>The generative result</returns>
    private static GenerativeResult BuildGenerativeResult(Protobuf.V1.GenerativeResult? generative)
    {
        return new GenerativeResult(BuildGenerativeReplyFromResult(generative?.Values));
    }

    /// <summary>
    /// Makes the refs using the specified ref props
    /// </summary>
    /// <param name="refProps">The ref props</param>
    /// <returns>The result</returns>
    internal static IDictionary<string, IList<WeaviateObject>> MakeRefs(
        Google.Protobuf.Collections.RepeatedField<Protobuf.V1.RefPropertiesResult> refProps
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

    /// <summary>
    /// Builds a <see cref="Models.QueryProfile"/> from a gRPC <see cref="Protobuf.V1.QueryProfile"/>.
    /// Returns null when the reply has no query profile.
    /// </summary>
    internal static Models.QueryProfile? BuildQueryProfile(Protobuf.V1.QueryProfile? profile)
    {
        if (profile is null)
            return null;

        return new Models.QueryProfile
        {
            Shards = profile
                .Shards.Select(s => new Models.ShardProfile
                {
                    Name = s.Name,
                    Node = s.Node,
                    Searches = s.Searches.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new Models.SearchProfile
                        {
                            Details = new Dictionary<string, string>(kvp.Value.Details),
                        }
                    ),
                })
                .ToList(),
        };
    }

    /// <summary>
    /// Builds the result using the specified reply
    /// </summary>
    /// <param name="reply">The reply</param>
    /// <returns>The weaviate result</returns>
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
                    .ToList()
                ?? [],
            QueryProfile = BuildQueryProfile(reply?.QueryProfile),
        };
    }

    /// <summary>
    /// Builds the group by result using the specified reply
    /// </summary>
    /// <param name="reply">The reply</param>
    /// <returns>The models group by result</returns>
    internal static GroupByResult BuildGroupByResult(Protobuf.V1.SearchReply? reply)
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

        return new GroupByResult(objects, groups)
        {
            QueryProfile = BuildQueryProfile(reply?.QueryProfile),
        };
    }

    /// <summary>
    /// Builds the generative result using the specified reply
    /// </summary>
    /// <param name="reply">The reply</param>
    /// <returns>The generative weaviate result</returns>
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
                    .ToList()
                ?? [],
            Generative = BuildGenerativeResult(reply.GenerativeGroupedResults),
            QueryProfile = BuildQueryProfile(reply?.QueryProfile),
        };
    }

    /// <summary>
    /// Builds the generative group by object from result using the specified collection
    /// </summary>
    /// <param name="collection">The collection</param>
    /// <param name="groupName">The group name</param>
    /// <param name="obj">The obj</param>
    /// <returns>The generative group by object</returns>
    internal static GenerativeGroupByObject BuildGenerativeGroupByObjectFromResult(
        string collection,
        string groupName,
        Protobuf.V1.SearchResult obj
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

    /// <summary>
    /// Builds the generative group by result using the specified reply
    /// </summary>
    /// <param name="reply">The reply</param>
    /// <returns>The result</returns>
    internal static GenerativeGroupByResult BuildGenerativeGroupByResult(
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
            gs = new GenerativeResult([
                new GenerativeReply(
                    Text: reply.GenerativeGroupedResult
                // Debug: null,
                // Metadata: null
                ),
            ]);
        }
#pragma warning restore CS0612 // Type or member is obsolete

        var result = new GenerativeGroupByResult(objects, groups, gs)
        {
            QueryProfile = BuildQueryProfile(reply?.QueryProfile),
        };

        return result;
    }
}
