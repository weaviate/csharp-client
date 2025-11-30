using Weaviate.Client.Models;
using Rerank = Weaviate.Client.Models.Rerank;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private static Grpc.Protobuf.V1.SearchRequest BaseSearchRequest(
        string collection,
        Filter? filters = null,
        IEnumerable<Sort>? sort = null,
        uint? autoCut = null,
        uint? limit = null,
        uint? offset = null,
        GroupByRequest? groupBy = null,
        Rerank? rerank = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        SinglePrompt? singlePrompt = null,
        GroupedPrompt? groupedPrompt = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        var metadataRequest = new Protobuf.V1.MetadataRequest()
        {
            Uuid = true,
            LastUpdateTimeUnix = returnMetadata?.LastUpdateTime ?? false,
            CreationTimeUnix = returnMetadata?.CreationTime ?? false,
            Certainty = returnMetadata?.Certainty ?? false,
            Distance = returnMetadata?.Distance ?? false,
            Score = returnMetadata?.Score ?? false,
            ExplainScore = returnMetadata?.ExplainScore ?? false,
            IsConsistent = returnMetadata?.IsConsistent ?? false,
            Vector =
                (includeVectors?.Vectors != null && includeVectors.Vectors.Length > 0)
                    ? false
                    : (includeVectors?.Vectors != null),
            Vectors = { includeVectors?.Vectors ?? [] },
        };

        var request = new Grpc.Protobuf.V1.SearchRequest()
        {
            Collection = collection,
            Filters = filters?.InternalFilter,
#pragma warning disable CS0612 // Type or member is obsolete
            Uses123Api = true,
            Uses125Api = true,
#pragma warning restore CS0612 // Type or member is obsolete
            Uses127Api = true,
            GroupBy = groupBy is not null
                ? new Protobuf.V1.GroupBy()
                {
                    Path = { groupBy.PropertyName.Decapitalize() },
                    NumberOfGroups = Convert.ToInt32(groupBy.NumberOfGroups),
                    ObjectsPerGroup = Convert.ToInt32(groupBy.ObjectsPerGroup),
                }
                : null,
            Metadata = metadataRequest,
            Properties = MakePropsRequest(returnProperties?.ToArray(), returnReferences),
            Tenant = tenant ?? string.Empty,
            Rerank = rerank is not null
                ? new()
                {
                    Property = rerank?.Property ?? string.Empty,
                    Query = rerank?.Query ?? string.Empty,
                }
                : null,
            Generative =
                (singlePrompt is null && groupedPrompt is null)
                    ? null
                    : new V1.GenerativeSearch()
                    {
                        Single = singlePrompt is not null
                            ? new V1.GenerativeSearch.Types.Single()
                            {
                                Prompt = singlePrompt.Prompt,
                                Debug = singlePrompt.Debug,
                                Queries =
                                {
                                    singlePrompt.Provider is null
                                        ? []
                                        : [GetGenerativeProvider(singlePrompt.Provider)],
                                },
                            }
                            : null,
                        Grouped = groupedPrompt is not null
                            ? new V1.GenerativeSearch.Types.Grouped()
                            {
                                Task = groupedPrompt.Task,
                                Properties = groupedPrompt.Properties.Any()
                                    ? new V1.TextArray { Values = { groupedPrompt.Properties } }
                                    : null,
                                Debug = groupedPrompt.Debug,
                                Queries =
                                {
                                    groupedPrompt.Provider is null
                                        ? []
                                        : [GetGenerativeProvider(groupedPrompt.Provider)],
                                },
                            }
                            : null,
                    },
        };

        if (consistencyLevel.HasValue)
        {
            request.ConsistencyLevel = MapConsistencyLevel(consistencyLevel.Value);
        }

        if (after.HasValue)
        {
            request.After = after.ToString();
        }
        if (sort is not null)
        {
            request.SortBy.AddRange(sort.Select(s => s.InternalSort));
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

    private static Grpc.Protobuf.V1.GenerativeProvider GetGenerativeProvider(
        Models.GenerativeProvider provider
    )
    {
        var result = new Grpc.Protobuf.V1.GenerativeProvider
        {
            ReturnMetadata = provider.ReturnMetadata,
        };

        switch (provider)
        {
            case Models.Generative.Providers.Anthropic a:
                result.Anthropic = new V1.GenerativeAnthropic
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    MaxTokens = a.MaxTokens ?? 0,
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    TopK = a.TopK ?? 0,
                    TopP = a.TopP ?? 0,
                    StopSequences =
                        a.StopSequences != null
                            ? new V1.TextArray { Values = { a.StopSequences } }
                            : null,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                break;
            case Models.Generative.Providers.Anyscale a:
                result.Anyscale = new V1.GenerativeAnyscale
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                };
                break;
            case Models.Generative.Providers.AWS a:
                result.Aws = new V1.GenerativeAWS
                {
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    Service = a.Service ?? string.Empty,
                    Region = a.Region ?? string.Empty,
                    Endpoint = a.Endpoint ?? string.Empty,
                    TargetModel = a.TargetModel ?? string.Empty,
                    TargetVariant = a.TargetVariant ?? string.Empty,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                    MaxTokens = a.MaxTokens ?? 0,
                };
                break;
            case Models.Generative.Providers.Cohere a:
                result.Cohere = new V1.GenerativeCohere
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    FrequencyPenalty = a.FrequencyPenalty ?? 0,
                    MaxTokens = a.MaxTokens ?? 0,
                    Model = a.Model ?? string.Empty,
                    K = a.K ?? 0,
                    P = a.P ?? 0,
                    PresencePenalty = a.PresencePenalty ?? 0,
                    StopSequences =
                        a.StopSequences != null
                            ? new V1.TextArray { Values = { a.StopSequences } }
                            : null,
                    Temperature = a.Temperature ?? 0,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                break;
            case Models.Generative.Providers.Dummy:
                result.Dummy = new V1.GenerativeDummy();
                break;
            case Models.Generative.Providers.Mistral a:
                result.Mistral = new V1.GenerativeMistral
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    MaxTokens = a.MaxTokens ?? 0,
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    TopP = a.TopP ?? 0,
                };
                break;
            case Models.Generative.Providers.Ollama a:
                result.Ollama = new V1.GenerativeOllama
                {
                    ApiEndpoint = a.ApiEndpoint ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                break;
            case Models.Generative.Providers.OpenAI a:
                result.Openai = new V1.GenerativeOpenAI
                {
                    FrequencyPenalty = a.FrequencyPenalty ?? 0,
                    MaxTokens = a.MaxTokens ?? 0,
                    Model = a.Model ?? string.Empty,
                    N = a.N ?? 0,
                    PresencePenalty = a.PresencePenalty ?? 0,
                    Stop = a.Stop != null ? new V1.TextArray { Values = { a.Stop } } : null,
                    Temperature = a.Temperature ?? 0,
                    TopP = a.TopP ?? 0,
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    ApiVersion = a.ApiVersion ?? string.Empty,
                    ResourceName = a.ResourceName ?? string.Empty,
                    DeploymentId = a.DeploymentId ?? string.Empty,
                    IsAzure = a.IsAzure ?? false,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                    ReasoningEffort = a.ReasoningEffort.HasValue
                        ? (V1.GenerativeOpenAI.Types.ReasoningEffort)a.ReasoningEffort.Value
                        : V1.GenerativeOpenAI.Types.ReasoningEffort.Unspecified,
                    Verbosity = a.Verbosity.HasValue
                        ? (V1.GenerativeOpenAI.Types.Verbosity)a.Verbosity.Value
                        : V1.GenerativeOpenAI.Types.Verbosity.Unspecified,
                };
                break;
            case Models.Generative.Providers.Google a:
                result.Google = new V1.GenerativeGoogle
                {
                    FrequencyPenalty = a.FrequencyPenalty ?? 0,
                    MaxTokens = a.MaxTokens ?? 0,
                    Model = a.Model ?? string.Empty,
                    PresencePenalty = a.PresencePenalty ?? 0,
                    Temperature = a.Temperature ?? 0,
                    TopK = a.TopK ?? 0,
                    TopP = a.TopP ?? 0,
                    StopSequences =
                        a.StopSequences != null
                            ? new V1.TextArray { Values = { a.StopSequences } }
                            : null,
                    ApiEndpoint = a.ApiEndpoint ?? string.Empty,
                    ProjectId = a.ProjectId ?? string.Empty,
                    EndpointId = a.EndpointId ?? string.Empty,
                    Region = a.Region ?? string.Empty,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                break;
            case Models.Generative.Providers.Databricks a:
                result.Databricks = new V1.GenerativeDatabricks
                {
                    Endpoint = a.Endpoint ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    FrequencyPenalty = a.FrequencyPenalty ?? 0,
                    LogProbs = a.LogProbs ?? false,
                    TopLogProbs = a.TopLogProbs ?? 0,
                    MaxTokens = a.MaxTokens ?? 0,
                    N = a.N ?? 0,
                    PresencePenalty = a.PresencePenalty ?? 0,
                    Stop = a.Stop != null ? new V1.TextArray { Values = { a.Stop } } : null,
                    Temperature = a.Temperature ?? 0,
                    TopP = a.TopP ?? 0,
                };
                break;
            case Models.Generative.Providers.FriendliAI a:
                result.Friendliai = new V1.GenerativeFriendliAI
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    MaxTokens = a.MaxTokens ?? 0,
                    Temperature = a.Temperature ?? 0,
                    N = a.N ?? 0,
                    TopP = a.TopP ?? 0,
                };
                break;
            case Models.Generative.Providers.Nvidia a:
                result.Nvidia = new V1.GenerativeNvidia
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    TopP = a.TopP ?? 0,
                    MaxTokens = a.MaxTokens ?? 0,
                };
                break;
            case Models.Generative.Providers.XAI a:
                result.Xai = new V1.GenerativeXAI
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    TopP = a.TopP ?? 0,
                    MaxTokens = a.MaxTokens ?? 0,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                break;
            default:
                throw new NotSupportedException(
                    $"Unknown generative provider type: {provider.GetType().Name}"
                );
        }

        return result;
    }

    private static Grpc.Protobuf.V1.ConsistencyLevel MapConsistencyLevel(ConsistencyLevels value)
    {
        return value switch
        {
            ConsistencyLevels.Unspecified => Grpc.Protobuf.V1.ConsistencyLevel.Unspecified,
            ConsistencyLevels.All => Grpc.Protobuf.V1.ConsistencyLevel.All,
            ConsistencyLevels.One => Grpc.Protobuf.V1.ConsistencyLevel.One,
            ConsistencyLevels.Quorum => Grpc.Protobuf.V1.ConsistencyLevel.Quorum,
            _ => throw new NotSupportedException($"Consistency level {value} is not supported."),
        };
    }

    private static (
        V1.Targets? targets,
        ICollection<V1.VectorForTarget>? vectorForTargets,
        ICollection<V1.Vectors>? vectors
    ) BuildTargetVector(TargetVectors? targetVector, Models.Vectors? vector = null)
    {
        V1.Targets? targets = null;
        ICollection<V1.VectorForTarget>? vectorForTarget = null;
        ICollection<V1.Vectors>? vectors = null;

        vector ??= new Models.Vectors();

        targetVector ??= vector.Keys.Where(tv => string.IsNullOrEmpty(tv) is false).ToArray();

        targets = targetVector;

        if (targetVector.Count() == 1 && vector.Count == 1)
        {
            // If only one target vector is specified, use Vectors
            // This also covers the case where no target vector is specified and only one vector is provided
            // In this case, we assume the single provided vector is the target
            vectors = vector
                .Select(v => new Grpc.Protobuf.V1.Vectors
                {
                    Name = v.Key,
                    Type = v.Value.IsMultiVector
                        ? Grpc.Protobuf.V1.Vectors.Types.VectorType.MultiFp32
                        : Grpc.Protobuf.V1.Vectors.Types.VectorType.SingleFp32,
                    VectorBytes = v.Value.ToByteString(),
                })
                .ToList();
            return (targets, vectorForTarget, vectors);
        }

        if (
            targetVector.Count() > 1
            && vector.Count == targetVector.Count()
            && targetVector.All(tv => vector.ContainsKey(tv)) // TODO Throw an exception if the TargetVector does not match the provided vectors?
        )
        {
            // If multiple target vectors are specified, use VectorForTargets
            vectorForTarget = targetVector
                .Select(
                    (v, idx) =>
                        new
                        {
                            Name = v,
                            Index = idx,
                            Vector = vector.ContainsKey(v)
                                ? vector[v]
                                : vector.Values.ElementAt(idx),
                        }
                )
                .Select(v => new Grpc.Protobuf.V1.VectorForTarget()
                {
                    Name = v.Name,
                    Vectors =
                    {
                        new Grpc.Protobuf.V1.Vectors
                        {
                            Name = v.Name,
                            Type = v.Vector.IsMultiVector
                                ? Grpc.Protobuf.V1.Vectors.Types.VectorType.MultiFp32
                                : Grpc.Protobuf.V1.Vectors.Types.VectorType.SingleFp32,
                            VectorBytes = v.Vector.ToByteString(),
                        },
                    },
                })
                .ToList();
        }
        else
        {
            vectors = vector
                .Select(v => new Grpc.Protobuf.V1.Vectors
                {
                    Name = v.Key,
                    Type = v.Value.IsMultiVector
                        ? Grpc.Protobuf.V1.Vectors.Types.VectorType.MultiFp32
                        : Grpc.Protobuf.V1.Vectors.Types.VectorType.SingleFp32,
                    VectorBytes = v.Value.ToByteString(),
                })
                .ToList();
        }

        return (targets, vectorForTarget, vectors);
    }

    private static Grpc.Protobuf.V1.NearTextSearch BuildNearText(
        string[] query,
        double? distance,
        double? certainty,
        Move? moveTo,
        Move? moveAway,
        TargetVectors? targetVector = null
    )
    {
        var (targets, _, _) = BuildTargetVector(targetVector, null);
        var nearText = new Grpc.Protobuf.V1.NearTextSearch { Query = { query }, Targets = targets };

        if (moveTo is not null)
        {
            var uuids = moveTo.Objects is null ? [] : moveTo.Objects.Select(x => x.ToString());
            var concepts = moveTo.Concepts is null ? new string[] { } : moveTo.Concepts;
            nearText.MoveTo = new Grpc.Protobuf.V1.NearTextSearch.Types.Move
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
            nearText.MoveAway = new Grpc.Protobuf.V1.NearTextSearch.Types.Move
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

    private static Grpc.Protobuf.V1.NearVector BuildNearVector(
        Models.Vectors vector,
        double? certainty,
        double? distance,
        TargetVectors? targetVector
    )
    {
        Grpc.Protobuf.V1.NearVector nearVector = new();

        if (distance.HasValue)
        {
            nearVector.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            nearVector.Certainty = certainty.Value;
        }

        var (targets, vectorForTarget, vectors) = BuildTargetVector(targetVector, vector);

        if (targets is not null)
        {
            nearVector.Targets = targets;
        }
        if (vectorForTarget is not null)
        {
            nearVector.VectorForTargets.Add(vectorForTarget);
        }
        else if (vectors is not null)
        {
            nearVector.Vectors.Add(vectors);
        }

        return nearVector;
    }

    private static void BuildBM25(
        Grpc.Protobuf.V1.SearchRequest request,
        string query,
        string[]? properties = null
    )
    {
        request.Bm25Search = new V1.BM25() { Query = query };

        if (properties is not null)
        {
            request.Bm25Search.Properties.AddRange(properties);
        }
    }

    private static void BuildHybrid(
        Grpc.Protobuf.V1.SearchRequest request,
        string? query = null,
        float? alpha = null,
        Models.Vectors? vector = null,
        HybridNearVector? nearVector = null,
        HybridNearText? nearText = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        BM25Operator? bm25Operator = null,
        TargetVectors? targetVector = null
    )
    {
        request.HybridSearch = new V1.Hybrid();

        if (!string.IsNullOrEmpty(query))
        {
            request.HybridSearch.Query = query;
        }
        else
        {
            alpha = 1.0f; // Default alpha if no query is provided
        }

        if (alpha.HasValue)
        {
            request.HybridSearch.Alpha = alpha.Value;
        }

        if (vector is not null && nearText is null && nearVector is null)
        {
            var (targets, vfts, vectors) = BuildTargetVector(targetVector, vector);

            if (vfts is not null)
            {
                nearVector = new HybridNearVector(
                    vector,
                    Certainty: null,
                    Distance: null,
                    targetVector: targetVector
                );
                vector = null; // Clear vector to avoid duplication
            }
            else if (vectors is not null)
            {
                request.HybridSearch.Vectors.Add(vectors);
                request.HybridSearch.Targets = targets;
            }
        }

        if (vector is null && nearText is not null && nearVector is null)
        {
            request.HybridSearch.NearText = BuildNearText(
                [nearText.Query],
                nearText.Distance,
                nearText.Certainty,
                nearText.MoveTo,
                nearText.MoveAway,
                targetVector
            );

            request.HybridSearch.Targets = request.HybridSearch.NearText.Targets;
        }

        if (
            vector is null
            && nearText is null
            && nearVector is not null
            && nearVector.Vector is not null
        )
        {
            request.HybridSearch.NearVector = BuildNearVector(
                nearVector.Vector,
                nearVector.Certainty,
                nearVector.Distance,
                targetVector
            );
            request.HybridSearch.Targets = request.HybridSearch.NearVector.Targets;
        }

        if (queryProperties is not null)
        {
            request.HybridSearch.Properties.AddRange(queryProperties);
        }
        if (fusionType.HasValue)
        {
            request.HybridSearch.FusionType = fusionType switch
            {
                HybridFusion.Ranked => V1.Hybrid.Types.FusionType.Ranked,
                HybridFusion.RelativeScore => V1.Hybrid.Types.FusionType.RelativeScore,
                _ => V1.Hybrid.Types.FusionType.Unspecified,
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

    private void BuildNearObject(
        V1.SearchRequest request,
        Guid objectID,
        double? certainty,
        double? distance,
        TargetVectors? targetVector
    )
    {
        request.NearObject = new V1.NearObject { Id = objectID.ToString() };

        if (certainty.HasValue)
        {
            request.NearObject.Certainty = certainty.Value;
        }

        if (distance.HasValue)
        {
            request.NearObject.Distance = distance.Value;
        }

        var (targets, _, _) = BuildTargetVector(targetVector);

        request.NearObject.Targets = targets;
    }
}
