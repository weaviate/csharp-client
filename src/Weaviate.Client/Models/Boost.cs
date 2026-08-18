using System.Globalization;

namespace Weaviate.Client.Models;

/// <summary>
/// Soft-rank search results: promote or demote objects without removing them from the result set.
///
/// A boost is a query-time rescorer. The primary search (vector, hybrid, or BM25) fetches a pool of
/// candidates, the boost re-scores them against its conditions, and the results are re-sorted. Unlike
/// a filter, a boost never excludes objects: non-matching objects stay in the result set but rank lower.
///
/// Use the static factory methods to build a boost, then pass it to a query or generate method via
/// the <c>boost</c> parameter:
/// <list type="bullet">
/// <item><see cref="Filter(Filter, double?, uint?)"/>: promote or demote objects matching a filter condition.</item>
/// <item><see cref="TimeDecay(string, TimeSpan, DateTimeOffset?, TimeSpan?, Curve?, double?, double?, uint?)"/>: rank by recency, decaying with distance from an origin date.</item>
/// <item><see cref="NumericDecay"/>: rank by closeness to a target numeric value.</item>
/// <item><see cref="NumericProperty"/>: rank by a numeric property's raw value.</item>
/// <item><see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>: combine several of the above, each with its own weight.</item>
/// </list>
///
/// Preview feature: requires Weaviate 1.38 or later. Older servers silently ignore the boost.
/// </summary>
public sealed record Boost
{
    /// <summary>
    /// The decay curve used by a distance-based boost (<c>TimeDecay</c>, <c>NumericDecay</c>).
    /// Each curve scores 1 at the origin and falls to the <c>decay</c> value at <c>scale</c> distance.
    /// </summary>
    public enum Curve
    {
        /// <summary>Heavy-tailed decay that halves geometrically. The server default if no curve is set.</summary>
        Exponential,

        /// <summary>Bell-shaped decay with a sharp falloff once past <c>scale</c>.</summary>
        Gaussian,

        /// <summary>Straight-line decay that reaches zero beyond <c>scale</c>.</summary>
        Linear,
    }

    /// <summary>
    /// The transform applied to a numeric property's value in <see cref="NumericProperty"/> before
    /// normalization. Use a modifier to reduce the impact of large property values. If no modifier
    /// is set, the raw value is used.
    /// </summary>
    public enum Modifier
    {
        /// <summary>Apply <c>log(1 + value)</c> to strongly reduce the impact of large values.</summary>
        Log1P,

        /// <summary>Apply <c>sqrt(value)</c> to mildly reduce the impact of large values.</summary>
        Sqrt,
    }

    internal sealed record TimeDecayFunction(
        string Property,
        string Origin,
        string Scale,
        string? Offset,
        Curve? DecayCurve,
        double? DecayValue
    );

    internal sealed record NumericDecayFunction(
        string Property,
        double Origin,
        double Scale,
        double? Offset,
        Curve? DecayCurve,
        double? DecayValue
    );

    internal sealed record PropertyValueFunction(string Property, Modifier? ValueModifier);

    internal sealed record Condition
    {
        internal Filter? Filter { get; init; }
        internal TimeDecayFunction? TimeDecay { get; init; }
        internal NumericDecayFunction? NumericDecay { get; init; }
        internal PropertyValueFunction? PropertyValue { get; init; }
        internal double? Weight { get; init; }
    }

    internal IReadOnlyList<Condition> Conditions { get; }
    internal double? Weight { get; }
    internal uint? Depth { get; }

    private Boost(IReadOnlyList<Condition> conditions, double? weight, uint? depth)
    {
        Conditions = conditions;
        Weight = weight;
        Depth = depth;
    }

    /// <summary>
    /// Promote or demote objects that match a filter condition.
    ///
    /// Matching objects score 1 and non-matching objects score 0, so this acts as a soft where-filter:
    /// non-matching objects are demoted but stay in the result set.
    /// </summary>
    /// <param name="filter">The filter condition, built the same way as for the <c>filters</c> parameter.
    /// Only equality/comparison operators and And/Or/Not are supported.</param>
    /// <param name="weight">How much the boost influences the final score, in [0, 1]: the result is
    /// (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.
    /// When this boost is passed to <see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>, the
    /// weight instead acts as the relative per-condition weight: unbounded, and negative values
    /// demote matching objects.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <returns>The boost</returns>
    public static Boost Filter(Filter filter, double? weight = null, uint? depth = null) =>
        new([new Condition { Filter = filter }], weight, depth);

    /// <summary>
    /// Rank objects by recency: the score decays with distance from an origin date.
    ///
    /// Objects at the origin score 1; the score falls along the chosen curve as the property
    /// value moves away from the origin. Use this to favour more recent (or near-a-date) objects.
    /// </summary>
    /// <param name="property">The name of the date property to measure distance from; the first
    /// letter is lower-cased to match the stored property name, as <c>Filter.Property</c> does.</param>
    /// <param name="scale">The distance from the origin at which the score equals <paramref name="decay"/>.
    /// Must be greater than zero.</param>
    /// <param name="origin">The reference point. If not set, the current time ("now") is used.</param>
    /// <param name="offset">Objects within this distance from the origin keep the full score of 1;
    /// decay starts beyond it. If not set, no offset is applied.</param>
    /// <param name="curve">The decay curve. If not set, the server default (<see cref="Curve.Exponential"/>) is used.</param>
    /// <param name="decay">The score at <paramref name="scale"/> distance from the origin, in (0, 1].
    /// If not set, the server default of 0.5 is used.</param>
    /// <param name="weight">How much the boost influences the final score, in [0, 1]: the result is
    /// (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.
    /// When this boost is passed to <see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>, the
    /// weight instead acts as the relative per-condition weight: unbounded, and negative values
    /// demote matching objects.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="scale"/> is not greater than
    /// zero, or <paramref name="offset"/> is negative. The server silently ignores a duration it
    /// cannot use, so such a value would quietly disable the boost rather than fail.</exception>
    /// <returns>The boost</returns>
    public static Boost TimeDecay(
        string property,
        TimeSpan scale,
        DateTimeOffset? origin = null,
        TimeSpan? offset = null,
        Curve? curve = null,
        double? decay = null,
        double? weight = null,
        uint? depth = null
    )
    {
        if (scale <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(scale),
                scale,
                "A time decay scale must be greater than zero. The server treats a non-positive scale as unusable and silently drops the boost."
            );
        }
        return TimeDecay(
            property,
            ToDurationString(scale, nameof(scale)),
            origin?.ToString("o", CultureInfo.InvariantCulture),
            offset is not null ? ToDurationString(offset.Value, nameof(offset)) : null,
            curve,
            decay,
            weight,
            depth
        );
    }

    /// <summary>
    /// Rank objects by recency, with the origin and distances given as strings.
    /// </summary>
    /// <param name="property">The name of the date property to measure distance from; the first
    /// letter is lower-cased to match the stored property name, as <c>Filter.Property</c> does.</param>
    /// <param name="scale">The distance from the origin at which the score equals <paramref name="decay"/>,
    /// as a duration string such as "7d", "24h", "30m".</param>
    /// <param name="origin">The reference point: "now" or an RFC3339 timestamp. If not set, "now" is used.</param>
    /// <param name="offset">Objects within this distance from the origin keep the full score of 1;
    /// decay starts beyond it. Accepts the same format as <paramref name="scale"/>. If not set, no offset is applied.</param>
    /// <param name="curve">The decay curve. If not set, the server default (<see cref="Curve.Exponential"/>) is used.</param>
    /// <param name="decay">The score at <paramref name="scale"/> distance from the origin, in (0, 1].
    /// If not set, the server default of 0.5 is used.</param>
    /// <param name="weight">How much the boost influences the final score, in [0, 1]: the result is
    /// (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.
    /// When this boost is passed to <see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>, the
    /// weight instead acts as the relative per-condition weight: unbounded, and negative values
    /// demote matching objects.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <returns>The boost</returns>
    public static Boost TimeDecay(
        string property,
        string scale,
        string? origin = null,
        string? offset = null,
        Curve? curve = null,
        double? decay = null,
        double? weight = null,
        uint? depth = null
    ) =>
        new(
            [
                new Condition
                {
                    TimeDecay = new TimeDecayFunction(
                        property.Decapitalize(),
                        origin ?? "now",
                        scale,
                        offset,
                        curve,
                        decay
                    ),
                },
            ],
            weight,
            depth
        );

    /// <summary>
    /// Rank objects by closeness to a target numeric value: the score decays with distance from it.
    ///
    /// Use this when "closer to X is better" (e.g. prefer prices near 50). For simple
    /// "higher is better" ranking without an origin, use <see cref="NumericProperty"/> instead.
    /// </summary>
    /// <param name="property">The name of the numeric (int/number) property to measure distance from;
    /// the first letter is lower-cased to match the stored property name, as <c>Filter.Property</c> does.</param>
    /// <param name="origin">The target value; objects closest to it score highest.</param>
    /// <param name="scale">The distance from the origin at which the score equals <paramref name="decay"/>.</param>
    /// <param name="offset">Objects within this distance from the origin keep the full score of 1;
    /// decay starts beyond it. If not set, no offset is applied.</param>
    /// <param name="curve">The decay curve. If not set, the server default (<see cref="Curve.Exponential"/>) is used.</param>
    /// <param name="decay">The score at <paramref name="scale"/> distance from the origin, in (0, 1].
    /// If not set, the server default of 0.5 is used.</param>
    /// <param name="weight">How much the boost influences the final score, in [0, 1]: the result is
    /// (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.
    /// When this boost is passed to <see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>, the
    /// weight instead acts as the relative per-condition weight: unbounded, and negative values
    /// demote matching objects.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <returns>The boost</returns>
    public static Boost NumericDecay(
        string property,
        double origin,
        double scale,
        double? offset = null,
        Curve? curve = null,
        double? decay = null,
        double? weight = null,
        uint? depth = null
    ) =>
        new(
            [
                new Condition
                {
                    NumericDecay = new NumericDecayFunction(
                        property.Decapitalize(),
                        origin,
                        scale,
                        offset,
                        curve,
                        decay
                    ),
                },
            ],
            weight,
            depth
        );

    /// <summary>
    /// Rank objects by a numeric property's raw value: higher values rank higher.
    ///
    /// Use this for simple proportional ranking (e.g. popularity count, review score). For
    /// distance-based decay from a target value, use <see cref="NumericDecay"/> instead.
    /// </summary>
    /// <param name="name">The name of the numeric (int/number) property to use as a ranking signal;
    /// the first letter is lower-cased to match the stored property name, as <c>Filter.Property</c> does.</param>
    /// <param name="modifier">A transform applied to the value before normalization:
    /// <see cref="Modifier.Log1P"/> or <see cref="Modifier.Sqrt"/>. If not set, the raw value is used.</param>
    /// <param name="weight">How much the boost influences the final score, in [0, 1]: the result is
    /// (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.
    /// When this boost is passed to <see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>, the
    /// weight instead acts as the relative per-condition weight: unbounded, and negative values
    /// demote matching objects.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <returns>The boost</returns>
    public static Boost NumericProperty(
        string name,
        Modifier? modifier = null,
        double? weight = null,
        uint? depth = null
    ) =>
        new(
            [
                new Condition
                {
                    PropertyValue = new PropertyValueFunction(name.Decapitalize(), modifier),
                },
            ],
            weight,
            depth
        );

    /// <summary>
    /// Combine several boosts into one, each weighted relative to the others.
    ///
    /// Each input boost's weight becomes a per-condition weight, balancing the conditions against
    /// each other (e.g. recency twice as important as popularity). A per-condition weight defaults
    /// to 1.0 and may be negative to actively demote matching objects. The <paramref name="weight"/>
    /// argument here is separate: it sets the overall strength of the combined boost. A boost may
    /// carry at most 20 conditions in total.
    /// </summary>
    /// <param name="boosts">The boosts to combine, created via the other factory methods.</param>
    /// <param name="weight">How much the combined boost influences the final score, in [0, 1]: the
    /// result is (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <exception cref="ArgumentException">No boosts are provided, or an input boost has its own
    /// depth set (set <paramref name="depth"/> here on <c>Blend</c> instead).</exception>
    /// <returns>The boost</returns>
    public static Boost Blend(IEnumerable<Boost> boosts, double? weight = null, uint? depth = null)
    {
        var inputs = boosts.ToList();
        if (inputs.Count == 0)
        {
            throw new ArgumentException("Boost.Blend requires at least one boost.", nameof(boosts));
        }
        if (inputs.Any(b => b.Depth is not null))
        {
            throw new ArgumentException(
                "Cannot set depth on sub-boosts passed to Boost.Blend. Use the top-level depth parameter instead.",
                nameof(boosts)
            );
        }
        var conditions = new List<Condition>();
        foreach (var input in inputs)
        {
            foreach (var condition in input.Conditions)
            {
                conditions.Add(
                    condition.Weight is null && input.Weight is not null
                        ? condition with
                        {
                            Weight = input.Weight,
                        }
                        : condition
                );
            }
        }
        return new Boost(conditions, weight, depth);
    }

    /// <summary>
    /// Re-weight a single boost, giving it an overall strength separate from its own weight.
    ///
    /// This is the one-boost form of <see cref="Blend(IEnumerable{Boost}, double?, uint?)"/>: the
    /// input boost's weight becomes the per-condition weight and <paramref name="weight"/> sets the
    /// overall strength of the result.
    /// </summary>
    /// <param name="boost">The boost to wrap, created via the other factory methods.</param>
    /// <param name="weight">How much the combined boost influences the final score, in [0, 1]: the
    /// result is (1 - weight) of the primary score plus weight of the boost score. 0 is a no-op.
    /// If not set, the server default of 0.5 is used.</param>
    /// <param name="depth">How many candidates the primary search fetches for the boost to re-score.
    /// If not set, the server default (100) is used.</param>
    /// <exception cref="ArgumentException"><paramref name="boost"/> has its own depth set (set
    /// <paramref name="depth"/> here on <c>Blend</c> instead).</exception>
    /// <returns>The boost</returns>
    public static Boost Blend(Boost boost, double? weight = null, uint? depth = null) =>
        Blend([boost], weight, depth);

    // The server accepts only ^(\d+(\.\d+)?)(d|h|m|s|ms)$ and silently ignores a duration it cannot
    // parse, which disables the boost instead of erroring. So format from the integer tick count:
    // a double renders small values in exponent notation ("1E-07s"), which the server would reject.
    private static string ToDurationString(TimeSpan value, string paramName)
    {
        var ticks = value.Ticks;
        if (ticks < 0)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                value,
                "A time decay duration cannot be negative."
            );
        }
        if (ticks >= TimeSpan.TicksPerDay && ticks % TimeSpan.TicksPerDay == 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{ticks / TimeSpan.TicksPerDay}d");
        }
        if (ticks >= TimeSpan.TicksPerHour && ticks % TimeSpan.TicksPerHour == 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{ticks / TimeSpan.TicksPerHour}h");
        }
        if (ticks >= TimeSpan.TicksPerMinute && ticks % TimeSpan.TicksPerMinute == 0)
        {
            return string.Create(
                CultureInfo.InvariantCulture,
                $"{ticks / TimeSpan.TicksPerMinute}m"
            );
        }
        var seconds = ticks / TimeSpan.TicksPerSecond;
        var fraction = ticks % TimeSpan.TicksPerSecond;
        if (fraction == 0)
        {
            return string.Create(CultureInfo.InvariantCulture, $"{seconds}s");
        }
        // TicksPerSecond is 10^7, so seven digits carry the fraction exactly.
        var digits = fraction.ToString("D7", CultureInfo.InvariantCulture).TrimEnd('0');
        return string.Create(CultureInfo.InvariantCulture, $"{seconds}.{digits}s");
    }
}
