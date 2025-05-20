using System.Linq.Expressions;
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

    public static PropertyFilter Property(string name)
    {
        return new PropertyFilter(name);
    }

    public static TimeFilter CreationTime => new("_creationTimeUnix");
    public static TimeFilter UpdateTime => new("_creationTimeUnix");
}

public record FilterBase<T> : FilterBase
{
    internal FilterBase(FilterBase parent)
        : base(parent) { }

    protected FilterBase() { }

    public FilterBase ContainsAny(IEnumerable<T> value) => base.ContainsAny(value);

    public FilterBase Equal(T value) => base.Equal(value);

    public FilterBase NotEqual(T value) => base.NotEqual(value);

    public FilterBase GreaterThan(T value) => base.GreaterThan(value);

    public FilterBase GreaterThanEqual(T value) => base.GreaterThanEqual(value);

    public FilterBase LessThan(T value) => base.LessThan(value);

    public FilterBase LessThanEqual(T value) => base.LessThanEqual(value);
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

    internal FilterBase ByValue<T>(T value)
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

public static class Filter<T>
{
    public class PropertyFilter<TResult>
    {
        private readonly PropertyFilter _prop;

        internal PropertyFilter(string name)
        {
            _prop = Filter.Property(name.ToLower());
        }

        public Filters ContainsAny(IEnumerable<TResult> values) => _prop.ContainsAny(values);

        public Filters ContainsAll(IEnumerable<TResult> values) => _prop.ContainsAll(values);

        public Filters Equal(TResult value) => _prop.Equal(value);

        public Filters NotEqual(TResult value) => _prop.NotEqual(value);

        public Filters GreaterThan(TResult value) => _prop.GreaterThan(value);

        public Filters GreaterThanEqual(TResult value) => _prop.GreaterThanEqual(value);

        public Filters LessThan(TResult value) => _prop.LessThan(value);

        public Filters LessThanEqual(TResult value) => _prop.LessThanEqual(value);

        public Filters WithinGeoRange(GeoCoordinatesConstraint value) =>
            _prop.WithinGeoRange(value);

        public Filters Like(TResult value) => _prop.Like(value);

        public Filters IsNull() => _prop.IsNull();
    }

    public static PropertyFilter<TResult> Property<TResult>(Expression<Func<T, TResult>> selector)
    {
        var member = selector.Body as MemberExpression;
        if (member != null)
        {
            var mi = member.Member;
            return new PropertyFilter<TResult>(mi.Name);
        }

        throw new ArgumentException("Expression is not a member access", nameof(selector));
    }
}

public record PropertyFilter : FilterBase
{
    internal PropertyFilter(string name)
    {
        ByProperty(name);
    }

    public FilterBase<T> As<T>() => new(this);
}

public record NestedFilter : FilterBase
{
    internal NestedFilter(Filters.Types.Operator op, IEnumerable<FilterBase> filters)
    {
        WithOperator(op);
        WithNestedFilters(filters);
    }
}

public record TimeFilter : FilterBase<DateTime>
{
    internal TimeFilter(string timeField)
    {
        _ = timeField switch
        {
            "_creationTimeUnix" => ByProperty(timeField),
            "_lastUpdateTimeUnix" => ByProperty(timeField),
            _ => throw new WeaviateException("Unsupported time field for filter"),
        };
    }
}
