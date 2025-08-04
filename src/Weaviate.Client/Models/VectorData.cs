using System.Collections;

namespace Weaviate.Client.Models;

public abstract record AbstractVectorData(Type ValueType) : ICollection
{
    public abstract int Count { get; }
    public abstract bool IsSynchronized { get; }
    public abstract object SyncRoot { get; }

    public abstract void CopyTo(Array array, int index);

    public abstract IEnumerator GetEnumerator();

    public static implicit operator double[](AbstractVectorData vectorData)
    {
        if (vectorData.ValueType != typeof(double))
        {
            throw new ArgumentException($"vectorData is not of type {typeof(double)}");
        }
        return ((ICollection<double>)vectorData).ToArray();
    }

    public static implicit operator double[][](AbstractVectorData vectorData)
    {
        if (vectorData.ValueType != typeof(double[]))
        {
            throw new ArgumentException($"vectorData is not of type {typeof(double[])}");
        }
        return ((ICollection<double[]>)vectorData).ToArray();
    }

    public static implicit operator float[](AbstractVectorData vectorData)
    {
        if (vectorData.ValueType != typeof(float))
        {
            throw new ArgumentException($"vectorData is not of type {typeof(float)}");
        }
        return ((ICollection<float>)vectorData).ToArray();
    }

    public static implicit operator float[][](AbstractVectorData vectorData)
    {
        if (vectorData.ValueType != typeof(float[]))
        {
            throw new ArgumentException($"vectorData is not of type {typeof(float[])}");
        }
        return ((ICollection<float[]>)vectorData).ToArray();
    }
}

public record AbstractVectorData<T> : AbstractVectorData, IList<T>
{
    private List<T> _data;

    protected AbstractVectorData(IEnumerable<T> values)
        : base(typeof(T))
    {
        _data = new(values);
    }

    public bool IsReadOnly => ((ICollection<T>)_data).IsReadOnly;

    public override int Count => ((ICollection<T>)_data).Count;

    public override bool IsSynchronized => ((ICollection)_data).IsSynchronized;

    public override object SyncRoot => ((ICollection)_data).SyncRoot;

    public T this[int index]
    {
        get => ((IList<T>)_data)[index];
        set => ((IList<T>)_data)[index] = value;
    }

    public int IndexOf(T item)
    {
        return ((IList<T>)_data).IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        ((IList<T>)_data).Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        ((IList<T>)_data).RemoveAt(index);
    }

    public void Add(T item)
    {
        ((ICollection<T>)_data).Add(item);
    }

    public void Clear()
    {
        ((ICollection<T>)_data).Clear();
    }

    public bool Contains(T item)
    {
        return ((ICollection<T>)_data).Contains(item);
    }

    public override void CopyTo(Array array, int arrayIndex)
    {
        ((ICollection)_data).CopyTo(array, arrayIndex);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        ((ICollection<T>)_data).CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return ((ICollection<T>)_data).Remove(item);
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return ((IEnumerable<T>)_data).GetEnumerator();
    }

    public override IEnumerator GetEnumerator()
    {
        return ((IEnumerable)_data).GetEnumerator();
    }

    public static implicit operator T[](AbstractVectorData<T> vectorData)
    {
        return vectorData.ToArray();
    }
}

// For simple struct values (int, double, etc.)
public record VectorData<T> : AbstractVectorData<T>
    where T : struct
{
    public VectorData()
        : this(new T[] { }) { }

    public VectorData(params T[] values)
        : base(values) { }

    public VectorData(IEnumerable<T> values)
        : this(values.ToArray()) { }

    public static implicit operator Vectors(VectorData<T> vectorData)
    {
        return VectorData.Create("default", vectorData);
    }

    public static implicit operator VectorData<T>(T[] vectorData)
    {
        return new VectorData<T>(vectorData);
    }

    public static implicit operator T[](VectorData<T> vectorData)
    {
        return vectorData.ToArray();
    }
}

public record MultiVectorData<T> : AbstractVectorData<T[]>
    where T : struct
{
    public MultiVectorData()
        : this(new T[][] { }) { }

    public MultiVectorData(params T[][] values)
        : base(values) { }

    public MultiVectorData(IEnumerable<T[]> values)
        : this(values.ToArray()) { }

    public static implicit operator Vectors(MultiVectorData<T> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator MultiVectorData<T>(T[][] vectorData)
    {
        return new MultiVectorData<T>(vectorData);
    }

    public static implicit operator T[][](MultiVectorData<T> vectorData)
    {
        return vectorData.ToArray();
    }
}

public class Vectors : Dictionary<string, AbstractVectorData>, IHybridVectorInput
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

    public static implicit operator Vectors(Dictionary<string, float[]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(Dictionary<string, float[][]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(Dictionary<string, double[]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(Dictionary<string, double[][]> vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(float[] vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(float[][] vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(double[] vectorData)
    {
        return VectorData.Create(vectorData);
    }

    public static implicit operator Vectors(double[][] vectorData)
    {
        return VectorData.Create(vectorData);
    }
}

public static class VectorData
{
    public static Vectors Create<T>(Dictionary<string, T[]> vectors)
        where T : struct
    {
        var container = new Vectors();
        foreach (var kvp in vectors)
        {
            container[kvp.Key] = new VectorData<T>(kvp.Value);
        }
        return container;
    }

    public static Vectors Create<T>(Dictionary<string, T[][]> vectors)
        where T : struct
    {
        var container = new Vectors();
        foreach (var kvp in vectors)
        {
            container[kvp.Key] = new MultiVectorData<T>(kvp.Value);
        }
        return container;
    }

    // Create vector data for simple struct values
    public static VectorData<T> Create<T>(params T[] values)
        where T : struct
    {
        return new(values);
    }

    // Create vector data for array values
    public static MultiVectorData<T> Create<T>(params T[][] values)
        where T : struct
    {
        return new(values);
    }

    // Create container with simple struct values
    public static Vectors Create<T>(string name, params T[] values)
        where T : struct
    {
        return new Vectors() { [name] = new VectorData<T>(values) };
    }

    // Create container with array values
    public static Vectors Create<T>(string name, params T[][] values)
        where T : struct
    {
        return new Vectors() { [name] = new MultiVectorData<T>(values) };
    }

    public static Vectors Create(AbstractVectorData values) => Create("default", values);

    public static Vectors Create(string name, AbstractVectorData values)
    {
        return new Vectors() { [name] = values };
    }
}
