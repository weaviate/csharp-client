using System.Linq.Expressions;
using Weaviate.V1;

namespace Weaviate.Client.Grpc;

public static class Filter
{
    public record GeoCoordinatesConstraint
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public float Distance { get; set; }
    }

    private static Filters WithOperator(Filters.Types.Operator op) => new() { Operator = op };

    private static Filters ByValue<T>(T value, Filters f)
    {
        Action<Filters> assigner = value switch
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
            IEnumerable<bool> v => f => f.ValueBooleanArray = new BooleanArray { Values = { v } },
            IEnumerable<long> v => f => f.ValueIntArray = new IntArray { Values = { v } },
            IEnumerable<double> v => f => f.ValueNumberArray = new NumberArray { Values = { v } },
            IEnumerable<string> v => f => f.ValueTextArray = new TextArray { Values = { v } },
            _ => throw new WeaviateException("Unsupported type for filter"),
        };

        assigner(f);

        return f;
    }

    private static Filters ByProperty(string property, Filters f)
    {
        f.Target = new FilterTarget() { Property = property };

        return f;
    }

    public class PropertyFilter
    {
        public string Name { get; }

        internal PropertyFilter(string name)
        {
            Name = name;
        }

        public Filters ContainsAll<T>(IEnumerable<T> value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.ContainsAll)));

        public Filters ContainsAny<T>(IEnumerable<T> value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.ContainsAny)));

        public Filters Equal<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.Equal)));

        public Filters NotEqual<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.NotEqual)));

        public Filters GreaterThan<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.GreaterThan)));

        public Filters GreaterThanEqual<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.GreaterThanEqual)));

        public Filters LessThan<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.LessThan)));

        public Filters LessThanEqual<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.LessThanEqual)));

        public Filters WithinGeoRange(GeoCoordinatesConstraint value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.WithinGeoRange)));

        public Filters Like<T>(T value) =>
            ByProperty(Name, ByValue(value, WithOperator(Filters.Types.Operator.Like)));

        public Filters IsNull() => ByProperty(Name, WithOperator(Filters.Types.Operator.IsNull));
    }

    internal static Filters WithID(Guid id) => Property("_id").Equal(id.ToString());

    internal static Filters WithIDs(ISet<Guid> ids) => Or(ids.Select(WithID));

    internal static Filters Or(IEnumerable<Filters> filters) =>
        new Filters { Operator = Filters.Types.Operator.Or, Filters_ = { filters } };

    internal static Filters And(IEnumerable<Filters> filters) =>
        new Filters { Operator = Filters.Types.Operator.And, Filters_ = { filters } };

    public static PropertyFilter Property(string name)
    {
        return new PropertyFilter(name);
    }
}

public static class Filter<T>
{
    public class PropertyFilter<TResult>
    {
        public string Name { get; }

        internal PropertyFilter(string name)
        {
            Name = name.ToLower();
        }

        public Filters ContainsAny(IEnumerable<TResult> values) =>
            Filter.Property(Name).ContainsAny(values);

        public Filters ContainsAll(IEnumerable<TResult> values) =>
            Filter.Property(Name).ContainsAll(values);

        public Filters Equal(TResult value) => Filter.Property(Name).Equal(value);

        public Filters NotEqual(TResult value) => Filter.Property(Name).NotEqual(value);

        public Filters GreaterThan(TResult value) => Filter.Property(Name).GreaterThan(value);

        public Filters GreaterThanEqual(TResult value) =>
            Filter.Property(Name).GreaterThanEqual(value);

        public Filters LessThan(TResult value) => Filter.Property(Name).LessThan(value);

        public Filters LessThanEqual(TResult value) => Filter.Property(Name).LessThanEqual(value);

        public Filters WithinGeoRange(Filter.GeoCoordinatesConstraint value) =>
            Filter.Property(Name).WithinGeoRange(value);

        public Filters Like(TResult value) => Filter.Property(Name).Like(value);

        public Filters IsNull() => Filter.Property(Name).IsNull();
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
