using System.Collections;

namespace Weaviate.Client.Models;

public interface IHybridVectorInput
{
    // This interface is used to mark hybrid vectors, which can be either near vector or near text.
    // It allows for polymorphic behavior in the Hybrid methods.
}

public interface INearVectorInput { }

public record NearVectorInput : IEnumerable<Vector>, IHybridVectorInput, INearVectorInput
{
    private readonly List<Vector> _vectors = [];

    public IReadOnlyDictionary<string, Vector[]> Vectors =>
        _vectors.GroupBy(v => v.Name).ToDictionary(g => g.Key, g => g.ToArray());

    public NearVectorInput() { }

    public NearVectorInput(params Vector[] vectors)
    {
        foreach (var v in vectors)
        {
            Add(v.Name, v);
        }
    }

    public static implicit operator NearVectorInput(Vector vector)
    {
        return new NearVectorInput([vector]);
    }

    public static implicit operator NearVectorInput(Vector[] vector)
    {
        return new NearVectorInput(vector);
    }

    public static implicit operator NearVectorInput(Vectors vectors)
    {
        return new NearVectorInput([.. vectors.Values]);
    }

    public void Add(string name, params Vector[] values) =>
        _vectors.AddRange(values.Select(v => Vector.Create(name, v)));

    public void Add(Models.Vectors vectors)
    {
        _vectors.AddRange(vectors.Values);
    }

    private static NearVectorInput FromVectorDictionary(
        IEnumerable<KeyValuePair<string, IEnumerable<Vector>>> vectors
    )
    {
        var ret = new NearVectorInput();
        foreach (var (name, values) in vectors)
        {
            ret.Add(name, [.. values]);
        }
        return ret;
    }

    private static NearVectorInput FromSingleVectorDictionary<T>(
        Dictionary<string, T> vectors,
        Func<T, Vector> converter
    )
    {
        var ret = new NearVectorInput();
        foreach (var (name, value) in vectors)
        {
            ret.Add(name, converter(value));
        }
        return ret;
    }

    public static implicit operator NearVectorInput(Dictionary<string, Vector[]> vectors) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.AsEnumerable()))
        );

    public static implicit operator NearVectorInput(Dictionary<string, float[]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(Dictionary<string, double[]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(Dictionary<string, float[,]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(Dictionary<string, double[,]> vectors) =>
        FromSingleVectorDictionary(vectors, Vector.Create);

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<float[]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<double[]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<float[,]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    public static implicit operator NearVectorInput(
        Dictionary<string, IEnumerable<double[,]>> vectors
    ) =>
        FromVectorDictionary(
            vectors.Select(kvp => KeyValuePair.Create(kvp.Key, kvp.Value.Select(Vector.Create)))
        );

    IEnumerator<Vector> IEnumerable<Vector>.GetEnumerator()
    {
        return ((IEnumerable<Vector>)_vectors).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_vectors).GetEnumerator();
    }
}

public record HybridNearVector(
    NearVectorInput Vector,
    float? Certainty = null,
    float? Distance = null,
    TargetVectors? targetVector = null
) : IHybridVectorInput { };

public record HybridNearText(
    string Query,
    float? Certainty = null,
    float? Distance = null,
    Move? MoveTo = null,
    Move? MoveAway = null
) : IHybridVectorInput;

public abstract record BM25Operator(string Operator)
{
    public record And() : BM25Operator("And");

    public record Or(int MinimumMatch) : BM25Operator("Or");
}
