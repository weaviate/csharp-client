using System.Dynamic;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

public record TestProperties
{
    public string? TestText { get; set; }
    public string[]? TestTextArray { get; set; }
    public int? TestInt { get; set; }
    public int[]? TestIntArray { get; set; }
    public bool? TestBool { get; set; }
    public bool[]? TestBoolArray { get; set; }
    public double? TestNumber { get; set; }
    public double[]? TestNumberArray { get; set; }
    public DateTime? TestDate { get; set; }
    public DateTime[]? TestDateArray { get; set; }
    public Guid? TestUuid { get; set; }
    public Guid[]? TestUuidArray { get; set; }
    public GeoCoordinate? TestGeo { get; set; }

    // // public byte[]? TestBlob { get; set; }
    public PhoneNumber? TestPhone { get; set; }
    // // public object? TestObject { get; set; }
    // // public object? TestObjectArray { get; set; }
}

[Collection("Unit Tests")]
public partial class UnitTests
{
    [Fact]
    public void NamedVectorInitialization()
    {
        // Arrange
        var v1 = new NamedVectors { { "default", 0.1f, 0.2f, 0.3f } };

        Assert.Equal(v1["default"], [0.1f, 0.2f, 0.3f]);
    }

    [Fact]
    public void MetadataQueryImplicitConversion()
    {
        // Arrange
        var vectors = new string[] { "default" };
        var options = MetadataOptions.Vector;

        // Act
        MetadataQuery q1 = vectors;
        MetadataQuery q2 = options;
        MetadataQuery q3 = (options, vectors);

        // Assert
        Assert.Equal(q1.Vectors, vectors);
        Assert.Equal(q2.Options, options);
        Assert.Equal(q3.Options, options);
        Assert.Equal(q3.Vectors, vectors);
    }

    [Fact]
    public void TestBuildDynamicObjectPropertiesGeoCoordinate()
    {
        var geo = new { TestingPropertyType = new GeoCoordinate(12.345f, 67.890f) };
        var props = ObjectHelper.BuildDataTransferObject(geo);

        dynamic? concrete = ObjectHelper.UnmarshallProperties<ExpandoObject>(props);

        Assert.NotNull(concrete);
        Assert.Equal(geo.TestingPropertyType.Latitude, concrete!.TestingPropertyType.Latitude);
        Assert.Equal(geo.TestingPropertyType.Longitude, concrete!.TestingPropertyType.Longitude);
    }

    [Fact]
    public void TestBuildDynamicObject()
    {
        // Arrange
        var review = new[]
        {
            new
            {
                author_username = "kineticandroid",
                content = @"Take the story of Frankenstein's monster.",
                rating = (double?)null,
                movie_id = 162,
                review_id = 162,
            },
            new
            {
                author_username = "r96sk",
                content = @"Very enjoyable.",
                rating = (double?)8.0,
                movie_id = 162,
                review_id = 162,
            },
        };

        // Act
        var obj = review.Select(r => ObjectHelper.BuildDataTransferObject(r)).ToList();

        // Assert
        Assert.Equal("kineticandroid", obj[0]["author_username"]);
        Assert.Equal(review[0].content, obj[0]["content"]);
        Assert.False(obj[0].ContainsKey("rating"));
        Assert.Equal(162, obj[0]["movie_id"]);
        Assert.Equal(162, obj[0]["review_id"]);

        Assert.Equal("r96sk", obj[1]["author_username"]);
        Assert.Equal(review[1].content, obj[1]["content"]);
        Assert.Equal(8.0, obj[1]["rating"]);
        Assert.Equal(162, obj[1]["movie_id"]);
        Assert.Equal(162, obj[1]["review_id"]);
    }

    [Theory]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(bool[]), true)]
    [InlineData(typeof(char), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(short), true)]
    [InlineData(typeof(ushort), true)]
    [InlineData(typeof(int), true)]
    [InlineData(typeof(uint), true)]
    [InlineData(typeof(long), true)]
    [InlineData(typeof(ulong), true)]
    [InlineData(typeof(float), true)]
    [InlineData(typeof(double), true)]
    [InlineData(typeof(double?), true)]
    [InlineData(typeof(decimal), true)]
    [InlineData(typeof(short[]), true)]
    [InlineData(typeof(ushort[]), true)]
    [InlineData(typeof(int[]), true)]
    [InlineData(typeof(uint[]), true)]
    [InlineData(typeof(long[]), true)]
    [InlineData(typeof(ulong[]), true)]
    [InlineData(typeof(float[]), true)]
    [InlineData(typeof(double[]), true)]
    [InlineData(typeof(decimal[]), true)]
    [InlineData(typeof(string), true)]
    [InlineData(typeof(string[]), true)]
    [InlineData(typeof(DateTime), true)]
    [InlineData(typeof(GeoCoordinate), true)]
    [InlineData(typeof(Object), false)]
    [InlineData(typeof(Object[]), false)]
    [InlineData(typeof(WeaviateObject), false)]
    public void Test_IsNativeType_Check(Type type, bool expected)
    {
        // Arrange

        // Act
        var result = type.IsNativeType();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Test_BatchProperties_Build()
    {
        var p = new TestProperties
        {
            TestText = "dummyText",
            TestTextArray = new[] { "dummyTextArray1", "dummyTextArray2" },
            TestInt = 345,
            TestIntArray = new[] { 3, 4, 6 },
            TestBool = true,
            TestNumber = 567.897,
            TestNumberArray = new[] { 6.7, 8.9 },
            TestDate = DateTime.Now.AddDays(+1),
            TestDateArray = new[] { DateTime.Now.AddDays(+2), DateTime.Now.AddDays(+3) },
            TestUuid = Guid.NewGuid(),
            TestUuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
            TestGeo = new GeoCoordinate(34.567f, 98.765f),
        };

        var bp = ObjectHelper.BuildBatchProperties(p);

        // Verify all expected properties are present
        Assert.Contains("TestText", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("TestInt", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("TestBool", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("TestNumber", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("TestDate", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("TestUuid", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("TestGeo", bp.NonRefProperties.Fields.Keys);

        // String properties
        Assert.Equal(p.TestText, bp.NonRefProperties.Fields["TestText"].StringValue);
        Assert.Equal(
            p.TestTextArray.Length,
            bp.TextArrayProperties.Where(p => p.PropName == "TestTextArray").First().Values.Count
        );
        Assert.Equal(
            p.TestTextArray[0],
            bp.TextArrayProperties.Where(p => p.PropName == "TestTextArray").First().Values[0]
        );
        Assert.Equal(
            p.TestTextArray[1],
            bp.TextArrayProperties.Where(p => p.PropName == "TestTextArray").First().Values[1]
        );

        // Integer properties
        Assert.Equal(
            Convert.ToDouble(p.TestInt),
            bp.NonRefProperties.Fields["TestInt"].NumberValue
        );
        Assert.Equal(
            p.TestIntArray.Length,
            bp.IntArrayProperties.Where(p => p.PropName == "TestIntArray").First().Values.Count
        );
        Assert.Equal(
            p.TestIntArray[0],
            bp.IntArrayProperties.Where(p => p.PropName == "TestIntArray").First().Values[0]
        );
        Assert.Equal(
            p.TestIntArray[1],
            bp.IntArrayProperties.Where(p => p.PropName == "TestIntArray").First().Values[1]
        );
        Assert.Equal(
            p.TestIntArray[2],
            bp.IntArrayProperties.Where(p => p.PropName == "TestIntArray").First().Values[2]
        );

        // Boolean property
        Assert.Equal(p.TestBool, bp.NonRefProperties.Fields["TestBool"].BoolValue);

        // Number properties
        Assert.Equal(p.TestNumber, bp.NonRefProperties.Fields["TestNumber"].NumberValue);

        Assert.NotEmpty(bp.NumberArrayProperties[0].ValuesBytes);

        // Additional check for number array field name
        Assert.Equal("TestNumberArray", bp.NumberArrayProperties[0].PropName);

        // Date properties
        Assert.Equal(
            p.TestDate.Value.ToUniversalTime().ToString("o"),
            bp.NonRefProperties.Fields["TestDate"].StringValue
        );
        var dateList = bp.TextArrayProperties.Single(p => p.PropName == "TestDateArray");
        Assert.Equal(p.TestDateArray.Length, dateList.Values.Count);
        Assert.Equal(p.TestDateArray[0].ToUniversalTime().ToString("o"), dateList.Values[0]);
        Assert.Equal(p.TestDateArray[1].ToUniversalTime().ToString("o"), dateList.Values[1]);

        // UUID properties
        Assert.Equal(p.TestUuid.ToString(), bp.NonRefProperties.Fields["TestUuid"].StringValue);
        var uuidList = bp.TextArrayProperties.Single(p => p.PropName == "TestUuidArray");
        Assert.Equal(p.TestUuidArray.Length, uuidList.Values.Count);
        Assert.Equal(p.TestUuidArray[0].ToString(), uuidList.Values[0]);
        Assert.Equal(p.TestUuidArray[1].ToString(), uuidList.Values[1]);

        // Geo property
        Assert.Equal(
            p.TestGeo.Latitude,
            bp.NonRefProperties.Fields["TestGeo"].StructValue.Fields["latitude"].NumberValue
        );
        Assert.Equal(
            p.TestGeo.Longitude,
            bp.NonRefProperties.Fields["TestGeo"].StructValue.Fields["longitude"].NumberValue
        );
    }
}
