using System.Net;
using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;
using Dto = Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Comprehensive tests for property serialization/deserialization using mock client.
/// Tests all Weaviate data types: text, int, number, bool, date, uuid, blob, geo, phone, object.
/// Verifies both single values and arrays for applicable types.
/// </summary>
public class SerializationWithMockClientTests
{
    /// <summary>
    /// Test model containing all supported Weaviate data types.
    /// </summary>
    public class AllPropertiesModel
    {
        // Simple types
        public string? TextProp { get; set; }
        public int IntProp { get; set; }
        public double NumberProp { get; set; }
        public bool BoolProp { get; set; }
        public DateTime DateProp { get; set; }
        public Guid UuidProp { get; set; }
        public byte[]? BlobProp { get; set; }
        public GeoCoordinate? GeoProp { get; set; }
        public PhoneNumber? PhoneProp { get; set; }

        // Array types
        public string[]? TextArrayProp { get; set; }
        public int[]? IntArrayProp { get; set; }
        public double[]? NumberArrayProp { get; set; }
        public bool[]? BoolArrayProp { get; set; }
        public DateTime[]? DateArrayProp { get; set; }
        public Guid[]? UuidArrayProp { get; set; }

        // Nested object
        public NestedObject? ObjectProp { get; set; }
        public NestedObject[]? ObjectArrayProp { get; set; }
    }

    public class NestedObject
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    [Fact]
    public async Task Insert_WithAllPropertyTypes_SerializesCorrectly()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        var testDate = new DateTime(2025, 11, 21, 10, 30, 0, DateTimeKind.Utc);
        var testUuid = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        var testObject = new AllPropertiesModel
        {
            TextProp = "Hello World",
            IntProp = 42,
            NumberProp = 3.14159,
            BoolProp = true,
            DateProp = testDate,
            UuidProp = testUuid,
            BlobProp = new byte[] { 0x01, 0x02, 0x03, 0x04 },
            GeoProp = new GeoCoordinate(52.52f, 13.405f),
            PhoneProp = PhoneNumber.FromNational("US", "1234567890"),
            TextArrayProp = ["one", "two", "three"],
            IntArrayProp = [1, 2, 3],
            NumberArrayProp = [1.1, 2.2, 3.3],
            BoolArrayProp = [true, false, true],
            DateArrayProp = [testDate, testDate.AddDays(1)],
            UuidArrayProp = [testUuid, Guid.Parse("87654321-4321-4321-4321-cba987654321")],
            ObjectProp = new NestedObject { Name = "nested", Value = 100 },
            ObjectArrayProp =
            [
                new NestedObject { Name = "first", Value = 1 },
                new NestedObject { Name = "second", Value = 2 },
            ],
        };

        // Mock collection creation response
        var collectionResponse = new Dto.Class { Class1 = "AllProperties", Properties = [] };
        handler.AddJsonResponse(collectionResponse, "/v1/schema");

        // Mock successful insert response
        var mockResponse = new Dto.Object
        {
            Id = testUuid,
            Class = "AllProperties",
            Properties = new Dictionary<string, object>(),
        };
        handler.AddJsonResponse(mockResponse, "/v1/objects");

        // Act
        var collection = await client.Collections.Create(
            new CollectionConfig { Name = "AllProperties" },
            TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            testObject,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(handler.LastRequest);
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Post).ShouldHavePath("/v1/objects");

        // Parse the request body to verify serialization
        var requestBody = await handler.LastRequest.GetBodyAsString();
        var requestJson = JsonDocument.Parse(requestBody);
        var props = requestJson.RootElement.GetProperty("properties");

        // Verify simple types
        Assert.Equal("Hello World", props.GetProperty("textProp").GetString());
        Assert.Equal(42, props.GetProperty("intProp").GetInt64());
        Assert.Equal(3.14159, props.GetProperty("numberProp").GetDouble(), precision: 5);
        Assert.True(props.GetProperty("boolProp").GetBoolean());
        Assert.Equal(testDate.ToString("o"), props.GetProperty("dateProp").GetString()); // ISO 8601 format
        Assert.Equal(testUuid.ToString(), props.GetProperty("uuidProp").GetString());

        // Verify blob (base64 encoded)
        var blobBase64 = props.GetProperty("blobProp").GetString();
        Assert.NotNull(blobBase64);
        var decodedBlob = Convert.FromBase64String(blobBase64!);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, decodedBlob);

        // Verify geo coordinates
        var geo = props.GetProperty("geoProp");
        Assert.Equal(52.52f, geo.GetProperty("latitude").GetDouble(), precision: 2);
        Assert.Equal(13.405f, geo.GetProperty("longitude").GetDouble(), precision: 3);

        // Verify phone number (serialized as the DTO structure)
        var phone = props.GetProperty("phoneProp");
        Assert.True(phone.TryGetProperty("input", out var inputProp));
        Assert.Equal("1234567890", inputProp.GetString());
        if (phone.TryGetProperty("defaultCountry", out var dcProp))
        {
            Assert.Equal("US", dcProp.GetString());
        }

        // Verify arrays
        var textArray = props.GetProperty("textArrayProp");
        Assert.Equal(3, textArray.GetArrayLength());
        Assert.Equal("one", textArray[0].GetString());
        Assert.Equal("two", textArray[1].GetString());
        Assert.Equal("three", textArray[2].GetString());

        var intArray = props.GetProperty("intArrayProp");
        Assert.Equal(3, intArray.GetArrayLength());
        Assert.Equal(1, intArray[0].GetInt64());
        Assert.Equal(2, intArray[1].GetInt64());
        Assert.Equal(3, intArray[2].GetInt64());

        var numberArray = props.GetProperty("numberArrayProp");
        Assert.Equal(3, numberArray.GetArrayLength());
        Assert.Equal(1.1, numberArray[0].GetDouble(), precision: 1);
        Assert.Equal(2.2, numberArray[1].GetDouble(), precision: 1);
        Assert.Equal(3.3, numberArray[2].GetDouble(), precision: 1);

        var boolArray = props.GetProperty("boolArrayProp");
        Assert.Equal(3, boolArray.GetArrayLength());
        Assert.True(boolArray[0].GetBoolean());
        Assert.False(boolArray[1].GetBoolean());
        Assert.True(boolArray[2].GetBoolean());

        // Verify nested object
        var obj = props.GetProperty("objectProp");
        Assert.Equal("nested", obj.GetProperty("name").GetString());
        Assert.Equal(100, obj.GetProperty("value").GetInt64());

        // Verify nested object array
        var objArray = props.GetProperty("objectArrayProp");
        Assert.Equal(2, objArray.GetArrayLength());
        Assert.Equal("first", objArray[0].GetProperty("name").GetString());
        Assert.Equal(1, objArray[0].GetProperty("value").GetInt64());
        Assert.Equal("second", objArray[1].GetProperty("name").GetString());
        Assert.Equal(2, objArray[1].GetProperty("value").GetInt64());
    }

    [Fact]
    public void Deserialization_WithAllPropertyTypes_WorksCorrectly()
    {
        // Arrange - Create a dictionary as would come from REST/GraphQL response
        var testDate = new DateTime(2025, 11, 21, 10, 30, 0, DateTimeKind.Utc);
        var testUuid = Guid.Parse("12345678-1234-1234-1234-123456789abc");

        var propertyDict = new Dictionary<string, object?>
        {
            { "textProp", "Hello World" },
            { "intProp", 42L },
            { "numberProp", 3.14159 },
            { "boolProp", true },
            { "dateProp", testDate.ToString("o") },
            { "uuidProp", testUuid.ToString() },
            { "blobProp", Convert.ToBase64String([0x01, 0x02, 0x03, 0x04]) },
            {
                "geoProp",
                new Dictionary<string, object?> { { "latitude", 52.52f }, { "longitude", 13.405f } }
            },
            {
                "phoneProp",
                new Dictionary<string, object?>
                {
                    { "input", "1234567890" },
                    { "defaultCountry", "US" },
                }
            },
            { "textArrayProp", new[] { "one", "two", "three" } },
            { "intArrayProp", new object[] { 1L, 2L, 3L } },
            { "numberArrayProp", new[] { 1.1, 2.2, 3.3 } },
            { "boolArrayProp", new[] { true, false, true } },
            {
                "dateArrayProp",
                new[] { testDate.ToString("o"), testDate.AddDays(1).ToString("o") }
            },
            {
                "uuidArrayProp",
                new[] { testUuid.ToString(), "87654321-4321-4321-4321-cba987654321" }
            },
            {
                "objectProp",
                new Dictionary<string, object?> { { "name", "nested" }, { "value", 100L } }
            },
            {
                "objectArrayProp",
                new object[]
                {
                    new Dictionary<string, object?> { { "name", "first" }, { "value", 1L } },
                    new Dictionary<string, object?> { { "name", "second" }, { "value", 2L } },
                }
            },
        };

        // Act - Deserialize using the converter registry
        var registry = Serialization.PropertyConverterRegistry.Default;
        var result = registry.BuildConcreteTypeFromProperties(
            propertyDict,
            typeof(AllPropertiesModel)
        );

        // Assert
        Assert.NotNull(result);
        var item = (AllPropertiesModel)result;

        // Verify simple types
        Assert.Equal("Hello World", item.TextProp);
        Assert.Equal(42, item.IntProp);
        Assert.Equal(3.14159, item.NumberProp, precision: 5);
        Assert.True(item.BoolProp);
        // Compare dates as UTC to avoid timezone issues
        Assert.Equal(testDate.ToUniversalTime(), item.DateProp.ToUniversalTime());
        Assert.Equal(testUuid, item.UuidProp);

        // Verify blob
        Assert.NotNull(item.BlobProp);
        Assert.Equal(new byte[] { 0x01, 0x02, 0x03, 0x04 }, item.BlobProp);

        // Verify geo coordinates
        Assert.NotNull(item.GeoProp);
        Assert.Equal(52.52f, item.GeoProp.Latitude, precision: 2);
        Assert.Equal(13.405f, item.GeoProp.Longitude, precision: 3);

        // Verify phone number
        Assert.NotNull(item.PhoneProp);
        Assert.Equal("1234567890", item.PhoneProp.Input);

        // Verify arrays
        Assert.NotNull(item.TextArrayProp);
        Assert.Equal(["one", "two", "three"], item.TextArrayProp);

        Assert.NotNull(item.IntArrayProp);
        Assert.Equal([1, 2, 3], item.IntArrayProp);

        Assert.NotNull(item.NumberArrayProp);
        Assert.Equal(3, item.NumberArrayProp.Length);
        Assert.Equal(1.1, item.NumberArrayProp[0], precision: 1);
        Assert.Equal(2.2, item.NumberArrayProp[1], precision: 1);
        Assert.Equal(3.3, item.NumberArrayProp[2], precision: 1);

        Assert.NotNull(item.BoolArrayProp);
        Assert.Equal([true, false, true], item.BoolArrayProp);

        Assert.NotNull(item.DateArrayProp);
        Assert.Equal(2, item.DateArrayProp.Length);
        Assert.Equal(testDate.ToUniversalTime(), item.DateArrayProp[0].ToUniversalTime());
        Assert.Equal(
            testDate.AddDays(1).ToUniversalTime(),
            item.DateArrayProp[1].ToUniversalTime()
        );

        Assert.NotNull(item.UuidArrayProp);
        Assert.Equal(2, item.UuidArrayProp.Length);
        Assert.Equal(testUuid, item.UuidArrayProp[0]);
        Assert.Equal(Guid.Parse("87654321-4321-4321-4321-cba987654321"), item.UuidArrayProp[1]);

        // Verify nested object
        Assert.NotNull(item.ObjectProp);
        Assert.Equal("nested", item.ObjectProp.Name);
        Assert.Equal(100, item.ObjectProp.Value);

        // Verify nested object array
        Assert.NotNull(item.ObjectArrayProp);
        Assert.Equal(2, item.ObjectArrayProp.Length);
        Assert.Equal("first", item.ObjectArrayProp[0].Name);
        Assert.Equal(1, item.ObjectArrayProp[0].Value);
        Assert.Equal("second", item.ObjectArrayProp[1].Name);
        Assert.Equal(2, item.ObjectArrayProp[1].Value);
    }

    [Fact]
    public async Task Insert_WithNullableProperties_HandlesNullsCorrectly()
    {
        // Arrange
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler();

        var testObject = new AllPropertiesModel
        {
            TextProp = null,
            IntProp = 0,
            NumberProp = 0,
            BoolProp = false,
            DateProp = DateTime.UtcNow,
            UuidProp = Guid.Empty,
            BlobProp = null,
            GeoProp = null,
            PhoneProp = null,
            TextArrayProp = null,
            IntArrayProp = null,
            NumberArrayProp = null,
            BoolArrayProp = null,
            DateArrayProp = null,
            UuidArrayProp = null,
            ObjectProp = null,
            ObjectArrayProp = null,
        };

        // Mock collection creation response
        var collectionResponse = new Dto.Class { Class1 = "AllProperties", Properties = [] };
        handler.AddJsonResponse(collectionResponse, "/v1/schema");

        var mockResponse = new Dto.Object
        {
            Id = Guid.NewGuid(),
            Class = "AllProperties",
            Properties = new Dictionary<string, object>(),
        };
        handler.AddJsonResponse(mockResponse, "/v1/objects");

        // Act
        var collection = await client.Collections.Create(
            new CollectionConfig { Name = "AllProperties" },
            TestContext.Current.CancellationToken
        );
        await collection.Data.Insert(
            testObject,
            cancellationToken: TestContext.Current.CancellationToken
        );

        // Assert
        Assert.NotNull(handler.LastRequest);

        var requestBody = await handler.LastRequest.GetBodyAsString();
        var requestJson = JsonDocument.Parse(requestBody);
        var props = requestJson.RootElement.GetProperty("properties");

        // Null properties should be omitted from serialization (not included in JSON)
        Assert.False(props.TryGetProperty("textProp", out _));
        Assert.False(props.TryGetProperty("blobProp", out _));
        Assert.False(props.TryGetProperty("geoProp", out _));
        Assert.False(props.TryGetProperty("phoneProp", out _));
        Assert.False(props.TryGetProperty("textArrayProp", out _));
        Assert.False(props.TryGetProperty("objectProp", out _));
        Assert.False(props.TryGetProperty("objectArrayProp", out _));

        // Non-nullable properties with default values should still be present
        Assert.True(props.TryGetProperty("intProp", out var intProp));
        Assert.Equal(0, intProp.GetInt64());
    }
}
