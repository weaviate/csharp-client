using Weaviate.V1;

namespace Weaviate.Client.Models;

public record GeoCoordinatesConstraint(float Latitude, float Longitude, float Distance);

public interface IFilterEquality<T>
{
    Filter Equal(T value);
    Filter NotEqual(T value);
}

public interface IFilterContainsAny<T>
{
    Filter ContainsAny(IEnumerable<T> value);
}

public interface IFilterContainsAll<T>
{
    Filter ContainsAll(IEnumerable<T> value);
}

public interface IFilterContains<T> : IFilterContainsAll<T>, IFilterContainsAny<T> { }

public interface IFilterCompare<T>
{
    public Filter GreaterThan(T value);
    public Filter GreaterThanEqual(T value);
    public Filter LessThan(T value);
    public Filter LessThanEqual(T value);
}

public abstract record TypedBase<T>
{
    protected PropertyFilter Internal { get; }

    protected TypedBase(PropertyFilter parent)
    {
        Internal = parent;
    }

    public static implicit operator Filter(TypedBase<T> filter)
    {
        return filter.Internal;
    }

    protected Filter InternalEqual(T value) => Internal.Equal(value);

    protected Filter InternalNotEqual(T value) => Internal.NotEqual(value);

    protected Filter InternalGreaterThan(T value) => Internal.GreaterThan(value);

    protected Filter InternalGreaterThanEqual(T value) => Internal.GreaterThanEqual(value);

    protected Filter InternalLessThan(T value) => Internal.LessThan(value);

    protected Filter InternalLessThanEqual(T value) => Internal.LessThanEqual(value);

    protected Filter InternalContainsAll(IEnumerable<T> value) => Internal.ContainsAll(value);

    protected Filter InternalContainsAny(IEnumerable<T> value) => Internal.ContainsAny(value);
}

public record TypedGuid(PropertyFilter Parent)
    : TypedBase<Guid>(Parent),
        IFilterEquality<Guid>,
        IFilterContainsAny<Guid>
{
    public Filter ContainsAny(IEnumerable<Guid> value) => InternalContainsAny(value);

    public Filter Equal(Guid value) => InternalEqual(value);

    public Filter NotEqual(Guid value) => InternalNotEqual(value);
}

public record TypedValue<T>(PropertyFilter Parent)
    : TypedBase<T>(Parent),
        IFilterEquality<T>,
        IFilterContains<T>,
        IFilterCompare<T>
    where T : struct
{
    public Filter ContainsAll(IEnumerable<T> value) => InternalContainsAll(value);

    public Filter ContainsAny(IEnumerable<T> value) => InternalContainsAny(value);

    public Filter Equal(T value) => InternalEqual(value);

    public Filter NotEqual(T value) => InternalNotEqual(value);

    public Filter GreaterThan(T value) => InternalGreaterThan(value);

    public Filter GreaterThanEqual(T value) => InternalGreaterThanEqual(value);

    public Filter LessThan(T value) => InternalLessThan(value);

    public Filter LessThanEqual(T value) => InternalLessThanEqual(value);
}

public partial record Filter
{
    protected V1.Filters FiltersMessage { get; init; } = new V1.Filters();

    public static implicit operator V1.Filters(Filter f) => f.FiltersMessage;

    protected Filter() { }

    public static Filter WithID(Guid id) => Property("_id").Equal(id);

    public static Filter WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    public static Filter Or(IEnumerable<Filter> filters) =>
        new NestedFilter(Filters.Types.Operator.Or, filters);

    public static Filter And(IEnumerable<Filter> filters) =>
        new NestedFilter(Filters.Types.Operator.And, filters);

    public static TypedGuid ID => new(Property("_id"));

    public static PropertyFilter Property(string name) => new(name.Decapitalize());

    public static ReferenceFilter Reference(string name) => new(name.Decapitalize());

    public static TypedValue<DateTime> CreationTime => new TimeFilter("_creationTimeUnix");

    public static TypedValue<DateTime> UpdateTime => new TimeFilter("_lastUpdateTimeUnix");

    internal Filter WithOperator(Filters.Types.Operator op)
    {
        FiltersMessage.Operator = op;
        return this;
    }

    internal Filter WithProperty(string property)
    {
        FiltersMessage.Target = new FilterTarget() { Property = property };
        return this;
    }

    internal Filter WithNestedFilters(IEnumerable<Filter> filters)
    {
        FiltersMessage.Filters_.AddRange(filters.Select(f => f.FiltersMessage));

        return this;
    }

    internal Filter WithValue<T>(T value)
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
                IEnumerable<Guid> v => f =>
                    f.ValueTextArray = new TextArray { Values = { v.Select(g => g.ToString()) } },
                _ => throw new WeaviateException(
                    $"Unsupported type '{typeof(T).Name}' for filter value. Check the documentation for supported filter value types."
                ),
            }
        );

        assigner(FiltersMessage);

        return this;
    }
}

public record PropertyFilter : Filter
{
    FilterTarget? _target;

    internal PropertyFilter(FilterTarget target, V1.Filters parentFilter)
    {
        FiltersMessage = parentFilter;
        _target = target;
    }

    internal PropertyFilter(string name)
    {
        WithProperty(name);
    }

    public TypedValue<int> Length
    {
        get
        {
            _target!.Property = $"len({_target!.Property})";

            return new TypedValue<int>(this);
        }
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

    public Filter Like<T>(T value) => WithOperator(Filters.Types.Operator.Like).WithValue(value);

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

        FiltersMessage.Target = _target;
    }

    public new ReferenceFilter Reference(string name)
    {
        _target.SingleTarget.Target = new FilterTarget()
        {
            SingleTarget = new FilterReferenceSingleTarget() { On = name },
        };

        return new ReferenceFilter(this) { _target = _target!.SingleTarget.Target };
    }

    public new PropertyFilter Property(string name)
    {
        _target.SingleTarget.Target = new FilterTarget() { Property = name };

        return new PropertyFilter(_target.SingleTarget.Target, FiltersMessage);
    }

    internal TypedValue<int> Count
    {
        get
        {
            _target.Count = new FilterReferenceCount { On = _target.SingleTarget.On };

            return new TypedValue<int>(new PropertyFilter(_target, FiltersMessage));
        }
    }

    public new TypedGuid ID
    {
        get { return new TypedGuid(Property("_id")); }
    }
}

public record NestedFilter : Filter
{
    internal NestedFilter(Filters.Types.Operator op, IEnumerable<Filter> filters) =>
        WithOperator(op).WithNestedFilters(filters);
}

public record TimeFilter : TypedValue<DateTime>
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
