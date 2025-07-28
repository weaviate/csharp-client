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
        Guid? after = null
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
                    Path = { groupBy.PropertyName },
                    NumberOfGroups = Convert.ToInt32(groupBy.NumberOfGroups),
                    ObjectsPerGroup = Convert.ToInt32(groupBy.ObjectsPerGroup),
                }
                : null,
            Metadata = metadataRequest,
            Properties = MakePropsRequest(fields, reference),
        };

        if (after is not null)
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

    private static void BuildNearText(
        string query,
        double? distance,
        double? certainty,
        SearchRequest request,
        Move? moveTo,
        Move? moveAway
    )
    {
        request.NearText = new NearTextSearch
        {
            Query = { query },
            // Targets = null,
            // VectorForTargets = { },
        };

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

        if (distance is not null)
        {
            request.NearText.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            request.NearText.Certainty = certainty.Value;
        }
    }

    private static void BuildNearVector(
        VectorContainer vector,
        double? distance,
        double? certainty,
        string? targetVector,
        SearchRequest request
    )
    {
        request.NearVector = new() { Vectors = { } };

        foreach (var v in vector)
        {
            request.NearVector.Vectors.Add(
                new Vectors
                {
                    Name = v.Key,
                    Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(
                        v.Value.ValueType
                    )
                        ? Vectors.Types.VectorType.MultiFp32
                        : Vectors.Types.VectorType.SingleFp32,
                    VectorBytes = v.Value.ToByteString(),
                }
            );
        }

        if (distance.HasValue)
        {
            request.NearVector.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            request.NearVector.Certainty = certainty.Value;
        }

        if (!string.IsNullOrEmpty(targetVector))
        {
            request.NearVector.Targets = new()
            {
                Combination = CombinationMethod.Unspecified,
                TargetVectors = { targetVector },
            };
        }
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
        VectorContainer? vector = null,
        string[]? queryProperties = null,
        string? fusionType = null,
        float? maxVectorDistance = null,
        object? bm25Operator = null,
        string? targetVector = null
    )
    {
        // TODO HybridVectorType: vector is a Union of either VectorContainer, or HybridNearText, or HybridNearVector

        request.HybridSearch = new Hybrid();

        if (!string.IsNullOrEmpty(query))
        {
            request.HybridSearch.Query = query;
        }
        else
        {
            alpha = 1.0f; // Default alpha if no query is provided
        }

        if (!string.IsNullOrEmpty(targetVector))
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

        if (vector is not null)
        {
            foreach (var v in vector)
            {
                request.HybridSearch.Vectors.Add(
                    new Vectors
                    {
                        Name = v.Key,
                        Type = typeof(System.Collections.IEnumerable).IsAssignableFrom(
                            v.Value.ValueType
                        )
                            ? Vectors.Types.VectorType.MultiFp32
                            : Vectors.Types.VectorType.SingleFp32,
                        VectorBytes = v.Value.ToByteString(),
                    }
                );
            }
        }
        if (queryProperties is not null)
        {
            request.HybridSearch.Properties.AddRange(queryProperties);
        }
        if (!string.IsNullOrEmpty(fusionType))
        {
            request.HybridSearch.FusionType = Enum.Parse<Hybrid.Types.FusionType>(fusionType);
        }
        if (maxVectorDistance.HasValue)
        {
            request.HybridSearch.VectorDistance = maxVectorDistance.Value;
        }
    }

    internal async Task<WeaviateResult> FetchObjects(
        string collection,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null,
        Guid? after = null
    ) =>
        BuildResult(
            collection,
            await InternalFetchObjects(
                collection,
                filter,
                sort,
                limit,
                fields,
                reference,
                metadata,
                after
            )
        );

    private async Task<SearchReply> InternalFetchObjects(
        string collection,
        Filter? filter = null,
        IEnumerable<Sort>? sort = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null,
        Guid? after = null
    )
    {
        var req = BaseSearchRequest(
            collection,
            filter: filter?.InternalFilter,
            sort: sort?.Select(s => s.InternalSort),
            limit: limit,
            fields: fields,
            metadata: metadata,
            reference: reference,
            after: after
        );

        SearchReply? reply = await _grpcClient.SearchAsync(req, headers: _defaultHeaders);

        return reply;
    }

    public async Task<WeaviateResult> SearchNearVector(
        string collection,
        VectorContainer vector,
        float? distance = null,
        float? certainty = null,
        string? targetVector = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    ) =>
        BuildResult(
            collection,
            await InternalSearchNearVector(
                collection,
                vector,
                distance,
                certainty,
                targetVector,
                limit,
                fields,
                groupBy: null,
                reference,
                metadata
            )
        );

    public async Task<Models.GroupByResult> SearchNearVector(
        string collection,
        VectorContainer vector,
        GroupByRequest? groupBy,
        float? distance = null,
        float? certainty = null,
        string? targetVector = null,
        uint? limit = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    ) =>
        BuildGroupByResult(
            collection,
            await InternalSearchNearVector(
                collection,
                vector,
                distance,
                certainty,
                targetVector,
                limit,
                fields,
                groupBy,
                reference,
                metadata
            )
        );

    private async Task<SearchReply?> InternalSearchNearVector(
        string collection,
        VectorContainer vector,
        float? distance = null,
        float? certainty = null,
        string? targetVector = null,
        uint? limit = null,
        string[]? fields = null,
        GroupByRequest? groupBy = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            groupBy: groupBy,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildNearVector(vector, distance, certainty, targetVector, request);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return reply;
    }

    public async Task<WeaviateResult> SearchNearText(
        string collection,
        string query,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    ) =>
        BuildResult(
            collection,
            await InternalSearchNearText(
                collection,
                query,
                distance,
                certainty,
                limit,
                moveTo,
                moveAway,
                fields,
                groupBy: null,
                reference,
                metadata
            )
        );

    public async Task<Models.GroupByResult> SearchNearText(
        string collection,
        string query,
        Models.GroupByRequest? groupBy,
        float? distance = null,
        float? certainty = null,
        uint? limit = null,
        Move? moveTo = null,
        Move? moveAway = null,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    ) =>
        BuildGroupByResult(
            collection,
            await InternalSearchNearText(
                collection,
                query,
                distance,
                certainty,
                limit,
                moveTo,
                moveAway,
                fields,
                groupBy,
                reference,
                metadata
            )
        );

    private async Task<SearchReply> InternalSearchNearText(
        string collection,
        string query,
        float? distance,
        float? certainty,
        uint? limit,
        Move? moveTo,
        Move? moveAway,
        string[]? fields = null,
        Models.GroupByRequest? groupBy = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: limit,
            fields: fields,
            groupBy: groupBy,
            metadata: metadata,
            reference: reference
        );

        BuildNearText(query, distance, certainty, request, moveTo, moveAway);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return reply;
    }

    public async Task<WeaviateResult> SearchBM25(
        string collection,
        string query,
        string[]? searchFields,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    ) =>
        BuildResult(
            collection,
            await InternalSearchBM25(collection, query, searchFields, fields, reference, metadata)
        );

    private async Task<SearchReply> InternalSearchBM25(
        string collection,
        string query,
        string[]? searchFields,
        string[]? fields = null,
        IList<QueryReference>? reference = null,
        MetadataQuery? metadata = null
    )
    {
        var request = BaseSearchRequest(
            collection,
            filter: null,
            limit: null,
            groupBy: null,
            fields: fields,
            metadata: metadata,
            reference: reference
        );

        BuildBM25(request, query, properties: searchFields);

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return reply;
    }

    public async Task<WeaviateResult> SearchHybrid(
        string collection,
        string? query = null,
        float? alpha = null,
        VectorContainer? vector = null,
        string[]? queryProperties = null,
        string? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        object? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        object? rerank = null,
        string? targetVector = null,
        string[]? fields = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    ) =>
        BuildResult(
            collection,
            await InternalSearchHybrid(
                collection,
                query,
                alpha,
                vector,
                queryProperties,
                fusionType,
                maxVectorDistance,
                limit,
                offset,
                bm25Operator,
                autoLimit,
                filters,
                rerank,
                targetVector,
                fields,
                groupBy: null,
                returnMetadata,
                returnReferences
            )
        );

    public async Task<Models.GroupByResult> SearchHybrid(
        string collection,
        GroupByRequest groupBy,
        string? query = null,
        float? alpha = null,
        VectorContainer? vector = null,
        string[]? queryProperties = null,
        string? fusionType = null,
        float? maxVectorDistance = null,
        uint? limit = null,
        uint? offset = null,
        object? bm25Operator = null,
        uint? autoLimit = null,
        Filter? filters = null,
        object? rerank = null,
        string? targetVector = null,
        string[]? fields = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    ) =>
        BuildGroupByResult(
            collection,
            await InternalSearchHybrid(
                collection,
                query,
                alpha,
                vector,
                queryProperties,
                fusionType,
                maxVectorDistance,
                limit,
                offset,
                bm25Operator,
                autoLimit,
                filters,
                rerank,
                targetVector,
                fields,
                groupBy,
                returnMetadata,
                returnReferences
            )
        );

    private async Task<SearchReply?> InternalSearchHybrid(
        string collection,
        string? query,
        float? alpha,
        VectorContainer? vector,
        string[]? queryProperties,
        string? fusionType,
        float? maxVectorDistance,
        uint? limit,
        uint? offset,
        object? bm25Operator,
        uint? autoLimit,
        Filter? filters,
        object? rerank,
        string? targetVector,
        string[]? fields,
        GroupByRequest? groupBy = null,
        MetadataQuery? returnMetadata = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        if (vector is null && string.IsNullOrEmpty(query))
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
            fields: fields,
            metadata: returnMetadata,
            reference: returnReferences
        );

        BuildHybrid(
            request,
            query,
            alpha,
            vector,
            queryProperties,
            fusionType,
            maxVectorDistance,
            bm25Operator,
            targetVector
        );

        SearchReply? reply = await _grpcClient.SearchAsync(request, headers: _defaultHeaders);

        return reply;
    }
}
