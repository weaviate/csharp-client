namespace Weaviate.Client.Models;

/// <summary>
/// The weighted field
/// </summary>
public record WeightedField(string Name, double Weight)
{
    /// <summary>
    /// Implicitly converts a tuple to a WeightedField
    /// </summary>
    /// <param name="tuple">The tuple containing name and weight</param>
    public static implicit operator WeightedField((string Name, double Weight) tuple) =>
        new(tuple.Name, tuple.Weight);
};

/// <summary>
/// The weighted fields class
/// </summary>
/// <seealso cref="List{WeightedField}"/>
public class WeightedFields : List<WeightedField>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedFields"/> class
    /// </summary>
    public WeightedFields() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedFields"/> class
    /// </summary>
    /// <param name="fields">The fields</param>
    public WeightedFields(IEnumerable<WeightedField> fields)
        : base(fields) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeightedFields"/> class
    /// </summary>
    /// <param name="fields">The fields</param>
    public WeightedFields(params WeightedField[] fields)
        : base(fields) { }

    /// <summary>
    /// Gets the value of the field names
    /// </summary>
    public string[] FieldNames => [.. this.Select(f => f.Name)];

    /// <summary>
    /// Gets the value of the weights
    /// </summary>
    public double[] Weights => [.. this.Select(f => f.Weight)];

    /// <summary>
    /// Implicitly converts WeightedFields to a string array of field names
    /// </summary>
    /// <param name="weightedFields">The weighted fields</param>
    public static implicit operator string[]?(WeightedFields? weightedFields) =>
        weightedFields?.FieldNames;
}
