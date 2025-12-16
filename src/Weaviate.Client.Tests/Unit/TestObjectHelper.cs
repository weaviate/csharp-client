using System.Dynamic;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;

namespace Weaviate.Client.Tests.Unit;

public partial class ObjectHelperTests
{
    [Fact]
    public void Test_BuildBatchProperties_WithBlob()
    {
        var testData = new TestProperties
        {
            TestBlob = System.Text.Encoding.UTF8.GetBytes("Weaviate"),
        };

        var bp = ObjectHelper.BuildBatchProperties(testData);

        Assert.True(bp.NonRefProperties.Fields.ContainsKey("testBlob"));
        Assert.Equal(
            bp.NonRefProperties.Fields["testBlob"].StringValue,
            Convert.ToBase64String(testData.TestBlob)
        );
    }

    [Fact]
    public void TestNestedRecursiveObject()
    {
        var testData = new TestProperties
        {
            TestText = "dummyText",
            TestTextArray = new[] { "dummyTextArray1", "dummyTextArray2" },
            TestInt = 123,
            TestIntArray = new[] { 1, 2, 3 },
            TestBlob = System.Text.Encoding.UTF8.GetBytes("Weaviate"),
            TestBool = true,
            TestBoolArray = new[] { true, false },
            TestNumber = 456.789,
            TestNumberArray = new[] { 4.5, 6.7 },
            TestDate = DateTime.Now.AddDays(-1).ToUniversalTime(),
            TestDateArray = new[]
            {
                DateTime.Now.AddDays(-2).ToUniversalTime(),
                DateTime.Now.AddDays(-3).ToUniversalTime(),
            },
            TestUuid = Guid.NewGuid(),
            TestUuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
            TestGeo = new GeoCoordinate(12.345f, 67.890f),
            TestPhone = new PhoneNumber("+1 555-123-4567") { DefaultCountry = "US" },
            TestObject = new TestNestedProperties
            {
                TestText = "nestedText",
                TestInt = 789,
                TestObject = new TestNestedProperties
                {
                    TestText = "nestedNestedText",
                    TestInt = 101112,
                },
            },
            TestObjectArray = new[]
            {
                new TestNestedProperties { TestText = "arrayObjectText1", TestInt = 111 },
                new TestNestedProperties
                {
                    TestText = "arrayObjectText2",
                    TestInt = 222,
                    TestObject = new TestNestedProperties
                    {
                        TestText = "arrayNestedObjectText",
                        TestInt = 333,
                    },
                },
            },
        };

        var props = ObjectHelper.BuildDataTransferObject(testData);

        TestProperties? concrete = ObjectHelper.UnmarshallProperties<TestProperties>(
            (IDictionary<string, object?>)props
        );

        Assert.Equivalent(testData, concrete);
    }

    [Fact]
    public void TestBuildDynamicObjectPropertiesGeoCoordinate()
    {
        var geo = new { TestingPropertyType = new GeoCoordinate(12.345f, 67.890f) };
        var props = ObjectHelper.BuildDataTransferObject(geo);

        dynamic? concrete = ObjectHelper.UnmarshallProperties<ExpandoObject>(
            (IDictionary<string, object?>)props
        );

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
        Assert.Equal(162L, obj[0]["movie_id"]);
        Assert.Equal(162L, obj[0]["review_id"]);

        Assert.Equal("r96sk", obj[1]["author_username"]);
        Assert.Equal(review[1].content, obj[1]["content"]);
        Assert.Equal(8.0, obj[1]["rating"]);
        Assert.Equal(162L, obj[1]["movie_id"]);
        Assert.Equal(162L, obj[1]["review_id"]);
    }

    [Theory]
    [InlineData(typeof(bool), true)]
    [InlineData(typeof(bool[]), true)]
    [InlineData(typeof(char), true)]
    [InlineData(typeof(sbyte), true)]
    [InlineData(typeof(byte), true)]
    [InlineData(typeof(byte[]), true)]
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
            // Add nested object
            TestObject = new TestNestedProperties
            {
                TestText = "nestedText",
                TestInt = 789,
                TestObject = new TestNestedProperties
                {
                    TestText = "deepNestedText",
                    TestInt = 101112,
                },
            },
            // Add array of nested objects
            TestObjectArray = new[]
            {
                new TestNestedProperties { TestText = "arrayObjectText1", TestInt = 111 },
                new TestNestedProperties
                {
                    TestText = "arrayObjectText2",
                    TestInt = 222,
                    TestObject = new TestNestedProperties
                    {
                        TestText = "arrayDeepNestedText",
                        TestInt = 333,
                    },
                },
            },
        };

        var bp = ObjectHelper.BuildBatchProperties(p);

        // Verify all expected properties are present
        Assert.Contains("testText", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("testInt", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("testBool", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("testNumber", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("testDate", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("testUuid", bp.NonRefProperties.Fields.Keys);
        Assert.Contains("testGeo", bp.NonRefProperties.Fields.Keys);

        // String properties
        Assert.Equal(p.TestText, bp.NonRefProperties.Fields["testText"].StringValue);
        Assert.Equal(
            p.TestTextArray.Length,
            bp.TextArrayProperties.Where(p => p.PropName == "testTextArray").First().Values.Count
        );
        Assert.Equal(
            p.TestTextArray[0],
            bp.TextArrayProperties.Where(p => p.PropName == "testTextArray").First().Values[0]
        );
        Assert.Equal(
            p.TestTextArray[1],
            bp.TextArrayProperties.Where(p => p.PropName == "testTextArray").First().Values[1]
        );

        // Integer properties
        Assert.Equal(
            Convert.ToDouble(p.TestInt),
            bp.NonRefProperties.Fields["testInt"].NumberValue
        );
        Assert.Equal(
            p.TestIntArray.Length,
            bp.IntArrayProperties.Where(p => p.PropName == "testIntArray").First().Values.Count
        );
        Assert.Equal(
            p.TestIntArray[0],
            bp.IntArrayProperties.Where(p => p.PropName == "testIntArray").First().Values[0]
        );
        Assert.Equal(
            p.TestIntArray[1],
            bp.IntArrayProperties.Where(p => p.PropName == "testIntArray").First().Values[1]
        );
        Assert.Equal(
            p.TestIntArray[2],
            bp.IntArrayProperties.Where(p => p.PropName == "testIntArray").First().Values[2]
        );

        // Boolean property
        Assert.Equal(p.TestBool, bp.NonRefProperties.Fields["testBool"].BoolValue);

        // Number properties
        Assert.Equal(p.TestNumber, bp.NonRefProperties.Fields["testNumber"].NumberValue);

        Assert.NotEmpty(bp.NumberArrayProperties[0].ValuesBytes);

        // Additional check for number array field name
        Assert.Equal("testNumberArray", bp.NumberArrayProperties[0].PropName);

        // Date properties
        Assert.Equal(
            p.TestDate.Value.ToUniversalTime().ToString("o"),
            bp.NonRefProperties.Fields["testDate"].StringValue
        );
        var dateList = bp.TextArrayProperties.Single(p => p.PropName == "testDateArray");
        Assert.Equal(p.TestDateArray.Length, dateList.Values.Count);
        Assert.Equal(p.TestDateArray[0].ToUniversalTime().ToString("o"), dateList.Values[0]);
        Assert.Equal(p.TestDateArray[1].ToUniversalTime().ToString("o"), dateList.Values[1]);

        // UUID properties
        Assert.Equal(p.TestUuid.ToString(), bp.NonRefProperties.Fields["testUuid"].StringValue);
        var uuidList = bp.TextArrayProperties.Single(p => p.PropName == "testUuidArray");
        Assert.Equal(p.TestUuidArray.Length, uuidList.Values.Count);
        Assert.Equal(p.TestUuidArray[0].ToString(), uuidList.Values[0]);
        Assert.Equal(p.TestUuidArray[1].ToString(), uuidList.Values[1]);

        // Geo property
        Assert.Equal(
            p.TestGeo.Latitude,
            bp.NonRefProperties.Fields["testGeo"].StructValue.Fields["latitude"].NumberValue
        );
        Assert.Equal(
            p.TestGeo.Longitude,
            bp.NonRefProperties.Fields["testGeo"].StructValue.Fields["longitude"].NumberValue
        );

        // Nested object property
        Assert.NotNull(bp.NonRefProperties.Fields["testObject"]);
        var nested = bp.NonRefProperties.Fields["testObject"].StructValue.Fields;
        Assert.Equal("nestedText", nested["testText"].StringValue);
        Assert.Equal(789.0, nested["testInt"].NumberValue);
        Assert.NotNull(nested["testObject"]);
        var deepNested = nested["testObject"].StructValue.Fields;
        Assert.Equal("deepNestedText", deepNested["testText"].StringValue);
        Assert.Equal(101112.0, deepNested["testInt"].NumberValue);

        // Array of nested objects property
        Assert.NotNull(bp.NonRefProperties.Fields["testObjectArray"]);
        var arr = bp.NonRefProperties.Fields["testObjectArray"].ListValue.Values;
        Assert.Equal("arrayObjectText1", arr[0].StructValue.Fields["testText"].StringValue);
        Assert.Equal(111.0, arr[0].StructValue.Fields["testInt"].NumberValue);
        Assert.Equal("arrayObjectText2", arr[1].StructValue.Fields["testText"].StringValue);
        Assert.Equal(222.0, arr[1].StructValue.Fields["testInt"].NumberValue);
        Assert.NotNull(arr[1].StructValue.Fields["testObject"]);
        var arrayDeepNested = arr[1].StructValue.Fields["testObject"].StructValue.Fields;
        Assert.Equal("arrayDeepNestedText", arrayDeepNested["testText"].StringValue);
        Assert.Equal(333.0, arrayDeepNested["testInt"].NumberValue);
    }

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
        Assert.Equal("John Doe", result["name"]);
        Assert.Equal(30L, result["age"]);
        Assert.Equal(true, result["isActive"]);
        Assert.Equal(95.5, result["score"]);
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
        Assert.Equal("Jane", result["name"]);
        Assert.Equal(25L, result["age"]);
        Assert.False(result.ContainsKey("email"));
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
        Assert.Equal("2025-10-24T15:30:00.0000000Z", result["createdAt"]);
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
        Assert.Equal(expectedUtc, result["createdAt"]);
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
        Assert.NotNull(result["location"]);
        Rest.Dto.GeoCoordinates geoResult = (Rest.Dto.GeoCoordinates)result["location"]!;
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
        Assert.Equal("Company", result["name"]);
        Assert.NotNull(result["address"]);

        var address = result["address"] as IDictionary<string, object?>;
        Assert.NotNull(address);
        Assert.Equal("Main St", address["street"]);
        Assert.Equal(123L, address["number"]);
        Assert.Equal("Berlin", address["city"]);
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
        var level1 = result["level1"] as IDictionary<string, object?>;
        Assert.NotNull(level1);

        var level2 = level1["level2"] as IDictionary<string, object?>;
        Assert.NotNull(level2);

        var level3 = level2["level3"] as IDictionary<string, object?>;
        Assert.NotNull(level3);
        Assert.Equal("Deep Value", level3["value"]);
        Assert.Equal(42L, level3["count"]);
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
        Assert.Equal("Hello", result["textValue"]);
        Assert.Equal(42L, result["intValue"]);
        Assert.Equal(9876543210L, result["longValue"]);
        Assert.Equal(3.14f, Convert.ToSingle(result["floatValue"]));
        Assert.Equal(2.718281828, Convert.ToDouble(result["doubleValue"]));
        Assert.Equal(true, result["boolValue"]);
        Assert.Equal("2025-01-01T00:00:00.0000000Z", result["dateValue"]);

        Assert.NotNull(result["geoValue"]);
        var geoResult = (Rest.Dto.GeoCoordinates)result["geoValue"]!;
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
        Assert.NotNull(result["tags"]);
        Assert.NotNull(result["numbers"]);

        var tags = result["tags"] as string[];
        Assert.NotNull(tags);
        Assert.Equal(3, tags.Length);
        Assert.Equal("tag1", tags[0]);

        var numbers = result["numbers"] as long[];
        Assert.NotNull(numbers);
        Assert.Equal(3, numbers.Length);
        Assert.Equal(1L, numbers[0]);
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
        Assert.Equal(data.Id.ToString(), result["id"]);
        Assert.Equal(data.Name, result["name"]);
        Assert.Equal(data.Price, result["price"]);
        Assert.Equal(data.InStock, result["inStock"]);
        Assert.NotNull(result["createdAt"]);
        Assert.NotNull(result["location"]);
        Assert.NotNull(result["details"]);
        Assert.NotNull(result["tags"]);
        Assert.False(result.ContainsKey("nullField"));

        var details = result["details"] as IDictionary<string, object?>;
        Assert.NotNull(details);
        Assert.Equal("TestBrand", details["brand"]);
        Assert.Equal("X1000", details["model"]);
        Assert.Equal(2025L, details["year"]);
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
        Assert.Equal("Readable", result["readableProperty"]);
        Assert.False(result.ContainsKey("writeOnlyProperty"));
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

    [Fact]
    public void TestBuildDataTransferObject_MultiLevelObject_ReturnsObjectDto()
    {
        // Arrange
        var data = new TestProperties
        {
            TestText = "dummyText",
            TestTextArray = new[] { "dummyTextArray1", "dummyTextArray2" },
            TestInt = 123,
            TestIntArray = new[] { 1, 2, 3 },
            TestBlob = System.Text.Encoding.UTF8.GetBytes("Weaviate"),
            TestBool = true,
            TestBoolArray = new[] { true, false },
            TestNumber = 456.789,
            TestNumberArray = new[] { 4.5, 6.7 },
            TestDate = DateTime.Now.AddDays(-1),
            TestDateArray = new[] { DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-3) },
            TestUuid = Guid.NewGuid(),
            TestUuidArray = new[] { Guid.NewGuid(), Guid.NewGuid() },
            TestGeo = new GeoCoordinate(12.345f, 67.890f),
            TestPhone = new PhoneNumber("+1 555-123-4567") { DefaultCountry = "US" },
            TestObject = new TestNestedProperties
            {
                TestText = "nestedText",
                TestInt = 789,
                TestObject = new TestNestedProperties
                {
                    TestText = "nestedNestedText",
                    TestInt = 101112,
                },
            },
            TestObjectArray = new[]
            {
                new TestNestedProperties { TestText = "arrayObjectText1", TestInt = 111 },
                new TestNestedProperties
                {
                    TestText = "arrayObjectText2",
                    TestInt = 222,
                    TestObject = new TestNestedProperties
                    {
                        TestText = "arrayNestedObjectText",
                        TestInt = 333,
                    },
                },
            },
        };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(data);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void TestUnmarshallProperties_EnumFromString_WithEnumMemberAttribute()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            { "TestEnum", "active" }, // EnumMember value
        };

        // Act
        var result = ObjectHelper.UnmarshallProperties<TestProperties>(dict);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestStatus.Active, result.TestEnum);
    }

    [Fact]
    public void TestUnmarshallProperties_EnumFromString_WithEnumName()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            { "TestEnum", "Active" }, // Enum name
        };

        // Act
        var result = ObjectHelper.UnmarshallProperties<TestProperties>(dict);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestStatus.Active, result.TestEnum);
    }

    [Fact]
    public void TestUnmarshallProperties_EnumFromString_CaseInsensitive()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            { "TestEnum", "PENDING" }, // Different case
        };

        // Act
        var result = ObjectHelper.UnmarshallProperties<TestProperties>(dict);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestStatus.Pending, result.TestEnum);
    }

    [Fact]
    public void TestUnmarshallProperties_EnumFromString_WithoutEnumMemberAttribute()
    {
        // Arrange
        var dict = new Dictionary<string, object?>
        {
            { "TestEnum", "Archived" }, // No EnumMember attribute
        };

        // Act
        var result = ObjectHelper.UnmarshallProperties<TestProperties>(dict);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestStatus.Archived, result.TestEnum);
    }

    [Fact]
    public void TestUnmarshallProperties_EnumFromNumber()
    {
        // Arrange - Numeric value
        var dict = new Dictionary<string, object?> { { "TestEnum", 1 } };

        // Act
        var result = ObjectHelper.UnmarshallProperties<TestProperties>(dict);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestStatus.Inactive, result.TestEnum);
    }

    [Fact]
    public void TestUnmarshallProperties_EnumFromString_InvalidValue_ThrowsException()
    {
        // Arrange
        var dict = new Dictionary<string, object?> { { "TestEnum", "InvalidValue" } };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            ObjectHelper.UnmarshallProperties<TestProperties>(dict)
        );
        Assert.Contains("InvalidValue", exception.Message);
        Assert.Contains("TestStatus", exception.Message);
    }

    [Fact]
    public void TestBuildDataTransferObject_WithExpandoObject()
    {
        // Arrange
        dynamic expando = new ExpandoObject();
        expando.Name = "test";
        expando.Age = 42;
        expando.Active = true;

        // Act
        var result = ObjectHelper.BuildDataTransferObject(expando);

        // Assert
        Assert.Equal("test", result["name"]);
        Assert.Equal(42, result["age"]);
        Assert.Equal(true, result["active"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_WithNestedExpandoObject()
    {
        // Arrange
        dynamic nested = new ExpandoObject();
        nested.City = "Amsterdam";
        nested.Country = "Netherlands";

        dynamic expando = new ExpandoObject();
        expando.Name = "test";
        expando.Address = nested;

        // Act
        var result = ObjectHelper.BuildDataTransferObject(expando);

        // Assert
        Assert.Equal("test", result["name"]);
        Assert.IsType<Dictionary<string, object>>(result["address"]);
        var addressDict = (Dictionary<string, object>)result["address"];
        Assert.Equal("Amsterdam", addressDict["city"]);
        Assert.Equal("Netherlands", addressDict["country"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_WithExpandoContainingArray()
    {
        // Arrange
        dynamic expando = new ExpandoObject();
        expando.Name = "test";
        expando.Tags = new[] { "tag1", "tag2" };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(expando);

        // Assert
        Assert.Equal("test", result["name"]);
        Assert.IsAssignableFrom<IEnumerable<object?>>(result["tags"]);
        var tags = ((IEnumerable<object?>)result["tags"]).ToList();
        Assert.Equal(2, tags.Count);
        Assert.Equal("tag1", tags[0]);
        Assert.Equal("tag2", tags[1]);
    }

    [Fact]
    public void TestBuildDataTransferObject_WithExpandoContainingNestedExpandoArray()
    {
        // Arrange
        dynamic item1 = new ExpandoObject();
        item1.Id = 1;
        item1.Value = "first";

        dynamic item2 = new ExpandoObject();
        item2.Id = 2;
        item2.Value = "second";

        dynamic expando = new ExpandoObject();
        expando.Items = new object[] { item1, item2 };

        // Act
        var result = ObjectHelper.BuildDataTransferObject(expando);

        // Assert
        Assert.IsAssignableFrom<IEnumerable<object?>>(result["items"]);
        var items = ((IEnumerable<object?>)result["items"]).ToList();
        Assert.Equal(2, items.Count);

        var firstItem = (Dictionary<string, object>)items[0]!;
        Assert.Equal(1, Convert.ToInt32(firstItem["id"]));
        Assert.Equal("first", firstItem["value"]);

        var secondItem = (Dictionary<string, object>)items[1]!;
        Assert.Equal(2, Convert.ToInt32(secondItem["id"]));
        Assert.Equal("second", secondItem["value"]);
    }

    [Fact]
    public void TestBuildDataTransferObject_WithNull_ReturnsEmptyDictionary()
    {
        // Act
        var result = ObjectHelper.BuildDataTransferObject(null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void TestBuildDataTransferObject_WithExpandoContainingDateTimeShouldConvert()
    {
        // Arrange
        var testDate = new DateTime(2024, 6, 15, 10, 30, 0, DateTimeKind.Utc);
        dynamic expando = new ExpandoObject();
        expando.CreatedAt = testDate;

        // Act
        var result = ObjectHelper.BuildDataTransferObject(expando);

        // Assert
        // DatePropertyConverter should convert DateTime to ISO 8601 string
        Assert.IsType<string>(result["createdAt"]);
    }
}
