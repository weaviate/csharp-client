using System.Linq.Expressions;

namespace Weaviate.Client.Models;

public static class Filter<T>
{
    public class PropertyFilter<TResult>
    {
        private readonly PropertyFilter _prop;

        internal PropertyFilter(string name)
        {
            _prop = Filter.Property(name);
        }

        public Filter ContainsAny(IEnumerable<TResult> values) => _prop.ContainsAny(values);

        public Filter ContainsAll(IEnumerable<TResult> values) => _prop.ContainsAll(values);

        public Filter Equal(TResult value) => _prop.Equal(value);

        public Filter NotEqual(TResult value) => _prop.NotEqual(value);

        public Filter GreaterThan(TResult value) => _prop.GreaterThan(value);

        public Filter GreaterThanEqual(TResult value) => _prop.GreaterThanEqual(value);

        public Filter LessThan(TResult value) => _prop.LessThan(value);

        public Filter LessThanEqual(TResult value) => _prop.LessThanEqual(value);

        public Filter WithinGeoRange(GeoCoordinateConstraint value) => _prop.WithinGeoRange(value);

        public Filter Like(TResult value) => _prop.Like(value);

        public Filter IsNull() => _prop.IsNull();
    }

    public static PropertyFilter<TResult> Property<TResult>(Expression<Func<T, TResult>> selector)
    {
        if (selector.Body is MemberExpression member)
        {
            var mi = member.Member;
            return new PropertyFilter<TResult>(mi.Name);
        }

        throw new ArgumentException("Expression is not a member access", nameof(selector));
    }
}
