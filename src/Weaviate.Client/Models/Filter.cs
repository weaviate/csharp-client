using Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Models;

/// <summary>
/// The geo coordinate constraint
/// </summary>
public record GeoCoordinateConstraint(float Latitude, float Longitude, float Distance);

/// <summary>
/// The filter equality interface
/// </summary>
public interface IFilterEquality<T>
{
    /// <summary>
    /// Ises the equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    Filter IsEqual(T value);

    /// <summary>
    /// Ises the not equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    Filter IsNotEqual(T value);
}

/// <summary>
/// The filter contains any interface
/// </summary>
public interface IFilterContainsAny<T>
{
    /// <summary>
    /// Containses the any using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    Filter ContainsAny(IEnumerable<T> value);
}

/// <summary>
/// The filter contains none interface
/// </summary>
public interface IFilterContainsNone<T>
{
    /// <summary>
    /// Containses the none using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    Filter ContainsNone(IEnumerable<T> value);
}

/// <summary>
/// The filter contains all interface
/// </summary>
public interface IFilterContainsAll<T>
{
    /// <summary>
    /// Containses the all using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    Filter ContainsAll(IEnumerable<T> value);
}

/// <summary>
/// The filter contains interface
/// </summary>
/// <seealso cref="IFilterContainsAll{T}"/>
/// <seealso cref="IFilterContainsAny{T}"/>
/// <seealso cref="IFilterContainsNone{T}"/>
public interface IFilterContains<T>
    : IFilterContainsAll<T>,
        IFilterContainsAny<T>,
        IFilterContainsNone<T> { }

/// <summary>
/// The filter compare interface
/// </summary>
public interface IFilterCompare<T>
{
    /// <summary>
    /// Ises the greater than using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsGreaterThan(T value);

    /// <summary>
    /// Ises the greater than equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsGreaterThanEqual(T value);

    /// <summary>
    /// Ises the less than using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLessThan(T value);

    /// <summary>
    /// Ises the less than equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLessThanEqual(T value);
}

/// <summary>
/// The typed base
/// </summary>
public abstract record TypedBase<T>
{
    /// <summary>
    /// Gets the value of the internal
    /// </summary>
    protected PropertyFilter Internal { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TypedBase"/> class
    /// </summary>
    /// <param name="parent">The parent</param>
    protected TypedBase(PropertyFilter parent)
    {
        Internal = parent;
    }

    public static implicit operator Filter(TypedBase<T> filter)
    {
        return filter.Internal;
    }

    /// <summary>
    /// Internals the equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalEqual(T value) => Internal.IsEqual(value);

    /// <summary>
    /// Internals the not equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalNotEqual(T value) => Internal.IsNotEqual(value);

    /// <summary>
    /// Internals the greater than using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalGreaterThan(T value) => Internal.IsGreaterThan(value);

    /// <summary>
    /// Internals the greater than equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalGreaterThanEqual(T value) => Internal.IsGreaterThanEqual(value);

    /// <summary>
    /// Internals the less than using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalLessThan(T value) => Internal.IsLessThan(value);

    /// <summary>
    /// Internals the less than equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalLessThanEqual(T value) => Internal.IsLessThanEqual(value);

    /// <summary>
    /// Internals the contains all using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalContainsAll(IEnumerable<T> value) => Internal.ContainsAll(value);

    /// <summary>
    /// Internals the contains any using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalContainsAny(IEnumerable<T> value) => Internal.ContainsAny(value);

    /// <summary>
    /// Internals the contains none using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    protected Filter InternalContainsNone(IEnumerable<T> value) => Internal.ContainsNone(value);
}

/// <summary>
/// The typed guid
/// </summary>
public record TypedGuid(PropertyFilter Parent)
    : TypedBase<Guid>(Parent),
        IFilterEquality<Guid>,
        IFilterContainsAny<Guid>
{
    /// <summary>
    /// Containses the any using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsAny(IEnumerable<Guid> value) => InternalContainsAny(value);

    /// <summary>
    /// Containses the none using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsNone(IEnumerable<Guid> value) => InternalContainsNone(value);

    /// <summary>
    /// Ises the equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsEqual(Guid value) => InternalEqual(value);

    /// <summary>
    /// Ises the not equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsNotEqual(Guid value) => InternalNotEqual(value);
}

/// <summary>
/// The typed value
/// </summary>
public record TypedValue<T>
    : TypedBase<T>,
        IFilterEquality<T>,
        IFilterContains<T>,
        IFilterCompare<T>
    where T : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TypedValue"/> class
    /// </summary>
    /// <param name="parent">The parent</param>
    internal TypedValue(PropertyFilter parent)
        : base(parent) { }

    /// <summary>
    /// Containses the all using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsAll(IEnumerable<T> value) => InternalContainsAll(value);

    /// <summary>
    /// Containses the any using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsAny(IEnumerable<T> value) => InternalContainsAny(value);

    /// <summary>
    /// Containses the none using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsNone(IEnumerable<T> value) => InternalContainsNone(value);

    /// <summary>
    /// Ises the equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsEqual(T value) => InternalEqual(value);

    /// <summary>
    /// Ises the not equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsNotEqual(T value) => InternalNotEqual(value);

    /// <summary>
    /// Ises the greater than using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsGreaterThan(T value) => InternalGreaterThan(value);

    /// <summary>
    /// Ises the greater than equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsGreaterThanEqual(T value) => InternalGreaterThanEqual(value);

    /// <summary>
    /// Ises the less than using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLessThan(T value) => InternalLessThan(value);

    /// <summary>
    /// Ises the less than equal using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLessThanEqual(T value) => InternalLessThanEqual(value);
}

/// <summary>
/// The filter
/// </summary>
public partial record Filter
{
    /// <summary>
    /// Gets or inits the value of the internal filter
    /// </summary>
    internal Filters InternalFilter { get; init; } = new Filters();

    /// <summary>
    /// Initializes a new instance of the <see cref="Filter"/> class
    /// </summary>
    protected Filter() { }

    /// <summary>
    /// Anies the of using the specified filters
    /// </summary>
    /// <param name="filters">The filters</param>
    /// <returns>The filter</returns>
    public static Filter AnyOf(params Filter[] filters) => new OrNestedFilter(filters);

    /// <summary>
    /// Alls the of using the specified filters
    /// </summary>
    /// <param name="filters">The filters</param>
    /// <returns>The filter</returns>
    public static Filter AllOf(params Filter[] filters) => new AndNestedFilter(filters);

    /// <summary>
    /// Nots the filter
    /// </summary>
    /// <param name="filter">The filter</param>
    /// <returns>The filter</returns>
    public static Filter Not(Filter filter) => new NotNestedFilter(filter);

    /// <summary>
    /// Gets the value of the uuid
    /// </summary>
    public static TypedGuid UUID => new(Property("_id"));

    /// <summary>
    /// Properties the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The property filter</returns>
    public static PropertyFilter Property(string name) => new(name.Decapitalize());

    /// <summary>
    /// References the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The reference filter</returns>
    public static ReferenceFilter Reference(string name) => new(name.Decapitalize());

    /// <summary>
    /// Gets the value of the creation time
    /// </summary>
    public static TypedValue<DateTime> CreationTime => new TimeFilter("_creationTimeUnix");

    /// <summary>
    /// Gets the value of the update time
    /// </summary>
    public static TypedValue<DateTime> UpdateTime => new TimeFilter("_lastUpdateTimeUnix");

    /// <summary>
    /// Adds the operator using the specified op
    /// </summary>
    /// <param name="op">The op</param>
    /// <returns>The filter</returns>
    internal Filter WithOperator(Filters.Types.Operator op)
    {
        InternalFilter.Operator = op;
        return this;
    }

    /// <summary>
    /// Adds the property using the specified property
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>The filter</returns>
    internal Filter WithProperty(string property)
    {
        InternalFilter.Target = new FilterTarget() { Property = property };
        return this;
    }

    /// <summary>
    /// Adds the nested filters using the specified filters
    /// </summary>
    /// <param name="filters">The filters</param>
    /// <returns>The filter</returns>
    internal Filter WithNestedFilters(params Filter[] filters)
    {
        InternalFilter.Filters_.AddRange(filters.Select(f => f.InternalFilter));

        return this;
    }

    /// <summary>
    /// Adds the value using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <exception cref="WeaviateClientException">Unsupported type '{typeof(T).Name}' for filter value. Check the documentation for supported filter value types.</exception>
    /// <returns>The filter</returns>
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

/// <summary>
/// The property filter
/// </summary>
public record PropertyFilter : Filter
{
    /// <summary>
    /// The target
    /// </summary>
    FilterTarget? _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyFilter"/> class
    /// </summary>
    /// <param name="target">The target</param>
    /// <param name="parentFilter">The parent filter</param>
    internal PropertyFilter(FilterTarget target, Filters parentFilter)
    {
        InternalFilter = parentFilter;
        _target = target;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyFilter"/> class
    /// </summary>
    /// <param name="name">The name</param>
    internal PropertyFilter(string name)
    {
        WithProperty(name);
    }

    /// <summary>
    /// Hases the length
    /// </summary>
    /// <returns>A typed value of int</returns>
    public TypedValue<int> HasLength()
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

    /// <summary>
    /// Containses the helper using the specified op
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="op">The op</param>
    /// <param name="value">The value</param>
    /// <exception cref="ArgumentException">The 'value' parameter must contain at least one value. </exception>
    /// <returns>The filter</returns>
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

    /// <summary>
    /// Containses the all using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsAll<T>(IEnumerable<T> value) =>
        ContainsHelper(Filters.Types.Operator.ContainsAll, value);

    /// <summary>
    /// Containses the any using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsAny<T>(IEnumerable<T> value) =>
        ContainsHelper(Filters.Types.Operator.ContainsAny, value);

    /// <summary>
    /// Containses the none using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter ContainsNone<T>(IEnumerable<T> value) =>
        ContainsHelper(Filters.Types.Operator.ContainsNone, value);

    /// <summary>
    /// Ises the equal using the specified value
    /// </summary>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsEqual<TResult>(TResult value) =>
        WithOperator(Filters.Types.Operator.Equal).WithValue(value);

    /// <summary>
    /// Ises the not equal using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsNotEqual<T>(T value) =>
        WithOperator(Filters.Types.Operator.NotEqual).WithValue(value);

    /// <summary>
    /// Ises the greater than using the specified value
    /// </summary>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsGreaterThan<TResult>(TResult value) =>
        WithOperator(Filters.Types.Operator.GreaterThan).WithValue(value);

    /// <summary>
    /// Ises the greater than equal using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsGreaterThanEqual<T>(T value) =>
        WithOperator(Filters.Types.Operator.GreaterThanEqual).WithValue(value);

    /// <summary>
    /// Ises the less than using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLessThan<T>(T value) =>
        WithOperator(Filters.Types.Operator.LessThan).WithValue(value);

    /// <summary>
    /// Ises the less than equal using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLessThanEqual<T>(T value) =>
        WithOperator(Filters.Types.Operator.LessThanEqual).WithValue(value);

    /// <summary>
    /// Ises the within geo range using the specified coord
    /// </summary>
    /// <param name="coord">The coord</param>
    /// <param name="radius">The radius</param>
    /// <returns>The filter</returns>
    public Filter IsWithinGeoRange(GeoCoordinate coord, float radius) =>
        WithOperator(Filters.Types.Operator.WithinGeoRange)
            .WithValue(new GeoCoordinateConstraint(coord.Latitude, coord.Longitude, radius));

    /// <summary>
    /// Ises the within geo range using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsWithinGeoRange(GeoCoordinateConstraint value) =>
        WithOperator(Filters.Types.Operator.WithinGeoRange).WithValue(value);

    /// <summary>
    /// Ises the like using the specified value
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsLike<T>(T value) => WithOperator(Filters.Types.Operator.Like).WithValue(value);

    /// <summary>
    /// Ises the null using the specified value
    /// </summary>
    /// <param name="value">The value</param>
    /// <returns>The filter</returns>
    public Filter IsNull(bool value = true) =>
        WithOperator(Filters.Types.Operator.IsNull).WithValue(value);
}

/// <summary>
/// The reference filter
/// </summary>
public record ReferenceFilter : Filter
{
    /// <summary>
    /// The target
    /// </summary>
    private FilterTarget _target;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReferenceFilter"/> class
    /// </summary>
    /// <param name="name">The name</param>
    internal ReferenceFilter(string name)
    {
        _target = new FilterTarget()
        {
            SingleTarget = new FilterReferenceSingleTarget() { On = name },
        };

        InternalFilter.Target = _target;
    }

    /// <summary>
    /// References the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The reference filter</returns>
    public new ReferenceFilter Reference(string name)
    {
        _target.SingleTarget.Target = new FilterTarget()
        {
            SingleTarget = new FilterReferenceSingleTarget() { On = name },
        };

        return new ReferenceFilter(this) { _target = _target!.SingleTarget.Target };
    }

    /// <summary>
    /// Properties the name
    /// </summary>
    /// <param name="name">The name</param>
    /// <returns>The property filter</returns>
    public new PropertyFilter Property(string name)
    {
        _target.SingleTarget.Target = new FilterTarget() { Property = name };

        return new PropertyFilter(_target.SingleTarget.Target, InternalFilter);
    }

    /// <summary>
    /// Gets the value of the count
    /// </summary>
    public TypedValue<int> Count
    {
        get
        {
            _target.Count = new FilterReferenceCount { On = _target.SingleTarget.On };

            return new TypedValue<int>(new PropertyFilter(_target, InternalFilter));
        }
    }

    /// <summary>
    /// Gets the value of the uuid
    /// </summary>
    public new TypedGuid UUID
    {
        get { return new TypedGuid(Property("_id")); }
    }
}

/// <summary>
/// The nested filter
/// </summary>
public abstract record NestedFilter : Filter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NestedFilter"/> class
    /// </summary>
    /// <param name="op">The op</param>
    /// <param name="filters">The filters</param>
    internal NestedFilter(Filters.Types.Operator op, params Filter[] filters) =>
        WithOperator(op).WithNestedFilters(filters);
}

/// <summary>
/// The and nested filter
/// </summary>
public record AndNestedFilter(params Filter[] filters)
    : NestedFilter(Filters.Types.Operator.And, filters) { }

/// <summary>
/// The or nested filter
/// </summary>
public record OrNestedFilter(params Filter[] filters)
    : NestedFilter(Filters.Types.Operator.Or, filters) { }

/// <summary>
/// The not nested filter
/// </summary>
public record NotNestedFilter(Filter filter) : NestedFilter(Filters.Types.Operator.Not, filter) { }

/// <summary>
/// The time filter
/// </summary>
public record TimeFilter : TypedValue<DateTime>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeFilter"/> class
    /// </summary>
    /// <param name="timeField">The time field</param>
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
