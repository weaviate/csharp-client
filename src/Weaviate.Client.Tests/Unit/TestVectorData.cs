using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class VectorDataTests
{
    [Fact]
    public void Vector_Create_Returns_VectorSingle()
    {
        var vector = Vector.Create(1, 2, 3);
        Assert.IsAssignableFrom<Vector>(vector);
        Assert.IsType<VectorSingle<int>>(vector);

        Assert.Equal(3, vector.Dimensions);
        Assert.Equal(1, vector.Count);
        Assert.False(vector.IsMultiVector);

        Assert.Equal(new[] { 1, 2, 3 }, vector);
        Assert.Equal(typeof(int), vector.ValueType);
    }

    [Fact]
    public void Vector_Create_Returns_VectorMulti()
    {
        var vector = Vector.Create(
            new[,]
            {
                { 1, 2 },
                { 3, 4 },
                { 5, 6 },
            }
        );
        Assert.IsAssignableFrom<Vector>(vector);
        Assert.IsType<VectorMulti<int>>(vector);

        Assert.Equal(3, vector.Dimensions);
        Assert.Equal(2, vector.Count);
        Assert.True(vector.IsMultiVector);

        var vectorM = vector as VectorMulti<int>;
        Assert.NotNull(vectorM);

        Assert.Equal(new[] { 1, 2 }, vectorM[0]);
        Assert.Equal(new[] { 3, 4 }, vectorM[1]);
        Assert.Equal(typeof(int[]), vectorM.ValueType);

        int[,] values = vector;
        Assert.Equal(1, values[0, 0]);
        Assert.Equal(2, values[0, 1]);
        Assert.Equal(3, values[1, 0]);
        Assert.Equal(4, values[1, 1]);
        Assert.Equal(5, values[2, 0]);
        Assert.Equal(6, values[2, 1]);
    }

    [Fact]
    public void Implicit_Conversion_VectorSingle_To_VectorContainer()
    {
        var vector = Vector.Create(1.1, 2.2);
        Vectors container = vector;
        Assert.True(container.ContainsKey("default"));
        var stored = container["default"];
        Assert.NotNull(stored);
        Assert.Equal(new[] { 1.1, 2.2 }, stored);
    }

    [Fact]
    public void Implicit_Conversion_VectorContainer_To_Array()
    {
        var vector = Vector.Create(1.1, 2.2);
        Vectors container = vector;
        Assert.True(container.ContainsKey("default"));
        double[] stored = container["default"];
        Assert.NotNull(stored);
        Assert.Equal(new[] { 1.1, 2.2 }, stored);
    }

    [Fact]
    public void Implicit_Conversion_VectorMulti_To_VectorContainer()
    {
        var multiVector = Vector.Create(
            new[,]
            {
                { 1f, 2f },
                { 3f, 4f },
            }
        );
        Vectors container = multiVector;
        Assert.True(container.ContainsKey("default"));
        var stored = container["default"] as VectorMulti<float>;
        Assert.NotNull(stored);
        Assert.Equal(2, stored.Dimensions);
        Assert.Equal(new[] { 1f, 2f }, stored[0]);
        Assert.Equal(new[] { 3f, 4f }, stored[1]);
    }

    [Fact]
    public void VectorContainer_Add_SingleVector()
    {
        var container = new Vectors();
        container.Add("vec", 10, 20, 30);
        Assert.True(container.ContainsKey("vec"));
        var vector = container["vec"] as VectorSingle<int>;
        Assert.NotNull(vector);
        Assert.Equal(new[] { 10, 20, 30 }, vector);
    }

    [Fact]
    public void VectorContainer_Add_MultiVector()
    {
        var container = new Vectors();
        container.Add(
            "multi",
            new[,]
            {
                { 1, 2 },
                { 3, 4 },
            }
        );
        Assert.True(container.ContainsKey("multi"));
        var multiVector = container["multi"] as VectorMulti<int>;
        Assert.NotNull(multiVector);
        Assert.Equal(2, multiVector.Count);
        Assert.Equal(new[] { 1, 2 }, multiVector[0]);
        Assert.Equal(new[] { 3, 4 }, multiVector[1]);
    }

    [Fact]
    public void VectorSingle_EnumerableBehavior()
    {
        var vector = Vector.Create(5, 6, 7);
        var list = new List<int>(vector.Cast<int>());
        Assert.Equal(new[] { 5, 6, 7 }, list);
    }

    [Fact]
    public void VectorMulti_EnumerableBehavior()
    {
        var multiVector = Vector.Create(
            new[,]
            {
                { 8, 9 },
                { 10, 11 },
            }
        );
        var list = new List<int[]>(multiVector.Cast<int[]>());
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { 8, 9 }, list[0]);
        Assert.Equal(new[] { 10, 11 }, list[1]);
    }

    [Fact]
    public void VectorContainer_TypeCheck_For_Single_And_MultiVector()
    {
        var c = new Vectors()
        {
            { "regular", new[] { 1f, 2f } },
            {
                "colbert",
                new[,]
                {
                    { 1f, 2f },
                    { 4f, 5f },
                }
            },
        };

        var v1 = Vector.Create(1f, 2f);
        Vectors vc1 = v1;

        Assert.IsType<float>(vc1["default"][0]);

        var v2 = Vector.Create(
            new[,]
            {
                { 1f, 2f },
                { 3f, 4f },
            }
        );
        Vectors vc2 = v2;

        Assert.IsType<float[]>(vc2["default"][0]);
    }

    #region Equality Tests
    [Fact]
    public void VectorSingle_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange: Create two single vectors with identical values
        var vector1 = new VectorSingle<float>(new[] { 1.0f, 2.0f, 3.0f });
        var vector2 = new VectorSingle<float>(new[] { 1.0f, 2.0f, 3.0f });

        // Act & Assert
        Assert.Equal(vector1, vector2);
        Assert.True(vector1 == vector2);
        Assert.False(vector1 != vector2);
        Assert.Equal(vector1.GetHashCode(), vector2.GetHashCode());
    }

    [Fact]
    public void VectorSingle_Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange: Create two single vectors with different values
        var vector1 = new VectorSingle<float>(new[] { 1.0f, 2.0f, 3.0f });
        var vector2 = new VectorSingle<float>(new[] { 1.0f, 2.0f, 4.0f });

        // Act & Assert
        Assert.NotEqual(vector1, vector2);
        Assert.False(vector1 == vector2);
        Assert.True(vector1 != vector2);
    }

    [Fact]
    public void VectorSingle_Equality_WithDifferentNames_ShouldNotBeEqual()
    {
        // Arrange: Create two single vectors with same values but different names
        var vector1 = new VectorSingle<float>(new[] { 1.0f, 2.0f, 3.0f }) { Name = "vector1" };
        var vector2 = new VectorSingle<float>(new[] { 1.0f, 2.0f, 3.0f }) { Name = "vector2" };

        // Act & Assert
        Assert.NotEqual(vector1, vector2);
    }

    [Fact]
    public void VectorMulti_Equality_WithSameValues_ShouldBeEqual()
    {
        // Arrange: Create two multi-vectors with identical values
        var vector1 = new VectorMulti<float>(
            new[,]
            {
                { 1.0f, 2.0f, 3.0f },
                { 4.0f, 5.0f, 6.0f },
                { 7.0f, 8.0f, 9.0f },
                { 10.0f, 11.0f, 12.0f },
            }
        );
        var vector2 = new VectorMulti<float>(
            new[,]
            {
                { 1.0f, 2.0f, 3.0f },
                { 4.0f, 5.0f, 6.0f },
                { 7.0f, 8.0f, 9.0f },
                { 10.0f, 11.0f, 12.0f },
            }
        );

        // Act & Assert
        Assert.Equal(vector1, vector2);
        Assert.True(vector1 == vector2);
        Assert.False(vector1 != vector2);
        Assert.Equal(vector1.GetHashCode(), vector2.GetHashCode());
    }

    [Fact]
    public void VectorMulti_Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange: Create two multi-vectors with different values
        var vector1 = new VectorMulti<float>(
            new[,]
            {
                { 1.0f, 2.0f, 3.0f },
                { 4.0f, 5.0f, 6.0f },
            }
        );
        var vector2 = new VectorMulti<float>(
            new[,]
            {
                { 1.0f, 2.0f, 3.0f },
                { 4.0f, 5.0f, 7.0f }, // Different value
            }
        );

        // Act & Assert
        Assert.NotEqual(vector1, vector2);
    }

    [Fact]
    public void VectorMulti_Equality_WithDifferentDimensions_ShouldNotBeEqual()
    {
        // Arrange: Create two multi-vectors with different dimensions
        var vector1 = new VectorMulti<float>(
            new[,]
            {
                { 1.0f, 2.0f, 3.0f },
                { 4.0f, 5.0f, 6.0f },
            }
        );
        var vector2 = new VectorMulti<float>(
            new[,]
            {
                { 1.0f, 2.0f, 3.0f },
                { 4.0f, 5.0f, 6.0f },
                { 7.0f, 8.0f, 9.0f },
            }
        );

        // Act & Assert
        Assert.NotEqual(vector1, vector2);
    }

    [Fact]
    public void Vectors_Equality_WithIdenticalSingleVectors_ShouldBeEqual()
    {
        // Arrange: Create two Vectors objects with identical single vectors
        var vectors1 = new Vectors { { "default", new[] { 1.0f, 2.0f, 3.0f } } };

        var vectors2 = new Vectors { { "default", new[] { 1.0f, 2.0f, 3.0f } } };

        // Act & Assert
        Assert.Equal(vectors1, vectors2);
    }

    [Fact]
    public void Vectors_Equality_WithIdenticalMultiVectors_ShouldBeEqual()
    {
        // Arrange: Create two Vectors objects with identical multi-vectors
        var vectors1 = new Vectors
        {
            {
                "default",
                new[,]
                {
                    { 1.0f, 2.0f, 3.0f },
                    { 4.0f, 5.0f, 6.0f },
                }
            },
        };

        var vectors2 = new Vectors
        {
            {
                "default",
                new[,]
                {
                    { 1.0f, 2.0f, 3.0f },
                    { 4.0f, 5.0f, 6.0f },
                }
            },
        };

        // Act & Assert
        Assert.Equal(vectors1, vectors2);
    }

    [Fact]
    public void Vectors_Equality_WithDifferentVectorNames_ShouldNotBeEqual()
    {
        // Arrange: Create two Vectors objects with vectors under different keys
        var vectors1 = new Vectors { { "vector1", new[] { 1.0f, 2.0f, 3.0f } } };

        var vectors2 = new Vectors { { "vector2", new[] { 1.0f, 2.0f, 3.0f } } };

        // Act & Assert
        Assert.NotEqual(vectors1, vectors2);
    }

    [Fact]
    public void Vectors_Equality_WithDifferentVectorValues_ShouldNotBeEqual()
    {
        // Arrange: Create two Vectors objects with different vector values
        var vectors1 = new Vectors { { "default", new[] { 1.0f, 2.0f, 3.0f } } };

        var vectors2 = new Vectors { { "default", new[] { 1.0f, 2.0f, 4.0f } } };

        // Act & Assert
        Assert.NotEqual(vectors1, vectors2);
    }

    [Fact]
    public void Vectors_Equality_CreatedInDifferentWays_ShouldBeEqual()
    {
        // Arrange: Create Vectors objects using different construction methods
        // Method 1: Using implicit conversion from array
        Vectors vectors1 = new[] { 1.0f, 2.0f, 3.0f };

        // Method 2: Using Add method
        var vectors2 = new Vectors();
        vectors2.Add(new[] { 1.0f, 2.0f, 3.0f });

        // Method 3: Using Create method
        var vectors3 = Vectors.Create(1.0f, 2.0f, 3.0f);

        // Act & Assert
        Assert.Equal(vectors1, vectors2);
        Assert.Equal(vectors1, vectors3);
        Assert.Equal(vectors2, vectors3);
    }

    [Fact]
    public void Vectors_Equality_WithMultipleNamedVectors_ShouldBeEqual()
    {
        // Arrange: Create Vectors objects with multiple named vectors
        var vectors1 = new Vectors
        {
            { "vec1", new[] { 1.0f, 2.0f } },
            { "vec2", new[] { 3.0f, 4.0f } },
            { "vec3", new[] { 5.0f, 6.0f } },
        };

        var vectors2 = new Vectors
        {
            { "vec1", new[] { 1.0f, 2.0f } },
            { "vec2", new[] { 3.0f, 4.0f } },
            { "vec3", new[] { 5.0f, 6.0f } },
        };

        // Act & Assert
        Assert.Equal(vectors1, vectors2);
    }

    [Fact]
    public void Vectors_Equality_WithMixedSingleAndMultiVectors_ShouldBeEqual()
    {
        // Arrange: Create Vectors objects with both single and multi vectors
        var vectors1 = new Vectors
        {
            { "single", new[] { 1.0f, 2.0f, 3.0f } },
            {
                "multi",
                new[,]
                {
                    { 4.0f, 5.0f },
                    { 6.0f, 7.0f },
                }
            },
        };

        var vectors2 = new Vectors
        {
            { "single", new[] { 1.0f, 2.0f, 3.0f } },
            {
                "multi",
                new[,]
                {
                    { 4.0f, 5.0f },
                    { 6.0f, 7.0f },
                }
            },
        };

        // Act & Assert
        Assert.Equal(vectors1, vectors2);
    }

    [Fact]
    public void Vectors_Equality_AfterReinsert_ShouldBeEqual()
    {
        // Arrange: Simulate the integration test scenario - create a Vectors object,
        // then create another one from the same properties
        var originalProperties = new { name = "Item5" };

        var vector5 = new[,]
        {
            { 0.8f, 0.9f, 0.1f },
            { 0.2f, 0.3f, 0.4f },
            { 0.5f, 0.6f, 0.7f },
            { 0.1f, 0.2f, 0.3f },
        };

        Vectors vectors1 = vector5;

        // Create a second Vectors object with the same vector
        Vectors vectors2 = vector5;

        // Act & Assert
        Assert.Equal(vectors1, vectors2);
    }

    [Fact]
    public void VectorSingle_Equality_WithIntegerType_ShouldBeEqual()
    {
        // Arrange: Test with integer type
        var vector1 = new VectorSingle<int>(new[] { 1, 2, 3, 4, 5 });
        var vector2 = new VectorSingle<int>(new[] { 1, 2, 3, 4, 5 });

        // Act & Assert
        Assert.Equal(vector1, vector2);
    }

    [Fact]
    public void VectorMulti_Equality_WithDoubleType_ShouldBeEqual()
    {
        // Arrange: Test with double type
        var vector1 = new VectorMulti<double>(
            new[,]
            {
                { 1.0, 2.0 },
                { 3.0, 4.0 },
            }
        );
        var vector2 = new VectorMulti<double>(
            new[,]
            {
                { 1.0, 2.0 },
                { 3.0, 4.0 },
            }
        );

        // Act & Assert
        Assert.Equal(vector1, vector2);
    }

    [Fact]
    public void VectorSingle_Equality_EmptyVectors_ShouldBeEqual()
    {
        // Arrange: Create two empty vectors
        var vector1 = new VectorSingle<float>(Array.Empty<float>());
        var vector2 = new VectorSingle<float>(Array.Empty<float>());

        // Act & Assert
        Assert.Equal(vector1, vector2);
    }
    #endregion
}
