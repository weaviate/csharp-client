using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// Integration tests for the Boost query parameter (soft-ranking, Weaviate 1.38+ Preview).
/// </summary>
public partial class SearchTests
{
    /// <summary>
    /// The boost test document class
    /// </summary>
    private class BoostDoc
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the label
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the category
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value of the view count
        /// </summary>
        public int ViewCount { get; set; }

        /// <summary>
        /// Gets or sets the value of the price
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets the value of the published at
        /// </summary>
        public DateTime PublishedAt { get; set; }
    }

    /// <summary>
    /// Creates a boost test collection with three documents that tie on BM25 relevance
    /// </summary>
    /// <returns>The collection client</returns>
    private async Task<CollectionClient> BoostCollectionFactory()
    {
        var collection = await CollectionFactory<BoostDoc>(
            properties: Property.FromClass<BoostDoc>(),
            vectorConfig: Configure.Vector(v => v.SelfProvided())
        );

        // All docs share the same name so BM25 scores tie and the boost alone decides the order.
        var now = DateTime.UtcNow;
        await collection.Data.InsertMany(
            new[]
            {
                new BoostDoc
                {
                    Name = "banana",
                    Label = "A",
                    Category = "promoted",
                    ViewCount = 5,
                    Price = 100,
                    PublishedAt = now.AddDays(-400),
                },
                new BoostDoc
                {
                    Name = "banana",
                    Label = "B",
                    Category = "regular",
                    ViewCount = 500,
                    Price = 50,
                    PublishedAt = now.AddHours(-1),
                },
                new BoostDoc
                {
                    Name = "banana",
                    Label = "C",
                    Category = "regular",
                    ViewCount = 50,
                    Price = 60,
                    PublishedAt = now.AddDays(-60),
                },
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        return collection;
    }

    /// <summary>
    /// Extracts the label ordering from a search result
    /// </summary>
    /// <param name="result">The result</param>
    /// <returns>The labels in result order</returns>
    private static List<string> Labels(WeaviateResult result) =>
        result.Objects.Select(o => o.As<BoostDoc>()!.Label).ToList();

    /// <summary>
    /// Tests that a filter boost promotes matching objects without excluding the rest
    /// </summary>
    [Fact]
    public async Task Test_Boost_Filter_PromotesMatchingObjects()
    {
        RequireVersion("1.38.0", message: "Boost API requires Weaviate >= 1.38.0 (Preview)");

        var collection = await BoostCollectionFactory();

        var result = await collection.Query.BM25(
            "banana",
            searchFields: ["name"],
            boost: Boost.Filter(Filter.Property("category").IsEqual("promoted"), weight: 0.9f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        var labels = Labels(result);
        Assert.Equal(3, labels.Count);
        Assert.Equal("A", labels[0]);
    }

    /// <summary>
    /// Tests that a numeric property boost ranks higher values first
    /// </summary>
    [Fact]
    public async Task Test_Boost_NumericProperty_RanksByValue()
    {
        RequireVersion("1.38.0", message: "Boost API requires Weaviate >= 1.38.0 (Preview)");

        var collection = await BoostCollectionFactory();

        var result = await collection.Query.BM25(
            "banana",
            searchFields: ["name"],
            boost: Boost.NumericProperty("viewCount", weight: 0.9f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(["B", "C", "A"], Labels(result));
    }

    /// <summary>
    /// Tests that a time decay boost prefers recent objects
    /// </summary>
    [Fact]
    public async Task Test_Boost_TimeDecay_PrefersRecentObjects()
    {
        RequireVersion("1.38.0", message: "Boost API requires Weaviate >= 1.38.0 (Preview)");

        var collection = await BoostCollectionFactory();

        var result = await collection.Query.BM25(
            "banana",
            searchFields: ["name"],
            boost: Boost.TimeDecay("publishedAt", scale: TimeSpan.FromDays(30), weight: 0.9f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(["B", "C", "A"], Labels(result));
    }

    /// <summary>
    /// Tests that a numeric decay boost prefers values closest to the origin on a near-vector search
    /// </summary>
    [Fact]
    public async Task Test_Boost_NumericDecay_OnNearVector_PrefersClosestToOrigin()
    {
        RequireVersion("1.38.0", message: "Boost API requires Weaviate >= 1.38.0 (Preview)");

        var collection = await CollectionFactory<BoostDoc>(
            properties: Property.FromClass<BoostDoc>(),
            vectorConfig: Configure.Vector(v => v.SelfProvided())
        );

        // Identical vectors so the primary vector search ties and the boost alone decides the order.
        var vector = new[] { 1f, 0f, 0f };
        foreach (
            var doc in new[]
            {
                new BoostDoc { Label = "A", Price = 100 },
                new BoostDoc { Label = "B", Price = 50 },
                new BoostDoc { Label = "C", Price = 60 },
            }
        )
        {
            await collection.Data.Insert(
                doc,
                vectors: vector,
                cancellationToken: TestContext.Current.CancellationToken
            );
        }

        var result = await collection.Query.NearVector(
            vector,
            boost: Boost.NumericDecay("price", origin: 50, scale: 10, weight: 0.9f),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(["B", "C", "A"], Labels(result));
    }

    /// <summary>
    /// Tests that a blended boost weighs its conditions against each other
    /// </summary>
    [Fact]
    public async Task Test_Boost_Blend_WeighsConditions()
    {
        RequireVersion("1.38.0", message: "Boost API requires Weaviate >= 1.38.0 (Preview)");

        var collection = await BoostCollectionFactory();

        // The promoted-filter condition carries twice the weight of the view-count condition,
        // so A (promoted, 5 views) must outrank B (regular, 500 views).
        var result = await collection.Query.BM25(
            "banana",
            searchFields: ["name"],
            boost: Boost.Blend(
                [
                    Boost.Filter(Filter.Property("category").IsEqual("promoted"), weight: 2f),
                    Boost.NumericProperty("viewCount", weight: 1f),
                ],
                weight: 0.9f
            ),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(["A", "B", "C"], Labels(result));
    }
}
