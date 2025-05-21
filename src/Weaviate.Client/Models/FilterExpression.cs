using System.Linq.Expressions;
using Weaviate.V1;

namespace Weaviate.Client.Models;

public static class Filter<T>
{
    public class PropertyFilter<TResult>
    {
        private readonly FilterBase _prop;

        internal PropertyFilter(string name)
        {
            _prop = Filter.Property(name);
        }

        public FilterBase ContainsAny(IEnumerable<TResult> values) => _prop.ContainsAny(values);

        public FilterBase ContainsAll(IEnumerable<TResult> values) => _prop.ContainsAll(values);

        public FilterBase Equal(TResult value) => _prop.Equal(value);

        public FilterBase NotEqual(TResult value) => _prop.NotEqual(value);

        public FilterBase GreaterThan(TResult value) => _prop.GreaterThan(value);

        public FilterBase GreaterThanEqual(TResult value) => _prop.GreaterThanEqual(value);

        public FilterBase LessThan(TResult value) => _prop.LessThan(value);

        public FilterBase LessThanEqual(TResult value) => _prop.LessThanEqual(value);

        public FilterBase WithinGeoRange(GeoCoordinatesConstraint value) =>
            _prop.WithinGeoRange(value);

        public FilterBase Like(TResult value) => _prop.Like(value);

        public FilterBase IsNull() => _prop.IsNull();
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
