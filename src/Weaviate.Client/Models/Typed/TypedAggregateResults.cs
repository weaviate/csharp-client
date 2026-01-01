using System.Reflection;

namespace Weaviate.Client.Models.Typed;

/// <summary>
/// Strongly-typed aggregate result that maps aggregation properties to a user-defined type.
/// Properties of type T are mapped to their corresponding aggregation types.
/// </summary>
/// <typeparam name="T">The type whose properties define the aggregation structure.</typeparam>
public record AggregateResult<T>
    where T : class, new()
{
    /// <summary>
    /// The underlying untyped aggregate result.
    /// </summary>
    public required AggregateResult Untyped { get; init; }

    /// <summary>
    /// The strongly-typed aggregation properties.
    /// </summary>
    public required T Properties { get; init; }

    /// <summary>
    /// The total count of objects aggregated.
    /// </summary>
    public long TotalCount => Untyped.TotalCount;

    /// <summary>
    /// Creates a typed aggregate result from an untyped result.
    /// </summary>
    internal static AggregateResult<T> FromUntyped(AggregateResult result)
    {
        var typed = AggregatePropertyMapper.MapToType<T>(result.Properties);
        return new AggregateResult<T> { Untyped = result, Properties = typed };
    }
}

/// <summary>
/// Strongly-typed group-by aggregate result.
/// </summary>
/// <typeparam name="T">The type whose properties define the aggregation structure.</typeparam>
public record AggregateGroupByResult<T>
    where T : class, new()
{
    /// <summary>
    /// The underlying untyped group-by result.
    /// </summary>
    public required AggregateGroupByResult Untyped { get; init; }

    /// <summary>
    /// The strongly-typed groups.
    /// </summary>
    public required IReadOnlyList<Group> Groups { get; init; }

    /// <summary>
    /// A strongly-typed group within a group-by aggregation.
    /// </summary>
    public record Group
    {
        /// <summary>
        /// The underlying untyped group.
        /// </summary>
        public required AggregateGroupByResult.Group Untyped { get; init; }

        /// <summary>
        /// Information about what this group is grouped by.
        /// </summary>
        public AggregateGroupByResult.Group.By GroupedBy => Untyped.GroupedBy;

        /// <summary>
        /// The total count of objects in this group.
        /// </summary>
        public long TotalCount => Untyped.TotalCount;

        /// <summary>
        /// The strongly-typed aggregation properties for this group.
        /// </summary>
        public required T Properties { get; init; }
    }

    /// <summary>
    /// Creates a typed group-by result from an untyped result.
    /// </summary>
    internal static AggregateGroupByResult<T> FromUntyped(AggregateGroupByResult result)
    {
        var typedGroups = result
            .Groups.Select(g => new Group
            {
                Untyped = g,
                Properties = AggregatePropertyMapper.MapToType<T>(g.Properties),
            })
            .ToList();

        return new AggregateGroupByResult<T> { Untyped = result, Groups = typedGroups };
    }
}

/// <summary>
/// Maps aggregate properties to strongly-typed objects.
/// </summary>
internal static class AggregatePropertyMapper
{
    /// <summary>
    /// Recognized suffixes for extracting specific values from aggregation properties.
    /// </summary>
    internal static readonly string[] Suffixes =
    [
        "Count",
        "Sum",
        "Mean",
        "Average",
        "Min",
        "Minimum",
        "Max",
        "Maximum",
        "Median",
        "Mode",
        "TotalTrue",
        "TotalFalse",
        "PercentageTrue",
        "PercentageFalse",
        "TopOccurrence",
        "TopOccurrences",
    ];

    /// <summary>
    /// Maps a dictionary of aggregate properties to a strongly-typed object.
    /// Property names are matched using case-insensitive comparison.
    /// For primitive types, a suffix must be used to indicate which value to extract
    /// (e.g., PriceSum, QuantityMean, TitleCount).
    /// </summary>
    public static T MapToType<T>(IReadOnlyDictionary<string, Aggregate.Property> properties)
        where T : class, new()
    {
        var result = new T();
        var targetProperties = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        // Create a case-insensitive lookup for properties
        var propertiesLookup = new Dictionary<string, Aggregate.Property>(
            StringComparer.OrdinalIgnoreCase
        );
        foreach (var kvp in properties)
        {
            propertiesLookup[kvp.Key] = kvp.Value;
        }

        foreach (var targetProperty in targetProperties)
        {
            var propertyName = targetProperty.Name;
            var targetType = targetProperty.PropertyType;

            // Check if target type is a full Aggregate.* type (direct mapping by name)
            if (IsAggregateType(targetType))
            {
                var fieldName = propertyName.Decapitalize();
                if (
                    propertiesLookup.TryGetValue(fieldName, out var aggregation)
                    && targetType.IsAssignableFrom(aggregation.GetType())
                )
                {
                    targetProperty.SetValue(result, aggregation);
                }

                continue;
            }

            // For primitive types, look for suffix-based mapping
            var (fieldName2, suffix) = ParsePropertyNameWithSuffix(propertyName);
            if (suffix == null || !propertiesLookup.TryGetValue(fieldName2, out var agg))
                continue;

            var value = ExtractValueBySuffix(agg, suffix, targetType);
            if (value != null)
            {
                targetProperty.SetValue(result, value);
            }
        }

        return result;
    }

    /// <summary>
    /// Maps a dictionary of aggregate properties to a strongly-typed object.
    /// </summary>
    public static T MapToType<T>(IDictionary<string, Aggregate.Property> properties)
        where T : class, new()
    {
        // Create a read-only wrapper and cast explicitly
        IReadOnlyDictionary<string, Aggregate.Property> readOnly = new Dictionary<
            string,
            Aggregate.Property
        >(properties);
        return MapToType<T>(readOnly);
    }

    private static bool IsAggregateType(Type type)
    {
        return type == typeof(Aggregate.Text)
            || type == typeof(Aggregate.Integer)
            || type == typeof(Aggregate.Number)
            || type == typeof(Aggregate.Boolean)
            || type == typeof(Aggregate.Date)
            || type == typeof(Aggregate.Property);
    }

    /// <summary>
    /// Parses a property name to extract the field name and suffix.
    /// Returns (fieldName, suffix) where suffix is null if no recognized suffix is found.
    /// </summary>
    internal static (string fieldName, string? suffix) ParsePropertyNameWithSuffix(
        string propertyName
    )
    {
        foreach (var suffix in Suffixes)
        {
            if (
                propertyName.Length > suffix.Length
                && propertyName.EndsWith(suffix, StringComparison.Ordinal)
            )
            {
                var fieldName = propertyName[..^suffix.Length].Decapitalize();
                return (fieldName, suffix);
            }
        }

        return (propertyName.Decapitalize(), null);
    }

    /// <summary>
    /// Extracts a value from an aggregation property based on the suffix.
    /// </summary>
    private static object? ExtractValueBySuffix(
        Aggregate.Property aggregation,
        string suffix,
        Type targetType
    )
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return suffix switch
        {
            // Count - available on all types
            "Count" => ConvertToNumeric(aggregation.Count, underlyingType),

            // Sum - Integer and Number
            "Sum" => aggregation switch
            {
                Aggregate.Integer i => ConvertToNumeric(i.Sum, underlyingType),
                Aggregate.Number n => ConvertToNumeric(n.Sum, underlyingType),
                _ => null,
            },

            // Mean/Average - Integer and Number
            "Mean" or "Average" => aggregation switch
            {
                Aggregate.Integer i => ConvertToNumeric(i.Mean, underlyingType),
                Aggregate.Number n => ConvertToNumeric(n.Mean, underlyingType),
                _ => null,
            },

            // Min/Minimum - Integer, Number, Date
            "Min" or "Minimum" => aggregation switch
            {
                Aggregate.Integer i => ConvertToNumeric(i.Minimum, underlyingType),
                Aggregate.Number n => ConvertToNumeric(n.Minimum, underlyingType),
                Aggregate.Date d when underlyingType == typeof(DateTime) => d.Minimum,
                _ => null,
            },

            // Max/Maximum - Integer, Number, Date
            "Max" or "Maximum" => aggregation switch
            {
                Aggregate.Integer i => ConvertToNumeric(i.Maximum, underlyingType),
                Aggregate.Number n => ConvertToNumeric(n.Maximum, underlyingType),
                Aggregate.Date d when underlyingType == typeof(DateTime) => d.Maximum,
                _ => null,
            },

            // Median - Integer, Number, Date
            "Median" => aggregation switch
            {
                Aggregate.Integer i => ConvertToNumeric(i.Median, underlyingType),
                Aggregate.Number n => ConvertToNumeric(n.Median, underlyingType),
                Aggregate.Date d when underlyingType == typeof(DateTime) => d.Median,
                _ => null,
            },

            // Mode - Integer, Number, Date, Text
            "Mode" => aggregation switch
            {
                Aggregate.Integer i => ConvertToNumeric(i.Mode, underlyingType),
                Aggregate.Number n => ConvertToNumeric(n.Mode, underlyingType),
                Aggregate.Date d when underlyingType == typeof(DateTime) => d.Mode,
                _ => null,
            },

            // TotalTrue - Boolean
            "TotalTrue" => aggregation switch
            {
                Aggregate.Boolean b => ConvertToNumeric(b.TotalTrue, underlyingType),
                _ => null,
            },

            // TotalFalse - Boolean
            "TotalFalse" => aggregation switch
            {
                Aggregate.Boolean b => ConvertToNumeric(b.TotalFalse, underlyingType),
                _ => null,
            },

            // PercentageTrue - Boolean
            "PercentageTrue" => aggregation switch
            {
                Aggregate.Boolean b => ConvertToNumeric(b.PercentageTrue, underlyingType),
                _ => null,
            },

            // PercentageFalse - Boolean
            "PercentageFalse" => aggregation switch
            {
                Aggregate.Boolean b => ConvertToNumeric(b.PercentageFalse, underlyingType),
                _ => null,
            },

            // TopOccurrence - Text (returns first occurrence value)
            "TopOccurrence" => aggregation switch
            {
                Aggregate.Text t when underlyingType == typeof(string) => t
                    .TopOccurrences.FirstOrDefault()
                    ?.Value,
                _ => null,
            },

            // TopOccurrences - Text (returns list)
            "TopOccurrences" => aggregation switch
            {
                Aggregate.Text t when targetType == typeof(List<Aggregate.TopOccurrence<string>>) =>
                    t.TopOccurrences,
                Aggregate.Text t
                    when targetType == typeof(IReadOnlyList<Aggregate.TopOccurrence<string>>) =>
                    t.TopOccurrences,
                Aggregate.Text t
                    when targetType == typeof(IList<Aggregate.TopOccurrence<string>>) =>
                    t.TopOccurrences,
                Aggregate.Text t
                    when targetType == typeof(IEnumerable<Aggregate.TopOccurrence<string>>) =>
                    t.TopOccurrences,
                _ => null,
            },

            _ => null,
        };
    }

    /// <summary>
    /// Converts a numeric value to the target type.
    /// </summary>
    private static object? ConvertToNumeric(long? value, Type targetType)
    {
        if (!value.HasValue)
            return null;

        return targetType switch
        {
            _ when targetType == typeof(long) => value.Value,
            _ when targetType == typeof(int) => (int)value.Value,
            _ when targetType == typeof(double) => (double)value.Value,
            _ when targetType == typeof(float) => (float)value.Value,
            _ when targetType == typeof(decimal) => (decimal)value.Value,
            _ => null,
        };
    }

    /// <summary>
    /// Converts a numeric value to the target type.
    /// </summary>
    private static object? ConvertToNumeric(double? value, Type targetType)
    {
        if (!value.HasValue)
            return null;

        return targetType switch
        {
            _ when targetType == typeof(double) => value.Value,
            _ when targetType == typeof(float) => (float)value.Value,
            _ when targetType == typeof(decimal) => (decimal)value.Value,
            _ when targetType == typeof(long) => (long)value.Value,
            _ when targetType == typeof(int) => (int)value.Value,
            _ => null,
        };
    }

    /// <summary>
    /// Converts a long value to the target type.
    /// </summary>
    private static object? ConvertToNumeric(long value, Type targetType)
    {
        return targetType switch
        {
            _ when targetType == typeof(long) => value,
            _ when targetType == typeof(int) => (int)value,
            _ when targetType == typeof(double) => (double)value,
            _ when targetType == typeof(float) => (float)value,
            _ when targetType == typeof(decimal) => (decimal)value,
            _ => null,
        };
    }

    /// <summary>
    /// Converts a double value to the target type.
    /// </summary>
    private static object? ConvertToNumeric(double value, Type targetType)
    {
        return targetType switch
        {
            _ when targetType == typeof(double) => value,
            _ when targetType == typeof(float) => (float)value,
            _ when targetType == typeof(decimal) => (decimal)value,
            _ when targetType == typeof(long) => (long)value,
            _ when targetType == typeof(int) => (int)value,
            _ => null,
        };
    }
}

/// <summary>
/// Extracts aggregate metrics from a type's properties for use with the returnMetrics parameter.
/// </summary>
public static class MetricsExtractor
{
    /// <summary>
    /// Extracts aggregate metrics from a type's properties.
    /// For properties with Aggregate.* types, the corresponding metric type is used with all flags enabled.
    /// For properties with primitive types and recognized suffixes, only the specific metric flags are enabled.
    /// </summary>
    /// <typeparam name="T">The type to extract metrics from.</typeparam>
    /// <returns>An array of metrics that can be passed to the returnMetrics parameter.</returns>
    /// <example>
    /// <code>
    /// public class ProductStats
    /// {
    ///     public double? PriceMean { get; set; }
    ///     public long? QuantitySum { get; set; }
    ///     public Aggregate.Text? Category { get; set; }
    /// }
    ///
    /// var metrics = MetricsExtractor.FromType&lt;ProductStats&gt;();
    /// var result = await client.Aggregate.OverAll(returnMetrics: metrics);
    /// var typed = result.ToTyped&lt;ProductStats&gt;();
    /// </code>
    /// </example>
    public static Aggregate.Metric[] FromType<T>()
        where T : class
    {
        return FromType(typeof(T));
    }

    /// <summary>
    /// Extracts aggregate metrics from a type's properties.
    /// </summary>
    /// <param name="type">The type to extract metrics from.</param>
    /// <returns>An array of metrics that can be passed to the returnMetrics parameter.</returns>
    public static Aggregate.Metric[] FromType(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        // Group properties by field name to combine metrics
        var metricsByField = new Dictionary<string, MetricBuilder>(
            StringComparer.OrdinalIgnoreCase
        );

        foreach (var property in properties)
        {
            var propertyType = property.PropertyType;
            var propertyName = property.Name;

            // Check if this is a full Aggregate.* type
            if (IsAggregateType(propertyType))
            {
                var fieldName = propertyName.Decapitalize();
                var metricType = GetMetricTypeFromAggregateType(propertyType);
                if (metricType != MetricType.Unknown)
                {
                    if (!metricsByField.TryGetValue(fieldName, out var builder))
                    {
                        builder = new MetricBuilder(fieldName, metricType);
                        metricsByField[fieldName] = builder;
                    }

                    // Enable all flags for full aggregate types
                    builder.EnableAll();
                }

                continue;
            }

            // For primitive types, look for suffix-based mapping
            var (fieldName2, suffix) = AggregatePropertyMapper.ParsePropertyNameWithSuffix(
                propertyName
            );
            if (suffix == null)
                continue;

            var inferredType = GetMetricTypeFromSuffix(suffix, propertyType);
            if (inferredType == MetricType.Unknown)
                continue;

            if (!metricsByField.TryGetValue(fieldName2, out var builder2))
            {
                builder2 = new MetricBuilder(fieldName2, inferredType);
                metricsByField[fieldName2] = builder2;
            }

            // Enable specific flag based on suffix
            builder2.EnableFlag(suffix);
        }

        return metricsByField.Values.Select(b => b.Build()).ToArray();
    }

    private static bool IsAggregateType(Type type)
    {
        return type == typeof(Aggregate.Text)
            || type == typeof(Aggregate.Integer)
            || type == typeof(Aggregate.Number)
            || type == typeof(Aggregate.Boolean)
            || type == typeof(Aggregate.Date)
            || type == typeof(Aggregate.Property);
    }

    private static MetricType GetMetricTypeFromAggregateType(Type type)
    {
        if (type == typeof(Aggregate.Text))
            return MetricType.Text;
        if (type == typeof(Aggregate.Integer))
            return MetricType.Integer;
        if (type == typeof(Aggregate.Number))
            return MetricType.Number;
        if (type == typeof(Aggregate.Boolean))
            return MetricType.Boolean;
        if (type == typeof(Aggregate.Date))
            return MetricType.Date;
        return MetricType.Unknown;
    }

    private static MetricType GetMetricTypeFromSuffix(string suffix, Type propertyType)
    {
        // Check if property is DateTime (for Date suffixes)
        var underlyingType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        var isDateTime = underlyingType == typeof(DateTime);

        return suffix switch
        {
            // Count is available on all types, default to Integer
            "Count" => MetricType.Integer,

            // Sum is only for numeric types, not dates
            "Sum" => MetricType.Number,

            // Mean/Average is only for numeric types
            "Mean" or "Average" => MetricType.Number,

            // Min/Max/Median/Mode can be Date or Number based on property type
            "Min" or "Minimum" or "Max" or "Maximum" or "Median" or "Mode" => isDateTime
                ? MetricType.Date
                : MetricType.Number,

            // Boolean-specific suffixes
            "TotalTrue" or "TotalFalse" or "PercentageTrue" or "PercentageFalse" =>
                MetricType.Boolean,

            // Text-specific suffixes
            "TopOccurrence" or "TopOccurrences" => MetricType.Text,

            _ => MetricType.Unknown,
        };
    }

    private enum MetricType
    {
        Unknown,
        Text,
        Integer,
        Number,
        Boolean,
        Date,
    }

    private sealed class MetricBuilder
    {
        private readonly string _fieldName;
        private readonly MetricType _type;
        private bool _count;
        private bool _sum;
        private bool _mean;
        private bool _minimum;
        private bool _maximum;
        private bool _median;
        private bool _mode;
        private bool _totalTrue;
        private bool _totalFalse;
        private bool _percentageTrue;
        private bool _percentageFalse;
        private bool _topOccurrencesCount;
        private bool _topOccurrencesValue;

        public MetricBuilder(string fieldName, MetricType type)
        {
            _fieldName = fieldName;
            _type = type;
        }

        public void EnableAll()
        {
            _count = true;
            _sum = true;
            _mean = true;
            _minimum = true;
            _maximum = true;
            _median = true;
            _mode = true;
            _totalTrue = true;
            _totalFalse = true;
            _percentageTrue = true;
            _percentageFalse = true;
            _topOccurrencesCount = true;
            _topOccurrencesValue = true;
        }

        public void EnableFlag(string suffix)
        {
            switch (suffix)
            {
                case "Count":
                    _count = true;
                    break;
                case "Sum":
                    _sum = true;
                    break;
                case "Mean":
                case "Average":
                    _mean = true;
                    break;
                case "Min":
                case "Minimum":
                    _minimum = true;
                    break;
                case "Max":
                case "Maximum":
                    _maximum = true;
                    break;
                case "Median":
                    _median = true;
                    break;
                case "Mode":
                    _mode = true;
                    break;
                case "TotalTrue":
                    _totalTrue = true;
                    break;
                case "TotalFalse":
                    _totalFalse = true;
                    break;
                case "PercentageTrue":
                    _percentageTrue = true;
                    break;
                case "PercentageFalse":
                    _percentageFalse = true;
                    break;
                case "TopOccurrence":
                case "TopOccurrences":
                    _topOccurrencesCount = true;
                    _topOccurrencesValue = true;
                    break;
            }
        }

        public Aggregate.Metric Build()
        {
            return _type switch
            {
                MetricType.Text => new Aggregate.Metric.Text(_fieldName)
                {
                    Count = _count,
                    TopOccurrencesCount = _topOccurrencesCount,
                    TopOccurrencesValue = _topOccurrencesValue,
                },
                MetricType.Integer => new Aggregate.Metric.Integer(_fieldName)
                {
                    Count = _count,
                    Sum = _sum,
                    Mean = _mean,
                    Minimum = _minimum,
                    Maximum = _maximum,
                    Median = _median,
                    Mode = _mode,
                },
                MetricType.Number => new Aggregate.Metric.Number(_fieldName)
                {
                    Count = _count,
                    Sum = _sum,
                    Mean = _mean,
                    Minimum = _minimum,
                    Maximum = _maximum,
                    Median = _median,
                    Mode = _mode,
                },
                MetricType.Boolean => new Aggregate.Metric.Boolean(_fieldName)
                {
                    Count = _count,
                    TotalTrue = _totalTrue,
                    TotalFalse = _totalFalse,
                    PercentageTrue = _percentageTrue,
                    PercentageFalse = _percentageFalse,
                },
                MetricType.Date => new Aggregate.Metric.Date(_fieldName)
                {
                    Count = _count,
                    Minimum = _minimum,
                    Maximum = _maximum,
                    Median = _median,
                    Mode = _mode,
                },
                _ => throw new InvalidOperationException($"Unknown metric type: {_type}"),
            };
        }
    }
}
