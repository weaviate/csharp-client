namespace Weaviate.Client.Models.Typed;

/// <summary>
/// Specifies which metrics to query for an Aggregate.Text property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TextMetricsAttribute : Attribute
{
    /// <summary>
    /// Gets or inits the value of the count
    /// </summary>
    public bool Count { get; init; }

    /// <summary>
    /// Gets or inits the value of the top occurrences
    /// </summary>
    public bool TopOccurrences { get; init; }

    /// <summary>
    /// Gets or inits the value of the min occurrences
    /// </summary>
    public uint MinOccurrences { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Integer property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class IntegerMetricsAttribute : Attribute
{
    /// <summary>
    /// Gets or inits the value of the count
    /// </summary>
    public bool Count { get; init; }

    /// <summary>
    /// Gets or inits the value of the sum
    /// </summary>
    public bool Sum { get; init; }

    /// <summary>
    /// Gets or inits the value of the mean
    /// </summary>
    public bool Mean { get; init; }

    /// <summary>
    /// Gets or inits the value of the minimum
    /// </summary>
    public bool Minimum { get; init; }

    /// <summary>
    /// Gets or inits the value of the maximum
    /// </summary>
    public bool Maximum { get; init; }

    /// <summary>
    /// Gets or inits the value of the median
    /// </summary>
    public bool Median { get; init; }

    /// <summary>
    /// Gets or inits the value of the mode
    /// </summary>
    public bool Mode { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Number property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NumberMetricsAttribute : Attribute
{
    /// <summary>
    /// Gets or inits the value of the count
    /// </summary>
    public bool Count { get; init; }

    /// <summary>
    /// Gets or inits the value of the sum
    /// </summary>
    public bool Sum { get; init; }

    /// <summary>
    /// Gets or inits the value of the mean
    /// </summary>
    public bool Mean { get; init; }

    /// <summary>
    /// Gets or inits the value of the minimum
    /// </summary>
    public bool Minimum { get; init; }

    /// <summary>
    /// Gets or inits the value of the maximum
    /// </summary>
    public bool Maximum { get; init; }

    /// <summary>
    /// Gets or inits the value of the median
    /// </summary>
    public bool Median { get; init; }

    /// <summary>
    /// Gets or inits the value of the mode
    /// </summary>
    public bool Mode { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Boolean property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BooleanMetricsAttribute : Attribute
{
    /// <summary>
    /// Gets or inits the value of the count
    /// </summary>
    public bool Count { get; init; }

    /// <summary>
    /// Gets or inits the value of the total true
    /// </summary>
    public bool TotalTrue { get; init; }

    /// <summary>
    /// Gets or inits the value of the total false
    /// </summary>
    public bool TotalFalse { get; init; }

    /// <summary>
    /// Gets or inits the value of the percentage true
    /// </summary>
    public bool PercentageTrue { get; init; }

    /// <summary>
    /// Gets or inits the value of the percentage false
    /// </summary>
    public bool PercentageFalse { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Date property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DateMetricsAttribute : Attribute
{
    /// <summary>
    /// Gets or inits the value of the count
    /// </summary>
    public bool Count { get; init; }

    /// <summary>
    /// Gets or inits the value of the minimum
    /// </summary>
    public bool Minimum { get; init; }

    /// <summary>
    /// Gets or inits the value of the maximum
    /// </summary>
    public bool Maximum { get; init; }

    /// <summary>
    /// Gets or inits the value of the median
    /// </summary>
    public bool Median { get; init; }

    /// <summary>
    /// Gets or inits the value of the mode
    /// </summary>
    public bool Mode { get; init; }
}
