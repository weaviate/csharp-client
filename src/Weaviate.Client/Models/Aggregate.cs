namespace Weaviate.Client.Models;

public partial record AggregateGroupByResult
{
    public partial record Group
    {
        public record By(string Property, Type Type);

        public required By GroupedBy { get; init; }
        IDictionary<string, Aggregate.Property> Properties { get; init; } =
            new Dictionary<string, Aggregate.Property>();
        public long TotalCount { get; init; } = 0;
    }

    public List<Group> Groups { get; init; } = new();

    internal static AggregateGroupByResult FromGrpcReply(object result)
    {
        throw new NotImplementedException();
    }
}

public partial record AggregateResult
{
    public IDictionary<string, Aggregate.Property> Properties { get; init; } =
        new Dictionary<string, Aggregate.Property>();

    public long TotalCount { get; init; }

    internal static AggregateResult FromGrpcReply(V1.AggregateReply reply)
    {
        throw new NotImplementedException();
    }
}

public abstract record MetricRequest(string Name)
{
    public record Text(string Name) : MetricRequest(Name)
    {
        public bool Count { get; init; }
        public bool TopOccurrencesCount { get; init; }
        public bool TopOccurrencesValue { get; init; }
        public int? MinOccurrences { get; init; }
    }

    public record Integer(string Name) : MetricRequest(Name)
    {
        public bool Count { get; init; }
        public bool Maximum { get; init; }
        public bool Mean { get; init; }
        public bool Median { get; init; }
        public bool Minimum { get; init; }
        public bool Mode { get; init; }
        public bool Sum { get; init; }
    }

    public record Number(string Name) : MetricRequest(Name)
    {
        public bool Count { get; init; }
        public bool Maximum { get; init; }
        public bool Mean { get; init; }
        public bool Median { get; init; }
        public bool Minimum { get; init; }
        public bool Mode { get; init; }
        public bool Sum { get; init; }
    }

    public record Boolean(string Name) : MetricRequest(Name)
    {
        public bool Count { get; init; }
        public bool PercentageFalse { get; init; }
        public bool PercentageTrue { get; init; }
        public bool TotalFalse { get; init; }
        public bool TotalTrue { get; init; }
    }

    public record Date(string Name) : MetricRequest(Name)
    {
        public bool Count { get; init; }
        public bool Maximum { get; init; }
        public bool Median { get; init; }
        public bool Minimum { get; init; }
        public bool Mode { get; init; }
    }
}

// TODO Perhaps implement this similar to https://github.com/weaviate/csharp-client/blob/db0be2e08870b5121f4f43d58d8d74025cba7fff/src/Weaviate.Client/Models/Property.cs#L16
public class PropertyMetricBuilder(string Name)
{
    public MetricRequest Text => new MetricRequest.Text(Name);

    public MetricRequest Integer => new MetricRequest.Integer(Name);

    public MetricRequest Number => new MetricRequest.Number(Name);

    public MetricRequest Boolean => new MetricRequest.Boolean(Name);

    public MetricRequest Date => new MetricRequest.Date(Name);
}

public static class Metrics
{
    public static PropertyMetricBuilder ForProperty(string propertyName) => new(propertyName);
}

public static class Aggregate
{
    public record GroupBy(string Property, int? Limit = null);

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
