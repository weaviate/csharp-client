namespace Weaviate.Client.Models.Typed;

/// <summary>
/// Specifies which metrics to query for an Aggregate.Text property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class TextMetricsAttribute : Attribute
{
    public bool Count { get; init; }
    public bool TopOccurrences { get; init; }
    public uint MinOccurrences { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Integer property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class IntegerMetricsAttribute : Attribute
{
    public bool Count { get; init; }
    public bool Sum { get; init; }
    public bool Mean { get; init; }
    public bool Minimum { get; init; }
    public bool Maximum { get; init; }
    public bool Median { get; init; }
    public bool Mode { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Number property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class NumberMetricsAttribute : Attribute
{
    public bool Count { get; init; }
    public bool Sum { get; init; }
    public bool Mean { get; init; }
    public bool Minimum { get; init; }
    public bool Maximum { get; init; }
    public bool Median { get; init; }
    public bool Mode { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Boolean property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class BooleanMetricsAttribute : Attribute
{
    public bool Count { get; init; }
    public bool TotalTrue { get; init; }
    public bool TotalFalse { get; init; }
    public bool PercentageTrue { get; init; }
    public bool PercentageFalse { get; init; }
}

/// <summary>
/// Specifies which metrics to query for an Aggregate.Date property.
/// Without this attribute, all metrics are queried by default.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class DateMetricsAttribute : Attribute
{
    public bool Count { get; init; }
    public bool Minimum { get; init; }
    public bool Maximum { get; init; }
    public bool Median { get; init; }
    public bool Mode { get; init; }
}
