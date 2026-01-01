using Weaviate.Client.Models;
using Weaviate.Client.Models.Typed;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests for typed aggregate results (AggregateResult{T}, AggregateGroupByResult{T}, AggregatePropertyMapper).
/// </summary>
public class TestTypedAggregateResults
{
    #region Test Types

    /// <summary>
    /// Maps full Aggregate.* types by matching property names.
    /// </summary>
    private class ArticleAggregations
    {
        public Aggregate.Text? Title { get; set; }
        public Aggregate.Integer? WordCount { get; set; }
        public Aggregate.Number? Rating { get; set; }
        public Aggregate.Boolean? IsPublished { get; set; }
        public Aggregate.Date? PublishedDate { get; set; }
    }

    /// <summary>
    /// Uses suffixes to extract specific values from aggregations.
    /// </summary>
    private class ArticleSummary
    {
        // Text aggregation - extract count
        public long? TitleCount { get; set; }

        // Number aggregation - extract mean
        public double? RatingMean { get; set; }

        // Integer aggregation - extract sum
        public long? WordCountSum { get; set; }

        // Boolean aggregation - extract percentage true
        public double? IsPublishedPercentageTrue { get; set; }

        // Date aggregation - extract minimum
        public DateTime? PublishedDateMinimum { get; set; }
    }

    /// <summary>
    /// Tests various suffix combinations.
    /// </summary>
    private class NumericSuffixTests
    {
        // Integer suffixes
        public long? QuantityCount { get; set; }
        public long? QuantitySum { get; set; }
        public double? QuantityMean { get; set; }
        public double? QuantityAverage { get; set; }
        public long? QuantityMin { get; set; }
        public long? QuantityMinimum { get; set; }
        public long? QuantityMax { get; set; }
        public long? QuantityMaximum { get; set; }
        public double? QuantityMedian { get; set; }
        public long? QuantityMode { get; set; }

        // Number suffixes
        public double? PriceSum { get; set; }
        public double? PriceMean { get; set; }
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
    }

    /// <summary>
    /// Tests Boolean aggregation suffixes.
    /// </summary>
    private class BooleanSuffixTests
    {
        public long? InStockTotalTrue { get; set; }
        public long? InStockTotalFalse { get; set; }
        public double? InStockPercentageTrue { get; set; }
        public double? InStockPercentageFalse { get; set; }
        public long? InStockCount { get; set; }
    }

    /// <summary>
    /// Tests Text aggregation suffixes.
    /// </summary>
    private class TextSuffixTests
    {
        public long? CategoryCount { get; set; }
        public string? CategoryTopOccurrence { get; set; }
        public List<Aggregate.TopOccurrence<string>>? CategoryTopOccurrences { get; set; }
    }

    /// <summary>
    /// Tests Date aggregation suffixes.
    /// </summary>
    private class DateSuffixTests
    {
        public DateTime? CreatedAtMinimum { get; set; }
        public DateTime? CreatedAtMaximum { get; set; }
        public DateTime? CreatedAtMedian { get; set; }
        public DateTime? CreatedAtMode { get; set; }
        public long? CreatedAtCount { get; set; }
    }

    /// <summary>
    /// Tests mixing full types with suffixes.
    /// </summary>
    private class MixedMappingTests
    {
        // Full type mapping
        public Aggregate.Number? Price { get; set; }

        // Suffix-based extraction from same field
        public double? PriceMean { get; set; }
        public double? PriceSum { get; set; }

        // Another full type
        public Aggregate.Text? Title { get; set; }

        // Suffix extraction
        public long? TitleCount { get; set; }
    }

    #endregion

    #region Test Data

    private static AggregateResult CreateTestAggregateResult()
    {
        return new AggregateResult
        {
            TotalCount = 100,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["title"] = new Aggregate.Text
                {
                    Count = 100,
                    TopOccurrences =
                    [
                        new Aggregate.TopOccurrence<string> { Value = "Introduction", Count = 15 },
                        new Aggregate.TopOccurrence<string> { Value = "Summary", Count = 10 },
                    ],
                },
                ["wordCount"] = new Aggregate.Integer
                {
                    Count = 100,
                    Minimum = 100,
                    Maximum = 5000,
                    Mean = 1500.5,
                    Median = 1200,
                    Mode = 1000,
                    Sum = 150050,
                },
                ["rating"] = new Aggregate.Number
                {
                    Count = 95,
                    Minimum = 1.0,
                    Maximum = 5.0,
                    Mean = 3.8,
                    Median = 4.0,
                    Mode = 4.5,
                    Sum = 361.0,
                },
                ["isPublished"] = new Aggregate.Boolean
                {
                    Count = 100,
                    TotalTrue = 75,
                    TotalFalse = 25,
                    PercentageTrue = 0.75,
                    PercentageFalse = 0.25,
                },
                ["publishedDate"] = new Aggregate.Date
                {
                    Count = 75,
                    Minimum = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Maximum = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc),
                    Median = new DateTime(2022, 6, 15, 0, 0, 0, DateTimeKind.Utc),
                    Mode = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
            },
        };
    }

    private static AggregateGroupByResult CreateTestGroupByResult()
    {
        return new AggregateGroupByResult
        {
            Groups =
            [
                new AggregateGroupByResult.Group
                {
                    GroupedBy = new AggregateGroupByResult.Group.By(
                        "category",
                        "Tech",
                        typeof(string)
                    ),
                    TotalCount = 60,
                    Properties = new Dictionary<string, Aggregate.Property>
                    {
                        ["title"] = new Aggregate.Text
                        {
                            Count = 60,
                            TopOccurrences =
                            [
                                new Aggregate.TopOccurrence<string>
                                {
                                    Value = "AI Guide",
                                    Count = 10,
                                },
                            ],
                        },
                        ["wordCount"] = new Aggregate.Integer
                        {
                            Count = 60,
                            Minimum = 500,
                            Maximum = 5000,
                            Mean = 2000.0,
                            Sum = 120000,
                        },
                        ["rating"] = new Aggregate.Number
                        {
                            Count = 58,
                            Mean = 4.2,
                            Sum = 243.6,
                        },
                    },
                },
                new AggregateGroupByResult.Group
                {
                    GroupedBy = new AggregateGroupByResult.Group.By(
                        "category",
                        "Science",
                        typeof(string)
                    ),
                    TotalCount = 40,
                    Properties = new Dictionary<string, Aggregate.Property>
                    {
                        ["title"] = new Aggregate.Text
                        {
                            Count = 40,
                            TopOccurrences =
                            [
                                new Aggregate.TopOccurrence<string>
                                {
                                    Value = "Research Paper",
                                    Count = 8,
                                },
                            ],
                        },
                        ["wordCount"] = new Aggregate.Integer
                        {
                            Count = 40,
                            Minimum = 1000,
                            Maximum = 8000,
                            Mean = 3000.0,
                            Sum = 120000,
                        },
                        ["rating"] = new Aggregate.Number
                        {
                            Count = 37,
                            Mean = 3.5,
                            Sum = 129.5,
                        },
                    },
                },
            ],
        };
    }

    #endregion

    #region Full Aggregate Type Mapping Tests

    [Fact]
    public void ToTyped_MapsFullAggregateTypes()
    {
        var untyped = CreateTestAggregateResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.NotNull(typed.Properties.Title);
        Assert.Equal(100, typed.Properties.Title.Count);
        Assert.Equal(2, typed.Properties.Title.TopOccurrences.Count);

        Assert.NotNull(typed.Properties.WordCount);
        Assert.Equal(100, typed.Properties.WordCount.Minimum);
        Assert.Equal(5000, typed.Properties.WordCount.Maximum);

        Assert.NotNull(typed.Properties.Rating);
        Assert.Equal(3.8, typed.Properties.Rating.Mean);

        Assert.NotNull(typed.Properties.IsPublished);
        Assert.Equal(75, typed.Properties.IsPublished.TotalTrue);

        Assert.NotNull(typed.Properties.PublishedDate);
        Assert.Equal(
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            typed.Properties.PublishedDate.Minimum
        );
    }

    [Fact]
    public void ToTyped_PreservesTotalCount()
    {
        var untyped = CreateTestAggregateResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal(100, typed.TotalCount);
    }

    [Fact]
    public void ToTyped_PreservesUntypedResult()
    {
        var untyped = CreateTestAggregateResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Same(untyped, typed.Untyped);
    }

    #endregion

    #region Suffix-Based Mapping Tests

    [Fact]
    public void ToTyped_ExtractsValuesBySuffix()
    {
        var untyped = CreateTestAggregateResult();

        var typed = untyped.ToTyped<ArticleSummary>();

        Assert.Equal(100, typed.Properties.TitleCount);
        Assert.Equal(3.8, typed.Properties.RatingMean);
        Assert.Equal(150050, typed.Properties.WordCountSum);
        Assert.Equal(0.75, typed.Properties.IsPublishedPercentageTrue);
        Assert.Equal(
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            typed.Properties.PublishedDateMinimum
        );
    }

    [Fact]
    public void ToTyped_IntegerSuffixes_ExtractCorrectValues()
    {
        var result = new AggregateResult
        {
            TotalCount = 50,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["quantity"] = new Aggregate.Integer
                {
                    Count = 50,
                    Sum = 1000,
                    Mean = 20.0,
                    Minimum = 1,
                    Maximum = 100,
                    Median = 15.5,
                    Mode = 10,
                },
            },
        };

        var typed = result.ToTyped<NumericSuffixTests>();

        Assert.Equal(50, typed.Properties.QuantityCount);
        Assert.Equal(1000, typed.Properties.QuantitySum);
        Assert.Equal(20.0, typed.Properties.QuantityMean);
        Assert.Equal(20.0, typed.Properties.QuantityAverage);
        Assert.Equal(1, typed.Properties.QuantityMin);
        Assert.Equal(1, typed.Properties.QuantityMinimum);
        Assert.Equal(100, typed.Properties.QuantityMax);
        Assert.Equal(100, typed.Properties.QuantityMaximum);
        Assert.Equal(15.5, typed.Properties.QuantityMedian);
        Assert.Equal(10, typed.Properties.QuantityMode);
    }

    [Fact]
    public void ToTyped_NumberSuffixes_ExtractCorrectValues()
    {
        var result = new AggregateResult
        {
            TotalCount = 25,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["price"] = new Aggregate.Number
                {
                    Count = 25,
                    Sum = 500.0,
                    Mean = 20.0,
                    Minimum = 5.0,
                    Maximum = 50.0,
                },
            },
        };

        var typed = result.ToTyped<NumericSuffixTests>();

        Assert.Equal(500.0, typed.Properties.PriceSum);
        Assert.Equal(20.0, typed.Properties.PriceMean);
        Assert.Equal(5.0, typed.Properties.PriceMin);
        Assert.Equal(50.0, typed.Properties.PriceMax);
    }

    [Fact]
    public void ToTyped_BooleanSuffixes_ExtractCorrectValues()
    {
        var result = new AggregateResult
        {
            TotalCount = 100,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["inStock"] = new Aggregate.Boolean
                {
                    Count = 100,
                    TotalTrue = 80,
                    TotalFalse = 20,
                    PercentageTrue = 0.8,
                    PercentageFalse = 0.2,
                },
            },
        };

        var typed = result.ToTyped<BooleanSuffixTests>();

        Assert.Equal(80, typed.Properties.InStockTotalTrue);
        Assert.Equal(20, typed.Properties.InStockTotalFalse);
        Assert.Equal(0.8, typed.Properties.InStockPercentageTrue);
        Assert.Equal(0.2, typed.Properties.InStockPercentageFalse);
        Assert.Equal(100, typed.Properties.InStockCount);
    }

    [Fact]
    public void ToTyped_TextSuffixes_ExtractCorrectValues()
    {
        var result = new AggregateResult
        {
            TotalCount = 50,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["category"] = new Aggregate.Text
                {
                    Count = 50,
                    TopOccurrences =
                    [
                        new Aggregate.TopOccurrence<string> { Value = "Electronics", Count = 20 },
                        new Aggregate.TopOccurrence<string> { Value = "Books", Count = 15 },
                    ],
                },
            },
        };

        var typed = result.ToTyped<TextSuffixTests>();

        Assert.Equal(50, typed.Properties.CategoryCount);
        Assert.Equal("Electronics", typed.Properties.CategoryTopOccurrence);
        Assert.NotNull(typed.Properties.CategoryTopOccurrences);
        Assert.Equal(2, typed.Properties.CategoryTopOccurrences.Count);
        Assert.Equal("Electronics", typed.Properties.CategoryTopOccurrences[0].Value);
    }

    [Fact]
    public void ToTyped_DateSuffixes_ExtractCorrectValues()
    {
        var minDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var maxDate = new DateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var medianDate = new DateTime(2022, 6, 15, 0, 0, 0, DateTimeKind.Utc);
        var modeDate = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = new AggregateResult
        {
            TotalCount = 75,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["createdAt"] = new Aggregate.Date
                {
                    Count = 75,
                    Minimum = minDate,
                    Maximum = maxDate,
                    Median = medianDate,
                    Mode = modeDate,
                },
            },
        };

        var typed = result.ToTyped<DateSuffixTests>();

        Assert.Equal(minDate, typed.Properties.CreatedAtMinimum);
        Assert.Equal(maxDate, typed.Properties.CreatedAtMaximum);
        Assert.Equal(medianDate, typed.Properties.CreatedAtMedian);
        Assert.Equal(modeDate, typed.Properties.CreatedAtMode);
        Assert.Equal(75, typed.Properties.CreatedAtCount);
    }

    #endregion

    #region Mixed Mapping Tests

    [Fact]
    public void ToTyped_MixedFullTypesAndSuffixes()
    {
        var result = new AggregateResult
        {
            TotalCount = 50,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["price"] = new Aggregate.Number
                {
                    Count = 50,
                    Mean = 29.99,
                    Sum = 1499.50,
                },
                ["title"] = new Aggregate.Text { Count = 50, TopOccurrences = [] },
            },
        };

        var typed = result.ToTyped<MixedMappingTests>();

        // Full type should be mapped
        Assert.NotNull(typed.Properties.Price);
        Assert.Equal(29.99, typed.Properties.Price.Mean);

        // Suffix extractions should also work
        Assert.Equal(29.99, typed.Properties.PriceMean);
        Assert.Equal(1499.50, typed.Properties.PriceSum);

        // Full text type
        Assert.NotNull(typed.Properties.Title);
        Assert.Equal(50, typed.Properties.Title.Count);

        // Suffix extraction
        Assert.Equal(50, typed.Properties.TitleCount);
    }

    #endregion

    #region GroupBy Result Tests

    [Fact]
    public void GroupByToTyped_MapsAllGroups()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal(2, typed.Groups.Count);
    }

    [Fact]
    public void GroupByToTyped_PreservesGroupedByInfo()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal("category", typed.Groups[0].GroupedBy.Property);
        Assert.Equal("Tech", typed.Groups[0].GroupedBy.Value);
        Assert.Equal("Science", typed.Groups[1].GroupedBy.Value);
    }

    [Fact]
    public void GroupByToTyped_PreservesTotalCountPerGroup()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal(60, typed.Groups[0].TotalCount);
        Assert.Equal(40, typed.Groups[1].TotalCount);
    }

    [Fact]
    public void GroupByToTyped_MapsFullTypesPerGroup()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        // First group (Tech)
        Assert.NotNull(typed.Groups[0].Properties.Title);
        Assert.Equal(60, typed.Groups[0].Properties.Title.Count);
        Assert.NotNull(typed.Groups[0].Properties.WordCount);
        Assert.Equal(2000.0, typed.Groups[0].Properties.WordCount.Mean);

        // Second group (Science)
        Assert.NotNull(typed.Groups[1].Properties.Title);
        Assert.Equal(40, typed.Groups[1].Properties.Title.Count);
        Assert.NotNull(typed.Groups[1].Properties.WordCount);
        Assert.Equal(3000.0, typed.Groups[1].Properties.WordCount.Mean);
    }

    [Fact]
    public void GroupByToTyped_ExtractsSuffixValuesPerGroup()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleSummary>();

        // First group (Tech)
        Assert.Equal(60, typed.Groups[0].Properties.TitleCount);
        Assert.Equal(4.2, typed.Groups[0].Properties.RatingMean);
        Assert.Equal(120000, typed.Groups[0].Properties.WordCountSum);

        // Second group (Science)
        Assert.Equal(40, typed.Groups[1].Properties.TitleCount);
        Assert.Equal(3.5, typed.Groups[1].Properties.RatingMean);
        Assert.Equal(120000, typed.Groups[1].Properties.WordCountSum);
    }

    [Fact]
    public void GroupByToTyped_PreservesUntypedResult()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Same(untyped, typed.Untyped);
        Assert.Same(untyped.Groups[0], typed.Groups[0].Untyped);
        Assert.Same(untyped.Groups[1], typed.Groups[1].Untyped);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void ToTyped_HandlesEmptyProperties()
    {
        var result = new AggregateResult
        {
            TotalCount = 0,
            Properties = new Dictionary<string, Aggregate.Property>(),
        };

        var typed = result.ToTyped<ArticleAggregations>();

        Assert.Equal(0, typed.TotalCount);
        Assert.Null(typed.Properties.Title);
        Assert.Null(typed.Properties.WordCount);
    }

    [Fact]
    public void GroupByToTyped_HandlesEmptyGroups()
    {
        var result = new AggregateGroupByResult { Groups = [] };

        var typed = result.ToTyped<ArticleAggregations>();

        Assert.Empty(typed.Groups);
    }

    [Fact]
    public void ToTyped_IgnoresPropertiesWithoutMatchingSuffix()
    {
        var result = new AggregateResult
        {
            TotalCount = 10,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["unknownField"] = new Aggregate.Text { Count = 5 },
            },
        };

        var typed = result.ToTyped<ArticleSummary>();

        // All properties should be null since no suffix matches
        Assert.Null(typed.Properties.TitleCount);
        Assert.Null(typed.Properties.RatingMean);
    }

    [Fact]
    public void ToTyped_HandlesNullValues()
    {
        var result = new AggregateResult
        {
            TotalCount = 5,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["wordCount"] = new Aggregate.Integer
                {
                    Count = 5,
                    // Sum is null
                },
            },
        };

        var typed = result.ToTyped<ArticleSummary>();

        // Sum is null, so WordCountSum should be null
        Assert.Null(typed.Properties.WordCountSum);
    }

    [Fact]
    public void ToTyped_HandlesCaseInsensitiveFieldMatching()
    {
        var result = new AggregateResult
        {
            TotalCount = 10,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                ["RATING"] = new Aggregate.Number { Mean = 4.5 },
            },
        };

        var typed = result.ToTyped<ArticleAggregations>();

        // Should match Rating property (case-insensitive)
        Assert.NotNull(typed.Properties.Rating);
        Assert.Equal(4.5, typed.Properties.Rating.Mean);
    }

    [Fact]
    public void ToTyped_WrongSuffixForAggregationType_ReturnsNull()
    {
        // Test that boolean suffixes don't work on text aggregations
        var result = new AggregateResult
        {
            TotalCount = 10,
            Properties = new Dictionary<string, Aggregate.Property>
            {
                // This is a Text aggregation, not Boolean
                ["inStock"] = new Aggregate.Text { Count = 10 },
            },
        };

        var typed = result.ToTyped<BooleanSuffixTests>();

        // Boolean-specific suffixes should not extract from Text
        Assert.Null(typed.Properties.InStockTotalTrue);
        Assert.Null(typed.Properties.InStockTotalFalse);
        Assert.Null(typed.Properties.InStockPercentageTrue);

        // But Count should work (it's available on all types)
        Assert.Equal(10, typed.Properties.InStockCount);
    }

    #endregion

    #region MetricsExtractor Tests

    /// <summary>
    /// Type with full Aggregate.* types for MetricsExtractor testing.
    /// </summary>
    private class FullAggregateTypeMetrics
    {
        public Aggregate.Text? Title { get; set; }
        public Aggregate.Integer? Quantity { get; set; }
        public Aggregate.Number? Price { get; set; }
        public Aggregate.Boolean? InStock { get; set; }
        public Aggregate.Date? CreatedAt { get; set; }
    }

    /// <summary>
    /// Type with suffix-based properties for MetricsExtractor testing.
    /// </summary>
    private class SuffixBasedMetrics
    {
        public double? PriceMean { get; set; }
        public long? QuantitySum { get; set; }
        public long? TitleCount { get; set; }
        public double? InStockPercentageTrue { get; set; }
        public DateTime? CreatedAtMinimum { get; set; }
    }

    /// <summary>
    /// Type with multiple suffixes for the same field.
    /// </summary>
    private class MultipleSuffixesPerField
    {
        public double? PriceMean { get; set; }
        public double? PriceMin { get; set; }
        public double? PriceMax { get; set; }
        public long? PriceCount { get; set; }
    }

    /// <summary>
    /// Type with mixed aggregate types and suffixes.
    /// </summary>
    private class MixedMetrics
    {
        public Aggregate.Text? Title { get; set; }
        public double? PriceMean { get; set; }
        public long? QuantitySum { get; set; }
    }

    [Fact]
    public void FromType_ExtractsFullAggregateTypes()
    {
        var metrics = MetricsExtractor.FromType<FullAggregateTypeMetrics>();

        Assert.Equal(5, metrics.Length);

        var textMetric = metrics
            .OfType<Aggregate.Metric.Text>()
            .FirstOrDefault(m => m.Name == "title");
        Assert.NotNull(textMetric);
        Assert.True(textMetric.Count);
        Assert.True(textMetric.TopOccurrencesCount);
        Assert.True(textMetric.TopOccurrencesValue);

        var intMetric = metrics
            .OfType<Aggregate.Metric.Integer>()
            .FirstOrDefault(m => m.Name == "quantity");
        Assert.NotNull(intMetric);
        Assert.True(intMetric.Count);
        Assert.True(intMetric.Sum);
        Assert.True(intMetric.Mean);
        Assert.True(intMetric.Minimum);
        Assert.True(intMetric.Maximum);
        Assert.True(intMetric.Median);
        Assert.True(intMetric.Mode);

        var numMetric = metrics
            .OfType<Aggregate.Metric.Number>()
            .FirstOrDefault(m => m.Name == "price");
        Assert.NotNull(numMetric);

        var boolMetric = metrics
            .OfType<Aggregate.Metric.Boolean>()
            .FirstOrDefault(m => m.Name == "inStock");
        Assert.NotNull(boolMetric);
        Assert.True(boolMetric.TotalTrue);
        Assert.True(boolMetric.TotalFalse);
        Assert.True(boolMetric.PercentageTrue);
        Assert.True(boolMetric.PercentageFalse);

        var dateMetric = metrics
            .OfType<Aggregate.Metric.Date>()
            .FirstOrDefault(m => m.Name == "createdAt");
        Assert.NotNull(dateMetric);
        Assert.True(dateMetric.Minimum);
        Assert.True(dateMetric.Maximum);
        Assert.True(dateMetric.Median);
        Assert.True(dateMetric.Mode);
    }

    [Fact]
    public void FromType_ExtractsSuffixBasedMetrics()
    {
        var metrics = MetricsExtractor.FromType<SuffixBasedMetrics>();

        Assert.Equal(5, metrics.Length);

        // Price with Mean suffix -> Number with only Mean enabled
        var priceMetric = metrics
            .OfType<Aggregate.Metric.Number>()
            .FirstOrDefault(m => m.Name == "price");
        Assert.NotNull(priceMetric);
        Assert.True(priceMetric.Mean);
        Assert.False(priceMetric.Sum);

        // Quantity with Sum suffix -> Number with only Sum enabled
        var quantityMetric = metrics
            .OfType<Aggregate.Metric.Number>()
            .FirstOrDefault(m => m.Name == "quantity");
        Assert.NotNull(quantityMetric);
        Assert.True(quantityMetric.Sum);
        Assert.False(quantityMetric.Mean);

        // Title with Count suffix -> Integer with only Count enabled
        var titleMetric = metrics
            .OfType<Aggregate.Metric.Integer>()
            .FirstOrDefault(m => m.Name == "title");
        Assert.NotNull(titleMetric);
        Assert.True(titleMetric.Count);
        Assert.False(titleMetric.Sum);

        // InStock with PercentageTrue suffix -> Boolean
        var inStockMetric = metrics
            .OfType<Aggregate.Metric.Boolean>()
            .FirstOrDefault(m => m.Name == "inStock");
        Assert.NotNull(inStockMetric);
        Assert.True(inStockMetric.PercentageTrue);
        Assert.False(inStockMetric.TotalTrue);

        // CreatedAt with Minimum suffix -> Date
        var dateMetric = metrics
            .OfType<Aggregate.Metric.Date>()
            .FirstOrDefault(m => m.Name == "createdAt");
        Assert.NotNull(dateMetric);
        Assert.True(dateMetric.Minimum);
        Assert.False(dateMetric.Maximum);
    }

    [Fact]
    public void FromType_CombinesMultipleSuffixesForSameField()
    {
        var metrics = MetricsExtractor.FromType<MultipleSuffixesPerField>();

        // Should have only one metric for "price" with all flags combined
        Assert.Single(metrics);
        var priceMetric = metrics
            .OfType<Aggregate.Metric.Number>()
            .FirstOrDefault(m => m.Name == "price");
        Assert.NotNull(priceMetric);
        Assert.True(priceMetric.Mean);
        Assert.True(priceMetric.Minimum);
        Assert.True(priceMetric.Maximum);
        Assert.True(priceMetric.Count);
        Assert.False(priceMetric.Sum); // Not specified in the type
    }

    [Fact]
    public void FromType_HandlesMixedTypes()
    {
        var metrics = MetricsExtractor.FromType<MixedMetrics>();

        Assert.Equal(3, metrics.Length);

        // Full Text type should have all flags
        var textMetric = metrics
            .OfType<Aggregate.Metric.Text>()
            .FirstOrDefault(m => m.Name == "title");
        Assert.NotNull(textMetric);
        Assert.True(textMetric.Count);
        Assert.True(textMetric.TopOccurrencesCount);
        Assert.True(textMetric.TopOccurrencesValue);

        // Suffix-based should have only specific flags
        var priceMetric = metrics
            .OfType<Aggregate.Metric.Number>()
            .FirstOrDefault(m => m.Name == "price");
        Assert.NotNull(priceMetric);
        Assert.True(priceMetric.Mean);
        Assert.False(priceMetric.Sum);
    }

    [Fact]
    public void FromType_IgnoresPropertiesWithoutSuffix()
    {
        var metrics = MetricsExtractor.FromType<PropertiesWithoutSuffix>();

        // Only properties with recognized suffixes or Aggregate types should be extracted
        Assert.Empty(metrics);
    }

    private class PropertiesWithoutSuffix
    {
        public string? Name { get; set; }
        public int? Price { get; set; } // No suffix
        public double? Rating { get; set; } // No suffix
    }

    [Fact]
    public void FromType_HandlesTextSuffixes()
    {
        var metrics = MetricsExtractor.FromType<TextSuffixMetrics>();

        Assert.Single(metrics);
        var textMetric = metrics
            .OfType<Aggregate.Metric.Text>()
            .FirstOrDefault(m => m.Name == "title");
        Assert.NotNull(textMetric);
        Assert.True(textMetric.TopOccurrencesCount);
        Assert.True(textMetric.TopOccurrencesValue);
    }

    private class TextSuffixMetrics
    {
        public string? TitleTopOccurrence { get; set; }
    }

    [Fact]
    public void FromType_HandlesBooleanSuffixes()
    {
        var metrics = MetricsExtractor.FromType<BooleanSuffixMetrics>();

        Assert.Single(metrics);
        var boolMetric = metrics
            .OfType<Aggregate.Metric.Boolean>()
            .FirstOrDefault(m => m.Name == "active");
        Assert.NotNull(boolMetric);
        Assert.True(boolMetric.TotalTrue);
        Assert.True(boolMetric.TotalFalse);
        Assert.True(boolMetric.PercentageTrue);
        Assert.True(boolMetric.PercentageFalse);
    }

    private class BooleanSuffixMetrics
    {
        public long? ActiveTotalTrue { get; set; }
        public long? ActiveTotalFalse { get; set; }
        public double? ActivePercentageTrue { get; set; }
        public double? ActivePercentageFalse { get; set; }
    }

    [Fact]
    public void FromType_NonGenericOverload_Works()
    {
        var metrics = MetricsExtractor.FromType(typeof(SuffixBasedMetrics));

        Assert.Equal(5, metrics.Length);
    }

    #endregion
}
