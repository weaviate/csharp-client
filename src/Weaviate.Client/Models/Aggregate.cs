namespace Weaviate.Client.Models;

public partial record AggregateGroupByResult
{
    public partial record Group
    {
        public record By(string Property, object Value, Type Type);

        public required By GroupedBy { get; init; }
        public IReadOnlyDictionary<string, Aggregate.Property> Properties { get; init; } =
            new Dictionary<string, Aggregate.Property>();
        public long TotalCount { get; init; } = 0;
    }

    public List<Group> Groups { get; init; } = new();

    internal static AggregateGroupByResult FromGrpcReply(V1.AggregateReply result)
    {
        var groupByToGrpc = (V1.AggregateReply.Types.Group.Types.GroupedBy gb) =>
            gb.ValueCase switch
            {
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Boolean =>
                    new Group.By(gb.Path[0], gb.Boolean, typeof(bool)),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Booleans =>
                    new Group.By(gb.Path[0], gb.Booleans.Values.ToArray(), typeof(bool[])),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Int => new Group.By(
                    gb.Path[0],
                    gb.Int,
                    typeof(int)
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Ints => new Group.By(
                    gb.Path[0],
                    gb.Ints.Values.ToArray(),
                    typeof(int[])
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Number => new Group.By(
                    gb.Path[0],
                    gb.Number,
                    typeof(double)
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Numbers =>
                    new Group.By(gb.Path[0], gb.Numbers.Values.ToArray(), typeof(double[])),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Text => new Group.By(
                    gb.Path[0],
                    gb.Text,
                    typeof(string)
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Texts => new Group.By(
                    gb.Path[0],
                    gb.Texts.Values.ToArray(),
                    typeof(string[])
                ),
                V1.AggregateReply.Types.Group.Types.GroupedBy.ValueOneofCase.Geo => new Group.By(
                    gb.Path[0],
                    new GeoCoordinate(gb.Geo.Latitude, gb.Geo.Longitude),
                    typeof(GeoCoordinate)
                ),

                _ => throw new NotImplementedException($"Unknown group by type: {gb.ValueCase}"),
            };

        var groupExtract = new Func<V1.AggregateReply.Types.Group, Group>(g =>
        {
            var groupedBy = groupByToGrpc(g.GroupedBy);
            var properties =
                g.Aggregations?.Aggregations_?.ToDictionary(
                    p => p.Property,
                    AggregateResult.FromGrpcProperty
                ) ?? new Dictionary<string, Aggregate.Property>();

            return new Group
            {
                GroupedBy = groupedBy,
                Properties = properties,
                TotalCount = g.ObjectsCount,
            };
        });

        var groupByGroups = result.GroupedResults?.Groups.Select(groupExtract).ToList() ?? [];

        return new AggregateGroupByResult { Groups = groupByGroups };
    }
}

public partial record AggregateResult
{
    public IDictionary<string, Aggregate.Property> Properties { get; init; } =
        new Dictionary<string, Aggregate.Property>();

    public long TotalCount { get; init; }

    internal static AggregateResult FromGrpcReply(V1.AggregateReply reply)
    {
        return new AggregateResult
        {
            Properties = (
                reply.SingleResult.Aggregations != null
                    ? reply.SingleResult.Aggregations
                    : new V1.AggregateReply.Types.Aggregations()
            ).Aggregations_.ToDictionary(x => x.Property, AggregateResult.FromGrpcProperty),
            TotalCount = reply.SingleResult.ObjectsCount,
        };
    }

    internal static Aggregate.Property FromGrpcProperty(
        Weaviate.V1.AggregateReply.Types.Aggregations.Types.Aggregation x
    )
    {
        return x.AggregationCase switch
        {
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Text =>
                (Aggregate.Property)
                    new Aggregate.Text
                    {
                        Count = x.Text.Count,
                        TopOccurrences = (
                            x.Text.TopOccurences is null
                                ? []
                                : x.Text.TopOccurences.Items.Select(
                                    o => new Aggregate.TopOccurrence<string>
                                    {
                                        Count = o.Occurs,
                                        Value = o.Value,
                                    }
                                )
                        ).ToList(),
                    },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Int =>
                new Aggregate.Integer
                {
                    Count = x.Int.Count,
                    Maximum = x.Int.Maximum,
                    Mean = x.Int.Mean,
                    Median = x.Int.Median,
                    Minimum = x.Int.Minimum,
                    Mode = x.Int.Mode,
                    Sum = x.Int.Sum,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Number =>
                new Aggregate.Number
                {
                    Count = x.Number.Count,
                    Maximum = x.Number.Maximum,
                    Mean = x.Number.Mean,
                    Median = x.Number.Median,
                    Minimum = x.Number.Minimum,
                    Mode = x.Number.Mode,
                    Sum = x.Number.Sum,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Boolean =>
                new Aggregate.Boolean
                {
                    Count = x.Boolean.Count,
                    PercentageFalse = x.Boolean.PercentageFalse,
                    PercentageTrue = x.Boolean.PercentageTrue,
                    TotalFalse = x.Boolean.TotalFalse,
                    TotalTrue = x.Boolean.TotalTrue,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Date =>
                new Aggregate.Date
                {
                    Count = x.Date.Count,
                    Maximum = x.Date.HasMaximum
                        ? DateTime.Parse(
                            x.Date.Maximum,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    Median = x.Date.HasMedian
                        ? DateTime.Parse(
                            x.Date.Median,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    Minimum = x.Date.HasMinimum
                        ? DateTime.Parse(
                            x.Date.Minimum,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                    Mode = x.Date.HasMode
                        ? DateTime.Parse(
                            x.Date.Mode,
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        )
                        : null,
                },
            V1.AggregateReply.Types.Aggregations.Types.Aggregation.AggregationOneofCase.Reference =>
                throw new NotImplementedException(),
            _ => throw new NotImplementedException(
                $"Unknown aggregation case: {x.AggregationCase}"
            ),
        };
    }
}

public class Metrics
{
    private string PropertyName { get; }

    protected Metrics(string name)
    {
        PropertyName = name;
    }

    public static Metrics ForProperty(string propertyName) => new(propertyName);

    public Aggregate.Metric Text(
        bool count = false,
        bool topOccurrencesCount = false,
        bool topOccurrencesValue = false,
        uint? minOccurrences = null
    )
    {
        if (!(count || topOccurrencesCount || topOccurrencesValue))
        {
            count = topOccurrencesCount = topOccurrencesValue = true;
        }

        return new Aggregate.Metric.Text(PropertyName)
        {
            Count = count,
            TopOccurrencesCount = topOccurrencesCount,
            TopOccurrencesValue = topOccurrencesValue,
            MinOccurrences = minOccurrences,
        };
    }

    public Aggregate.Metric Integer(
        bool count = false,
        bool maximum = false,
        bool mean = false,
        bool median = false,
        bool minimum = false,
        bool mode = false,
        bool sum = false
    )
    {
        // If all parameters are false, enable all by default
        if (!(count || maximum || mean || median || minimum || mode || sum))
        {
            count = maximum = mean = median = minimum = mode = sum = true;
        }

        return new Aggregate.Metric.Integer(PropertyName)
        {
            Count = count,
            Maximum = maximum,
            Mean = mean,
            Median = median,
            Minimum = minimum,
            Mode = mode,
            Sum = sum,
        };
    }

    public Aggregate.Metric Number(
        bool count = false,
        bool maximum = false,
        bool mean = false,
        bool median = false,
        bool minimum = false,
        bool mode = false,
        bool sum = false
    )
    {
        // If all parameters are false, enable all by default
        if (!(count || maximum || mean || median || minimum || mode || sum))
        {
            count = maximum = mean = median = minimum = mode = sum = true;
        }

        return new Aggregate.Metric.Number(PropertyName)
        {
            Count = count,
            Maximum = maximum,
            Mean = mean,
            Median = median,
            Minimum = minimum,
            Mode = mode,
            Sum = sum,
        };
    }

    public Aggregate.Metric Boolean(
        bool count = false,
        bool percentageFalse = false,
        bool percentageTrue = false,
        bool totalFalse = false,
        bool totalTrue = false
    )
    {
        if (!(count || percentageFalse || percentageTrue || totalFalse || totalTrue))
        {
            count = percentageFalse = percentageTrue = totalFalse = totalTrue = true;
        }

        return new Aggregate.Metric.Boolean(PropertyName)
        {
            Count = count,
            PercentageFalse = percentageFalse,
            PercentageTrue = percentageTrue,
            TotalFalse = totalFalse,
            TotalTrue = totalTrue,
        };
    }

    public Aggregate.Metric Date(
        bool count = false,
        bool maximum = false,
        bool median = false,
        bool minimum = false,
        bool mode = false
    )
    {
        if (!(count || maximum || median || minimum || mode))
        {
            count = maximum = median = minimum = mode = true;
        }

        return new Aggregate.Metric.Date(PropertyName)
        {
            Count = count,
            Maximum = maximum,
            Median = median,
            Minimum = minimum,
            Mode = mode,
        };
    }

    public Aggregate.Metric Reference()
    {
        throw new NotImplementedException("Reference metrics are not implemented yet.");
    }
}

public static partial class Aggregate
{
    public abstract record Metric(string Name)
    {
        public bool Count { get; init; }

        public record Text(string Name) : Metric(Name)
        {
            public bool TopOccurrencesCount { get; init; }
            public bool TopOccurrencesValue { get; init; }
            public uint? MinOccurrences { get; init; }
        }

        public record Integer(string Name) : Metric(Name)
        {
            public bool Maximum { get; init; }
            public bool Mean { get; init; }
            public bool Median { get; init; }
            public bool Minimum { get; init; }
            public bool Mode { get; init; }
            public bool Sum { get; init; }
        }

        public record Number(string Name) : Metric(Name)
        {
            public bool Maximum { get; init; }
            public bool Mean { get; init; }
            public bool Median { get; init; }
            public bool Minimum { get; init; }
            public bool Mode { get; init; }
            public bool Sum { get; init; }
        }

        public record Boolean(string Name) : Metric(Name)
        {
            public bool PercentageFalse { get; init; }
            public bool PercentageTrue { get; init; }
            public bool TotalFalse { get; init; }
            public bool TotalTrue { get; init; }
        }

        public record Date(string Name) : Metric(Name)
        {
            public bool Maximum { get; init; }
            public bool Median { get; init; }
            public bool Minimum { get; init; }
            public bool Mode { get; init; }
        }
    }
}

public static partial class Aggregate
{
    public record GroupBy(string Property, uint? Limit = null)
    {
        public static implicit operator GroupBy(string property) => new(property);
    };

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
        public DateTime? Maximum { get; internal set; }
        public DateTime? Median { get; internal set; }
        public DateTime? Minimum { get; internal set; }
        public DateTime? Mode { get; internal set; }
    };
}
