namespace Weaviate.Client.Models;

public partial record GroupedBy(string Property, Type Type);

public partial record AggregateGroup
{
    public required GroupedBy GroupedBy { get; init; }
    IDictionary<string, Aggregate.Property> Properties { get; init; } =
        new Dictionary<string, Aggregate.Property>();
    public long TotalCount { get; init; } = 0;
}

public partial record AggregateGroupByResult
{
    public List<AggregateGroup> Groups { get; init; } = new();
}

public partial record AggregateResult
{
    public IDictionary<string, Aggregate.Property> Properties { get; init; } =
        new Dictionary<string, Aggregate.Property>();

    public long TotalCount { get; init; }
}

public abstract record Metric(string Name)
{
    public record Text(string Name) : Metric(Name);

    public record Integer(string Name) : Metric(Name);

    public record Number(string Name) : Metric(Name);

    public record Boolean(string Name) : Metric(Name);

    public record Date(string Name) : Metric(Name);
}

public class PropertyMetricBuilder(string Name)
{
    public Metric Text => new Metric.Text(Name);

    public Metric Integer => new Metric.Integer(Name);

    public Metric Number => new Metric.Number(Name);

    public Metric Boolean => new Metric.Boolean(Name);

    public Metric Date => new Metric.Date(Name);
}

public static class Metrics
{
    public static PropertyMetricBuilder ForProperty(string propertyName) => new(propertyName);
}

public static class Aggregate
{
    public abstract record Property
    {
        public long? Count { get; internal init; }
    }

    public record TopOccurrence<T> : Property
    {
        public T? Value { get; internal init; }
    }

    public record Text : Property
    {
        public List<TopOccurrence<string>> TopOccurrences { get; internal init; } = new();
    };

    public abstract record Numeric<T> : Property
        where T : struct
    {
        public T? Maximum { get; internal set; }
        public double? Mean { get; internal set; }
        public double? Median { get; internal set; }
        public T? Minimum { get; internal set; }
        public T? Mode { get; internal set; }
        public T? Sum { get; internal set; }
    }

    public record Integer : Numeric<long> { };

    public record Number : Numeric<double> { };

    public record Boolean : Property
    {
        public double PercentageFalse { get; internal set; }
        public double PercentageTrue { get; internal set; }
        public long TotalFalse { get; internal set; }
        public long TotalTrue { get; internal set; }
    };

    public record Date : Property
    {
        public DateTime Maximum { get; internal set; }
        public DateTime Median { get; internal set; }
        public DateTime Minimum { get; internal set; }
        public DateTime Mode { get; internal set; }
    };
}
