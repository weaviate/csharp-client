using System.Collections;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class VectorDataTests
{
    [Fact]
    public void VectorData_Create_SingleValues()
    {
        var vector = VectorData.Create(1, 2, 3);
        Assert.IsType<Vectors>(vector);
        Assert.IsType<VectorData<int>>(vector["default"]);
        Assert.Equal(new[] { 1, 2, 3 }, vector["default"]);
        Assert.Equal(typeof(int), vector["default"].ValueType);
    }

    [Fact]
    public void VectorData_Create_ArrayValues()
    {
        var vector = VectorData.Create([1, 2], [3, 4], [5, 6]);
        Assert.IsType<Vectors>(vector);
        Assert.IsType<MultiVectorData<int>>(vector["default"]);
        Assert.Single(vector);
        Assert.Equal(3, vector["default"].Count);
        Assert.Equal(new[] { 1, 2 }, ((MultiVectorData<int>)vector["default"])[0]);
        Assert.Equal(new[] { 3, 4 }, ((MultiVectorData<int>)vector["default"])[1]);
        Assert.Equal(typeof(int[]), vector["default"].ValueType);
    }

    [Fact]
    public void VectorData_ImplicitConversion_ToVectorContainer()
    {
        var vector = new VectorData<double>(1.1, 2.2);
        Vectors container = vector;
        Assert.True(container.ContainsKey("default"));
        var stored = (VectorData<double>)container["default"];
        Assert.NotNull(stored);
        Assert.Equal(new[] { 1.1, 2.2 }, stored);
    }

    [Fact]
    public void VectorData_ImplicitConversion_FromVectorContainer_ToArray()
    {
        var vector = new VectorData<double>(1.1, 2.2);
        Vectors container = vector;
        Assert.True(container.ContainsKey("default"));
        double[] stored = container["default"];
        Assert.NotNull(stored);
        Assert.Equal(new[] { 1.1, 2.2 }, stored);
    }

    [Fact]
    public void MultiVectorData_ImplicitConversion_ToVectorContainer()
    {
        var multiVector = new MultiVectorData<float>(new[] { 1f, 2f }, new[] { 3f, 4f });
        Vectors container = multiVector;
        Assert.True(container.ContainsKey("default"));
        var stored = container["default"] as MultiVectorData<float>;
        Assert.NotNull(stored);
        Assert.Equal(2, stored.Count);
        Assert.Equal(new[] { 1f, 2f }, stored[0]);
        Assert.Equal(new[] { 3f, 4f }, stored[1]);
    }

    [Fact]
    public void VectorContainer_Add_SingleValues()
    {
        var container = new Vectors();
        container.Add("vec", 10, 20, 30);
        Assert.True(container.ContainsKey("vec"));
        var vector = container["vec"] as VectorData<int>;
        Assert.NotNull(vector);
        Assert.Equal(new[] { 10, 20, 30 }, vector);
    }

    [Fact]
    public void VectorContainer_Add_ArrayValues()
    {
        var container = new Vectors();
        container.Add("multi", new[] { 1, 2 }, new[] { 3, 4 });
        Assert.True(container.ContainsKey("multi"));
        var multiVector = container["multi"] as MultiVectorData<int>;
        Assert.NotNull(multiVector);
        Assert.Equal(2, multiVector.Count);
        Assert.Equal(new[] { 1, 2 }, multiVector[0]);
        Assert.Equal(new[] { 3, 4 }, multiVector[1]);
    }

    [Fact]
    public void VectorData_EnumerableBehavior()
    {
        var vector = new VectorData<int>(5, 6, 7);
        var list = new List<int>(vector);
        Assert.Equal(new[] { 5, 6, 7 }, list);
    }

    [Fact]
    public void MultiVectorData_EnumerableBehavior()
    {
        var multiVector = new MultiVectorData<int>(new[] { 8, 9 }, new[] { 10, 11 });
        var list = new List<int[]>(multiVector);
        Assert.Equal(2, list.Count);
        Assert.Equal(new[] { 8, 9 }, list[0]);
        Assert.Equal(new[] { 10, 11 }, list[1]);
    }

    [Fact]
    public void Test_IVectorData_Not_Added_As_MultiVectorData()
    {
        var c = new Vectors()
        {
            { "regular", new[] { 1f, 2f } },
            { "colbert", new[] { new[] { 1f, 2f }, new[] { 4f, 5f } } },
        };

        var v1 = VectorData.Create(1f, 2f);
        Vectors vc1 = v1;

        Assert.IsType<float>((vc1["default"] as VectorData<float>)![0]);

        var v2 = VectorData.Create(new[] { 1f, 2f }, new[] { 3f, 4f });
        Vectors vc2 = v2;

        Assert.IsType<float[]>((vc2["default"] as MultiVectorData<float>)![0]);
    }
}
