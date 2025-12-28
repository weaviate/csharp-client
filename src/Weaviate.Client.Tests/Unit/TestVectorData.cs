using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class VectorDataTests
{
    [Fact]
    public void Vector_Create_Returns_Single_Vector()
    {
        var vectors = new Vectors([1, 2, 3]);
        Assert.True(vectors.ContainsKey("default"));
        var vector = vectors["default"];

        Assert.IsAssignableFrom<Vector>(vector);
        Assert.Equal((1, 3), vector.Dimensions);
        Assert.Equal(3, vector.Count);
        Assert.False(vector.IsMultiVector);

        int[] values = vector;
        Assert.Equal(new[] { 1, 2, 3 }, values);
        Assert.Equal(typeof(int), vector.ValueType);
    }

    [Fact]
    public void Vector_Create_Returns_Multi_Vector()
    {
        var vectors = new Vectors(
            new[,]
            {
                { 1, 2 },
                { 3, 4 },
                { 5, 6 },
            }
        );
        Assert.True(vectors.ContainsKey("default"));
        var vector = vectors["default"];

        Assert.IsAssignableFrom<Vector>(vector);
        Assert.Equal((3, 2), vector.Dimensions);
        Assert.Equal(6, vector.Count);
        Assert.True(vector.IsMultiVector);

        Assert.Equal(typeof(int[]), vector.ValueType);

        int[,] values = vector;
        Assert.Equal(1, values[0, 0]);
        Assert.Equal(2, values[0, 1]);
        Assert.Equal(3, values[1, 0]);
        Assert.Equal(4, values[1, 1]);
        Assert.Equal(5, values[2, 0]);
        Assert.Equal(6, values[2, 1]);
    }

    [Fact]
    public void Implicit_Conversion_Array_To_Vectors()
    {
        Vectors container = new[] { 1.1, 2.2 };
        Assert.True(container.ContainsKey("default"));
        var stored = container["default"];
        Assert.NotNull(stored);
        double[] values = stored;
        Assert.Equal(new[] { 1.1, 2.2 }, values);
    }

    [Fact]
    public void Implicit_Conversion_Vectors_NamedVector_To_Array()
    {
        Vectors container = new[] { 1.1, 2.2 };
        Assert.True(container.ContainsKey("default"));
        double[] stored = container["default"];
        Assert.NotNull(stored);
        Assert.Equal(new[] { 1.1, 2.2 }, stored);
    }

    [Fact]
    public void Implicit_Conversion_MultiArray_To_Vectors()
    {
        Vectors container = new[,]
        {
            { 1f, 2f },
            { 3f, 4f },
        };
        Assert.True(container.ContainsKey("default"));
        var stored = container["default"];
        Assert.NotNull(stored);
        Assert.Equal((2, 2), stored.Dimensions);
        Assert.Equal(4, stored.Count);
        Assert.True(stored.IsMultiVector);

        float[,] values = stored;
        Assert.Equal(1f, values[0, 0]);
        Assert.Equal(2f, values[0, 1]);
        Assert.Equal(3f, values[1, 0]);
        Assert.Equal(4f, values[1, 1]);
    }

    [Fact]
    public void VectorContainer_Add_SingleVector()
    {
        var container = new Vectors { { "vec", new[] { 10, 20, 30 } } };
        Assert.True(container.ContainsKey("vec"));
        var vector = container["vec"];
        Assert.NotNull(vector);
        int[] values = vector;
        Assert.Equal([10, 20, 30], values);
    }

    [Fact]
    public void VectorContainer_Add_SingleVector_CollectionExpression()
    {
        var container = new Vectors("vec", [10, 20, 30]);
        Assert.True(container.ContainsKey("vec"));
        var vector = container["vec"];
        Assert.NotNull(vector);
        int[] values = vector;
        Assert.Equal(new[] { 10, 20, 30 }, values);
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
        var multiVector = container["multi"];
        Assert.NotNull(multiVector);
        Assert.Equal((2, 2), multiVector.Dimensions);
        Assert.Equal(4, multiVector.Count);

        int[,] values = multiVector;
        Assert.Equal(1, values[0, 0]);
        Assert.Equal(2, values[0, 1]);
        Assert.Equal(3, values[1, 0]);
        Assert.Equal(4, values[1, 1]);
    }

    [Fact]
    public void Vector_Single_EnumerableBehavior()
    {
        Vector vector = new[] { 5, 6, 7 };
        var list = new List<int>(vector.Cast<int>());
        Assert.Equal(new[] { 5, 6, 7 }, list);
    }

    [Fact]
    public void Vector_Multi_EnumerableBehavior()
    {
        Vector multiVector = new[,]
        {
            { 8, 9 },
            { 10, 11 },
        };
        var list = multiVector.Cast<int[]>();
        Assert.Equal(2, list.Count());
        Assert.Equal(new[] { 8, 9 }, list.ElementAt(0));
        Assert.Equal(new[] { 10, 11 }, list.ElementAt(1));
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

        Vectors vc1 = new[] { 1f, 2f };
        var singleVector = vc1["default"];
        Assert.False(singleVector.IsMultiVector);
        Assert.Equal(typeof(float), singleVector.ValueType);

        Vectors vc2 = new[,]
        {
            { 1f, 2f },
            { 3f, 4f },
        };
        var multiVector = vc2["default"];
        Assert.True(multiVector.IsMultiVector);
        Assert.Equal(typeof(float[]), multiVector.ValueType);
    }

    #region Equality Tests
    // Note: VectorSingle<T> and VectorMulti<T> are now internal types.
    // The equality tests have been removed as they tested internal implementation details.
    // The public API (Vector, NamedVector, Vectors) should be tested for equality instead.

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
        var vectors3 = new Vectors([1.0f, 2.0f, 3.0f]);

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

    #endregion
}
