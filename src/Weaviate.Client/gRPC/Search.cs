using Weaviate.Client.Models;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private static SearchRequest BaseSearchRequest(
        string collection,
        Filters? filter = null,
        IEnumerable<SortBy>? sort = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        GroupByRequest? groupBy = null,
        MetadataQuery? metadata = null,
        IList<QueryReference>? reference = null,
        string[]? fields = null,
        Guid? after = null,
        string? tenant = null
    )
    {
        var metadataRequest = new MetadataRequest()
        {
            Uuid = true,
            Vector = metadata?.Vector ?? false,
            LastUpdateTimeUnix = metadata?.LastUpdateTime ?? false,
            CreationTimeUnix = metadata?.CreationTime ?? false,
            Certainty = metadata?.Certainty ?? false,
            Distance = metadata?.Distance ?? false,
            Score = metadata?.Score ?? false,
            ExplainScore = metadata?.ExplainScore ?? false,
            IsConsistent = metadata?.IsConsistent ?? false,
        };

        metadataRequest.Vectors.AddRange(metadata?.Vectors.ToArray() ?? []);

        var request = new SearchRequest()
        {
            Collection = collection,
            Filters = filter,
#pragma warning disable CS0612 // Type or member is obsolete
            Uses123Api = true,
            Uses125Api = true,
#pragma warning restore CS0612 // Type or member is obsolete
            Uses127Api = true,
            GroupBy = groupBy is not null
                ? new GroupBy()
                {
                    Path = { groupBy.PropertyName.ToLowerInvariant() },
                    NumberOfGroups = Convert.ToInt32(groupBy.NumberOfGroups),
                    ObjectsPerGroup = Convert.ToInt32(groupBy.ObjectsPerGroup),
                }
                : null,
            Metadata = metadataRequest,
            Properties = MakePropsRequest(fields, reference),
            Tenant = tenant ?? string.Empty,
        };

        if (after.HasValue)
        {
            request.After = after.ToString();
        }
        if (sort is not null)
        {
            request.SortBy.AddRange(sort);
        }
        if (autoCut.HasValue)
        {
            request.Autocut = autoCut.Value;
        }
        if (offset.HasValue)
        {
            request.Offset = offset.Value;
        }
        if (limit.HasValue)
        {
            request.Limit = limit.Value;
        }

        return request;
    }

    private static PropertiesRequest? MakePropsRequest(
        string[]? fields,
        IList<QueryReference>? reference
    )
    {
        if (fields is null && reference is null)
            return null;

        var req = new PropertiesRequest();

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

    private static RefPropertiesRequest? MakeRefPropsRequest(QueryReference? reference)
    {
        if (reference is null)
            return null;

        return new RefPropertiesRequest()
        {
            Metadata = new MetadataRequest()
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
            Properties = MakePropsRequest(reference.Fields, reference.References),
            ReferenceProperty = reference.LinkOn,
        };
    }

    private static NearTextSearch BuildNearText(
        string query,
        double? distance,
        double? certainty,
        Move? moveTo,
        Move? moveAway
    )
    {
        var nearText = new NearTextSearch
        {
            Query = { query },
            // Targets = null,
            // VectorForTargets = { },
        };

        if (moveTo is not null)
        {
            var uuids = moveTo.Objects is null ? [] : moveTo.Objects.Select(x => x.ToString());
            var concepts = moveTo.Concepts is null ? new string[] { } : moveTo.Concepts;
            nearText.MoveTo = new NearTextSearch.Types.Move
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
            nearText.MoveAway = new NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveAway.Force,
            };
        }

        if (distance is not null)
        {
            nearText.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            nearText.Certainty = certainty.Value;
        }

        return nearText;
    }

    private static NearVector BuildNearVector(
        Models.Vectors vector,
        double? distance,
        double? certainty,
        string[]? targetVector
    )
    {
        NearVector nearVector = new() { Vectors = { } };

        nearVector.Vectors.Add(
            vector.Select(v => new V1.Vectors
            {
                Name = v.Key,
                Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(v.Value.ValueType)
                    ? V1.Vectors.Types.VectorType.MultiFp32
                    : V1.Vectors.Types.VectorType.SingleFp32,
                VectorBytes = v.Value.ToByteString(),
            })
        );

        if (distance.HasValue)
        {
            nearVector.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            nearVector.Certainty = certainty.Value;
        }

        if (targetVector is not null && targetVector.Length > 0)
        {
            nearVector.Targets = new()
            {
                Combination = CombinationMethod.Unspecified,
                TargetVectors = { targetVector },
            };
        }

        return nearVector;
    }

    private static void BuildBM25(SearchRequest request, string query, string[]? properties = null)
    {
        request.Bm25Search = new BM25() { Query = query };

        if (properties is not null)
        {
            request.Bm25Search.Properties.AddRange(properties);
        }
    }

    private static void BuildHybrid(
        SearchRequest request,
        string? query = null,
        float? alpha = null,
        Models.Vectors? vector = null,
        HybridNearVector? nearVector = null,
        HybridNearText? nearText = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        BM25Operator? bm25Operator = null,
        string[]? targetVector = null
    )
    {
        request.HybridSearch = new Hybrid();

        if (!string.IsNullOrEmpty(query))
        {
            request.HybridSearch.Query = query;
        }
        else
        {
            alpha = 1.0f; // Default alpha if no query is provided
        }

        if (targetVector is not null && targetVector.Length > 0)
        {
            request.HybridSearch.Targets = new()
            {
                Combination = CombinationMethod.Unspecified,
                TargetVectors = { targetVector },
            };
        }

        if (alpha.HasValue)
        {
            request.HybridSearch.Alpha = alpha.Value;
        }

        if (vector is not null && nearText is null && nearVector is null)
        {
            foreach (var v in vector)
            {
                request.HybridSearch.Vectors.Add(
                    new V1.Vectors
                    {
                        Name = v.Key,
                        Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(
                            v.Value.ValueType
                        )
                            ? V1.Vectors.Types.VectorType.MultiFp32
                            : V1.Vectors.Types.VectorType.SingleFp32,
                        VectorBytes = v.Value.ToByteString(),
                    }
                );
            }
        }

        if (vector is null && nearText is not null && nearVector is null)
        {
            request.HybridSearch.NearText = BuildNearText(
                nearText.Query,
                nearText.Distance,
                nearText.Certainty,
                nearText.MoveTo,
                nearText.MoveAway
            );
        }

        if (
            vector is null
            && nearText is null
            && nearVector is not null
            && nearVector.Vector is not null
        )
        {
            if (nearVector.Vector.Count == 1 && targetVector is null)
            {
                // If only one vector is provided, use it directly
                var singleVector = nearVector.Vector.First();
                request.HybridSearch.NearVector = new NearVector
                {
                    Vectors =
                    {
                        new V1.Vectors
                        {
                            Name = singleVector.Key,
                            Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(
                                singleVector.Value.ValueType
                            )
                                ? V1.Vectors.Types.VectorType.MultiFp32
                                : V1.Vectors.Types.VectorType.SingleFp32,
                            VectorBytes = singleVector.Value.ToByteString(),
                        },
                    },
                };
            }
            else
            {
                List<VectorForTarget> vectorForTargetsTmp = new();
                List<string> targetVectorTmp = new();

                if (targetVector is not null && targetVector.Length > 0)
                {
                    targetVectorTmp.AddRange(targetVector);
                }
                else
                {
                    // If no target vector is specified, use the keys from nearVector
                    targetVectorTmp.AddRange(
                        nearVector.Vector.Keys.Where(tv => string.IsNullOrEmpty(tv) is false)
                    );
                }

                foreach (var v in nearVector.Vector)
                {
                    vectorForTargetsTmp.Add(
                        new()
                        {
                            Name = v.Key,
                            Vectors =
                            {
                                new V1.Vectors
                                {
                                    Name = v.Key,
                                    Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(
                                        v.Value.ValueType
                                    )
                                        ? V1.Vectors.Types.VectorType.MultiFp32
                                        : V1.Vectors.Types.VectorType.SingleFp32,
                                    VectorBytes = v.Value.ToByteString(),
                                },
                            },
                        }
                    );
                }

                NearVector nv = new()
                {
                    VectorForTargets = { vectorForTargetsTmp },
                    Targets = new()
                    {
                        Combination = CombinationMethod.Unspecified,
                        TargetVectors = { targetVectorTmp },
                    },
                };

                request.HybridSearch.NearVector = nv;
            }

            if (nearVector.Distance.HasValue)
            {
                request.HybridSearch.NearVector.Distance = nearVector.Distance.Value;
            }

            if (nearVector.Certainty.HasValue)
            {
                request.HybridSearch.NearVector.Certainty = nearVector.Certainty.Value;
            }
        }

        if (queryProperties is not null)
        {
            request.HybridSearch.Properties.AddRange(queryProperties);
        }
        if (fusionType.HasValue)
        {
            request.HybridSearch.FusionType = fusionType switch
            {
                HybridFusion.Ranked => Hybrid.Types.FusionType.Ranked,
                HybridFusion.RelativeScore => Hybrid.Types.FusionType.RelativeScore,
                _ => Hybrid.Types.FusionType.Unspecified,
            };
        }
        if (maxVectorDistance.HasValue)
        {
            request.HybridSearch.VectorDistance = maxVectorDistance.Value;
        }
        if (bm25Operator != null)
        {
            request.HybridSearch.Bm25SearchOperator = new()
            {
                Operator = bm25Operator switch
                {
                    BM25Operator.And => V1.SearchOperatorOptions.Types.Operator.And,
                    BM25Operator.Or => V1.SearchOperatorOptions.Types.Operator.Or,
                    _ => V1.SearchOperatorOptions.Types.Operator.Unspecified,
                },
                MinimumOrTokensMatch = (bm25Operator as BM25Operator.Or)?.MinimumMatch ?? 1,
            };
        }
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> FetchObjects(
        string collection,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        uint? limit = null,
        string[]? fields = null,
        GroupByRequest? groupBy = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null,
        Guid? after = null,
        string? tenant = null
    )
    {
        var req = BaseSearchRequest(
            collection,
            filter: filter?.InternalFilter,
            sort: sort?.Select(s => s.InternalSort),
            limit: limit,
            fields: fields,
            groupBy: groupBy,
            metadata: metadata,
            reference: reference,
            after: after,
            tenant: tenant
        );

        SearchReply? reply = await _grpcClient.SearchAsync(req, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> SearchNearVector(
        string collection,
        Models.Vectors vector,
        GroupByRequest? groupBy = null,
        float? distance = null,
        float? certainty = null,
        string[]? targetVector = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null,
        string? tenant = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            groupBy: groupBy,
            fields: fields,
            metadata: metadata,
            reference: reference,
            tenant: tenant
        );

        request.NearVector = BuildNearVector(vector, distance, certainty, targetVector);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> SearchNearText(
        string collection,
        string query,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        string[]? fields = null,
        GroupByRequest? groupBy = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null,
        string? tenant = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            fields: fields,
            groupBy: groupBy,
            metadata: metadata,
            reference: reference,
            tenant: tenant
        );

        request.NearText = BuildNearText(query, distance, certainty, moveTo, moveAway);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> SearchBM25(
        string collection,
        string query,
        string[]? searchFields,
        string[]? fields = null,
        GroupByRequest? groupBy = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null,
        string? tenant = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: null,
            groupBy: groupBy,
            fields: fields,
            metadata: metadata,
            reference: reference,
            tenant: tenant
        );

        BuildBM25(request, query, properties: searchFields);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> SearchHybrid(
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
        object? rerank = null,
        string[]? targetVector = null,
        string[]? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null,
        string? tenant = null
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
            filter: filters?.InternalFilter,
            autoCut: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            fields: returnProperties,
            metadata: returnMetadata,
            reference: returnReferences
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

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> SearchNearObject(
        string collection,
        Guid objectID,
        double? certainty,
        double? distance,
        uint? limit,
        uint? offset,
        uint? autoLimit,
        Filter? filters,
        GroupByRequest? groupBy,
        object? rerank,
        string[]? targetVector,
        MetadataQuery? returnMetadata,
        string[]? returnProperties,
        IList<QueryReference>? returnReferences,
        string? tenant = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: filters?.InternalFilter,
            autoCut: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            fields: returnProperties,
            metadata: returnMetadata,
            reference: returnReferences
        );

        BuildNearObject(request, objectID, certainty, distance, targetVector);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }

    private void BuildNearObject(
        SearchRequest request,
        Guid objectID,
        double? certainty,
        double? distance,
        string[]? targetVector
    )
    {
        request.NearObject = new NearObject { Id = objectID.ToString() };

        if (certainty.HasValue)
        {
            request.NearObject.Certainty = certainty.Value;
        }

        if (distance.HasValue)
        {
            request.NearObject.Distance = distance.Value;
        }

        request.NearObject.Targets = BuildTargetVector(targetVector);
    }

    private static Targets? BuildTargetVector(string[]? targetVector)
    {
        if (targetVector is not null && targetVector.Length > 0)
        {
            return new Targets
            {
                Combination = CombinationMethod.Unspecified,
                TargetVectors = { targetVector },
            };
        }

        return null;
    }

    internal async Task<(
        WeaviateResult result,
        Models.GroupByResult group,
        bool isGroups
    )> SearchNearMedia(
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
        object? rerank,
        string? tenant,
        string[]? targetVector,
        MetadataQuery? returnMetadata,
        string[]? returnProperties,
        IList<QueryReference>? returnReferences
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: filters?.InternalFilter,
            autoCut: autoLimit,
            limit: limit,
            offset: offset,
            groupBy: groupBy,
            fields: returnProperties,
            metadata: returnMetadata,
            reference: returnReferences,
            tenant: tenant
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

                request.NearImage.Targets = BuildTargetVector(targetVector);

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

                request.NearVideo.Targets = BuildTargetVector(targetVector);
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

                request.NearAudio.Targets = BuildTargetVector(targetVector);
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

                request.NearDepth.Targets = BuildTargetVector(targetVector);
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

                request.NearThermal.Targets = BuildTargetVector(targetVector);
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

                request.NearImu.Targets = BuildTargetVector(targetVector);
                break;
            default:
                throw new ArgumentException("Unsupported media type for near media search.");
        }

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return BuildCombinedResult(collection, reply);
    }
}
