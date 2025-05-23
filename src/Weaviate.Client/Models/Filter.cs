using Weaviate.V1;

namespace Weaviate.Client.Models;

public record GeoCoordinatesConstraint(float Latitude, float Longitude, float Distance);

public record Filter
{
    public record TypedBase<T>
    {
        protected readonly PropertyFilter _internal;

        internal TypedBase(PropertyFilter parent)
        {
            _internal = parent;
        }

        public static implicit operator Filter(TypedBase<T> filter)
        {
            return filter._internal;
        }
    }

    public record TypedEquality<T>(PropertyFilter parent) : TypedBase<T>(parent)
    {
        public Filter Equal(T value) => _internal.Equal(value);

        public Filter NotEqual(T value) => _internal.NotEqual(value);
    }

    public record Typed<T>(PropertyFilter parent) : TypedEquality<T>(parent)
    {
        public Filter ContainsAll(IEnumerable<T> value) => _internal.ContainsAll(value);

        public Filter ContainsAny(IEnumerable<T> value) => _internal.ContainsAny(value);

        public virtual Filter GreaterThan(T value) => _internal.GreaterThan(value);

        public virtual Filter GreaterThanEqual(T value) => _internal.GreaterThanEqual(value);

        public virtual Filter LessThan(T value) => _internal.LessThan(value);

        public virtual Filter LessThanEqual(T value) => _internal.LessThanEqual(value);
    }

    protected V1.Filters _filter { get; init; } = new V1.Filters();

    public static implicit operator V1.Filters(Filter f) => f._filter;

    protected Filter() { }

    public static Filter WithID(Guid id) => Property("_id").Equal(id.ToString());

    public static Filter WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    public static Filter Or(IEnumerable<Filter> filters) =>
        new NestedFilter(Filters.Types.Operator.Or, filters);

    public static Filter And(IEnumerable<Filter> filters) =>
        new NestedFilter(Filters.Types.Operator.And, filters);

    public static PropertyFilter Property(string name) => new PropertyFilter(name.Decapitalize());

    public static ReferenceFilter Reference(string name) =>
        new ReferenceFilter(name.Decapitalize());

    public static Typed<DateTime> CreationTime => new TimeFilter("_creationTimeUnix");
    public static Typed<DateTime> UpdateTime => new TimeFilter("_lastUpdateTimeUnix");

    protected Filter WithOperator(Filters.Types.Operator op)
    {
        _filter.Operator = op;
        return this;
    }

    protected Filter WithProperty(string property)
    {
        _filter.Target = new FilterTarget() { Property = property };
        return this;
    }

    protected Filter WithNestedFilters(IEnumerable<Filter> filters)
    {
        _filter.Filters_.AddRange(filters.Select(f => f._filter));

        return this;
    }

    protected Filter WithValue<T>(T value)
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
                Guid v => f => f.ValueText = v.ToString(),
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
                _ => throw new WeaviateException(
                    $"Unsupported type '{typeof(T).Name}' for filter value. Check the documentation for supported filter value types."
                ),
            }
        );

        assigner(_filter);

        return this;
    }

    public record PropertyFilter : Filter
    {
        FilterTarget? _target;

        internal PropertyFilter(string name, FilterTarget target, V1.Filters parentFilter)
        {
            _filter = parentFilter;
            _target = target;
            _target.Property = name;
        }

        internal PropertyFilter(string name)
        {
            WithProperty(name);
        }

        public Typed<int> Length()
        {
            _target!.Property = $"len({_target!.Property})";

            return new Typed<int>(this);
        }

        public Filter ContainsAll<T>(IEnumerable<T> value) =>
            WithOperator(Filters.Types.Operator.ContainsAll).WithValue(value);

        public Filter ContainsAny<T>(IEnumerable<T> value) =>
            WithOperator(Filters.Types.Operator.ContainsAny).WithValue(value);

        public Filter Equal<TResult>(TResult value) =>
            WithOperator(Filters.Types.Operator.Equal).WithValue(value);

        public Filter NotEqual<T>(T value) =>
            WithOperator(Filters.Types.Operator.NotEqual).WithValue(value);

        public Filter GreaterThan<TResult>(TResult value) =>
            WithOperator(Filters.Types.Operator.GreaterThan).WithValue(value);

        public Filter GreaterThanEqual<T>(T value) =>
            WithOperator(Filters.Types.Operator.GreaterThanEqual).WithValue(value);

        public Filter LessThan<T>(T value) =>
            WithOperator(Filters.Types.Operator.LessThan).WithValue(value);

        public Filter LessThanEqual<T>(T value) =>
            WithOperator(Filters.Types.Operator.LessThanEqual).WithValue(value);

        public Filter WithinGeoRange(GeoCoordinatesConstraint value) =>
            WithOperator(Filters.Types.Operator.WithinGeoRange).WithValue(value);

        public Filter Like<T>(T value) =>
            WithOperator(Filters.Types.Operator.Like).WithValue(value);

        public Filter IsNull() => WithOperator(Filters.Types.Operator.IsNull);
    }

    public record ReferenceFilter : Filter
    {
        private FilterTarget _target;

        internal ReferenceFilter(string name)
        {
            _target = new FilterTarget()
            {
                SingleTarget = new FilterReferenceSingleTarget() { On = name },
            };

            _filter.Target = _target;
        }

        public new ReferenceFilter Reference(string name)
        {
            var nextTarget = new FilterTarget()
            {
                SingleTarget = new FilterReferenceSingleTarget() { On = name },
            };

            _target.SingleTarget.Target = nextTarget;

            return new ReferenceFilter(this) { _target = nextTarget };
        }

        public new PropertyFilter Property(string name)
        {
            _target.SingleTarget.Target = new FilterTarget();
            return new PropertyFilter(name, _target.SingleTarget.Target, _filter);
        }

        public TypedEquality<Guid> ID()
        {
            _target.SingleTarget.Target = new FilterTarget();
            return new TypedEquality<Guid>(
                new PropertyFilter("_id", _target.SingleTarget.Target, _filter)
            );
        }
    }

    public record NestedFilter : Filter
    {
        internal NestedFilter(Filters.Types.Operator op, IEnumerable<Filter> filters) =>
            WithOperator(op).WithNestedFilters(filters);
    }

    public record TimeFilter : Typed<DateTime>
    {
        internal TimeFilter(string timeField)
            : base(
                timeField switch
                {
                    "_creationTimeUnix" => Property(timeField),
                    "_lastUpdateTimeUnix" => Property(timeField),
                    _ => throw new WeaviateException("Unsupported time field for filter"),
                }
            ) { }
    }
}
