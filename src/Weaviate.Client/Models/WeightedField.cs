namespace Weaviate.Client.Models;

public record WeightedField(string Name, double Weight)
{
    public static implicit operator WeightedField((string Name, double Weight) tuple) =>
        new(tuple.Name, tuple.Weight);
};

public class WeightedFields : List<WeightedField>
{
    public WeightedFields() { }

    public WeightedFields(IEnumerable<WeightedField> fields)
        : base(fields) { }

    public WeightedFields(params WeightedField[] fields)
        : base(fields) { }

    public string[] FieldNames => [.. this.Select(f => f.Name)];
    public double[] Weights => [.. this.Select(f => f.Weight)];

    public static implicit operator string[]?(WeightedFields? weightedFields) =>
        weightedFields?.FieldNames;
}
