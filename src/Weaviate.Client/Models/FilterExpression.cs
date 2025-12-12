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

        public Filter Equal(TResult value) => _prop.IsEqual(value);

        public Filter NotEqual(TResult value) => _prop.IsNotEqual(value);

        public Filter GreaterThan(TResult value) => _prop.IsGreaterThan(value);

        public Filter GreaterThanEqual(TResult value) => _prop.IsGreaterThanEqual(value);

        public Filter LessThan(TResult value) => _prop.IsLessThan(value);

        public Filter LessThanEqual(TResult value) => _prop.IsLessThanEqual(value);

        public Filter WithinGeoRange(GeoCoordinateConstraint value) => _prop.WithinGeoRange(value);

        public Filter Like(TResult value) => _prop.IsLike(value);

        public Filter IsNull(bool value = true) => _prop.IsNull(value);
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
