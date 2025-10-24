using System.Dynamic;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public partial class ObjectHelperTests
{
    [Fact]
    public void TestBuildDataTransferObject_NullInput_ReturnsEmptyDictionary()
    {
        // Arrange
        object? data = null;

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void TestBuildDataTransferObject_SimpleProperties_MapsCorrectly()
    {
        // Arrange
        var data = new
        {
            Name = "John Doe",
            Age = 30,
            IsActive = true,
            Score = 95.5,
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Equal("John Doe", result["Name"]);
        Assert.Equal(30, result["Age"]);
        Assert.Equal(true, result["IsActive"]);
        Assert.Equal(95.5, result["Score"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_NullProperties_AreSkipped()
    {
        // Arrange
        var data = new
        {
            Name = "Jane",
            Email = (string?)null,
            Age = 25,
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Equal("Jane", result["Name"]);
        Assert.Equal(25, result["Age"]);
        Assert.False(result.ContainsKey("Email"));
    }

    [Fact]
    public void TestBuildDataTransferObject_DateTime_ConvertsToIso8601()
    {
        // Arrange
        var dateTime = new DateTime(2025, 10, 24, 15, 30, 0, DateTimeKind.Utc);
        var data = new { CreatedAt = dateTime };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Equal("2025-10-24T15:30:00.0000000Z", result["CreatedAt"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_DateTime_LocalTime_ConvertsToUtc()
    {
        // Arrange
        var localDateTime = new DateTime(2025, 10, 24, 15, 30, 0, DateTimeKind.Local);
        var data = new { CreatedAt = localDateTime };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        var expectedUtc = localDateTime.ToUniversalTime().ToString("o");
        Assert.Equal(expectedUtc, result["CreatedAt"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_GeoCoordinate_ConvertsToGeoCoordinates()
    {
        // Arrange
        var geo = new GeoCoordinate(52.5200f, 13.4050f);
        var data = new { Location = geo };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.NotNull(result["Location"]);
        dynamic geoResult = result["Location"]!;
        Assert.Equal(52.5200f, geoResult.Latitude);
        Assert.Equal(13.4050f, geoResult.Longitude);
    }

    [Fact]
    public void TestBuildDataTransferObject_NestedObject_RecursivelyProcessed()
    {
        // Arrange
        var data = new
        {
            Name = "Company",
            Address = new
            {
                Street = "Main St",
                Number = 123,
                City = "Berlin",
            },
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Equal("Company", result["Name"]);
        Assert.NotNull(result["Address"]);

        var address = result["Address"] as IDictionary<string, object?>;
        Assert.NotNull(address);
        Assert.Equal("Main St", address["Street"]);
        Assert.Equal(123, address["Number"]);
        Assert.Equal("Berlin", address["City"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_DeeplyNestedObject_RecursivelyProcessed()
    {
        // Arrange
        var data = new
        {
            Level1 = new { Level2 = new { Level3 = new { Value = "Deep Value", Count = 42 } } },
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        var level1 = result["Level1"] as IDictionary<string, object?>;
        Assert.NotNull(level1);

        var level2 = level1["Level2"] as IDictionary<string, object?>;
        Assert.NotNull(level2);

        var level3 = level2["Level3"] as IDictionary<string, object?>;
        Assert.NotNull(level3);
        Assert.Equal("Deep Value", level3["Value"]);
        Assert.Equal(42, level3["Count"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_MixedTypes_AllHandledCorrectly()
    {
        // Arrange
        var data = new
        {
            TextValue = "Hello",
            IntValue = 42,
            LongValue = 9876543210L,
            FloatValue = 3.14f,
            DoubleValue = 2.718281828,
            BoolValue = true,
            DateValue = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            GeoValue = new GeoCoordinate(40.7128f, -74.0060f),
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Equal("Hello", result["TextValue"]);
        Assert.Equal(42, result["IntValue"]);
        Assert.Equal(9876543210L, result["LongValue"]);
        Assert.Equal(3.14f, result["FloatValue"]);
        Assert.Equal(2.718281828, result["DoubleValue"]);
        Assert.Equal(true, result["BoolValue"]);
        Assert.Equal("2025-01-01T00:00:00.0000000Z", result["DateValue"]);

        Assert.NotNull(result["GeoValue"]);
        dynamic geoResult = result["GeoValue"]!;
        Assert.Equal(40.7128f, geoResult.Latitude);
        Assert.Equal(-74.0060f, geoResult.Longitude);
    }

    [Fact]
    public void TestBuildDataTransferObject_ArrayProperties_AreIncluded()
    {
        // Arrange
        var data = new { Tags = new[] { "tag1", "tag2", "tag3" }, Numbers = new[] { 1, 2, 3 } };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.NotNull(result["Tags"]);
        Assert.NotNull(result["Numbers"]);

        var tags = result["Tags"] as string[];
        Assert.NotNull(tags);
        Assert.Equal(3, tags.Length);
        Assert.Equal("tag1", tags[0]);

        var numbers = result["Numbers"] as int[];
        Assert.NotNull(numbers);
        Assert.Equal(3, numbers.Length);
        Assert.Equal(1, numbers[0]);
    }

    [Fact]
    public void TestBuildDataTransferObject_ComplexScenario_WithMultipleFeatures()
    {
        // Arrange
        var data = new
        {
            Id = Guid.NewGuid(),
            Name = "Test Product",
            Price = 99.99,
            InStock = true,
            CreatedAt = DateTime.UtcNow,
            Location = new GeoCoordinate(51.5074f, -0.1278f),
            Details = new
            {
                Brand = "TestBrand",
                Model = "X1000",
                Year = 2025,
            },
            Tags = new[] { "electronics", "featured" },
            NullField = (string?)null,
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Equal(data.Id, result["Id"]);
        Assert.Equal(data.Name, result["Name"]);
        Assert.Equal(data.Price, result["Price"]);
        Assert.Equal(data.InStock, result["InStock"]);
        Assert.NotNull(result["CreatedAt"]);
        Assert.NotNull(result["Location"]);
        Assert.NotNull(result["Details"]);
        Assert.NotNull(result["Tags"]);
        Assert.False(result.ContainsKey("NullField"));

        var details = result["Details"] as IDictionary<string, object?>;
        Assert.NotNull(details);
        Assert.Equal("TestBrand", details["Brand"]);
        Assert.Equal("X1000", details["Model"]);
        Assert.Equal(2025, details["Year"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_WriteOnlyProperty_IsSkipped()
    {
        // Arrange
        var data = new TestClassWithWriteOnly { ReadableProperty = "Readable" };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.Single(result);
        Assert.Equal("Readable", result["ReadableProperty"]);
        Assert.False(result.ContainsKey("WriteOnlyProperty"));
    }

    [Fact]
    public void TestBuildDataTransferObject_EmptyObject_ReturnsEmptyDictionary()
    {
        // Arrange
        var data = new { };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private class TestClassWithWriteOnly
    {
        public string ReadableProperty { get; set; } = string.Empty;

        private string _writeOnly = string.Empty;
        public string WriteOnlyProperty
        {
            set => _writeOnly = value;
        }
    }
}
