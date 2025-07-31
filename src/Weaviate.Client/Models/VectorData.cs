using System.Collections;

namespace Weaviate.Client.Models;

public interface IVectorData : ICollection
{
    Type ValueType { get; }
}

public interface IVectorData<T> : IVectorData { }

public static class VectorData
{
    public static VectorContainer Create<T>(Dictionary<string, T[]> vectors)
        where T : struct
    {
        var container = new VectorContainer();
        foreach (var kvp in vectors)
        {
            container[kvp.Key] = (VectorData<T>)kvp.Value;
        }
        return container;
    }

    public static VectorContainer Create<T>(Dictionary<string, T[][]> vectors)
        where T : struct
    {
        var container = new VectorContainer();
        foreach (var kvp in vectors)
        {
            container[kvp.Key] = (MultiVectorData<T>)kvp.Value;
        }
        return container;
    }

    // Create vector data for simple struct values
    public static VectorData<T> Create<T>(params T[] values)
        where T : struct
    {
        return new VectorData<T>(values);
    }

    // Create vector data for array values
    public static MultiVectorData<T> Create<T>(params T[][] values)
        where T : struct
    {
        return new MultiVectorData<T>(values);
    }

    // Create container with simple struct values
    public static VectorContainer Create<T>(string name, params T[] values)
        where T : struct
    {
        return new VectorContainer() { [name] = new VectorData<T>(values) };
    }

    // Create container with array values
    public static VectorContainer Create<T>(string name, params T[][] values)
        where T : struct
    {
        return new VectorContainer() { [name] = new MultiVectorData<T>(values) };
    }

    public static VectorContainer Create(IVectorData values) => Create("default", values);

    public static VectorContainer Create(string name, IVectorData values)
    {
        return new VectorContainer() { [name] = values };
    }
}

// For simple struct values (int, double, etc.)
public class VectorData<T> : List<T>, IVectorData<T>
    where T : struct
{
    public Type ValueType => typeof(T);

    public VectorData() { }

    public VectorData(params T[] values)
        : base(values) { }

    public VectorData(IEnumerable<T> values)
        : base(values) { }

    public static implicit operator VectorContainer(VectorData<T> vectorData)
    {
        return VectorData.Create("default", vectorData);
    }

    public static implicit operator VectorData<T>(T[] vectorData)
    {
        return new VectorData<T>(vectorData);
    }
}

public class MultiVectorData<T> : List<T[]>, IVectorData<T[]>
    where T : struct
{
    public Type ValueType => typeof(T[]);

    public MultiVectorData() { }

    public MultiVectorData(params T[][] values)
        : base(values) { }

    public MultiVectorData(IEnumerable<T[]> values)
        : base(values) { }

    public static implicit operator VectorContainer(MultiVectorData<T> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator MultiVectorData<T>(T[][] vectorData)
    {
        return new MultiVectorData<T>(vectorData);
    }
}

public class VectorContainer : Dictionary<string, IVectorData>, IHybridVectorInput
{
    public void Add<T>(string name, params T[] values)
        where T : struct
    {
        Add(name, new VectorData<T>(values));
    }

    public void Add<T>(string name, params T[][] values)
        where T : struct
    {
        Add(name, new MultiVectorData<T>(values));
    }

    public void Add<T>(string name, VectorData<T> values)
        where T : struct
    {
        base.Add(name, values);
    }

    public void Add<T>(string name, MultiVectorData<T> values)
        where T : struct
    {
        base.Add(name, values);
    }

    public static implicit operator VectorContainer(Dictionary<string, float[]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(Dictionary<string, float[][]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(Dictionary<string, double[]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(Dictionary<string, double[][]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(float[] vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(float[][] vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(double[] vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator VectorContainer(double[][] vectorData)
    {
        return VectorData.Create(vectorData);
    }
}
