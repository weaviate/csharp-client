using System.Linq.Expressions;

namespace Weaviate.Client.Models;

/// <summary>
/// The filter class
/// </summary>
public static class Filter<T>
{
    /// <summary>
    /// The property filter class
    /// </summary>
    public class PropertyFilter<TResult>
    {
        /// <summary>
        /// The prop
        /// </summary>
        private readonly PropertyFilter _prop;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyFilter{TResult}"/> class
        /// </summary>
        /// <param name="name">The name</param>
        internal PropertyFilter(string name)
        {
            _prop = Filter.Property(name);
        }

        /// <summary>
        /// Containses the any using the specified values
        /// </summary>
        /// <param name="values">The values</param>
        /// <returns>The filter</returns>
        public Filter ContainsAny(IEnumerable<TResult> values) => _prop.ContainsAny(values);

        /// <summary>
        /// Containses the all using the specified values
        /// </summary>
        /// <param name="values">The values</param>
        /// <returns>The filter</returns>
        public Filter ContainsAll(IEnumerable<TResult> values) => _prop.ContainsAll(values);

        /// <summary>
        /// Equals the value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter Equal(TResult value) => _prop.IsEqual(value);

        /// <summary>
        /// Nots the equal using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter NotEqual(TResult value) => _prop.IsNotEqual(value);

        /// <summary>
        /// Greaters the than using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter GreaterThan(TResult value) => _prop.IsGreaterThan(value);

        /// <summary>
        /// Greaters the than equal using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter GreaterThanEqual(TResult value) => _prop.IsGreaterThanEqual(value);

        /// <summary>
        /// Lesses the than using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter LessThan(TResult value) => _prop.IsLessThan(value);

        /// <summary>
        /// Lesses the than equal using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter LessThanEqual(TResult value) => _prop.IsLessThanEqual(value);

        /// <summary>
        /// Withins the geo range using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter WithinGeoRange(GeoCoordinateConstraint value) =>
            _prop.IsWithinGeoRange(value);

        /// <summary>
        /// Likes the value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter Like(TResult value) => _prop.IsLike(value);

        /// <summary>
        /// Ises the null using the specified value
        /// </summary>
        /// <param name="value">The value</param>
        /// <returns>The filter</returns>
        public Filter IsNull(bool value = true) => _prop.IsNull(value);
    }

    /// <summary>
    /// Properties the selector
    /// </summary>
    /// <typeparam name="TResult">The result</typeparam>
    /// <param name="selector">The selector</param>
    /// <exception cref="ArgumentException">Expression is not a member access </exception>
    /// <returns>A property filter of t result</returns>
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
