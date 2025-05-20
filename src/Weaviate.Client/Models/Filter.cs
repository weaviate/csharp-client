using System.Linq.Expressions;
using Weaviate.V1;

namespace Weaviate.Client.Models;

public record GeoCoordinatesConstraint
{
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Distance { get; set; }
}

public record Filter
{
    private V1.Filters _filter = new V1.Filters();

    private Filter() { }

    private Filter WithOperator(Filters.Types.Operator op)
    {
        _filter.Operator = op;
        return this;
    }

    public static implicit operator V1.Filters(Filter f) => f._filter;

    private Filter ByProperty(string property)
    {
        _filter.Target = new FilterTarget() { Property = property };
        return this;
    }

    private Filter WithNestedFilters(IEnumerable<Filter> filters)
    {
        _filter.Filters_.AddRange(filters.Select(f => f._filter));

        return this;
    }

    private Filter ByValue<T>(T value)
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

    internal static Filter WithID(Guid id) => Property("_id").Equal(id.ToString());

    internal static Filter WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    internal static Filter Or(IEnumerable<Filter> filters) =>
        new NestedFilter(Filters.Types.Operator.Or, filters);

    internal static Filter And(IEnumerable<Filter> filters) =>
        new NestedFilter(Filters.Types.Operator.And, filters);

    public static PropertyFilter Property(string name)
    {
        return new PropertyFilter(name);
    }

    public record NestedFilter
    {
        private readonly Filter _filter = new();

        public static implicit operator Filter(NestedFilter nestedFilter) => nestedFilter._filter;

        internal NestedFilter(Filters.Types.Operator op, IEnumerable<Filter> filters)
        {
            _filter = _filter.WithOperator(op);
            _filter = _filter.WithNestedFilters(filters);
        }
    }

    public record PropertyFilter
    {
        private readonly Filter _filter = new();

        internal PropertyFilter(string name)
        {
            _filter = _filter.ByProperty(name);
        }

        public Filter ContainsAll<T>(IEnumerable<T> value) =>
            _filter.WithOperator(Filters.Types.Operator.ContainsAll).ByValue(value);

        public Filter ContainsAny<T>(IEnumerable<T> value) =>
            _filter.WithOperator(Filters.Types.Operator.ContainsAny).ByValue(value);

        public Filter Equal<TResult>(TResult value) =>
            _filter.WithOperator(Filters.Types.Operator.Equal).ByValue(value);

        public Filter NotEqual<T>(T value) =>
            _filter.WithOperator(Filters.Types.Operator.NotEqual).ByValue(value);

        public Filter GreaterThan<TResult>(TResult value) =>
            _filter.WithOperator(Filters.Types.Operator.GreaterThan).ByValue(value);

        public Filter GreaterThanEqual<T>(T value) =>
            _filter.WithOperator(Filters.Types.Operator.GreaterThanEqual).ByValue(value);

        public Filter LessThan<T>(T value) =>
            _filter.WithOperator(Filters.Types.Operator.LessThan).ByValue(value);

        public Filter LessThanEqual<T>(T value) =>
            _filter.WithOperator(Filters.Types.Operator.LessThanEqual).ByValue(value);

        public Filter WithinGeoRange(GeoCoordinatesConstraint value) =>
            _filter.WithOperator(Filters.Types.Operator.WithinGeoRange).ByValue(value);

        public Filter Like<T>(T value) =>
            _filter.WithOperator(Filters.Types.Operator.Like).ByValue(value);

        public Filter IsNull() => _filter.WithOperator(Filters.Types.Operator.IsNull);
    }
}

public static class Filter<T>
{
    public class PropertyFilter<TResult>
    {
        private readonly Filter.PropertyFilter _prop;

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
