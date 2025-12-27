using System.Collections;

namespace Weaviate.Client.Models;

/// <summary>
/// Represents a vector for use with Weaviate, supporting both single and multi-vector configurations.
/// Vectors can contain numeric values of various types (double, float, int, etc.) and can be implicitly converted from native arrays.
/// </summary>
/// <remarks>
/// This is the base class for all vector types in the Weaviate client library.
/// Use <see cref="VectorSingle{T}"/> for single vectors or <see cref="VectorMulti{T}"/> for multi-vector representations.
/// Supports implicit conversion from and to native C# arrays.
/// </remarks>
public abstract record Vector : IEnumerable, IHybridVectorInput, INearVectorInput
{
    /// <summary>
    /// Gets or initializes the name of this vector. Defaults to "default".
    /// Used to identify specific vectors in multi-vector configurations.
    /// </summary>
    public string Name { get; init; } = "default";

    /// <summary>
    /// Gets the number of dimensions in this vector.
    /// For single vectors, this is the length of the array. For multi-vectors, this is the number of rows.
    /// </summary>
    public abstract int Dimensions { get; }

    /// <summary>
    /// Gets the number of vectors contained. Returns 1 for single vectors, greater than 1 for multi-vectors.
    /// </summary>
    public abstract int Count { get; }

    /// <summary>
    /// Gets the type of values stored in this vector (e.g., typeof(float), typeof(double)).
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// Gets a value indicating whether this is a multi-vector (Count > 1).
    /// </summary>
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

    /// <summary>
    /// Creates a copy of a vector with a new name.
    /// </summary>
    /// <param name="name">The name to assign to the vector.</param>
    /// <param name="values">The vector to copy.</param>
    /// <returns>A new vector instance with the specified name.</returns>
    public static Vector Create(string name, Vector values)
    {
        return values with { Name = name };
    }

    /// <summary>
    /// Creates a single vector from an array of values.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="values">The vector values.</param>
    /// <returns>A new <see cref="VectorSingle{T}"/> instance.</returns>
    public static Vector Create<T>(params T[] values) => new VectorSingle<T>(values);

    /// <summary>
    /// Creates a multi-vector from a 2D array of values.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="values">The 2D array representing multiple vectors.</param>
    /// <returns>A new <see cref="VectorMulti{T}"/> instance.</returns>
    public static Vector Create<T>(T[,] values) => new VectorMulti<T>(values);

    /// <summary>
    /// Creates a named single vector from an array of values.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="name">The name to assign to this vector.</param>
    /// <param name="values">The vector values.</param>
    /// <returns>A new <see cref="VectorSingle{T}"/> instance with the specified name.</returns>
    public static Vector Create<T>(string name, params T[] values)
        where T : struct => new VectorSingle<T>(values) { Name = name };

    /// <summary>
    /// Creates a named multi-vector from a 2D array of values.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="name">The name to assign to this vector.</param>
    /// <param name="values">The 2D array representing multiple vectors.</param>
    /// <returns>A new <see cref="VectorMulti{T}"/> instance with the specified name.</returns>
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

/// <summary>
/// Represents a single vector containing values of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of values in the vector (e.g., float, double, int).</typeparam>
/// <remarks>
/// This class is used for standard vector representations where you have a single embedding.
/// For multi-vector scenarios (e.g., late interaction), use <see cref="VectorMulti{T}"/> instead.
/// </remarks>
public sealed record VectorSingle<T> : Vector, IEnumerable<T>
{
    /// <summary>
    /// Gets the number of dimensions (length of the values array).
    /// </summary>
    public override int Dimensions => Values.Length;

    /// <summary>
    /// Gets the count of vectors, which is always 1 for a single vector.
    /// </summary>
    public override int Count => 1;

    /// <summary>
    /// Gets the type of values stored in this vector.
    /// </summary>
    public override Type ValueType => typeof(T);

    /// <summary>
    /// Returns an enumerator that iterates through the vector values.
    /// </summary>
    /// <returns>An enumerator for the values.</returns>
    public new IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)Values).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Gets or initializes the array of vector values.
    /// </summary>
    public T[] Values { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorSingle{T}"/> class with the specified values.
    /// </summary>
    /// <param name="values">The values for this vector.</param>
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

/// <summary>
/// Represents a multi-vector containing multiple vectors of type <typeparamref name="T"/> in a 2D array.
/// </summary>
/// <typeparam name="T">The type of values in each vector (e.g., float, double, int).</typeparam>
/// <remarks>
/// This class is used for multi-vector representations such as late interaction or ColBERT-style embeddings,
/// where each object has multiple vectors. The 2D array is organized as [rows, columns] where each row
/// represents a separate vector.
/// </remarks>
public sealed record VectorMulti<T> : Vector, IEnumerable<T[]>
{
    /// <summary>
    /// Gets the number of dimensions (number of rows in the 2D array).
    /// </summary>
    public override int Dimensions => _rows;

    /// <summary>
    /// Gets the type of values stored (array of <typeparamref name="T"/>).
    /// </summary>
    public override Type ValueType => typeof(T[]);

    /// <summary>
    /// Gets the number of vectors (number of columns in the 2D array).
    /// </summary>
    public override int Count => _cols;

    /// <summary>
    /// Returns an enumerator that iterates through each vector (row) in the multi-vector.
    /// </summary>
    /// <returns>An enumerator for the vectors.</returns>
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

    /// <summary>
    /// Initializes a new instance of the <see cref="VectorMulti{T}"/> class with the specified 2D array.
    /// </summary>
    /// <param name="values">The 2D array of vectors, organized as [rows, columns].</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="values"/> is null.</exception>
    public VectorMulti(T[,] values)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        _rows = values.GetLength(0);
        _cols = values.GetLength(1);
    }

    /// <summary>
    /// Gets the underlying 2D array of vector values.
    /// </summary>
    public T[,] Values => _values;

    /// <summary>
    /// Gets the value at the specified dimension (row) and index (column).
    /// </summary>
    /// <param name="dimension">The row index.</param>
    /// <param name="index">The column index.</param>
    /// <returns>The value at the specified position.</returns>
    public T this[int dimension, int index] => _values[dimension, index];

    /// <summary>
    /// Gets a specific vector (row) from the multi-vector.
    /// </summary>
    /// <param name="dimension">The index of the vector to retrieve.</param>
    /// <returns>An array representing the vector at the specified dimension.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown when <paramref name="dimension"/> is out of range.</exception>
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

/// <summary>
/// A collection of named vectors, used when working with multi-vector configurations or named vector spaces.
/// </summary>
/// <remarks>
/// This class extends <see cref="Dictionary{TKey, TValue}"/> where keys are vector names and values are <see cref="Vector"/> instances.
/// Provides convenient methods for adding and creating vectors with various formats.
/// Supports implicit conversion from native arrays and dictionaries.
/// </remarks>
public class Vectors : Dictionary<string, Vector>, IHybridVectorInput, INearVectorInput
{
    /// <summary>
    /// Adds a vector with the default name ("default") using the specified values.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="value">The array of values.</param>
    public void Add<T>(T[] value)
        where T : struct
    {
        Add("default", value);
    }

    /// <summary>
    /// Adds a multi-vector with the default name ("default") using the specified 2D array.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="value">The 2D array of values.</param>
    public void Add<T>(T[,] value)
        where T : struct
    {
        Add("default", value);
    }

    /// <summary>
    /// Adds a named vector using the specified values.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="name">The name for this vector.</param>
    /// <param name="values">The array of values.</param>
    public void Add<T>(string name, params T[] values)
        where T : struct
    {
        Add(new VectorSingle<T>(values) { Name = name });
    }

    /// <summary>
    /// Adds a named multi-vector using the specified 2D array.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="name">The name for this multi-vector.</param>
    /// <param name="values">The 2D array of values.</param>
    public void Add<T>(string name, T[,] values)
        where T : struct
    {
        Add(new VectorMulti<T>(values) { Name = name });
    }

    /// <summary>
    /// Adds a <see cref="Vector"/> instance to the collection using its name as the key.
    /// </summary>
    /// <param name="vector">The vector to add.</param>
    public void Add(Vector vector)
    {
        base.Add(vector.Name, vector);
    }

    /// <summary>
    /// Creates a new <see cref="Vectors"/> collection containing a single vector with the default name.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="values">The array of values.</param>
    /// <returns>A new <see cref="Vectors"/> instance.</returns>
    public static Vectors Create<T>(params T[] values)
        where T : struct
    {
        return new Vectors { new VectorSingle<T>(values) };
    }

    /// <summary>
    /// Creates a new <see cref="Vectors"/> collection containing a multi-vector with the default name.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="values">The 2D array of values.</param>
    /// <returns>A new <see cref="Vectors"/> instance.</returns>
    public static Vectors Create<T>(T[,] values)
        where T : struct
    {
        return new Vectors { new VectorMulti<T>(values) };
    }

    /// <summary>
    /// Creates a new <see cref="Vectors"/> collection containing a single vector.
    /// </summary>
    /// <param name="vector">The vector to include.</param>
    /// <returns>A new <see cref="Vectors"/> instance.</returns>
    public static Vectors Create(Vector vector) => new Vectors { vector };

    /// <summary>
    /// Creates a new <see cref="Vectors"/> collection containing a named single vector.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="name">The name for this vector.</param>
    /// <param name="values">The array of values.</param>
    /// <returns>A new <see cref="Vectors"/> instance.</returns>
    public static Vectors Create<T>(string name, params T[] values)
        where T : struct
    {
        return new Vectors { new VectorSingle<T>(values) { Name = name } };
    }

    /// <summary>
    /// Creates a new <see cref="Vectors"/> collection containing a named multi-vector.
    /// </summary>
    /// <typeparam name="T">The type of vector values.</typeparam>
    /// <param name="name">The name for this multi-vector.</param>
    /// <param name="values">The 2D array of values.</param>
    /// <returns>A new <see cref="Vectors"/> instance.</returns>
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
