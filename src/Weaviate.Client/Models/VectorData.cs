using System.Collections;
using System.Runtime.CompilerServices;

namespace Weaviate.Client.Models;

/// <summary>
/// Internal interface for vector data storage.
/// </summary>
internal interface IVectorData
{
    (int rows, int cols) Dimensions { get; }
    int Count { get; }
    Type ValueType { get; }
    bool IsMultiVector { get; }
}

/// <summary>
/// Internal storage for single-dimension vectors.
/// </summary>
internal sealed record VectorSingle<T>(T[] Values) : IVectorData, IEnumerable<T>
    where T : struct
{
    public (int rows, int cols) Dimensions => (1, Values.Length);
    public int Count => Values.Length;
    public Type ValueType => typeof(T);
    public bool IsMultiVector => false;

    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(VectorSingle<T>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        // Compare Values only
        return Values.SequenceEqual(other.Values);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (var value in Values)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }
}

/// <summary>
/// Internal storage for multi-dimension vectors (ColBERT-style).
/// </summary>
internal sealed record VectorMulti<T>(T[,] Values) : IVectorData, IEnumerable<T[]>
    where T : struct
{
    private readonly int _rows = Values.GetLength(0);
    private readonly int _cols = Values.GetLength(1);

    public (int rows, int cols) Dimensions => (_rows, _cols);
    public int Count => _rows * _cols;
    public Type ValueType => typeof(T[]);
    public bool IsMultiVector => true;

    public T[] this[Index row]
    {
        get
        {
            if (row.Value < 0 || row.Value >= _rows)
                throw new IndexOutOfRangeException(nameof(row));
            var result = new T[_cols];
            for (int i = 0; i < _cols; i++)
            {
                result[i] = Values[row.Value, i];
            }
            return result;
        }
    }

    public IEnumerator<T[]> GetEnumerator()
    {
        for (int i = 0; i < _rows; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Equals(VectorMulti<T>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        // Compare dimensions and all values
        if (_rows != other._rows || _cols != other._cols)
            return false;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                if (!EqualityComparer<T>.Default.Equals(Values[i, j], other.Values[i, j]))
                    return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_rows);
        hash.Add(_cols);
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                hash.Add(Values[i, j]);
            }
        }
        return hash.ToHashCode();
    }
}

/// <summary>
/// Represents vector data.
/// This is the user-facing type for vector inputs.
/// </summary>
[CollectionBuilder(typeof(VectorBuilder), nameof(VectorBuilder.Create))]
public class Vector : IEnumerable // Not sealed - allows NamedVector inheritance
{
    private readonly IVectorData _data;

    // Internal constructor for VectorSearchInputBuilder and derived classes
    internal Vector(IVectorData data)
    {
        _data = data;
    }

    /// <summary>
    /// Internal accessor for derived classes to extract data from other Vector instances.
    /// </summary>
    internal IVectorData GetData() => _data;

    public (int rows, int cols) Dimensions => _data.Dimensions;
    public int Count => _data.Count;
    public Type ValueType => _data.ValueType;
    public bool IsMultiVector => _data.IsMultiVector;

    // Implicit conversions for ergonomic syntax
    public static implicit operator Vector(float[] values) => new(new VectorSingle<float>(values));

    public static implicit operator Vector(double[] values) =>
        new(new VectorSingle<double>(values));

    public static implicit operator Vector(int[] values) => new(new VectorSingle<int>(values));

    public static implicit operator Vector(long[] values) => new(new VectorSingle<long>(values));

    public static implicit operator Vector(short[] values) => new(new VectorSingle<short>(values));

    public static implicit operator Vector(byte[] values) => new(new VectorSingle<byte>(values));

    public static implicit operator Vector(bool[] values) => new(new VectorSingle<bool>(values));

    public static implicit operator Vector(decimal[] values) =>
        new(new VectorSingle<decimal>(values));

    public static implicit operator Vector(float[,] values) => new(new VectorMulti<float>(values));

    public static implicit operator Vector(double[,] values) =>
        new(new VectorMulti<double>(values));

    public static implicit operator Vector(int[,] values) => new(new VectorMulti<int>(values));

    public static implicit operator Vector(long[,] values) => new(new VectorMulti<long>(values));

    public static implicit operator Vector(short[,] values) => new(new VectorMulti<short>(values));

    public static implicit operator Vector(byte[,] values) => new(new VectorMulti<byte>(values));

    public static implicit operator Vector(bool[,] values) => new(new VectorMulti<bool>(values));

    public static implicit operator Vector(decimal[,] values) =>
        new(new VectorMulti<decimal>(values));

    /// <summary>
    /// Pattern matching helper for type-safe access to underlying vector data.
    /// Used by gRPC layer to convert vectors to protobuf format.
    /// </summary>
    internal TResult Match<TResult>(Func<IVectorData, TResult> handler)
    {
        return handler(_data);
    }

    public IEnumerator GetEnumerator() =>
        _data switch
        {
            VectorSingle<float> v => v.Values.GetEnumerator(),
            VectorSingle<double> v => v.Values.GetEnumerator(),
            VectorSingle<int> v => v.Values.GetEnumerator(),
            VectorSingle<long> v => v.Values.GetEnumerator(),
            VectorSingle<short> v => v.Values.GetEnumerator(),
            VectorSingle<byte> v => v.Values.GetEnumerator(),
            VectorSingle<bool> v => v.Values.GetEnumerator(),
            VectorSingle<decimal> v => v.Values.GetEnumerator(),
            VectorMulti<float> v => v.GetEnumerator(),
            VectorMulti<double> v => v.GetEnumerator(),
            VectorMulti<int> v => v.GetEnumerator(),
            VectorMulti<long> v => v.GetEnumerator(),
            VectorMulti<short> v => v.GetEnumerator(),
            VectorMulti<byte> v => v.GetEnumerator(),
            VectorMulti<bool> v => v.GetEnumerator(),
            VectorMulti<decimal> v => v.GetEnumerator(),
            _ => throw new NotSupportedException(),
        };

    #region Implicit Operators to Native Arrays
    // SingleVector implicit operators (inverted)
    public static implicit operator double[](Vector vector) =>
        vector.GetData() is VectorSingle<double> v ? v.Values : throw new InvalidCastException();

    public static implicit operator float[](Vector vector) =>
        vector.GetData() is VectorSingle<float> v ? v.Values : throw new InvalidCastException();

    public static implicit operator int[](Vector vector) =>
        vector.GetData() is VectorSingle<int> v ? v.Values : throw new InvalidCastException();

    public static implicit operator long[](Vector vector) =>
        vector.GetData() is VectorSingle<long> v ? v.Values : throw new InvalidCastException();

    public static implicit operator short[](Vector vector) =>
        vector.GetData() is VectorSingle<short> v ? v.Values : throw new InvalidCastException();

    public static implicit operator byte[](Vector vector) =>
        vector.GetData() is VectorSingle<byte> v ? v.Values : throw new InvalidCastException();

    public static implicit operator bool[](Vector vector) =>
        vector.GetData() is VectorSingle<bool> v ? v.Values : throw new InvalidCastException();

    public static implicit operator decimal[](Vector vector) =>
        vector.GetData() is VectorSingle<decimal> v ? v.Values : throw new InvalidCastException();

    // MultiVector implicit operators (inverted)
    public static implicit operator double[,](Vector vector) =>
        vector.GetData() is VectorMulti<double> v ? v.Values : throw new InvalidCastException();

    public static implicit operator float[,](Vector vector) =>
        vector.GetData() is VectorMulti<float> v ? v.Values : throw new InvalidCastException();

    public static implicit operator int[,](Vector vector) =>
        vector.GetData() is VectorMulti<int> v ? v.Values : throw new InvalidCastException();

    public static implicit operator long[,](Vector vector) =>
        vector.GetData() is VectorMulti<long> v ? v.Values : throw new InvalidCastException();

    public static implicit operator short[,](Vector vector) =>
        vector.GetData() is VectorMulti<short> v ? v.Values : throw new InvalidCastException();

    public static implicit operator byte[,](Vector vector) =>
        vector.GetData() is VectorMulti<byte> v ? v.Values : throw new InvalidCastException();

    public static implicit operator bool[,](Vector vector) =>
        vector.GetData() is VectorMulti<bool> v ? v.Values : throw new InvalidCastException();

    public static implicit operator decimal[,](Vector vector) =>
        vector.GetData() is VectorMulti<decimal> v ? v.Values : throw new InvalidCastException();
    #endregion
}

/// <summary>
/// Builder for collection expression support on Vector type.
/// Defaults to int[] for integer literals (10, 20, 30).
/// Use float literals (10f, 20f, 30f) or explicit casts for other types.
/// </summary>
internal static class VectorBuilder
{
    public static Vector Create(ReadOnlySpan<object> values)
    {
        if (values.Length == 0)
            throw new ArgumentException(
                "Cannot create a Vector from an empty collection.",
                nameof(values)
            );

        var first = values[0];
        return first switch
        {
            double _ => values.ToArray().Cast<double>().ToArray(),
            float _ => values.ToArray().Cast<float>().ToArray(),
            int _ => values.ToArray().Cast<int>().ToArray(),
            long _ => values.ToArray().Cast<long>().ToArray(),
            short _ => values.ToArray().Cast<short>().ToArray(),
            byte _ => values.ToArray().Cast<byte>().ToArray(),
            bool _ => values.ToArray().Cast<bool>().ToArray(),
            decimal _ => values.ToArray().Cast<decimal>().ToArray(),
            _ => throw new NotSupportedException(
                $"Type {first.GetType()} is not supported for Vector creation."
            ),
        };
    }

    public static Vector Create(ReadOnlySpan<double> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<float> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<int> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<long> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<short> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<byte> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<bool> values) => values.ToArray();

    public static Vector Create(ReadOnlySpan<decimal> values) => values.ToArray();
}

/// <summary>
/// Represents a named vector - inherits from Vector and adds a Name property.
/// Used internally after builder processing.
/// </summary>
public sealed class NamedVector : Vector
{
    public string Name { get; init; } = "default";

    internal NamedVector(string name, IVectorData data)
        : base(data)
    {
        Name = name;
    }

    /// <summary>
    /// Creates a NamedVector from a Vector with a specified name.
    /// Uses protected GetData() to extract internal data.
    /// </summary>
    public NamedVector(string name, Vector data)
        : this(name, data.GetData()) { }

    /// <summary>
    /// Creates a NamedVector from a Vector with a specified name.
    /// Uses protected GetData() to extract internal data.
    /// </summary>
    public NamedVector(Vector data)
        : this(data is NamedVector named ? named.Name : "default", data.GetData()) { }

    public static implicit operator NamedVector((string name, Vector vector) namedVector) =>
        new(namedVector.name, namedVector.vector);
}

/// <summary>
/// Collection of named vectors.
/// </summary>
public class Vectors : Internal.KeySortedList<string, Vector>
{
    public Vectors()
        : base(v => (v as NamedVector)?.Name ?? "default") { }

    public Vectors(IEnumerable<Vector> vector)
        : this()
    {
        foreach (var v in vector)
        {
            Add(v);
        }
    }

    public Vectors(params Vector[] vector)
        : this(vector.AsEnumerable()) { }

    public Vectors(string name, Vector vector)
        : this((name, vector)) { }

    public Vectors(params (string name, Vector vector)[] vectors)
        : this(vectors.Select(v => new NamedVector(v.name, v.vector))) { }

    // Implicit conversions
    public static implicit operator Vectors(NamedVector vector) => new(vector);

    public static implicit operator Vectors(NamedVector[] vector) => new(vector);

    public static implicit operator Vectors(Vector vector) => new(vector);

    public static implicit operator Vectors((string name, Vector vector) v) =>
        new(v.name, v.vector);

    public static implicit operator Vectors((string name, Vector vector)[] v) => new(v);

    #region Implicit Operators: Vectors from Native Arrays
    public static implicit operator Vectors(float[] values) => new(values);

    public static implicit operator Vectors(double[] values) => new(values);

    public static implicit operator Vectors(int[] values) => new(values);

    public static implicit operator Vectors(long[] values) => new(values);

    public static implicit operator Vectors(short[] values) => new(values);

    public static implicit operator Vectors(byte[] values) => new(values);

    public static implicit operator Vectors(bool[] values) => new(values);

    public static implicit operator Vectors(decimal[] values) => new(values);

    public static implicit operator Vectors(float[,] values) => new(values);

    public static implicit operator Vectors(double[,] values) => new(values);

    public static implicit operator Vectors(int[,] values) => new(values);

    public static implicit operator Vectors(long[,] values) => new(values);

    public static implicit operator Vectors(short[,] values) => new(values);

    public static implicit operator Vectors(byte[,] values) => new(values);

    public static implicit operator Vectors(bool[,] values) => new(values);

    public static implicit operator Vectors(decimal[,] values) => new(values);
    #endregion

    #region Implicit Operators: Vectors from Dictionary<string, T[]>
    public static implicit operator Vectors(Dictionary<string, float[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, double[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, int[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, long[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, short[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, byte[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, bool[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, decimal[]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());
    #endregion

    #region Implicit Operators: Vectors from Dictionary<string, T[,]>
    public static implicit operator Vectors(Dictionary<string, float[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, double[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, int[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, long[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, short[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, byte[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, bool[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());

    public static implicit operator Vectors(Dictionary<string, decimal[,]> vectors) =>
        new(vectors.Select(kvp => (kvp.Key, (Vector)kvp.Value)).ToArray());
    #endregion
}
