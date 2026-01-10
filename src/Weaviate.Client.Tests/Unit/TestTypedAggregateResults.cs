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
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public Aggregate.Text? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of the word count
        /// </summary>
        public Aggregate.Integer? WordCount { get; set; }

        /// <summary>
        /// Gets or sets the value of the rating
        /// </summary>
        public Aggregate.Number? Rating { get; set; }

        /// <summary>
        /// Gets or sets the value of the is published
        /// </summary>
        public Aggregate.Boolean? IsPublished { get; set; }

        /// <summary>
        /// Gets or sets the value of the published date
        /// </summary>
        public Aggregate.Date? PublishedDate { get; set; }
    }

    /// <summary>
    /// Uses suffixes to extract specific values from aggregations.
    /// </summary>
    private class ArticleSummary
    {
        // Text aggregation - extract count
        /// <summary>
        /// Gets or sets the value of the title count
        /// </summary>
        public long? TitleCount { get; set; }

        // Number aggregation - extract mean
        /// <summary>
        /// Gets or sets the value of the rating mean
        /// </summary>
        public double? RatingMean { get; set; }

        // Integer aggregation - extract sum
        /// <summary>
        /// Gets or sets the value of the word count sum
        /// </summary>
        public long? WordCountSum { get; set; }

        // Boolean aggregation - extract percentage true
        /// <summary>
        /// Gets or sets the value of the is published percentage true
        /// </summary>
        public double? IsPublishedPercentageTrue { get; set; }

        // Date aggregation - extract minimum
        /// <summary>
        /// Gets or sets the value of the published date minimum
        /// </summary>
        public DateTime? PublishedDateMinimum { get; set; }
    }

    /// <summary>
    /// Tests various suffix combinations.
    /// </summary>
    private class NumericSuffixTests
    {
        // Integer suffixes
        /// <summary>
        /// Gets or sets the value of the quantity count
        /// </summary>
        public long? QuantityCount { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity sum
        /// </summary>
        public long? QuantitySum { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity mean
        /// </summary>
        public double? QuantityMean { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity average
        /// </summary>
        public double? QuantityAverage { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity min
        /// </summary>
        public long? QuantityMin { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity minimum
        /// </summary>
        public long? QuantityMinimum { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity max
        /// </summary>
        public long? QuantityMax { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity maximum
        /// </summary>
        public long? QuantityMaximum { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity median
        /// </summary>
        public double? QuantityMedian { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity mode
        /// </summary>
        public long? QuantityMode { get; set; }

        // Number suffixes
        /// <summary>
        /// Gets or sets the value of the price sum
        /// </summary>
        public double? PriceSum { get; set; }

        /// <summary>
        /// Gets or sets the value of the price mean
        /// </summary>
        public double? PriceMean { get; set; }

        /// <summary>
        /// Gets or sets the value of the price min
        /// </summary>
        public double? PriceMin { get; set; }

        /// <summary>
        /// Gets or sets the value of the price max
        /// </summary>
        public double? PriceMax { get; set; }
    }

    /// <summary>
    /// Tests Boolean aggregation suffixes.
    /// </summary>
    private class BooleanSuffixTests
    {
        /// <summary>
        /// Gets or sets the value of the in stock total true
        /// </summary>
        public long? InStockTotalTrue { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock total false
        /// </summary>
        public long? InStockTotalFalse { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock percentage true
        /// </summary>
        public double? InStockPercentageTrue { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock percentage false
        /// </summary>
        public double? InStockPercentageFalse { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock count
        /// </summary>
        public long? InStockCount { get; set; }
    }

    /// <summary>
    /// Tests Text aggregation suffixes.
    /// </summary>
    private class TextSuffixTests
    {
        /// <summary>
        /// Gets or sets the value of the category count
        /// </summary>
        public long? CategoryCount { get; set; }

        /// <summary>
        /// Gets or sets the value of the category top occurrence
        /// </summary>
        public string? CategoryTopOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the value of the category top occurrences
        /// </summary>
        public List<Aggregate.TopOccurrence<string>>? CategoryTopOccurrences { get; set; }
    }

    /// <summary>
    /// Tests Date aggregation suffixes.
    /// </summary>
    private class DateSuffixTests
    {
        /// <summary>
        /// Gets or sets the value of the created at minimum
        /// </summary>
        public DateTime? CreatedAtMinimum { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at maximum
        /// </summary>
        public DateTime? CreatedAtMaximum { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at median
        /// </summary>
        public DateTime? CreatedAtMedian { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at mode
        /// </summary>
        public DateTime? CreatedAtMode { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at count
        /// </summary>
        public long? CreatedAtCount { get; set; }
    }

    /// <summary>
    /// Tests mixing full types with suffixes.
    /// </summary>
    private class MixedMappingTests
    {
        // Full type mapping
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public Aggregate.Number? Price { get; set; }

        // Suffix-based extraction from same field
        /// <summary>
        /// Gets or sets the value of the price mean
        /// </summary>
        public double? PriceMean { get; set; }

        /// <summary>
        /// Gets or sets the value of the price sum
        /// </summary>
        public double? PriceSum { get; set; }

        // Another full type
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public Aggregate.Text? Title { get; set; }

        // Suffix extraction
        /// <summary>
        /// Gets or sets the value of the title count
        /// </summary>
        public long? TitleCount { get; set; }
    }

    #endregion

    #region Test Data

    /// <summary>
    /// Creates the test aggregate result
    /// </summary>
    /// <returns>The aggregate result</returns>
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

    /// <summary>
    /// Creates the test group by result
    /// </summary>
    /// <returns>The aggregate group by result</returns>
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

    /// <summary>
    /// Tests that to typed maps full aggregate types
    /// </summary>
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

    /// <summary>
    /// Tests that to typed preserves total count
    /// </summary>
    [Fact]
    public void ToTyped_PreservesTotalCount()
    {
        var untyped = CreateTestAggregateResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal(100, typed.TotalCount);
    }

    /// <summary>
    /// Tests that to typed preserves untyped result
    /// </summary>
    [Fact]
    public void ToTyped_PreservesUntypedResult()
    {
        var untyped = CreateTestAggregateResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Same(untyped, typed.Untyped);
    }

    #endregion

    #region Suffix-Based Mapping Tests

    /// <summary>
    /// Tests that to typed extracts values by suffix
    /// </summary>
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

    /// <summary>
    /// Tests that to typed integer suffixes extract correct values
    /// </summary>
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

    /// <summary>
    /// Tests that to typed number suffixes extract correct values
    /// </summary>
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

    /// <summary>
    /// Tests that to typed boolean suffixes extract correct values
    /// </summary>
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

    /// <summary>
    /// Tests that to typed text suffixes extract correct values
    /// </summary>
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

    /// <summary>
    /// Tests that to typed date suffixes extract correct values
    /// </summary>
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

    /// <summary>
    /// Tests that to typed mixed full types and suffixes
    /// </summary>
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

    /// <summary>
    /// Tests that group by to typed maps all groups
    /// </summary>
    [Fact]
    public void GroupByToTyped_MapsAllGroups()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal(2, typed.Groups.Count);
    }

    /// <summary>
    /// Tests that group by to typed preserves grouped by info
    /// </summary>
    [Fact]
    public void GroupByToTyped_PreservesGroupedByInfo()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal("category", typed.Groups[0].GroupedBy.Property);
        Assert.Equal("Tech", typed.Groups[0].GroupedBy.Value);
        Assert.Equal("Science", typed.Groups[1].GroupedBy.Value);
    }

    /// <summary>
    /// Tests that group by to typed preserves total count per group
    /// </summary>
    [Fact]
    public void GroupByToTyped_PreservesTotalCountPerGroup()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        Assert.Equal(60, typed.Groups[0].TotalCount);
        Assert.Equal(40, typed.Groups[1].TotalCount);
    }

    /// <summary>
    /// Tests that group by to typed maps full types per group
    /// </summary>
    [Fact]
    public void GroupByToTyped_MapsFullTypesPerGroup()
    {
        var untyped = CreateTestGroupByResult();

        var typed = untyped.ToTyped<ArticleAggregations>();

        // First group (Tech)
        Assert.NotNull(typed.Groups[0].Properties.Title);
        Assert.NotNull(typed.Groups[0].Properties.Title!.Count);
        Assert.Equal(60, typed.Groups[0].Properties.Title!.Count!);
        Assert.NotNull(typed.Groups[0].Properties.WordCount);
        Assert.NotNull(typed.Groups[0].Properties.WordCount!.Mean);
        Assert.Equal(2000.0, typed.Groups[0].Properties.WordCount!.Mean!.Value);

        // Second group (Science)
        Assert.NotNull(typed.Groups[1].Properties.Title);
        Assert.NotNull(typed.Groups[1].Properties.Title!.Count);
        Assert.Equal(40, typed.Groups[1].Properties.Title!.Count!);
        Assert.NotNull(typed.Groups[1].Properties.WordCount);
        Assert.NotNull(typed.Groups[1].Properties.WordCount!.Mean);
        Assert.Equal(3000.0, typed.Groups[1].Properties.WordCount!.Mean!.Value);
    }

    /// <summary>
    /// Tests that group by to typed extracts suffix values per group
    /// </summary>
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

    /// <summary>
    /// Tests that group by to typed preserves untyped result
    /// </summary>
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

    /// <summary>
    /// Tests that to typed handles empty properties
    /// </summary>
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

    /// <summary>
    /// Tests that group by to typed handles empty groups
    /// </summary>
    [Fact]
    public void GroupByToTyped_HandlesEmptyGroups()
    {
        var result = new AggregateGroupByResult { Groups = [] };

        var typed = result.ToTyped<ArticleAggregations>();

        Assert.Empty(typed.Groups);
    }

    /// <summary>
    /// Tests that to typed ignores properties without matching suffix
    /// </summary>
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

    /// <summary>
    /// Tests that to typed handles null values
    /// </summary>
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

    /// <summary>
    /// Tests that to typed handles case insensitive field matching
    /// </summary>
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

    /// <summary>
    /// Tests that to typed wrong suffix for aggregation type returns null
    /// </summary>
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
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public Aggregate.Text? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity
        /// </summary>
        public Aggregate.Integer? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public Aggregate.Number? Price { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock
        /// </summary>
        public Aggregate.Boolean? InStock { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at
        /// </summary>
        public Aggregate.Date? CreatedAt { get; set; }
    }

    /// <summary>
    /// Type with suffix-based properties for MetricsExtractor testing.
    /// </summary>
    private class SuffixBasedMetrics
    {
        /// <summary>
        /// Gets or sets the value of the price mean
        /// </summary>
        public double? PriceMean { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity sum
        /// </summary>
        public long? QuantitySum { get; set; }

        /// <summary>
        /// Gets or sets the value of the title count
        /// </summary>
        public long? TitleCount { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock percentage true
        /// </summary>
        public double? InStockPercentageTrue { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at minimum
        /// </summary>
        public DateTime? CreatedAtMinimum { get; set; }
    }

    /// <summary>
    /// Type with multiple suffixes for the same field.
    /// </summary>
    private class MultipleSuffixesPerField
    {
        /// <summary>
        /// Gets or sets the value of the price mean
        /// </summary>
        public double? PriceMean { get; set; }

        /// <summary>
        /// Gets or sets the value of the price min
        /// </summary>
        public double? PriceMin { get; set; }

        /// <summary>
        /// Gets or sets the value of the price max
        /// </summary>
        public double? PriceMax { get; set; }

        /// <summary>
        /// Gets or sets the value of the price count
        /// </summary>
        public long? PriceCount { get; set; }
    }

    /// <summary>
    /// Type with mixed aggregate types and suffixes.
    /// </summary>
    private class MixedMetrics
    {
        /// <summary>
        /// Gets or sets the value of the title
        /// </summary>
        public Aggregate.Text? Title { get; set; }

        /// <summary>
        /// Gets or sets the value of the price mean
        /// </summary>
        public double? PriceMean { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity sum
        /// </summary>
        public long? QuantitySum { get; set; }
    }

    /// <summary>
    /// Tests that from type extracts full aggregate types
    /// </summary>
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

    /// <summary>
    /// Tests that from type extracts suffix based metrics
    /// </summary>
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

    /// <summary>
    /// Tests that from type combines multiple suffixes for same field
    /// </summary>
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

    /// <summary>
    /// Tests that from type handles mixed types
    /// </summary>
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

    /// <summary>
    /// Tests that from type ignores properties without suffix
    /// </summary>
    [Fact]
    public void FromType_IgnoresPropertiesWithoutSuffix()
    {
        var metrics = MetricsExtractor.FromType<PropertiesWithoutSuffix>();

        // Only properties with recognized suffixes or Aggregate types should be extracted
        Assert.Empty(metrics);
    }

    /// <summary>
    /// The properties without suffix class
    /// </summary>
    private class PropertiesWithoutSuffix
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public int? Price { get; set; } // No suffix

        /// <summary>
        /// Gets or sets the value of the rating
        /// </summary>
        public double? Rating { get; set; } // No suffix
    }

    /// <summary>
    /// Tests that from type handles text suffixes
    /// </summary>
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

    /// <summary>
    /// The text suffix metrics class
    /// </summary>
    private class TextSuffixMetrics
    {
        /// <summary>
        /// Gets or sets the value of the title top occurrence
        /// </summary>
        public string? TitleTopOccurrence { get; set; }
    }

    /// <summary>
    /// Tests that from type handles boolean suffixes
    /// </summary>
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

    /// <summary>
    /// The boolean suffix metrics class
    /// </summary>
    private class BooleanSuffixMetrics
    {
        /// <summary>
        /// Gets or sets the value of the active total true
        /// </summary>
        public long? ActiveTotalTrue { get; set; }

        /// <summary>
        /// Gets or sets the value of the active total false
        /// </summary>
        public long? ActiveTotalFalse { get; set; }

        /// <summary>
        /// Gets or sets the value of the active percentage true
        /// </summary>
        public double? ActivePercentageTrue { get; set; }

        /// <summary>
        /// Gets or sets the value of the active percentage false
        /// </summary>
        public double? ActivePercentageFalse { get; set; }
    }

    /// <summary>
    /// Tests that from type non generic overload works
    /// </summary>
    [Fact]
    public void FromType_NonGenericOverload_Works()
    {
        var metrics = MetricsExtractor.FromType(typeof(SuffixBasedMetrics));

        Assert.Equal(5, metrics.Length);
    }

    #endregion

    #region MetricsExtractor Attribute Tests

    /// <summary>
    /// The selective metrics class
    /// </summary>
    private class SelectiveMetrics
    {
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        [NumberMetrics(Minimum = true, Maximum = true, Mean = true)]
        public Aggregate.Number? Price { get; set; }

        /// <summary>
        /// Gets or sets the value of the category
        /// </summary>
        [TextMetrics(Count = true, TopOccurrences = true, MinOccurrences = 5)]
        public Aggregate.Text? Category { get; set; }

        /// <summary>
        /// Gets or sets the value of the in stock
        /// </summary>
        [BooleanMetrics(TotalTrue = true, PercentageTrue = true)]
        public Aggregate.Boolean? InStock { get; set; }
    }

    /// <summary>
    /// Tests that from type with attributes enables only specified metrics
    /// </summary>
    [Fact]
    public void FromType_WithAttributes_EnablesOnlySpecifiedMetrics()
    {
        var metrics = MetricsExtractor.FromType<SelectiveMetrics>();

        Assert.Equal(3, metrics.Length);

        // Price: Only Min, Max, Mean
        var price = metrics.OfType<Aggregate.Metric.Number>().First(m => m.Name == "price");
        Assert.True(price.Minimum);
        Assert.True(price.Maximum);
        Assert.True(price.Mean);
        Assert.False(price.Sum);
        Assert.False(price.Count);
        Assert.False(price.Median);

        // Category: Count + TopOccurrences with MinOccurrences
        var category = metrics.OfType<Aggregate.Metric.Text>().First(m => m.Name == "category");
        Assert.True(category.Count);
        Assert.True(category.TopOccurrencesCount);
        Assert.True(category.TopOccurrencesValue);
        Assert.Equal(5u, category.MinOccurrences);

        // InStock: Only TotalTrue, PercentageTrue
        var inStock = metrics.OfType<Aggregate.Metric.Boolean>().First(m => m.Name == "inStock");
        Assert.True(inStock.TotalTrue);
        Assert.True(inStock.PercentageTrue);
        Assert.False(inStock.TotalFalse);
        Assert.False(inStock.PercentageFalse);
        Assert.False(inStock.Count);
    }

    /// <summary>
    /// Tests that from type no attribute enables all
    /// </summary>
    [Fact]
    public void FromType_NoAttribute_EnablesAll()
    {
        var metrics = MetricsExtractor.FromType<NoAttributeMetrics>();

        var quantity = metrics.OfType<Aggregate.Metric.Integer>().First(m => m.Name == "quantity");
        Assert.True(quantity.Count);
        Assert.True(quantity.Sum);
        Assert.True(quantity.Mean);
        Assert.True(quantity.Minimum);
        Assert.True(quantity.Maximum);
        Assert.True(quantity.Median);
        Assert.True(quantity.Mode);
    }

    /// <summary>
    /// The no attribute metrics class
    /// </summary>
    private class NoAttributeMetrics
    {
        /// <summary>
        /// Gets or sets the value of the quantity
        /// </summary>
        public Aggregate.Integer? Quantity { get; set; }
    }

    /// <summary>
    /// Tests that from type mixed attributes works correctly
    /// </summary>
    [Fact]
    public void FromType_MixedAttributes_WorksCorrectly()
    {
        var metrics = MetricsExtractor.FromType<MixedAttributes>();

        Assert.Equal(2, metrics.Length);

        // Price has attribute - only specified metrics
        var price = metrics.OfType<Aggregate.Metric.Number>().First(m => m.Name == "price");
        Assert.True(price.Minimum);
        Assert.True(price.Maximum);
        Assert.False(price.Mean);
        Assert.False(price.Sum);

        // Quantity has no attribute - all metrics
        var quantity = metrics.OfType<Aggregate.Metric.Integer>().First(m => m.Name == "quantity");
        Assert.True(quantity.Count);
        Assert.True(quantity.Sum);
        Assert.True(quantity.Mean);
    }

    /// <summary>
    /// The mixed attributes class
    /// </summary>
    private class MixedAttributes
    {
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        [NumberMetrics(Minimum = true, Maximum = true)]
        public Aggregate.Number? Price { get; set; }

        /// <summary>
        /// Gets or sets the value of the quantity
        /// </summary>
        public Aggregate.Integer? Quantity { get; set; }
    }

    /// <summary>
    /// Tests that from type empty attribute enables all
    /// </summary>
    [Fact]
    public void FromType_EmptyAttribute_EnablesAll()
    {
        var metrics = MetricsExtractor.FromType<EmptyAttributeMetrics>();

        var price = metrics.OfType<Aggregate.Metric.Number>().First(m => m.Name == "price");

        // Empty attribute with no true values - enables all
        Assert.True(price.Count);
        Assert.True(price.Sum);
        Assert.True(price.Mean);
        Assert.True(price.Minimum);
        Assert.True(price.Maximum);
        Assert.True(price.Median);
        Assert.True(price.Mode);
    }

    /// <summary>
    /// The empty attribute metrics class
    /// </summary>
    private class EmptyAttributeMetrics
    {
        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        [NumberMetrics] // All properties default to false - enables all
        public Aggregate.Number? Price { get; set; }
    }

    /// <summary>
    /// Tests that from type integer metrics attribute enables only specified metrics
    /// </summary>
    [Fact]
    public void FromType_IntegerMetricsAttribute_EnablesOnlySpecifiedMetrics()
    {
        var metrics = MetricsExtractor.FromType<IntegerAttributeMetrics>();

        var quantity = metrics.OfType<Aggregate.Metric.Integer>().First(m => m.Name == "quantity");
        Assert.True(quantity.Sum);
        Assert.True(quantity.Count);
        Assert.False(quantity.Mean);
        Assert.False(quantity.Minimum);
        Assert.False(quantity.Maximum);
    }

    /// <summary>
    /// The integer attribute metrics class
    /// </summary>
    private class IntegerAttributeMetrics
    {
        /// <summary>
        /// Gets or sets the value of the quantity
        /// </summary>
        [IntegerMetrics(Sum = true, Count = true)]
        public Aggregate.Integer? Quantity { get; set; }
    }

    /// <summary>
    /// Tests that from type date metrics attribute enables only specified metrics
    /// </summary>
    [Fact]
    public void FromType_DateMetricsAttribute_EnablesOnlySpecifiedMetrics()
    {
        var metrics = MetricsExtractor.FromType<DateAttributeMetrics>();

        var createdAt = metrics.OfType<Aggregate.Metric.Date>().First(m => m.Name == "createdAt");
        Assert.True(createdAt.Minimum);
        Assert.True(createdAt.Maximum);
        Assert.False(createdAt.Median);
        Assert.False(createdAt.Mode);
        Assert.False(createdAt.Count);
    }

    /// <summary>
    /// The date attribute metrics class
    /// </summary>
    private class DateAttributeMetrics
    {
        /// <summary>
        /// Gets or sets the value of the created at
        /// </summary>
        [DateMetrics(Minimum = true, Maximum = true)]
        public Aggregate.Date? CreatedAt { get; set; }
    }

    #endregion
}
