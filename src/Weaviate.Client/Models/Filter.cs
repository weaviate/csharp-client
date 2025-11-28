using Weaviate.V1;

namespace Weaviate.Client.Models;

public record GeoCoordinateConstraint(float Latitude, float Longitude, float Distance);

public interface IFilterEquality<T>
{
    Filter Equal(T value);
    Filter NotEqual(T value);
}

public interface IFilterContainsAny<T>
{
    Filter ContainsAny(IEnumerable<T> value);
}

public interface IFilterContainsNone<T>
{
    Filter ContainsNone(IEnumerable<T> value);
}

public interface IFilterContainsAll<T>
{
    Filter ContainsAll(IEnumerable<T> value);
}

public interface IFilterContains<T>
    : IFilterContainsAll<T>,
        IFilterContainsAny<T>,
        IFilterContainsNone<T> { }

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

    protected Filter InternalContainsNone(IEnumerable<T> value) => Internal.ContainsNone(value);
}

public record TypedGuid(PropertyFilter Parent)
    : TypedBase<Guid>(Parent),
        IFilterEquality<Guid>,
        IFilterContainsAny<Guid>
{
    public Filter ContainsAny(IEnumerable<Guid> value) => InternalContainsAny(value);

    public Filter ContainsNone(IEnumerable<Guid> value) => InternalContainsNone(value);

    public Filter Equal(Guid value) => InternalEqual(value);

    public Filter NotEqual(Guid value) => InternalNotEqual(value);
}

public record TypedValue<T>
    : TypedBase<T>,
        IFilterEquality<T>,
        IFilterContains<T>,
        IFilterCompare<T>
    where T : struct
{
    internal TypedValue(PropertyFilter parent)
        : base(parent) { }

    public Filter ContainsAll(IEnumerable<T> value) => InternalContainsAll(value);

    public Filter ContainsAny(IEnumerable<T> value) => InternalContainsAny(value);

    public Filter ContainsNone(IEnumerable<T> value) => InternalContainsNone(value);

    public Filter Equal(T value) => InternalEqual(value);

    public Filter NotEqual(T value) => InternalNotEqual(value);

    public Filter GreaterThan(T value) => InternalGreaterThan(value);

    public Filter GreaterThanEqual(T value) => InternalGreaterThanEqual(value);

    public Filter LessThan(T value) => InternalLessThan(value);

    public Filter LessThanEqual(T value) => InternalLessThanEqual(value);
}

public partial record Filter
{
    internal V1.Filters InternalFilter { get; init; } = new V1.Filters();

    protected Filter() { }

    public static Filter WithID(Guid id) => Property("_id").Equal(id);

    public static Filter WithIDs(ISet<Guid> ids) => Or([.. ids.Select(WithID)]);

    public static Filter Or(params Filter[] filters) => new OrNestedFilter(filters);

    public static Filter And(params Filter[] filters) => new AndNestedFilter(filters);

    public static Filter Not(Filter filter) => new NotNestedFilter(filter);

    public static TypedGuid ID => new(Property("_id"));

    public static PropertyFilter Property(string name) => new(name.Decapitalize());

    public static ReferenceFilter Reference(string name) => new(name.Decapitalize());

    public static TypedValue<DateTime> CreationTime => new TimeFilter("_creationTimeUnix");

    public static TypedValue<DateTime> UpdateTime => new TimeFilter("_lastUpdateTimeUnix");

    internal Filter WithOperator(Filters.Types.Operator op)
    {
        InternalFilter.Operator = op;
        return this;
    }

    internal Filter WithProperty(string property)
    {
        InternalFilter.Target = new FilterTarget() { Property = property };
        return this;
    }

    internal Filter WithNestedFilters(params Filter[] filters)
    {
        InternalFilter.Filters_.AddRange(filters.Select(f => f.InternalFilter));

        return this;
    }

    internal Filter WithValue<T>(T value)
    {
        Action<Filters> assigner = (
            value switch
            {
                bool v => f => f.ValueBoolean = v,
                GeoCoordinateConstraint v => f =>
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
                IEnumerable<int> v => f =>
                    f.ValueIntArray = new IntArray { Values = { v.Select(Convert.ToInt64) } },
                IEnumerable<long> v => f => f.ValueIntArray = new IntArray { Values = { v } },
                IEnumerable<double> v => f =>
                    f.ValueNumberArray = new NumberArray { Values = { v } },
                IEnumerable<string> v => f => f.ValueTextArray = new TextArray { Values = { v } },
                IEnumerable<Guid> v => f =>
                    f.ValueTextArray = new TextArray { Values = { v.Select(g => g.ToString()) } },
                _ => throw new WeaviateClientException(
                    $"Unsupported type '{typeof(T).Name}' for filter value. Check the documentation for supported filter value types."
                ),
            }
        );

        assigner(InternalFilter);

        return this;
    }

    internal static Filter AllOf(params Filter[] filters) => And(filters);

    internal static Filter AnyOf(params Filter[] filters) => Or(filters);

    #region Operators
    public static Filter operator &(Filter left, Filter right)
    {
        // If left is already an AND filter, combine with its operands
        if (left is AndNestedFilter leftNested)
        {
            // If right is also an AND filter, combine all operands
            if (right is AndNestedFilter rightNested)
            {
                return new AndNestedFilter([.. leftNested.filters, .. rightNested.filters]);
            }
            else
            {
                return new AndNestedFilter([.. leftNested.filters, right]);
            }
        }
        // If right is an AND filter but left is not
        else if (right is AndNestedFilter rightNested)
        {
            return new AndNestedFilter([left, .. rightNested.filters]);
        }
        // Neither is an AND filter
        else
        {
            return new AndNestedFilter(left, right);
        }
    }

    // OR operator (|)
    public static Filter operator |(Filter left, Filter right)
    {
        // If left is already an OR filter, combine with its operands
        if (left is OrNestedFilter leftNested)
        {
            // If right is also an OR filter, combine all operands
            if (right is OrNestedFilter rightNested)
            {
                return new OrNestedFilter([.. leftNested.filters, .. rightNested.filters]);
            }
            else
            {
                return new OrNestedFilter([.. leftNested.filters, right]);
            }
        }
        // If right is an OR filter but left is not
        else if (right is OrNestedFilter rightNested)
        {
            return new OrNestedFilter([left, .. rightNested.filters]);
        }
        // Neither is an OR filter
        else
        {
            return new OrNestedFilter(left, right);
        }
    }
    #endregion
}

public record PropertyFilter : Filter
{
    FilterTarget? _target;

    internal PropertyFilter(FilterTarget target, V1.Filters parentFilter)
    {
        InternalFilter = parentFilter;
        _target = target;
    }

    internal PropertyFilter(string name)
    {
        WithProperty(name);
    }

    public TypedValue<int> Length()
    {
        if (_target is null)
        {
            InternalFilter.Target.Property = $"len({InternalFilter.Target.Property})";
        }
        else
        {
            _target!.Property = $"len({_target!.Property})";
        }

        return new TypedValue<int>(this);
    }

    private Filter ContainsHelper<T>(Filters.Types.Operator op, IEnumerable<T> value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (!value.Any())
        {
            throw new ArgumentException(
                "The 'value' parameter must contain at least one value.",
                nameof(value)
            );
        }
        return WithOperator(op).WithValue(value);
    }

    public Filter ContainsAll<T>(IEnumerable<T> value) =>
        ContainsHelper(Filters.Types.Operator.ContainsAll, value);

    public Filter ContainsAny<T>(IEnumerable<T> value) =>
        ContainsHelper(Filters.Types.Operator.ContainsAny, value);

    public Filter ContainsNone<T>(IEnumerable<T> value) =>
        ContainsHelper(Filters.Types.Operator.ContainsNone, value);

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

    public Filter WithinGeoRange(GeoCoordinate coord, float radius) =>
        WithOperator(Filters.Types.Operator.WithinGeoRange)
            .WithValue(new GeoCoordinateConstraint(coord.Latitude, coord.Longitude, radius));

    public Filter WithinGeoRange(GeoCoordinateConstraint value) =>
        WithOperator(Filters.Types.Operator.WithinGeoRange).WithValue(value);

    public Filter Like<T>(T value) => WithOperator(Filters.Types.Operator.Like).WithValue(value);

    public Filter IsNull(bool value = true) =>
        WithOperator(Filters.Types.Operator.IsNull).WithValue(value);
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

        InternalFilter.Target = _target;
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

        return new PropertyFilter(_target.SingleTarget.Target, InternalFilter);
    }

    public TypedValue<int> Count
    {
        get
        {
            _target.Count = new FilterReferenceCount { On = _target.SingleTarget.On };

            return new TypedValue<int>(new PropertyFilter(_target, InternalFilter));
        }
    }

    public new TypedGuid ID
    {
        get { return new TypedGuid(Property("_id")); }
    }
}

public abstract record NestedFilter : Filter
{
    internal NestedFilter(Filters.Types.Operator op, params Filter[] filters) =>
        WithOperator(op).WithNestedFilters(filters);
}

public record AndNestedFilter(params Filter[] filters)
    : NestedFilter(V1.Filters.Types.Operator.And, filters) { }

public record OrNestedFilter(params Filter[] filters)
    : NestedFilter(V1.Filters.Types.Operator.Or, filters) { }

public record NotNestedFilter(Filter filter)
    : NestedFilter(V1.Filters.Types.Operator.Not, filter) { }

public record TimeFilter : TypedValue<DateTime>
{
    internal TimeFilter(string timeField)
        : base(
            timeField switch
            {
                "_creationTimeUnix" => Filter.Property(timeField),
                "_lastUpdateTimeUnix" => Filter.Property(timeField),
                _ => throw new WeaviateClientException("Unsupported time field for filter"),
            }
        ) { }
}
