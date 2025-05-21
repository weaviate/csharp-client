using Weaviate.V1;

namespace Weaviate.Client.Models;

public record GeoCoordinatesConstraint
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Distance { get; set; }
}

public static class Filter
{
    public static FilterBase WithID(Guid id) => Property("_id").Equal(id.ToString());

    public static FilterBase WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    public static FilterBase Or(IEnumerable<FilterBase> filters) =>
        new NestedFilter(Filters.Types.Operator.Or, filters);

    public static FilterBase And(IEnumerable<FilterBase> filters) =>
        new NestedFilter(Filters.Types.Operator.And, filters);

    public static FilterBase Property(string name)
    {
        return new PropertyFilter(name.Decapitalize());
    }

    public static FilterBase CreationTime => new TimeFilter("_creationTimeUnix");
    public static FilterBase UpdateTime => new TimeFilter("_creationTimeUnix");
}

public record TypedFilter<T>
{
    readonly FilterBase _internal;

    internal TypedFilter(FilterBase parent)
    {
        _internal = parent;
    }

    public static implicit operator FilterBase(TypedFilter<T> filter)
    {
        return filter._internal;
    }

    public FilterBase ContainsAll(IEnumerable<T> value) => _internal.ContainsAll(value);

    public FilterBase ContainsAny(IEnumerable<T> value) => _internal.ContainsAny(value);

    public FilterBase Equal(T value) => _internal.Equal(value);

    public FilterBase NotEqual(T value) => _internal.NotEqual(value);

    public FilterBase GreaterThan(T value) => _internal.GreaterThan(value);

    public FilterBase GreaterThanEqual(T value) => _internal.GreaterThanEqual(value);

    public FilterBase LessThan(T value) => _internal.LessThan(value);

    public FilterBase LessThanEqual(T value) => _internal.LessThanEqual(value);
}

public record FilterBase
{
    private V1.Filters _filter = new V1.Filters();

    internal FilterBase() { }

    public static implicit operator V1.Filters(FilterBase f) => f._filter;

    protected FilterBase WithOperator(Filters.Types.Operator op)
    {
        _filter.Operator = op;
        return this;
    }

    protected FilterBase ByProperty(string property)
    {
        _filter.Target = new FilterTarget() { Property = property };
        return this;
    }

    protected FilterBase WithNestedFilters(IEnumerable<FilterBase> filters)
    {
        _filter.Filters_.AddRange(filters.Select(f => f._filter));

        return this;
    }

    protected FilterBase ByValue<T>(T value)
    {
        Action<Filters> assigner = (
            value switch
            {
                bool v => f => f.ValueBoolean = v,
                GeoCoordinatesConstraint v => f =>
                    f.ValueGeo = new GeoCoordinatesFilter
                    {
                        Distance = v.Distance,
                        Latitude = v.Latitude,
                        Longitude = v.Longitude,
                    },
                int v => f => f.ValueInt = v,
                double v => f => f.ValueNumber = v,
                string v => f => f.ValueText = v,
                DateTime v => f => f.ValueText = v.ToUniversalTime().ToString("o"),
                IEnumerable<DateTime> v => f =>
                    f.ValueTextArray = new TextArray
                    {
                        Values = { v.Select(vv => vv.ToUniversalTime().ToString("o")) },
                    },
                IEnumerable<bool> v => f =>
                    f.ValueBooleanArray = new BooleanArray { Values = { v } },
                IEnumerable<long> v => f => f.ValueIntArray = new IntArray { Values = { v } },
                IEnumerable<double> v => f =>
                    f.ValueNumberArray = new NumberArray { Values = { v } },
                IEnumerable<string> v => f => f.ValueTextArray = new TextArray { Values = { v } },
                _ => throw new WeaviateException("Unsupported type for filter"),
            }
        );

        assigner(_filter);

        return this;
    }

    public FilterBase ContainsAll<T>(IEnumerable<T> value) =>
        WithOperator(Filters.Types.Operator.ContainsAll).ByValue(value);

    public FilterBase ContainsAny<T>(IEnumerable<T> value) =>
        WithOperator(Filters.Types.Operator.ContainsAny).ByValue(value);

    public FilterBase Equal<TResult>(TResult value) =>
        WithOperator(Filters.Types.Operator.Equal).ByValue(value);

    public FilterBase NotEqual<T>(T value) =>
        WithOperator(Filters.Types.Operator.NotEqual).ByValue(value);

    public FilterBase GreaterThan<TResult>(TResult value) =>
        WithOperator(Filters.Types.Operator.GreaterThan).ByValue(value);

    public FilterBase GreaterThanEqual<T>(T value) =>
        WithOperator(Filters.Types.Operator.GreaterThanEqual).ByValue(value);

    public FilterBase LessThan<T>(T value) =>
        WithOperator(Filters.Types.Operator.LessThan).ByValue(value);

    public FilterBase LessThanEqual<T>(T value) =>
        WithOperator(Filters.Types.Operator.LessThanEqual).ByValue(value);

    public FilterBase WithinGeoRange(GeoCoordinatesConstraint value) =>
        WithOperator(Filters.Types.Operator.WithinGeoRange).ByValue(value);

    public FilterBase Like<T>(T value) => WithOperator(Filters.Types.Operator.Like).ByValue(value);

    public FilterBase IsNull() => WithOperator(Filters.Types.Operator.IsNull);
}

internal record PropertyFilter : FilterBase
{
    internal PropertyFilter(string name)
    {
        ByProperty(name);
    }
}

internal record NestedFilter : FilterBase
{
    internal NestedFilter(Filters.Types.Operator op, IEnumerable<FilterBase> filters)
    {
        WithOperator(op);
        WithNestedFilters(filters);
    }
}

internal record TimeFilter : TypedFilter<DateTime>
{
    internal TimeFilter(string timeField)
        : base(
            timeField switch
            {
                "_creationTimeUnix" => Filter.Property(timeField),
                "_lastUpdateTimeUnix" => Filter.Property(timeField),
                _ => throw new WeaviateException("Unsupported time field for filter"),
            }
        ) { }
}
