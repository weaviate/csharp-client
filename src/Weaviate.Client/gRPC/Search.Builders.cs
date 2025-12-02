using Weaviate.Client.Models;
using Weaviate.V1;
using Rerank = Weaviate.Client.Models.Rerank;

namespace Weaviate.Client.Grpc;

internal partial class WeaviateGrpcClient
{
    private static void SetIfNotNull<T>(Action<T> setter, T? value)
        where T : struct
    {
        if (value.HasValue)
            setter(value.Value);
    }

    private static V1.SearchRequest BaseSearchRequest(
        string collection,
        Filter? filters = null,
        IEnumerable<Sort>? sort = null,
        uint? autoLimit = null,
        uint? limit = null,
        uint? offset = null,
        GroupByRequest? groupBy = null,
        Rerank? rerank = null,
        Guid? after = null,
        string? tenant = null,
        ConsistencyLevels? consistencyLevel = null,
        SinglePrompt? singlePrompt = null,
        GroupedTask? groupedTask = null,
        OneOrManyOf<string>? returnProperties = null,
        MetadataQuery? returnMetadata = null,
        VectorQuery? includeVectors = null,
        IList<QueryReference>? returnReferences = null
    )
    {
        var metadataRequest = new V1.MetadataRequest()
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

        var request = new V1.SearchRequest()
        {
            Collection = collection,
            Filters = filters?.InternalFilter,
#pragma warning disable CS0612 // Type or member is obsolete
            Uses123Api = true,
            Uses125Api = true,
#pragma warning restore CS0612 // Type or member is obsolete
            Uses127Api = true,
            GroupBy = groupBy is not null
                ? new V1.GroupBy()
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
                (singlePrompt is null && groupedTask is null)
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
                        Grouped = groupedTask is not null
                            ? new V1.GenerativeSearch.Types.Grouped()
                            {
                                Task = groupedTask.Task,
                                Properties = groupedTask.Properties.Any()
                                    ? new V1.TextArray { Values = { groupedTask.Properties } }
                                    : null,
                                Debug = groupedTask.Debug,
                                Queries =
                                {
                                    groupedTask.Provider is null
                                        ? []
                                        : [GetGenerativeProvider(groupedTask.Provider)],
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
        if (autoLimit.HasValue)
        {
            request.Autocut = autoLimit.Value;
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

    private static V1.GenerativeProvider GetGenerativeProvider(Models.GenerativeProvider provider)
    {
        var result = new V1.GenerativeProvider { ReturnMetadata = provider.ReturnMetadata };

        switch (provider)
        {
            case Models.Generative.Providers.Anthropic a:
                result.Anthropic = new V1.GenerativeAnthropic
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
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
                SetIfNotNull(v => result.Anthropic.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Anthropic.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Anthropic.TopK = v, a.TopK);
                SetIfNotNull(v => result.Anthropic.TopP = (float)v, a.TopP);
                break;
            case Models.Generative.Providers.Anyscale a:
                result.Anyscale = new V1.GenerativeAnyscale
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                };
                SetIfNotNull(v => result.Anyscale.Temperature = (float)v, a.Temperature);
                break;
            case Models.Generative.Providers.AWS a:
                result.Aws = new V1.GenerativeAWS
                {
                    Model = a.Model ?? string.Empty,
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
                };
                SetIfNotNull(v => result.Aws.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Aws.MaxTokens = v, a.MaxTokens);
                break;
            case Models.Generative.Providers.Cohere a:
                result.Cohere = new V1.GenerativeCohere
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
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
                SetIfNotNull(v => result.Cohere.FrequencyPenalty = (float)v, a.FrequencyPenalty);
                SetIfNotNull(v => result.Cohere.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Cohere.K = v, a.K);
                SetIfNotNull(v => result.Cohere.P = (float)v, a.P);
                SetIfNotNull(v => result.Cohere.PresencePenalty = (float)v, a.PresencePenalty);
                SetIfNotNull(v => result.Cohere.Temperature = (float)v, a.Temperature);
                break;
            case Models.Generative.Providers.Dummy:
                result.Dummy = new V1.GenerativeDummy();
                break;
            case Models.Generative.Providers.Mistral a:
                result.Mistral = new V1.GenerativeMistral
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                };
                SetIfNotNull(v => result.Mistral.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Mistral.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Mistral.TopP = (float)v, a.TopP);
                break;
            case Models.Generative.Providers.Ollama a:
                result.Ollama = new V1.GenerativeOllama
                {
                    ApiEndpoint = a.ApiEndpoint ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                SetIfNotNull(v => result.Ollama.Temperature = (float)v, a.Temperature);
                break;
            case Models.Generative.Providers.OpenAI a:
                result.Openai = new V1.GenerativeOpenAI
                {
                    Model = a.Model ?? string.Empty,
                    Stop = a.Stop != null ? new V1.TextArray { Values = { a.Stop } } : null,
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
                SetIfNotNull(v => result.Openai.FrequencyPenalty = (float)v, a.FrequencyPenalty);
                SetIfNotNull(v => result.Openai.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Openai.N = v, a.N);
                SetIfNotNull(v => result.Openai.PresencePenalty = (float)v, a.PresencePenalty);
                SetIfNotNull(v => result.Openai.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Openai.TopP = (float)v, a.TopP);
                break;
            case Models.Generative.Providers.Google a:
                result.Google = new V1.GenerativeGoogle
                {
                    Model = a.Model ?? string.Empty,
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
                SetIfNotNull(v => result.Google.FrequencyPenalty = (float)v, a.FrequencyPenalty);
                SetIfNotNull(v => result.Google.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Google.PresencePenalty = (float)v, a.PresencePenalty);
                SetIfNotNull(v => result.Google.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Google.TopK = v, a.TopK);
                SetIfNotNull(v => result.Google.TopP = (float)v, a.TopP);
                break;
            case Models.Generative.Providers.Databricks a:
                result.Databricks = new V1.GenerativeDatabricks
                {
                    Endpoint = a.Endpoint ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    LogProbs = a.LogProbs ?? false,
                    Stop = a.Stop != null ? new V1.TextArray { Values = { a.Stop } } : null,
                };
                SetIfNotNull(
                    v => result.Databricks.FrequencyPenalty = (float)v,
                    a.FrequencyPenalty
                );
                SetIfNotNull(v => result.Databricks.TopLogProbs = v, a.TopLogProbs);
                SetIfNotNull(v => result.Databricks.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Databricks.N = v, a.N);
                SetIfNotNull(v => result.Databricks.PresencePenalty = (float)v, a.PresencePenalty);
                SetIfNotNull(v => result.Databricks.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Databricks.TopP = (float)v, a.TopP);
                break;
            case Models.Generative.Providers.FriendliAI a:
                result.Friendliai = new V1.GenerativeFriendliAI
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                };
                SetIfNotNull(v => result.Friendliai.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Friendliai.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Friendliai.N = v, a.N);
                SetIfNotNull(v => result.Friendliai.TopP = (float)v, a.TopP);
                break;
            case Models.Generative.Providers.Nvidia a:
                result.Nvidia = new V1.GenerativeNvidia
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                };
                SetIfNotNull(v => result.Nvidia.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Nvidia.TopP = (float)v, a.TopP);
                SetIfNotNull(v => result.Nvidia.MaxTokens = v, a.MaxTokens);
                break;
            case Models.Generative.Providers.XAI a:
                result.Xai = new V1.GenerativeXAI
                {
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    Model = a.Model ?? string.Empty,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                SetIfNotNull(v => result.Xai.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Xai.TopP = (float)v, a.TopP);
                SetIfNotNull(v => result.Xai.MaxTokens = v, a.MaxTokens);
                break;
            case Models.Generative.Providers.ContextualAI a:
                result.Contextualai = new V1.GenerativeContextualAI
                {
                    Model = a.Model ?? string.Empty,
                    Temperature = a.Temperature ?? 0,
                    TopP = a.TopP ?? 0,
                    MaxNewTokens = a.MaxNewTokens ?? 0,
                    SystemPrompt = a.SystemPrompt ?? string.Empty,
                    AvoidCommentary = a.AvoidCommentary ?? false,
                    Knowledge = a.Knowledge != null
                        ? new V1.TextArray { Values = { a.Knowledge } }
                        : new V1.TextArray(),
                };
                break;
            default:
                throw new NotSupportedException(
                    $"Unknown generative provider type: {provider.GetType().Name}"
                );
        }

        return result;
    }

    private static V1.ConsistencyLevel MapConsistencyLevel(ConsistencyLevels value)
    {
        return value switch
        {
            ConsistencyLevels.Unspecified => V1.ConsistencyLevel.Unspecified,
            ConsistencyLevels.All => V1.ConsistencyLevel.All,
            ConsistencyLevels.One => V1.ConsistencyLevel.One,
            ConsistencyLevels.Quorum => V1.ConsistencyLevel.Quorum,
            _ => throw new NotSupportedException($"Consistency level {value} is not supported."),
        };
    }

    private static (
        Targets? targets,
        ICollection<VectorForTarget>? vectorForTargets,
        ICollection<V1.Vectors>? vectors
    ) BuildTargetVector(TargetVectors? targetVector, Models.Vectors? vector = null)
    {
        Targets? targets = null;
        ICollection<VectorForTarget>? vectorForTarget = null;
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
                .Select(v => new V1.Vectors
                {
                    Name = v.Key,
                    Type = v.Value.IsMultiVector
                        ? V1.Vectors.Types.VectorType.MultiFp32
                        : V1.Vectors.Types.VectorType.SingleFp32,
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
                .Select(v => new VectorForTarget()
                {
                    Name = v.Name,
                    Vectors =
                    {
                        new V1.Vectors
                        {
                            Name = v.Name,
                            Type = v.Vector.IsMultiVector
                                ? V1.Vectors.Types.VectorType.MultiFp32
                                : V1.Vectors.Types.VectorType.SingleFp32,
                            VectorBytes = v.Vector.ToByteString(),
                        },
                    },
                })
                .ToList();
        }
        else
        {
            vectors = vector
                .Select(v => new V1.Vectors
                {
                    Name = v.Key,
                    Type = v.Value.IsMultiVector
                        ? V1.Vectors.Types.VectorType.MultiFp32
                        : V1.Vectors.Types.VectorType.SingleFp32,
                    VectorBytes = v.Value.ToByteString(),
                })
                .ToList();
        }

        return (targets, vectorForTarget, vectors);
    }

    private static NearTextSearch BuildNearText(
        string[] query,
        double? distance,
        double? certainty,
        Move? moveTo,
        Move? moveAway,
        TargetVectors? targetVector = null
    )
    {
        var (targets, _, _) = BuildTargetVector(targetVector, null);
        var nearText = new NearTextSearch { Query = { query }, Targets = targets };

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
        double? certainty,
        double? distance,
        TargetVectors? targetVector
    )
    {
        NearVector nearVector = new();

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
        SearchRequest request,
        string query,
        string[]? properties = null,
        BM25Operator? searchOperator = null
    )
    {
        request.Bm25Search = new BM25() { Query = query };

        if (properties is not null)
        {
            request.Bm25Search.Properties.AddRange(properties);
        }
        if (searchOperator != null)
        {
            request.Bm25Search.SearchOperator = new()
            {
                Operator = searchOperator switch
                {
                    BM25Operator.And => V1.SearchOperatorOptions.Types.Operator.And,
                    BM25Operator.Or => V1.SearchOperatorOptions.Types.Operator.Or,
                    _ => V1.SearchOperatorOptions.Types.Operator.Unspecified,
                },
                MinimumOrTokensMatch = (searchOperator as BM25Operator.Or)?.MinimumMatch ?? 1,
            };
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
        TargetVectors? targetVector = null
    )
    {
        request.HybridSearch = new Hybrid();

        if (!string.IsNullOrEmpty(query))
        {
            request.HybridSearch.Query = query;
        }
        else
        {
            // If no query is provided, move the alpha all the way to vector search
            alpha = 1.0f;
        }

        alpha ??= 0.7f; // Default alpha if not provided

        request.HybridSearch.Alpha = alpha.Value;

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

        if (vector is null && nearText is null && nearVector is null && targetVector is not null)
        {
            request.HybridSearch.Targets = BuildTargetVector(targetVector).targets;
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

    private void BuildNearObject(
        SearchRequest request,
        Guid objectID,
        double? certainty,
        double? distance,
        TargetVectors? targetVector
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

        var (targets, _, _) = BuildTargetVector(targetVector);

        request.NearObject.Targets = targets;
    }
}
