using Weaviate.Client.Models;
using Rerank = Weaviate.Client.Models.Rerank;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

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
        AutoArray<string>? returnProperties = null,
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
                ((includeVectors?.Vectors) == null || includeVectors.Vectors.Length == 0)
                && (includeVectors?.Vectors != null),
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
                                Properties =
                                    groupedTask.Properties.Length != 0
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
            case Models.Generative.Providers.AWSBedrock a:
                result.Aws = new V1.GenerativeAWS
                {
                    Model = a.Model ?? string.Empty,
                    Service = "bedrock",
                    Region = a.Region ?? string.Empty,
                    Endpoint = a.Endpoint ?? string.Empty,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                SetIfNotNull(v => result.Aws.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Aws.MaxTokens = v, a.MaxTokens);
                // TODO - add top_k, top_p & stop_sequences here when added to server-side proto
                // Check the latest available version of `grpc/proto/v1/generative.proto` (see GenerativeAWS) in the server repo
                break;
            case Models.Generative.Providers.AWSSagemaker a:
                result.Aws = new V1.GenerativeAWS
                {
                    Service = "sagemaker",
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
                // TODO - add top_k, top_p & stop_sequences here when added to server-side proto
                // Check the latest available version of `grpc/proto/v1/generative.proto` (see GenerativeAWS) in the server repo
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
            case Models.Generative.Providers.AzureOpenAI a:
                result.Openai = new V1.GenerativeOpenAI
                {
                    Model = a.Model ?? string.Empty,
                    Stop = a.Stop != null ? new V1.TextArray { Values = { a.Stop } } : null,
                    BaseUrl = a.BaseUrl ?? string.Empty,
                    ApiVersion = a.ApiVersion ?? string.Empty,
                    ResourceName = a.ResourceName ?? string.Empty,
                    DeploymentId = a.DeploymentId ?? string.Empty,
                    IsAzure = true,
                    Images = a.Images != null ? new V1.TextArray { Values = { a.Images } } : null,
                    ImageProperties =
                        a.ImageProperties != null
                            ? new V1.TextArray { Values = { a.ImageProperties } }
                            : null,
                };
                SetIfNotNull(v => result.Openai.FrequencyPenalty = (float)v, a.FrequencyPenalty);
                SetIfNotNull(v => result.Openai.MaxTokens = v, a.MaxTokens);
                SetIfNotNull(v => result.Openai.N = v, a.N);
                SetIfNotNull(v => result.Openai.PresencePenalty = (float)v, a.PresencePenalty);
                SetIfNotNull(v => result.Openai.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Openai.TopP = (float)v, a.TopP);
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
                    IsAzure = false,
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
            case Models.Generative.Providers.GoogleVertex a:
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
            case Models.Generative.Providers.GoogleGemini a:
                result.Google = new V1.GenerativeGoogle
                {
                    Model = a.Model ?? string.Empty,
                    StopSequences =
                        a.StopSequences != null
                            ? new V1.TextArray { Values = { a.StopSequences } }
                            : null,
                    ApiEndpoint = "generativelanguage.googleapis.com",
                    ProjectId = string.Empty,
                    EndpointId = string.Empty,
                    Region = string.Empty,
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
                    SystemPrompt = a.SystemPrompt ?? string.Empty,
                    Knowledge =
                        a.Knowledge != null ? new V1.TextArray { Values = { a.Knowledge } } : null,
                };
                SetIfNotNull(v => result.Contextualai.Temperature = (float)v, a.Temperature);
                SetIfNotNull(v => result.Contextualai.TopP = (float)v, a.TopP);
                SetIfNotNull(v => result.Contextualai.MaxNewTokens = v, a.MaxNewTokens);
                SetIfNotNull(v => result.Contextualai.AvoidCommentary = v, a.AvoidCommentary);
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

    /// <summary>
    /// Converts a NamedVector to gRPC V1.Vectors format.
    /// </summary>
    private static V1.Vectors ConvertToGrpcVector(NamedVector namedVector)
    {
        return new V1.Vectors
        {
            Name = namedVector.Name,
            Type = namedVector.IsMultiVector
                ? V1.Vectors.Types.VectorType.MultiFp32
                : V1.Vectors.Types.VectorType.SingleFp32,
            VectorBytes = namedVector.ToByteString(),
        };
    }

    internal static (
        V1.Targets? targets,
        IList<V1.VectorForTarget>? vectorForTargets,
        IList<V1.Vectors>? vectors
    ) BuildTargetVector(TargetVectors? targetVector, IEnumerable<NamedVector>? vector = null)
    {
        IList<V1.VectorForTarget>? vectorForTarget = null;
        IList<V1.Vectors>? vectors = null;

        vector ??= [];

        // If no target vector specified, create one from vector names
        targetVector ??= new SimpleTargetVectors(
            [
                .. vector
                    .Select(v => v.Name)
                    .Where(tv => string.IsNullOrEmpty(tv) is false)
                    .ToHashSet()
                    .Order(),
            ]
        );

        if (targetVector.Count() == 1 && vector.Count() == 1)
        {
            // If only one target vector is specified, use Vectors
            // This also covers the case where no target vector is specified and only one vector is provided
            // In this case, we assume the single provided vector is the target
            vectors = [.. vector.Select(v => ConvertToGrpcVector(v)).OrderBy(v => v.Name)];

            return (targetVector, vectorForTarget, vectors);
        }

        var vectorUniqueNames = vector.Select(v => v.Name).ToHashSet();
        var vectorsByName = vector.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.ToList());

        // Get weights if targetVector is WeightedTargetVectors
        Dictionary<string, List<double?>> vectorInstanceWeights;
        if (targetVector is WeightedTargetVectors weighted)
        {
            vectorInstanceWeights = weighted
                .GetTargetWithWeights()
                .GroupBy(v => v.name)
                .ToDictionary(g => g.Key, g => g.Select(w => (double?)w.weight).ToList());
        }
        else
        {
            // For SimpleTargetVectors, no weights
            vectorInstanceWeights = targetVector.Targets.ToDictionary(
                t => t,
                t => new List<double?> { null }
            );
        }

        // Determine if we should use VectorForTargets:
        // 1. Multiple distinct target names, OR
        // 2. Multiple vectors for the same target (e.g., Sum with same name)
        // 3. A combination method is specified (Sum, Average, Minimum, etc.)
        bool vectorNamesMatch = vectorUniqueNames.SetEquals(targetVector.Targets);
        bool vectorsMatchTargets = vectorsByName.Count == vectorInstanceWeights.Count;
        bool hasCombinationMethod = targetVector.Combination != V1.CombinationMethod.Unspecified;
        bool hasMultipleVectorsPerTarget = vectorsByName.Any(kvp => kvp.Value.Count > 1);
        bool weightsCheck = true;

        if (
            targetVector.Combination == V1.CombinationMethod.TypeManual
            || targetVector.Combination == V1.CombinationMethod.TypeRelativeScore
        )
        {
            // Ensure all vectors are present and match the weights provided
            bool allVectorsPresentAndMatchingWeights = vectorsByName.All(kv =>
                vectorInstanceWeights.TryGetValue(kv.Key, out var weights)
                && weights.Count == kv.Value.Count
            );
            bool vectorsUseWeights =
                vector.Count() == vectorInstanceWeights.Sum(kvp => kvp.Value.Count);

            weightsCheck = allVectorsPresentAndMatchingWeights && vectorsUseWeights;
        }

        // Use VectorForTargets when:
        // - Multiple distinct targets with matching vectors, OR
        // - Multiple vectors for the same target (e.g., Sum, or just multiple vectors with same name)
        bool useVectorForTargets =
            (targetVector.Count() > 1 && vectorNamesMatch && vectorsMatchTargets && weightsCheck)
            || (hasMultipleVectorsPerTarget && vectorNamesMatch);

        if (useVectorForTargets)
        {
            vectorForTarget =
            [
                .. vectorUniqueNames
                    .Select(name => new V1.VectorForTarget()
                    {
                        Name = name,
                        Vectors = { vectorsByName[name].Select(v => ConvertToGrpcVector(v)) },
                    })
                    .OrderBy(vft => vft.Name),
            ];

            return (targetVector, vectorForTarget, vectors);
        }

        vectors = [.. vector.Select(v => ConvertToGrpcVector(v))];

        return (targetVector, vectorForTarget, vectors);
    }

    /// <summary>
    /// Overload for VectorSearchInput which contains both vectors and target configuration.
    /// </summary>
    internal static (
        V1.Targets? targets,
        IList<V1.VectorForTarget>? vectorForTargets,
        IList<V1.Vectors>? vectors
    ) BuildTargetVector(VectorSearchInput? vectorSearchInput)
    {
        if (vectorSearchInput == null)
            return (null, null, null);

        var namedVectors = vectorSearchInput.ToList();

        // Create TargetVectors from VectorSearchInput configuration
        TargetVectors targetVector;
        if (vectorSearchInput.Weights != null && vectorSearchInput.Weights.Count > 0)
        {
            // Weighted targets
            targetVector = new WeightedTargetVectors(
                vectorSearchInput.Targets ?? [.. vectorSearchInput.Vectors.Keys],
                vectorSearchInput.Combination,
                vectorSearchInput.Weights
            );
        }
        else if (vectorSearchInput.Targets != null)
        {
            // Simple targets with combination method
            targetVector = new SimpleTargetVectors(
                vectorSearchInput.Targets,
                vectorSearchInput.Combination
            );
        }
        else
        {
            // Default: use vector names as targets
            targetVector = new SimpleTargetVectors(
                [.. vectorSearchInput.Vectors.Keys],
                vectorSearchInput.Combination
            );
        }

        // Reuse the existing BuildTargetVector logic
        return BuildTargetVector(targetVector, namedVectors);
    }

    private static V1.NearTextSearch BuildNearText(
        string[] query,
        double? distance,
        double? certainty,
        Move? moveTo,
        Move? moveAway,
        TargetVectors? targetVector = null
    )
    {
        var nearText = new V1.NearTextSearch
        {
            Query = { query },
            Targets = targetVector ?? new V1.Targets(),
        };

        if (moveTo is not null)
        {
            var uuids = moveTo.Objects is null ? [] : moveTo.Objects.Select(x => x.ToString());
            var concepts = moveTo.Concepts is null ? [] : moveTo.Concepts;
            nearText.MoveTo = new V1.NearTextSearch.Types.Move
            {
                Uuids = { uuids },
                Concepts = { concepts },
                Force = moveTo.Force,
            };
        }

        if (moveAway is not null)
        {
            var uuids = moveAway.Objects is null ? [] : moveAway.Objects.Select(x => x.ToString());
            var concepts = moveAway.Concepts is null ? [] : moveAway.Concepts;
            nearText.MoveAway = new V1.NearTextSearch.Types.Move
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

    private static V1.NearVector BuildNearVector(
        VectorSearchInput vectors,
        double? certainty,
        double? distance
    )
    {
        V1.NearVector nearVector = new();

        if (distance.HasValue)
        {
            nearVector.Distance = distance.Value;
        }

        if (certainty.HasValue)
        {
            nearVector.Certainty = certainty.Value;
        }

        var (targets, vectorForTarget, vectorsLocal) = BuildTargetVector(vectors);

        if (targets is not null)
        {
            nearVector.Targets = targets;
        }
        if (vectorForTarget is not null)
        {
            nearVector.VectorForTargets.Add(vectorForTarget);
        }
        else if (vectorsLocal is not null)
        {
            nearVector.Vectors.Add(vectorsLocal);
        }

        return nearVector;
    }

    private static void BuildBM25(
        V1.SearchRequest request,
        string query,
        string[]? properties = null,
        BM25Operator? searchOperator = null
    )
    {
        request.Bm25Search = new V1.BM25() { Query = query };

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

    private static V1.Hybrid BuildHybrid(
        string? query = null,
        float? alpha = null,
        HybridVectorInput? vectors = null,
        string[]? queryProperties = null,
        HybridFusion? fusionType = null,
        float? maxVectorDistance = null,
        BM25Operator? bm25Operator = null
    )
    {
        var hybrid = new V1.Hybrid();

        if (!string.IsNullOrEmpty(query))
        {
            hybrid.Query = query;
        }
        else
        {
            // If no query is provided, move the alpha all the way to vector search
            alpha = 1.0f;
        }

        alpha ??= 0.7f; // Default alpha if not provided

        hybrid.Alpha = alpha.Value;

        // Pattern match on HybridVectorInput to build the appropriate search type
        if (vectors != null)
        {
            vectors.Match<object?>(
                onVectorSearch: vectorSearchInput =>
                {
                    // Build target vectors from VectorSearchInput (includes combination method)
                    var (targets, vfts, vectorsGrpc) = BuildTargetVector(vectorSearchInput);
                    // Set targets for hybrid search
                    if (targets is not null)
                    {
                        hybrid.Targets = targets;
                    }
                    // For Hybrid, vectors go in Vectors field (not VectorForTargets)
                    // If VectorForTargets was computed, flatten them into Vectors
                    if (vfts is not null)
                    {
                        foreach (var vft in vfts)
                        {
                            hybrid.Vectors.Add(vft.Vectors);
                        }
                    }
                    else if (vectorsGrpc is not null)
                    {
                        hybrid.Vectors.Add(vectorsGrpc);
                    }
                    return null;
                },
                onNearText: nearText =>
                {
                    hybrid.NearText = BuildNearText(
                        nearText.Query.ToArray(),
                        nearText.Distance,
                        nearText.Certainty,
                        nearText.MoveTo,
                        nearText.MoveAway,
                        nearText.TargetVectors
                    );
                    // Move targets to Hybrid message (matching Python client behavior)
                    hybrid.Targets = hybrid.NearText.Targets;
                    hybrid.NearText.Targets = new V1.Targets();
                    return null;
                },
                onNearVector: nearVector =>
                {
                    hybrid.NearVector = BuildNearVector(
                        nearVector.Vector,
                        nearVector.Certainty,
                        nearVector.Distance
                    );
                    // Move targets to Hybrid message (matching Python client behavior)
                    hybrid.Targets = hybrid.NearVector.Targets;
                    hybrid.NearVector.Targets = new V1.Targets();
                    return null;
                }
            );
        }

        if (queryProperties is not null)
        {
            hybrid.Properties.AddRange(queryProperties);
        }
        if (fusionType.HasValue)
        {
            hybrid.FusionType = fusionType switch
            {
                HybridFusion.Ranked => V1.Hybrid.Types.FusionType.Ranked,
                HybridFusion.RelativeScore => V1.Hybrid.Types.FusionType.RelativeScore,
                _ => V1.Hybrid.Types.FusionType.Unspecified,
            };
        }
        if (maxVectorDistance.HasValue)
        {
            hybrid.VectorDistance = maxVectorDistance.Value;
        }
        if (bm25Operator != null)
        {
            hybrid.Bm25SearchOperator = new()
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

        return hybrid;
    }

    private static V1.NearObject BuildNearObject(
        Guid objectID,
        double? certainty,
        double? distance,
        TargetVectors? targetVector
    )
    {
        var nearObject = new V1.NearObject { Id = objectID.ToString() };

        if (certainty.HasValue)
        {
            nearObject.Certainty = certainty.Value;
        }

        if (distance.HasValue)
        {
            nearObject.Distance = distance.Value;
        }

        nearObject.Targets = targetVector ?? new V1.Targets();

        return nearObject;
    }
}
