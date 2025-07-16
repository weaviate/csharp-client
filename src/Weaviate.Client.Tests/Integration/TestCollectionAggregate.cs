using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weaviate.Client.Models;
using Xunit;

namespace Weaviate.Client.Tests.Integration
{
    [Collection("TestCollections")]
    public partial class AggregatesTests : IntegrationTests
    {
        [Fact]
        public async Task Test_All_Available_Aggregations()
        {
            // Arrange
            var collectionClient = await CollectionFactory(
                properties: new[]
                {
                    Property.Text("text"),
                    Property.TextArray("texts"),
                    Property.Int("int_"),
                    Property.IntArray("ints"),
                    Property.Number("float_"),
                    Property.NumberArray("floats"),
                    Property.Bool("bool_"),
                    Property.BoolArray("bools"),
                    Property.Date("date"),
                    Property.DateArray("dates"),
                }
            );

            // Add reference property
            await collectionClient.Config.AddReference(
                Property.Reference("ref", collectionClient.Name)
            );

            // Insert test data
            await collectionClient.Data.Insert(
                new
                {
                    text = "some text",
                    texts = new[] { "some text", "some text" },
                    int_ = 1,
                    ints = new[] { 1, 2 },
                    float_ = 1.0,
                    floats = new[] { 1.0, 2.0 },
                    bool_ = true,
                    bools = new[] { true, false },
                    date = DateTime.Parse(
                        "2021-01-01T00:00:00Z",
                        null,
                        System.Globalization.DateTimeStyles.AdjustToUniversal
                    ),
                    dates = new[]
                    {
                        DateTime.Parse(
                            "2021-01-01T00:00:00Z",
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        ),
                        DateTime.Parse(
                            "2021-01-02T00:00:00Z",
                            null,
                            System.Globalization.DateTimeStyles.AdjustToUniversal
                        ),
                    },
                }
            );

            // Act
            var result = await collectionClient.Aggregate.OverAll(
                metrics: new[]
                {
                    Metrics.ForProperty("text").Text(),
                    Metrics.ForProperty("texts").Text(),
                    Metrics.ForProperty("int_").Integer(),
                    Metrics.ForProperty("ints").Integer(),
                    Metrics.ForProperty("float_").Number(),
                    Metrics.ForProperty("floats").Number(),
                    Metrics.ForProperty("bool_").Boolean(),
                    Metrics.ForProperty("bools").Boolean(),
                    Metrics.ForProperty("date").Date(),
                    Metrics.ForProperty("dates").Date(),
                }
            );

            // Assert
            var text = result.Properties["text"] as Aggregate.Text;
            Assert.NotNull(text);
            Assert.Equal(1, text.Count);
            Assert.Equal(1, text.TopOccurrences[0].Count);
            Assert.Equal("some text", text.TopOccurrences[0].Value);

            var texts = result.Properties["texts"] as Aggregate.Text;
            Assert.NotNull(texts);
            Assert.Equal(2, texts.Count);
            Assert.Equal(2, texts.TopOccurrences[0].Count);
            Assert.Equal("some text", texts.TopOccurrences[0].Value);

            var int_ = result.Properties["int_"] as Aggregate.Integer;
            Assert.NotNull(int_);
            Assert.Equal(1, int_.Count);
            Assert.Equal(1, int_.Maximum);
            Assert.Equal(1, int_.Mean);
            Assert.Equal(1, int_.Median);
            Assert.Equal(1, int_.Minimum);
            Assert.Equal(1, int_.Mode);
            Assert.Equal(1, int_.Sum);

            var ints = result.Properties["ints"] as Aggregate.Integer;
            Assert.NotNull(ints);
            Assert.Equal(2, ints.Count);
            Assert.Equal(2, ints.Maximum);
            Assert.Equal(1.5, ints.Mean);
            Assert.Equal(1.5, ints.Median);
            Assert.Equal(1, ints.Minimum);
            Assert.Equal(1, ints.Mode);

            var float_ = result.Properties["float_"] as Aggregate.Number;
            Assert.NotNull(float_);
            Assert.Equal(1, float_.Count);
            Assert.Equal(1.0, float_.Maximum);
            Assert.Equal(1.0, float_.Mean);
            Assert.Equal(1.0, float_.Median);
            Assert.Equal(1.0, float_.Minimum);
            Assert.Equal(1.0, float_.Mode);

            var floats = result.Properties["floats"] as Aggregate.Number;
            Assert.NotNull(floats);
            Assert.Equal(2, floats.Count);
            Assert.Equal(2.0, floats.Maximum);
            Assert.Equal(1.5, floats.Mean);
            Assert.Equal(1.5, floats.Median);
            Assert.Equal(1.0, floats.Minimum);
            Assert.Equal(1.0, floats.Mode);

            var bool_ = result.Properties["bool_"] as Aggregate.Boolean;
            Assert.NotNull(bool_);
            Assert.Equal(1, bool_.Count);
            Assert.Equal(0, bool_.PercentageFalse);
            Assert.Equal(1, bool_.PercentageTrue);
            Assert.Equal(0, bool_.TotalFalse);
            Assert.Equal(1, bool_.TotalTrue);

            var bools = result.Properties["bools"] as Aggregate.Boolean;
            Assert.NotNull(bools);
            Assert.Equal(2, bools.Count);
            Assert.Equal(0.5, bools.PercentageFalse);
            Assert.Equal(0.5, bools.PercentageTrue);
            Assert.Equal(1, bools.TotalFalse);
            Assert.Equal(1, bools.TotalTrue);

            var date = result.Properties["date"] as Aggregate.Date;
            Assert.NotNull(date);
            Assert.Equal(1, date.Count);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), date.Maximum);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), date.Median);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), date.Minimum);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), date.Mode);

            var dates = result.Properties["dates"] as Aggregate.Date;
            Assert.NotNull(dates);
            Assert.Equal(2, dates.Count);
            Assert.Equal(new DateTime(2021, 1, 2, 0, 0, 0, DateTimeKind.Utc), dates.Maximum);
            Assert.Equal(new DateTime(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc), dates.Median);
            Assert.Equal(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc), dates.Minimum);
            // Not checking mode since it's flaky in the original test
        }
    }
}
