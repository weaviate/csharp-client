using System.Collections;

namespace Weaviate.Client.Models;

public abstract record Vector : IEnumerable, IHybridVectorInput
{
    public string Name { get; init; } = "default";
    public abstract int Dimensions { get; }
    public abstract int Count { get; }
    public abstract Type ValueType { get; }
    public bool IsMultiVector => Count > 1;

    public object this[Index index]
    {
        get
        {
            return this switch
            {
                VectorSingle<double> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<float> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<int> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<long> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<short> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<byte> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<bool> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorSingle<decimal> v => v.Values[index.GetOffset(v.Values.Length)],
                VectorMulti<double> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<float> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<int> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<long> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<short> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<byte> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<bool> v => v[index.GetOffset(v.Dimensions)],
                VectorMulti<decimal> v => v[index.GetOffset(v.Dimensions)],
                _ => throw new NotImplementedException(
                    "Indexing not supported for this vector type."
                ),
            };
        }
    }

    public static Vector Create<T>(params T[] values) => new VectorSingle<T>(values);

    public static Vector Create<T>(T[,] values) => new VectorMulti<T>(values);

    public static Vector Create<T>(string name, params T[] values)
        where T : struct => new VectorSingle<T>(values) { Name = name };

    public static Vector Create<T>(string name, T[,] values)
        where T : struct => new VectorMulti<T>(values) { Name = name };

    public IEnumerator GetEnumerator()
    {
        return this switch
        {
            VectorSingle<double> v => v.Values.GetEnumerator(),
            VectorSingle<float> v => v.Values.GetEnumerator(),
            VectorSingle<int> v => v.Values.GetEnumerator(),
            VectorSingle<long> v => v.Values.GetEnumerator(),
            VectorSingle<short> v => v.Values.GetEnumerator(),
            VectorSingle<byte> v => v.Values.GetEnumerator(),
            VectorSingle<bool> v => v.Values.GetEnumerator(),
            VectorSingle<decimal> v => v.Values.GetEnumerator(),
            VectorMulti<double> v => v.Values.GetEnumerator(),
            VectorMulti<float> v => v.Values.GetEnumerator(),
            VectorMulti<int> v => v.Values.GetEnumerator(),
            VectorMulti<long> v => v.Values.GetEnumerator(),
            VectorMulti<short> v => v.Values.GetEnumerator(),
            VectorMulti<byte> v => v.Values.GetEnumerator(),
            VectorMulti<bool> v => v.Values.GetEnumerator(),
            VectorMulti<decimal> v => v.Values.GetEnumerator(),
            _ => throw new NotImplementedException("Use a derived type to enumerate values."),
        };
    }

    #region Implicit Operators to Native Arrays
    // SingleVector implicit operators (inverted)
    public static implicit operator double[](Vector vector) =>
        vector is VectorSingle<double> v ? v.Values : throw new InvalidCastException();

    public static implicit operator float[](Vector vector) =>
        vector is VectorSingle<float> v ? v.Values : throw new InvalidCastException();

    public static implicit operator int[](Vector vector) =>
        vector is VectorSingle<int> v ? v.Values : throw new InvalidCastException();

    public static implicit operator long[](Vector vector) =>
        vector is VectorSingle<long> v ? v.Values : throw new InvalidCastException();

    public static implicit operator short[](Vector vector) =>
        vector is VectorSingle<short> v ? v.Values : throw new InvalidCastException();

    public static implicit operator byte[](Vector vector) =>
        vector is VectorSingle<byte> v ? v.Values : throw new InvalidCastException();

    public static implicit operator bool[](Vector vector) =>
        vector is VectorSingle<bool> v ? v.Values : throw new InvalidCastException();

    public static implicit operator decimal[](Vector vector) =>
        vector is VectorSingle<decimal> v ? v.Values : throw new InvalidCastException();

    // MultiVector implicit operators (inverted)
    public static implicit operator double[,](Vector vector) =>
        vector is VectorMulti<double> v ? v.Values : throw new InvalidCastException();

    public static implicit operator float[,](Vector vector) =>
        vector is VectorMulti<float> v ? v.Values : throw new InvalidCastException();

    public static implicit operator int[,](Vector vector) =>
        vector is VectorMulti<int> v ? v.Values : throw new InvalidCastException();

    public static implicit operator long[,](Vector vector) =>
        vector is VectorMulti<long> v ? v.Values : throw new InvalidCastException();

    public static implicit operator short[,](Vector vector) =>
        vector is VectorMulti<short> v ? v.Values : throw new InvalidCastException();

    public static implicit operator byte[,](Vector vector) =>
        vector is VectorMulti<byte> v ? v.Values : throw new InvalidCastException();

    public static implicit operator bool[,](Vector vector) =>
        vector is VectorMulti<bool> v ? v.Values : throw new InvalidCastException();

    public static implicit operator decimal[,](Vector vector) =>
        vector is VectorMulti<decimal> v ? v.Values : throw new InvalidCastException();
    #endregion

    #region Implicit Operators to Vector
    // SingleVector implicit operators
    public static implicit operator Vector(double[] values) => new VectorSingle<double>(values);

    public static implicit operator Vector(float[] values) => new VectorSingle<float>(values);

    public static implicit operator Vector(int[] values) => new VectorSingle<int>(values);

    public static implicit operator Vector(long[] values) => new VectorSingle<long>(values);

    public static implicit operator Vector(short[] values) => new VectorSingle<short>(values);

    public static implicit operator Vector(byte[] values) => new VectorSingle<byte>(values);

    public static implicit operator Vector(bool[] values) => new VectorSingle<bool>(values);

    public static implicit operator Vector(decimal[] values) => new VectorSingle<decimal>(values);

    // MultiVector implicit operators
    public static implicit operator Vector(double[,] values) => new VectorMulti<double>(values);

    public static implicit operator Vector(float[,] values) => new VectorMulti<float>(values);

    public static implicit operator Vector(int[,] values) => new VectorMulti<int>(values);

    public static implicit operator Vector(long[,] values) => new VectorMulti<long>(values);

    public static implicit operator Vector(short[,] values) => new VectorMulti<short>(values);

    public static implicit operator Vector(byte[,] values) => new VectorMulti<byte>(values);

    public static implicit operator Vector(bool[,] values) => new VectorMulti<bool>(values);

    public static implicit operator Vector(decimal[,] values) => new VectorMulti<decimal>(values);
    #endregion
}

public sealed record VectorSingle<T> : Vector, IEnumerable<T>
{
    public override int Dimensions => Values.Length;
    public override int Count => 1;
    public override Type ValueType => typeof(T);

    public new IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public T[] Values { get; init; }

    public VectorSingle(params T[] values)
    {
        Values = values;
    }

    public bool Equals(VectorSingle<T>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        // Compare Name and Values
        return Name == other.Name && Values.SequenceEqual(other.Values);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        foreach (var value in Values)
        {
            hash.Add(value);
        }
        return hash.ToHashCode();
    }
}

public sealed record VectorMulti<T> : Vector, IEnumerable<T[]>
{
    public override int Dimensions => _rows;
    public override Type ValueType => typeof(T[]);
    public override int Count => _cols;

    public new IEnumerator<T[]> GetEnumerator()
    {
        for (int i = 0; i < _rows; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private readonly T[,] _values;
    private readonly int _rows;
    private readonly int _cols;

    public VectorMulti(T[,] values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        _rows = values.GetLength(0);
        _cols = values.GetLength(1);
    }

    // Expose the underlying array as a property if needed
    public T[,] Values => _values;

    // Optionally, provide an indexer for row/col access
    public T this[int dimension, int index] => _values[dimension, index];

    public T[] this[int dimension]
    {
        get
        {
            if (dimension < 0 || dimension >= _rows)
                throw new IndexOutOfRangeException(nameof(dimension));
            var result = new T[_cols];
            for (int i = 0; i < _cols; i++)
            {
                result[i] = _values[dimension, i];
            }
            return result;
        }
    }

    public bool Equals(VectorMulti<T>? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;

        // Compare Name, dimensions, and all values
        if (Name != other.Name || _rows != other._rows || _cols != other._cols)
            return false;

        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                if (!EqualityComparer<T>.Default.Equals(_values[i, j], other._values[i, j]))
                    return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(_rows);
        hash.Add(_cols);
        for (int i = 0; i < _rows; i++)
        {
            for (int j = 0; j < _cols; j++)
            {
                hash.Add(_values[i, j]);
            }
        }
        return hash.ToHashCode();
    }
}

public class Vectors : Dictionary<string, Vector>, IHybridVectorInput
{
    public void Add<T>(T[] value)
        where T : struct
    {
        Add("default", value);
    }

    public void Add<T>(T[,] value)
        where T : struct
    {
        Add("default", value);
    }

    public void Add<T>(string name, params T[] values)
        where T : struct
    {
        Add(new VectorSingle<T>(values) { Name = name });
    }

    public void Add<T>(string name, T[,] values)
        where T : struct
    {
        Add(new VectorMulti<T>(values) { Name = name });
    }

    public void Add(Vector vector)
    {
        base.Add(vector.Name, vector);
    }

    // Create vector data for simple struct values
    public static Vectors Create<T>(params T[] values)
        where T : struct
    {
        return new Vectors { new VectorSingle<T>(values) };
    }

    public static Vectors Create<T>(T[,] values)
        where T : struct
    {
        return new Vectors { new VectorMulti<T>(values) };
    }

    public static Vectors Create(Vector vector) => new Vectors { vector };

    public static Vectors Create<T>(string name, params T[] values)
        where T : struct
    {
        return new Vectors { new VectorSingle<T>(values) { Name = name } };
    }

    public static Vectors Create<T>(string name, T[,] values)
        where T : struct
    {
        return new Vectors { new VectorMulti<T>(values) { Name = name } };
    }

    public static implicit operator Vectors(Vector vector)
    {
        return Vectors.Create(vector);
    }

    #region Implicit Operators: Vectors from Native Arrays
    public static implicit operator Vectors(double[] values) => Create(values);

    public static implicit operator Vectors(float[] values) => Create(values);

    public static implicit operator Vectors(int[] values) => Create(values);

    public static implicit operator Vectors(long[] values) => Create(values);

    public static implicit operator Vectors(short[] values) => Create(values);

    public static implicit operator Vectors(byte[] values) => Create(values);

    public static implicit operator Vectors(bool[] values) => Create(values);

    public static implicit operator Vectors(decimal[] values) => Create(values);

    public static implicit operator Vectors(double[,] values) => Create(values);

    public static implicit operator Vectors(float[,] values) => Create(values);

    public static implicit operator Vectors(int[,] values) => Create(values);

    public static implicit operator Vectors(long[,] values) => Create(values);

    public static implicit operator Vectors(short[,] values) => Create(values);

    public static implicit operator Vectors(byte[,] values) => Create(values);

    public static implicit operator Vectors(bool[,] values) => Create(values);

    public static implicit operator Vectors(decimal[,] values) => Create(values);
    #endregion

    #region Implicit Operators: Vectors from Dictionary<string, T[]>
    public static implicit operator Vectors(Dictionary<string, double[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, float[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, int[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, long[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, short[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, byte[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, bool[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, decimal[]> vectors) =>
        CreateVectorsFromDictionary(vectors);

    private static Vectors CreateVectorsFromDictionary<T>(Dictionary<string, T[]> vectors)
        where T : struct
    {
        var container = new Vectors();
        foreach (var kvp in vectors)
        {
            container.Add(new VectorSingle<T>(kvp.Value) { Name = kvp.Key });
        }
        return container;
    }
    #endregion

    #region Implicit Operators: Vectors from Dictionary<string, T[,]>
    public static implicit operator Vectors(Dictionary<string, double[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, float[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, int[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, long[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, short[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, byte[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, bool[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    public static implicit operator Vectors(Dictionary<string, decimal[,]> vectors) =>
        CreateVectorsFromMultiDictionary(vectors);

    private static Vectors CreateVectorsFromMultiDictionary<T>(Dictionary<string, T[,]> vectors)
        where T : struct
    {
        var container = new Vectors();
        foreach (var kvp in vectors)
        {
            container.Add(new VectorMulti<T>(kvp.Value) { Name = kvp.Key });
        }
        return container;
    }
    #endregion
}
