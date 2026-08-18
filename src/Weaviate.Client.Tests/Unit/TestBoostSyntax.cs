using System.Globalization;
using System.Text.RegularExpressions;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using Weaviate.Client.Typed;
using V1 = Weaviate.Client.Grpc.Protobuf.V1;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests verifying that the Boost query parameter serializes to the expected
/// SearchRequest proto across the query, generate, and typed surfaces.
/// </summary>
[Collection("Unit Tests")]
public class TestBoostSyntax : IAsyncLifetime
{
    /// <summary>
    /// The collection name
    /// </summary>
    private const string CollectionName = "TestCollection";

    /// <summary>
    /// The test media bytes
    /// </summary>
    private static readonly byte[] TestMediaBytes = [1, 2, 3, 4];

    /// <summary>
    /// The only duration format the server accepts for a time decay scale or offset
    /// (weaviate 1.38.4, usecases/traverser/boost_scorer.go: durationPattern). A string outside
    /// this pattern is silently ignored server-side, which disables the boost instead of erroring.
    /// </summary>
    private static readonly Regex ServerDurationPattern = new(@"^(\d+(?:\.\d+)?)(d|h|m|s|ms)$");

    /// <summary>
    /// The get request
    /// </summary>
    private Func<V1.SearchRequest?> _getRequest = null!;

    /// <summary>
    /// The collection
    /// </summary>
    private CollectionClient _collection = null!;

    /// <summary>
    /// Initializes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public ValueTask InitializeAsync()
    {
        var (client, getRequest) = MockGrpcClient.CreateWithSearchCapture();
        _getRequest = getRequest;
        _collection = client.Collections.Use(CollectionName);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Tests that a query without a boost leaves the boost proto field unset
    /// </summary>
    [Fact]
    public async Task BM25_WithoutBoost_OmitsBoost()
    {
        await _collection.Query.BM25(
            "banana",
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.Null(request.Boost);
    }

    /// <summary>
    /// Tests that a filter boost with weight and depth serializes all fields
    /// </summary>
    [Fact]
    public async Task BM25_FilterBoost_SerializesFilterWeightAndDepth()
    {
        await _collection.Query.BM25(
            "banana",
            boost: Boost.Filter(
                Filter.Property("category").IsEqual("fruit"),
                weight: 0.7f,
                depth: 200
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        Assert.True(request.Boost.HasWeight);
        Assert.Equal(0.7f, request.Boost.Weight);
        Assert.True(request.Boost.HasDepth);
        Assert.Equal(200u, request.Boost.Depth);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.False(condition.HasWeight);
        Assert.NotNull(condition.Filter);
        Assert.Equal("category", condition.Filter.Target.Property);
        Assert.Equal(V1.Filters.Types.Operator.Equal, condition.Filter.Operator);
    }

    /// <summary>
    /// Tests that server-side defaults are not re-encoded client-side
    /// </summary>
    [Fact]
    public async Task Hybrid_FilterBoost_LeavesDefaultsToServer()
    {
        await _collection.Query.Hybrid(
            "banana",
            boost: Boost.Filter(Filter.Property("category").IsEqual("fruit")),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        Assert.False(request.Boost.HasWeight);
        Assert.False(request.Boost.HasDepth);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.False(condition.HasWeight);
    }

    /// <summary>
    /// Tests that a fully-specified time decay boost serializes all fields
    /// </summary>
    [Fact]
    public async Task NearText_TimeDecayBoost_SerializesAllValues()
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay(
                "publishedAt",
                scale: "7d",
                origin: "2024-01-01T00:00:00Z",
                offset: "1d",
                curve: Boost.Curve.Gaussian,
                decay: 0.4f,
                weight: 0.6f,
                depth: 150
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        Assert.Equal(0.6f, request.Boost.Weight);
        Assert.Equal(150u, request.Boost.Depth);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal("publishedAt", condition.TimeDecay.Property);
        Assert.Equal("2024-01-01T00:00:00Z", condition.TimeDecay.Origin);
        Assert.Equal("7d", condition.TimeDecay.Scale);
        Assert.Equal("1d", condition.TimeDecay.Offset);
        Assert.Equal(V1.Boost.Types.DecayCurve.Gauss, condition.TimeDecay.Curve);
        Assert.Equal(0.4f, condition.TimeDecay.DecayValue);
    }

    /// <summary>
    /// Tests that a minimal time decay boost defaults the origin to "now" and leaves the rest unset
    /// </summary>
    [Fact]
    public async Task NearText_TimeDecayBoost_DefaultsOriginToNow()
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay("publishedAt", scale: "7d"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal("now", condition.TimeDecay.Origin);
        Assert.False(condition.TimeDecay.HasOffset);
        Assert.False(condition.TimeDecay.HasCurve);
        Assert.False(condition.TimeDecay.HasDecayValue);
        Assert.False(request.Boost.HasWeight);
        Assert.False(request.Boost.HasDepth);
    }

    /// <summary>
    /// Tests that the TimeSpan and DateTimeOffset overload converts to duration and RFC3339 strings
    /// </summary>
    [Fact]
    public async Task NearText_TimeDecayBoost_ConvertsTimeSpanAndDateTimeOffset()
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay(
                "publishedAt",
                scale: TimeSpan.FromDays(7),
                origin: new DateTimeOffset(2024, 5, 1, 12, 0, 0, TimeSpan.Zero),
                offset: TimeSpan.FromHours(36)
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal("7d", condition.TimeDecay.Scale);
        Assert.Equal("36h", condition.TimeDecay.Offset);
        Assert.Equal("2024-05-01T12:00:00.0000000+00:00", condition.TimeDecay.Origin);
    }

    /// <summary>
    /// Tests that a TimeSpan scale serializes to the same duration string as the Python
    /// client's _decay_duration_to_str, including the seconds branch where the two
    /// implementations diverge textually
    /// </summary>
    /// <param name="seconds">The scale duration in seconds</param>
    /// <param name="expected">The expected duration string</param>
    [Theory]
    [InlineData(604800, "7d")]
    [InlineData(129600, "36h")]
    [InlineData(60, "1m")]
    [InlineData(90, "90s")]
    [InlineData(30, "30s")]
    [InlineData(1.5, "1.5s")]
    [InlineData(17280000, "200d")]
    public async Task NearText_TimeDecayBoost_TimeSpanScaleMatchesPythonDurationString(
        double seconds,
        string expected
    )
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay("publishedAt", scale: TimeSpan.FromSeconds(seconds)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal(expected, condition.TimeDecay.Scale);
        AssertServerParsableNonZeroDuration(condition.TimeDecay.Scale);
    }

    /// <summary>
    /// Tests that a sub-millisecond scale still serializes to a duration the server can parse.
    /// The previous double-based formatting emitted exponent notation ("1E-07s"), and rounding
    /// such a value to "0ms" would parse but silently turn the boost into a no-op
    /// </summary>
    /// <param name="ticks">The scale in ticks; 500 ticks is TimeSpan.FromMilliseconds(0.05)</param>
    /// <param name="expected">The expected duration string</param>
    [Theory]
    [InlineData(1, "0.0000001s")]
    [InlineData(500, "0.00005s")]
    [InlineData(9999, "0.0009999s")]
    public async Task NearText_TimeDecayBoost_SubMillisecondScaleStaysInServerContract(
        long ticks,
        string expected
    )
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay("publishedAt", scale: TimeSpan.FromTicks(ticks)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        AssertServerParsableNonZeroDuration(condition.TimeDecay.Scale);
        Assert.Equal(expected, condition.TimeDecay.Scale);
    }

    /// <summary>
    /// Tests that a negative scale fails fast client-side rather than serializing to "-2592000s",
    /// which the server cannot parse and silently ignores
    /// </summary>
    [Fact]
    public void TimeDecay_Throws_OnNegativeScale()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Boost.TimeDecay("publishedAt", scale: TimeSpan.FromDays(-30))
        );
        Assert.Equal("scale", exception.ParamName);
    }

    /// <summary>
    /// Tests that a zero scale fails fast client-side: the server treats a non-positive scale as
    /// unusable and silently drops the boost
    /// </summary>
    [Fact]
    public void TimeDecay_Throws_OnZeroScale()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Boost.TimeDecay("publishedAt", scale: TimeSpan.Zero)
        );
        Assert.Equal("scale", exception.ParamName);
    }

    /// <summary>
    /// Tests that a negative offset fails fast client-side, naming the offset parameter
    /// </summary>
    [Fact]
    public void TimeDecay_Throws_OnNegativeOffset()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            Boost.TimeDecay(
                "publishedAt",
                scale: TimeSpan.FromDays(7),
                offset: TimeSpan.FromDays(-1)
            )
        );
        Assert.Equal("offset", exception.ParamName);
    }

    /// <summary>
    /// Tests that a zero offset is accepted: unlike scale, "no offset" is a meaningful request and
    /// serializes to a duration the server parses to the same zero it would use when unset
    /// </summary>
    [Fact]
    public async Task NearText_TimeDecayBoost_AcceptsZeroOffset()
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay(
                "publishedAt",
                scale: TimeSpan.FromDays(7),
                offset: TimeSpan.Zero
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal("0s", condition.TimeDecay.Offset);
        Assert.Matches(ServerDurationPattern, condition.TimeDecay.Offset);
    }

    /// <summary>
    /// Tests that the string time decay overload lower-cases the first letter of the property
    /// name, the way Filter.Property does, so the server's exact-match lookup resolves it
    /// </summary>
    [Fact]
    public async Task NearText_TimeDecayBoost_DecapitalizesPropertyName()
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay("PublishedAt", scale: "7d"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal("publishedAt", condition.TimeDecay.Property);
    }

    /// <summary>
    /// Tests that the TimeSpan time decay overload normalizes the property name the same way
    /// </summary>
    [Fact]
    public async Task NearText_TimeDecayBoost_TimeSpanOverloadDecapitalizesPropertyName()
    {
        await _collection.Query.NearText(
            "banana",
            boost: Boost.TimeDecay("PublishedAt", scale: TimeSpan.FromDays(7)),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.TimeDecay);
        Assert.Equal("publishedAt", condition.TimeDecay.Property);
    }

    /// <summary>
    /// Tests that a numeric decay boost serializes its fields
    /// </summary>
    [Fact]
    public async Task NearVector_NumericDecayBoost_SerializesAllValues()
    {
        await _collection.Query.NearVector(
            new float[] { 1f, 2f, 3f },
            boost: Boost.NumericDecay(
                "price",
                origin: 50,
                scale: 10,
                offset: 5,
                curve: Boost.Curve.Linear,
                decay: 0.3f
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.NumericDecay);
        Assert.Equal("price", condition.NumericDecay.Property);
        Assert.Equal(50d, condition.NumericDecay.Origin);
        Assert.Equal(10d, condition.NumericDecay.Scale);
        Assert.Equal(5d, condition.NumericDecay.Offset);
        Assert.Equal(V1.Boost.Types.DecayCurve.Linear, condition.NumericDecay.Curve);
        Assert.Equal(0.3f, condition.NumericDecay.DecayValue);
    }

    /// <summary>
    /// Tests that the exponential curve maps to the proto exponential value
    /// </summary>
    [Fact]
    public async Task NearVector_NumericDecayBoost_MapsExponentialCurve()
    {
        await _collection.Query.NearVector(
            new float[] { 1f, 2f, 3f },
            boost: Boost.NumericDecay(
                "price",
                origin: 50,
                scale: 10,
                curve: Boost.Curve.Exponential
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.NumericDecay);
        Assert.Equal(V1.Boost.Types.DecayCurve.Exponential, condition.NumericDecay.Curve);
        Assert.False(condition.NumericDecay.HasOffset);
        Assert.False(condition.NumericDecay.HasDecayValue);
    }

    /// <summary>
    /// Tests that a numeric decay boost lower-cases the first letter of the property name
    /// </summary>
    [Fact]
    public async Task NearVector_NumericDecayBoost_DecapitalizesPropertyName()
    {
        await _collection.Query.NearVector(
            new float[] { 1f, 2f, 3f },
            boost: Boost.NumericDecay("Rating_number", origin: 5, scale: 1),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.NumericDecay);
        Assert.Equal("rating_number", condition.NumericDecay.Property);
    }

    /// <summary>
    /// Tests that a numeric property boost with a modifier serializes it
    /// </summary>
    [Fact]
    public async Task NearObject_NumericPropertyBoost_SerializesModifier()
    {
        await _collection.Query.NearObject(
            Guid.NewGuid(),
            boost: Boost.NumericProperty("viewCount", modifier: Boost.Modifier.Log1P),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.PropertyValue);
        Assert.Equal("viewCount", condition.PropertyValue.Property);
        Assert.Equal(V1.Boost.Types.PropertyValueModifier.Log1P, condition.PropertyValue.Modifier);
    }

    /// <summary>
    /// Tests that a numeric property boost without a modifier leaves the field unset
    /// </summary>
    [Fact]
    public async Task NearMedia_NumericPropertyBoost_OmitsModifierWhenUnset()
    {
        await _collection.Query.NearMedia(
            m => m.Image(TestMediaBytes).Build(),
            boost: Boost.NumericProperty("viewCount"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.NearImage);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.PropertyValue);
        Assert.False(condition.PropertyValue.HasModifier);
    }

    /// <summary>
    /// Tests that a numeric property boost lower-cases the first letter of the property name
    /// </summary>
    [Fact]
    public async Task NearObject_NumericPropertyBoost_DecapitalizesPropertyName()
    {
        await _collection.Query.NearObject(
            Guid.NewGuid(),
            boost: Boost.NumericProperty("Rating_number"),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.PropertyValue);
        Assert.Equal("rating_number", condition.PropertyValue.Property);
    }

    /// <summary>
    /// Tests that blend turns sub-boost weights into per-condition weights
    /// </summary>
    [Fact]
    public async Task BM25_BlendBoost_CombinesConditionsAndWeights()
    {
        var blended = Boost.Blend(
            [
                Boost.TimeDecay("publishedAt", scale: "7d", weight: 2f),
                Boost.NumericProperty("viewCount"),
                Boost.NumericProperty("spamScore", weight: -1f),
            ],
            weight: 0.8f,
            depth: 300
        );

        await _collection.Query.BM25(
            "banana",
            boost: blended,
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        Assert.Equal(0.8f, request.Boost.Weight);
        Assert.Equal(300u, request.Boost.Depth);
        Assert.Equal(3, request.Boost.Conditions.Count);
        Assert.True(request.Boost.Conditions[0].HasWeight);
        Assert.Equal(2f, request.Boost.Conditions[0].Weight);
        Assert.NotNull(request.Boost.Conditions[0].TimeDecay);
        Assert.False(request.Boost.Conditions[1].HasWeight);
        Assert.NotNull(request.Boost.Conditions[1].PropertyValue);
        Assert.True(request.Boost.Conditions[2].HasWeight);
        Assert.Equal(-1f, request.Boost.Conditions[2].Weight);
    }

    /// <summary>
    /// Tests that inside one blend a capitalised property name normalizes identically for a filter
    /// condition and a property-value condition, so one cannot resolve while the other fails with
    /// "no such prop"
    /// </summary>
    [Fact]
    public async Task BM25_BlendBoost_NormalizesPropertyNamesLikeFilterProperty()
    {
        await _collection.Query.BM25(
            "banana",
            boost: Boost.Blend([
                Boost.Filter(Filter.Property("Rating_number").IsGreaterThan(4)),
                Boost.NumericProperty("Rating_number"),
            ]),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        Assert.Equal(2, request.Boost.Conditions.Count);
        var filterCondition = request.Boost.Conditions[0].Filter;
        var propertyCondition = request.Boost.Conditions[1].PropertyValue;
        Assert.NotNull(filterCondition);
        Assert.NotNull(propertyCondition);
        Assert.Equal("rating_number", propertyCondition.Property);
        Assert.Equal(filterCondition.Target.Property, propertyCondition.Property);
    }

    /// <summary>
    /// Tests that blend rejects an empty input
    /// </summary>
    [Fact]
    public void Blend_Throws_OnEmptyInput()
    {
        Assert.Throws<ArgumentException>(() => Boost.Blend([]));
    }

    /// <summary>
    /// Tests that blend rejects sub-boosts carrying their own depth
    /// </summary>
    [Fact]
    public void Blend_Throws_OnSubBoostDepth()
    {
        Assert.Throws<ArgumentException>(() =>
            Boost.Blend([Boost.NumericProperty("viewCount", depth: 10)])
        );
    }

    /// <summary>
    /// Tests that the generate surface threads the boost into the request
    /// </summary>
    [Fact]
    public async Task Generate_BM25_WithBoost_SerializesBoost()
    {
        await _collection.Generate.BM25(
            "banana",
            boost: Boost.NumericProperty("viewCount", modifier: Boost.Modifier.Sqrt),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        var condition = Assert.Single(request.Boost.Conditions);
        Assert.NotNull(condition.PropertyValue);
        Assert.Equal(V1.Boost.Types.PropertyValueModifier.Sqrt, condition.PropertyValue.Modifier);
    }

    /// <summary>
    /// Tests that the typed query surface threads the boost into the request
    /// </summary>
    [Fact]
    public async Task TypedQuery_BM25_WithBoost_SerializesBoost()
    {
        var typed = new TypedQueryClient<TestDocument>(_collection.Query);

        await typed.BM25(
            "banana",
            boost: Boost.Filter(Filter.Property("category").IsEqual("fruit"), weight: 0.5f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var request = _getRequest();
        Assert.NotNull(request);
        Assert.NotNull(request.Boost);
        Assert.True(request.Boost.HasWeight);
        Assert.Equal(0.5f, request.Boost.Weight);
    }

    /// <summary>
    /// Asserts that a duration string matches the server's pattern and carries a non-zero
    /// magnitude. A zero magnitude parses but makes the boost a silent no-op.
    /// </summary>
    /// <param name="duration">The duration string as it is sent on the wire</param>
    private static void AssertServerParsableNonZeroDuration(string duration)
    {
        var match = ServerDurationPattern.Match(duration);
        Assert.True(match.Success, $"'{duration}' is outside the server's duration pattern.");
        Assert.True(
            double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) > 0,
            $"'{duration}' has a zero magnitude, which disables the boost server-side."
        );
    }

    /// <summary>
    /// The test document class used for typed client tests
    /// </summary>
    private class TestDocument
    {
        /// <summary>
        /// Gets or sets the value of the category
        /// </summary>
        public string Category { get; set; } = string.Empty;
    }
}
