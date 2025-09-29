using System.Collections;
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
}
