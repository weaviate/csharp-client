using Weaviate.Client.Grpc;
using Weaviate.Client.Models;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

public class BuildTargetVectorTest
{
    #region Single Vector Tests

    [Fact]
    public void SingleVector_NoTargetSpecified_CreatesTargetFromVectorName()
    {
        // Arrange
        var vector = new NamedVector("myVector", new[] { 1f, 2f, 3f });

        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("myVector", targets.TargetVectors[0]);
        Assert.Null(vectorForTargets);
        Assert.NotNull(vectors);
        Assert.Single(vectors);
        Assert.Equal("myVector", vectors[0].Name);
    }

    [Fact]
    public void SingleVector_SingleTargetSpecified_UsesProvidedTarget()
    {
        // Arrange
        var vector = new NamedVector("myVector", new[] { 1f, 2f, 3f });
        var targetVectors = new SimpleTargetVectors(new[] { "myVector" });

        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("myVector", targets.TargetVectors[0]);
        Assert.Null(vectorForTargets);
        Assert.NotNull(vectors);
        Assert.Single(vectors);
    }

    [Fact]
    public void SingleVector_DefaultName_CreatesTargetFromVectorName()
    {
        // Arrange - using implicit conversion which creates "default" named vector
        var vector = new NamedVector(new[] { 1f, 2f, 3f });

        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("default", targets.TargetVectors[0]);
    }

    #endregion

    #region Multiple Vector Tests

    [Fact]
    public void MultipleVectors_NoTargetSpecified_CreatesTargetsFromAllVectorNames()
    {
        // Arrange
        var vectors = new[]
        {
            new NamedVector("vector1", new[] { 1f, 2f }),
            new NamedVector("vector2", new[] { 3f, 4f }),
        };

        // Act
        var (targets, vectorForTargets, vectorsResult) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: vectors
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(2, targets.TargetVectors.Count);
        Assert.Contains("vector1", targets.TargetVectors);
        Assert.Contains("vector2", targets.TargetVectors);
    }

    [Fact]
    public void MultipleVectors_MatchingTargets_UsesVectorForTargets()
    {
        // Arrange
        var vectors = new[]
        {
            new NamedVector("vector1", new[] { 1f, 2f }),
            new NamedVector("vector2", new[] { 3f, 4f }),
        };
        var targetVectors = new SimpleTargetVectors(new[] { "vector1", "vector2" });

        // Act
        var (targets, vectorForTargets, vectorsResult) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: vectors
        );

        // Assert
        Assert.NotNull(targets);
        Assert.NotNull(vectorForTargets);
        Assert.Equal(2, vectorForTargets.Count);
        Assert.Null(vectorsResult);

        // VectorForTargets should be ordered by name
        Assert.Equal("vector1", vectorForTargets[0].Name);
        Assert.Equal("vector2", vectorForTargets[1].Name);
    }

    [Fact]
    public void MultipleVectors_TargetsAreOrdered()
    {
        // Arrange - add vectors in reverse order
        var vectors = new[]
        {
            new NamedVector("zeta", new[] { 1f, 2f }),
            new NamedVector("alpha", new[] { 3f, 4f }),
            new NamedVector("beta", new[] { 5f, 6f }),
        };

        // Act
        var (targets, vectorForTargets, vectorsResult) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: vectors
        );

        // Assert - targets should be sorted alphabetically
        Assert.NotNull(targets);
        Assert.Equal(new[] { "alpha", "beta", "zeta" }, targets.TargetVectors.ToArray());
    }

    #endregion

    #region Combination Method Tests

    [Fact]
    public void SimpleTargetVectors_SumCombination_SetsCombinationMethod()
    {
        // Arrange
        var vector = new NamedVector("myVector", new[] { 1f, 2f });
        var targetVectors = TargetVectors.Sum("myVector");

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, targets.Combination);
    }

    [Fact]
    public void SimpleTargetVectors_AverageCombination_SetsCombinationMethod()
    {
        // Arrange
        var vector = new NamedVector("myVector", new[] { 1f, 2f });
        var targetVectors = TargetVectors.Average("myVector");

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeAverage, targets.Combination);
    }

    [Fact]
    public void SimpleTargetVectors_MinimumCombination_SetsCombinationMethod()
    {
        // Arrange
        var vector = new NamedVector("myVector", new[] { 1f, 2f });
        var targetVectors = TargetVectors.Minimum("myVector");

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeMin, targets.Combination);
    }

    #endregion

    #region Weighted Target Vector Tests

    [Fact]
    public void WeightedTargetVectors_ManualWeights_SetsWeightsAndCombination()
    {
        // Arrange
        var vectors = new[]
        {
            new NamedVector("vector1", new[] { 1f, 2f }),
            new NamedVector("vector2", new[] { 3f, 4f }),
        };
        var targetVectors = TargetVectors.ManualWeights(("vector1", 0.7), ("vector2", 0.3));

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: vectors
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeManual, targets.Combination);
        Assert.Equal(2, targets.WeightsForTargets.Count);

        // Weights should be ordered by target name
        var weight1 = targets.WeightsForTargets.First(w => w.Target == "vector1");
        var weight2 = targets.WeightsForTargets.First(w => w.Target == "vector2");
        Assert.Equal(0.7f, weight1.Weight, precision: 5);
        Assert.Equal(0.3f, weight2.Weight, precision: 5);
    }

    [Fact]
    public void WeightedTargetVectors_RelativeScore_SetsWeightsAndCombination()
    {
        // Arrange
        var vectors = new[]
        {
            new NamedVector("vector1", new[] { 1f, 2f }),
            new NamedVector("vector2", new[] { 3f, 4f }),
        };
        var targetVectors = TargetVectors.RelativeScore(("vector1", 0.8), ("vector2", 0.2));

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: vectors
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeRelativeScore, targets.Combination);
        Assert.Equal(2, targets.WeightsForTargets.Count);
    }

    #endregion

    #region VectorSearchInput Tests

    [Fact]
    public void VectorSearchInput_SingleVector_BuildsCorrectly()
    {
        // Arrange
        VectorSearchInput input = ("myVector", new[] { 1f, 2f, 3f });

        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(input);

        // Assert
        Assert.NotNull(targets);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("myVector", targets.TargetVectors[0]);
    }

    [Fact]
    public void VectorSearchInput_Null_ReturnsAllNulls()
    {
        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(
            (VectorSearchInput?)null
        );

        // Assert
        Assert.Null(targets);
        Assert.Null(vectorForTargets);
        Assert.Null(vectors);
    }

    [Fact]
    public void VectorSearchInput_MultiVector_BuildsCorrectly()
    {
        // Arrange - ColBERT style multi-vector
        VectorSearchInput input = (
            "colbert",
            new[,]
            {
                { 1f, 2f },
                { 3f, 4f },
            }
        );

        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(input);

        // Assert
        Assert.NotNull(targets);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("colbert", targets.TargetVectors[0]);
        Assert.NotNull(vectors);
        Assert.Single(vectors);
        Assert.Equal(V1.Vectors.Types.VectorType.MultiFp32, vectors[0].Type);
    }

    [Fact]
    public void VectorSearchInput_WithBuilder_SumCombination()
    {
        // Arrange
        var input = new VectorSearchInput.Builder().Sum(
            ("vector1", new[] { 1f, 2f }),
            ("vector2", new[] { 3f, 4f })
        );

        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(input);

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, targets.Combination);
        Assert.Equal(2, targets.TargetVectors.Count);
    }

    [Fact]
    public void VectorSearchInput_WithBuilder_AverageCombination()
    {
        // Arrange
        var input = new VectorSearchInput.Builder().Average(
            ("vector1", new[] { 1f, 2f }),
            ("vector2", new[] { 3f, 4f })
        );

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(input);

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeAverage, targets.Combination);
    }

    [Fact]
    public void VectorSearchInput_WithBuilder_ManualWeights()
    {
        // Arrange
        var input = new VectorSearchInput.Builder().ManualWeights(
            ("vector1", 0.7, new[] { 1f, 2f }),
            ("vector2", 0.3, new[] { 3f, 4f })
        );

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(input);

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeManual, targets.Combination);
        Assert.Equal(2, targets.WeightsForTargets.Count);
    }

    #endregion

    #region VectorType Tests

    [Fact]
    public void SingleVector_FloatArray_HasCorrectVectorType()
    {
        // Arrange
        var vector = new NamedVector("myVector", new[] { 1f, 2f, 3f });

        // Act
        var (_, _, vectors) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(vectors);
        Assert.Single(vectors);
        Assert.Equal(V1.Vectors.Types.VectorType.SingleFp32, vectors[0].Type);
    }

    [Fact]
    public void MultiVector_2DArray_HasCorrectVectorType()
    {
        // Arrange
        var vector = new NamedVector(
            "colbert",
            new[,]
            {
                { 1f, 2f },
                { 3f, 4f },
            }
        );

        // Act
        var (_, _, vectors) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: new[] { vector }
        );

        // Assert
        Assert.NotNull(vectors);
        Assert.Single(vectors);
        Assert.Equal(V1.Vectors.Types.VectorType.MultiFp32, vectors[0].Type);
    }

    #endregion

    #region Multiple Vectors Same Name (Sum/Average)

    [Fact]
    public void MultipleVectorsSameName_SumCombination_UsesVectorForTargets()
    {
        // Arrange - two vectors with the same name for Sum combination
        var vectors = new[]
        {
            new NamedVector("regular", new[] { 1f, 2f }),
            new NamedVector("regular", new[] { 2f, 1f }),
        };
        var targetVectors = TargetVectors.Sum("regular");

        // Act
        var (targets, vectorForTargets, vectorsResult) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: targetVectors,
            vector: vectors
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, targets.Combination);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("regular", targets.TargetVectors[0]);

        // Should use VectorForTargets, not Vectors
        Assert.NotNull(vectorForTargets);
        Assert.Single(vectorForTargets); // One target name
        Assert.Equal("regular", vectorForTargets[0].Name);
        Assert.Equal(2, vectorForTargets[0].Vectors.Count); // Two vectors for that target

        Assert.Null(vectorsResult);
    }

    [Fact]
    public void VectorSearchInput_SumWithSameName_UsesVectorForTargets()
    {
        // Arrange - using the builder syntax like in the failing test
        var input = new VectorSearchInput.Builder().Sum(
            ("regular", new[] { 1f, 2f }),
            ("regular", new[] { 2f, 1f })
        );

        // Act
        var (targets, vectorForTargets, vectorsResult) = WeaviateGrpcClient.BuildTargetVector(
            input
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Equal(V1.CombinationMethod.TypeSum, targets.Combination);

        // Should use VectorForTargets with both vectors
        Assert.NotNull(vectorForTargets);
        Assert.Single(vectorForTargets);
        Assert.Equal("regular", vectorForTargets[0].Name);
        Assert.Equal(2, vectorForTargets[0].Vectors.Count);

        Assert.Null(vectorsResult);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void EmptyVectorList_NoTargetSpecified_ReturnsEmptyTargets()
    {
        // Act
        var (targets, vectorForTargets, vectors) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: Array.Empty<NamedVector>()
        );

        // Assert
        Assert.NotNull(targets);
        Assert.Empty(targets.TargetVectors);
    }

    [Fact]
    public void DuplicateVectorNames_DeduplicatesTargets()
    {
        // Arrange - two vectors with the same name
        var vectors = new[]
        {
            new NamedVector("same", new[] { 1f, 2f }),
            new NamedVector("same", new[] { 3f, 4f }),
        };

        // Act
        var (targets, _, _) = WeaviateGrpcClient.BuildTargetVector(
            targetVector: null,
            vector: vectors
        );

        // Assert - should deduplicate target names
        Assert.NotNull(targets);
        Assert.Single(targets.TargetVectors);
        Assert.Equal("same", targets.TargetVectors[0]);
    }

    #endregion

    #region VectorSearchInput.Combine Tests

    [Fact]
    public void Combine_WithTupleArray_CreatesCorrectVectorSearchInput()
    {
        // Arrange
        var targetVectors = TargetVectors.Sum("first", "second");
        var vectors = new (string, Vector)[]
        {
            ("first", new[] { 1f, 2f }),
            ("second", new[] { 3f, 4f }),
        };

        // Act
        var result = VectorSearchInput.Combine(targetVectors, vectors);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains("first", result.Vectors.Keys);
        Assert.Contains("second", result.Vectors.Keys);
    }

    [Fact]
    public void Combine_WithVectors_CreatesCorrectVectorSearchInput()
    {
        // Arrange
        var targetVectors = TargetVectors.Average("first", "second");
        var vectors = new Vectors { { "first", new[] { 1f, 2f } }, { "second", new[] { 3f, 4f } } };

        // Act
        var result = VectorSearchInput.Combine(targetVectors, vectors);

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains("first", result.Vectors.Keys);
        Assert.Contains("second", result.Vectors.Keys);
    }

    [Fact]
    public void Combine_WithNamedVectorList_CreatesCorrectVectorSearchInput()
    {
        // Arrange
        var targetVectors = TargetVectors.Minimum("first", "second");
        var vectors = new List<NamedVector>
        {
            new("first", new[] { 1f, 2f }),
            new("second", new[] { 3f, 4f }),
            new("second", new[] { 5f, 6f }), // Multiple vectors for same target
        };

        // Act
        var result = VectorSearchInput.Combine(targetVectors, vectors);

        // Assert
        Assert.Equal(3, result.Count());
        Assert.Single(result.Vectors["first"]);
        Assert.Equal(2, result.Vectors["second"].Length);
    }

    [Fact]
    public void Combine_WithWeightedTargets_PreservesWeights()
    {
        // Arrange
        var targetVectors = TargetVectors.ManualWeights(("first", 0.7), ("second", 0.3));
        var vectors = new Vectors { { "first", new[] { 1f, 2f } }, { "second", new[] { 3f, 4f } } };

        // Act
        var result = VectorSearchInput.Combine(targetVectors, vectors);

        // Assert
        Assert.NotNull(result.Weights);
        Assert.Equal(0.7, result.Weights["first"][0]);
        Assert.Equal(0.3, result.Weights["second"][0]);
    }

    [Fact]
    public void Combine_WithMultipleWeightsPerTarget_PreservesAllWeights()
    {
        // Arrange - multiple weights for "second" target
        var targetVectors = TargetVectors.ManualWeights(
            ("first", 1.0),
            ("second", 1.0),
            ("second", 2.0)
        );
        var vectors = new (string, Vector)[]
        {
            ("first", new[] { 1f, 2f }),
            ("second", new[] { 3f, 4f }),
            ("second", new[] { 5f, 6f }),
        };

        // Act
        var result = VectorSearchInput.Combine(targetVectors, vectors);

        // Assert
        Assert.NotNull(result.Weights);
        Assert.Single(result.Weights["first"]);
        Assert.Equal(2, result.Weights["second"].Count);
        Assert.Equal(1.0, result.Weights["second"][0]);
        Assert.Equal(2.0, result.Weights["second"][1]);
    }

    [Fact]
    public void Combine_PreservesTargetOrder()
    {
        // Arrange
        var targetVectors = TargetVectors.Sum("zeta", "alpha", "beta");
        var vectors = new (string, Vector)[]
        {
            ("zeta", new[] { 1f }),
            ("alpha", new[] { 2f }),
            ("beta", new[] { 3f }),
        };

        // Act
        var result = VectorSearchInput.Combine(targetVectors, vectors);

        // Assert - targets should preserve order from TargetVectors
        Assert.NotNull(result.Targets);
        Assert.Equal(new[] { "zeta", "alpha", "beta" }, result.Targets);
    }

    #endregion
}
