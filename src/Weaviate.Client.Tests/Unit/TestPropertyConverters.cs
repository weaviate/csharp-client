using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;
using Weaviate.Client.Serialization;
using Weaviate.Client.Serialization.Converters;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// The test property converters class
/// </summary>
public class TestPropertyConverters
{
    #region TextPropertyConverter Tests

    /// <summary>
    /// Tests that text converter to rest returns string
    /// </summary>
    [Fact]
    public void TextConverter_ToRest_ReturnsString()
    {
        var converter = new TextPropertyConverter();
        Assert.Equal("hello", converter.ToRest("hello"));
        Assert.Null(converter.ToRest(null));
    }

    /// <summary>
    /// Tests that text converter to grpc returns string value
    /// </summary>
    [Fact]
    public void TextConverter_ToGrpc_ReturnsStringValue()
    {
        var converter = new TextPropertyConverter();
        var result = converter.ToGrpc("hello");
        Assert.Equal(Value.KindOneofCase.StringValue, result.KindCase);
        Assert.Equal("hello", result.StringValue);
    }

    /// <summary>
    /// Tests that text converter from rest returns string
    /// </summary>
    [Fact]
    public void TextConverter_FromRest_ReturnsString()
    {
        var converter = new TextPropertyConverter();
        Assert.Equal("hello", converter.FromRest("hello", typeof(string)));
    }

    /// <summary>
    /// Tests that text converter from grpc returns string
    /// </summary>
    [Fact]
    public void TextConverter_FromGrpc_ReturnsString()
    {
        var converter = new TextPropertyConverter();
        var grpcValue = Value.ForString("hello");
        Assert.Equal("hello", converter.FromGrpc(grpcValue, typeof(string)));
    }

    /// <summary>
    /// Tests that text converter array round trip
    /// </summary>
    [Fact]
    public void TextConverter_Array_RoundTrip()
    {
        var converter = new TextPropertyConverter();
        var input = new object?[] { "a", "b", "c" };

        var rest = converter.ToRestArray(input);
        Assert.IsType<string?[]>(rest);
        Assert.Equal(3, ((string?[])rest).Length);

        var grpc = converter.ToGrpcArray(input);
        Assert.Equal(3, grpc.Values.Count);
    }

    #endregion

    #region IntPropertyConverter Tests

    /// <summary>
    /// Tests that int converter to rest converts all to long
    /// </summary>
    [Fact]
    public void IntConverter_ToRest_ConvertsAllToLong()
    {
        var converter = new IntPropertyConverter();
        Assert.Equal(42L, converter.ToRest(42));
        Assert.Equal(42L, converter.ToRest(42L));
        Assert.Equal(42L, converter.ToRest((short)42));
        Assert.Equal(42L, converter.ToRest((byte)42));
    }

    /// <summary>
    /// Tests that int converter to grpc returns number value
    /// </summary>
    [Fact]
    public void IntConverter_ToGrpc_ReturnsNumberValue()
    {
        var converter = new IntPropertyConverter();
        var result = converter.ToGrpc(42);
        Assert.Equal(Value.KindOneofCase.NumberValue, result.KindCase);
        Assert.Equal(42, result.NumberValue);
    }

    /// <summary>
    /// Tests that int converter from rest returns correct type
    /// </summary>
    [Fact]
    public void IntConverter_FromRest_ReturnsCorrectType()
    {
        var converter = new IntPropertyConverter();

        // When target type is int, returns int
        var resultInt = converter.FromRest(42.0, typeof(int));
        Assert.IsType<int>(resultInt);
        Assert.Equal(42, resultInt);

        // When target type is long, returns long
        var resultLong = converter.FromRest(42.0, typeof(long));
        Assert.IsType<long>(resultLong);
        Assert.Equal(42L, resultLong);
    }

    /// <summary>
    /// Tests that int converter from grpc returns correct type
    /// </summary>
    [Fact]
    public void IntConverter_FromGrpc_ReturnsCorrectType()
    {
        var converter = new IntPropertyConverter();
        var grpcValue = Value.ForNumber(42);

        // When target type is int, returns int
        var resultInt = converter.FromGrpc(grpcValue, typeof(int));
        Assert.IsType<int>(resultInt);
        Assert.Equal(42, resultInt);

        // When target type is long, returns long
        var resultLong = converter.FromGrpc(grpcValue, typeof(long));
        Assert.IsType<long>(resultLong);
        Assert.Equal(42L, resultLong);
    }

    #endregion

    #region NumberPropertyConverter Tests

    /// <summary>
    /// Tests that number converter to rest handles various number types
    /// </summary>
    [Fact]
    public void NumberConverter_ToRest_HandlesVariousNumberTypes()
    {
        var converter = new NumberPropertyConverter();
        Assert.Equal(3.14, converter.ToRest(3.14));
        Assert.Equal(3.14, (double)converter.ToRest(3.14f)!, 2);
        Assert.Equal(3.14, converter.ToRest(3.14m));
    }

    /// <summary>
    /// Tests that number converter from rest converts to target type
    /// </summary>
    [Fact]
    public void NumberConverter_FromRest_ConvertsToTargetType()
    {
        var converter = new NumberPropertyConverter();
        Assert.Equal(3.14, converter.FromRest(3.14, typeof(double)));
        Assert.Equal(3.14f, (float)converter.FromRest(3.14, typeof(float))!, 2);
        Assert.Equal(3.14m, converter.FromRest(3.14, typeof(decimal)));
    }

    #endregion

    #region BoolPropertyConverter Tests

    /// <summary>
    /// Tests that bool converter to rest returns bool
    /// </summary>
    [Fact]
    public void BoolConverter_ToRest_ReturnsBool()
    {
        var converter = new BoolPropertyConverter();
        Assert.Equal(true, converter.ToRest(true));
        Assert.Equal(false, converter.ToRest(false));
    }

    /// <summary>
    /// Tests that bool converter to grpc returns bool value
    /// </summary>
    [Fact]
    public void BoolConverter_ToGrpc_ReturnsBoolValue()
    {
        var converter = new BoolPropertyConverter();
        var result = converter.ToGrpc(true);
        Assert.Equal(Value.KindOneofCase.BoolValue, result.KindCase);
        Assert.True(result.BoolValue);
    }

    /// <summary>
    /// Tests that bool converter from grpc returns bool
    /// </summary>
    [Fact]
    public void BoolConverter_FromGrpc_ReturnsBool()
    {
        var converter = new BoolPropertyConverter();
        Assert.Equal(true, converter.FromGrpc(Value.ForBool(true), typeof(bool)));
        Assert.Equal(false, converter.FromGrpc(Value.ForBool(false), typeof(bool)));
    }

    #endregion

    #region DatePropertyConverter Tests

    /// <summary>
    /// Tests that date converter to rest returns round trip format
    /// </summary>
    [Fact]
    public void DateConverter_ToRest_ReturnsRoundTripFormat()
    {
        var converter = new DatePropertyConverter();
        var date = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var result = converter.ToRest(date);
        Assert.Equal("2024-01-15T10:30:00.0000000Z", result);
    }

    /// <summary>
    /// Tests that date converter from rest parses iso 8601
    /// </summary>
    [Fact]
    public void DateConverter_FromRest_ParsesIso8601()
    {
        var converter = new DatePropertyConverter();
        var result = converter.FromRest("2024-01-15T10:30:00Z", typeof(DateTime));
        Assert.IsType<DateTime>(result);
        var dt = (DateTime)result!;
        // Date is parsed and returned as UTC
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
        Assert.Equal(2024, dt.Year);
        Assert.Equal(1, dt.Month);
        Assert.Equal(15, dt.Day);
        Assert.Equal(10, dt.Hour);
        Assert.Equal(30, dt.Minute);
    }

    /// <summary>
    /// Tests that date converter from rest to date time offset
    /// </summary>
    [Fact]
    public void DateConverter_FromRest_ToDateTimeOffset()
    {
        var converter = new DatePropertyConverter();
        var result = converter.FromRest("2024-01-15T10:30:00Z", typeof(DateTimeOffset));
        Assert.IsType<DateTimeOffset>(result);
    }

    #endregion

    #region UuidPropertyConverter Tests

    /// <summary>
    /// Tests that uuid converter to rest returns guid string
    /// </summary>
    [Fact]
    public void UuidConverter_ToRest_ReturnsGuidString()
    {
        var converter = new UuidPropertyConverter();
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var result = converter.ToRest(guid);
        Assert.Equal("12345678-1234-1234-1234-123456789012", result);
    }

    /// <summary>
    /// Tests that uuid converter from rest parses guid
    /// </summary>
    [Fact]
    public void UuidConverter_FromRest_ParsesGuid()
    {
        var converter = new UuidPropertyConverter();
        var result = converter.FromRest("12345678-1234-1234-1234-123456789012", typeof(Guid));
        Assert.IsType<Guid>(result);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789012"), result);
    }

    #endregion

    #region BlobPropertyConverter Tests

    /// <summary>
    /// Tests that blob converter to rest returns base 64
    /// </summary>
    [Fact]
    public void BlobConverter_ToRest_ReturnsBase64()
    {
        var converter = new BlobPropertyConverter();
        var bytes = new byte[] { 1, 2, 3, 4 };
        var result = converter.ToRest(bytes);
        Assert.Equal(Convert.ToBase64String(bytes), result);
    }

    /// <summary>
    /// Tests that blob converter from rest decodes base 64
    /// </summary>
    [Fact]
    public void BlobConverter_FromRest_DecodesBase64()
    {
        var converter = new BlobPropertyConverter();
        var base64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var result = converter.FromRest(base64, typeof(byte[]));
        Assert.IsType<byte[]>(result);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, result);
    }

    /// <summary>
    /// Tests that blob converter does not support array
    /// </summary>
    [Fact]
    public void BlobConverter_DoesNotSupportArray()
    {
        var converter = new BlobPropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    #endregion

    #region BlobHashPropertyConverter Tests

    /// <summary>
    /// Tests that blob hash converter to rest returns string
    /// </summary>
    [Fact]
    public void BlobHashConverter_ToRest_ReturnsString()
    {
        var converter = new BlobHashPropertyConverter();
        var hash = "abc123def456";
        var result = converter.ToRest(hash);
        Assert.Equal(hash, result);
    }

    /// <summary>
    /// Tests that blob hash converter from rest returns string
    /// </summary>
    [Fact]
    public void BlobHashConverter_FromRest_ReturnsString()
    {
        var converter = new BlobHashPropertyConverter();
        var hash = "abc123def456";
        var result = converter.FromRest(hash, typeof(string));
        Assert.IsType<string>(result);
        Assert.Equal(hash, result);
    }

    /// <summary>
    /// Tests that blob hash converter does not support array
    /// </summary>
    [Fact]
    public void BlobHashConverter_DoesNotSupportArray()
    {
        var converter = new BlobHashPropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    /// <summary>
    /// Tests that blob hash converter has correct data type
    /// </summary>
    [Fact]
    public void BlobHashConverter_HasCorrectDataType()
    {
        var converter = new BlobHashPropertyConverter();
        Assert.Equal("blobHash", converter.DataType);
    }

    #endregion

    #region GeoPropertyConverter Tests

    /// <summary>
    /// Tests that geo converter to rest returns dto
    /// </summary>
    [Fact]
    public void GeoConverter_ToRest_ReturnsDto()
    {
        var converter = new GeoPropertyConverter();
        var geo = new GeoCoordinate(52.52f, 13.405f);
        var result = converter.ToRest(geo);

        Assert.IsType<Rest.Dto.GeoCoordinates>(result);
        var dto = (Rest.Dto.GeoCoordinates)result!;
        Assert.Equal(52.52, dto.Latitude!.Value, 2);
        Assert.Equal(13.405, dto.Longitude!.Value, 2);
    }

    /// <summary>
    /// Tests that geo converter from rest parses dictionary
    /// </summary>
    [Fact]
    public void GeoConverter_FromRest_ParsesDictionary()
    {
        var converter = new GeoPropertyConverter();
        var dict = new Dictionary<string, object> { ["latitude"] = 52.52, ["longitude"] = 13.405 };
        var result = converter.FromRest(dict, typeof(GeoCoordinate));

        Assert.IsType<GeoCoordinate>(result);
        var geo = (GeoCoordinate)result!;
        Assert.Equal(52.52f, geo.Latitude, 2);
        Assert.Equal(13.405f, geo.Longitude, 2);
    }

    /// <summary>
    /// Tests that geo converter does not support array
    /// </summary>
    [Fact]
    public void GeoConverter_DoesNotSupportArray()
    {
        var converter = new GeoPropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    #endregion

    #region PhonePropertyConverter Tests

    /// <summary>
    /// Tests that phone converter to rest returns dto
    /// </summary>
    [Fact]
    public void PhoneConverter_ToRest_ReturnsDto()
    {
        var converter = new PhonePropertyConverter();
        var phone = new PhoneNumber("+49 123 456789") { DefaultCountry = "DE" };
        var result = converter.ToRest(phone);

        Assert.IsType<Rest.Dto.PhoneNumber>(result);
        var dto = (Rest.Dto.PhoneNumber)result!;
        Assert.Equal("+49 123 456789", dto.Input);
        Assert.Equal("DE", dto.DefaultCountry);
    }

    /// <summary>
    /// Tests that phone converter from rest parses dictionary
    /// </summary>
    [Fact]
    public void PhoneConverter_FromRest_ParsesDictionary()
    {
        var converter = new PhonePropertyConverter();
        var dict = new Dictionary<string, object?>
        {
            ["input"] = "+49 123 456789",
            ["defaultCountry"] = "DE",
        };
        var result = converter.FromRest(dict, typeof(PhoneNumber));

        Assert.IsType<PhoneNumber>(result);
        var phone = (PhoneNumber)result!;
        Assert.Equal("+49 123 456789", phone.Input);
        Assert.Equal("DE", phone.DefaultCountry);
    }

    /// <summary>
    /// Tests that phone converter does not support array
    /// </summary>
    [Fact]
    public void PhoneConverter_DoesNotSupportArray()
    {
        var converter = new PhonePropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    #endregion

    #region PropertyConverterRegistry Tests

    /// <summary>
    /// Tests that registry get converter by data type returns correct converter
    /// </summary>
    [Fact]
    public void Registry_GetConverterByDataType_ReturnsCorrectConverter()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<TextPropertyConverter>(registry.GetConverterByDataType("text"));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterByDataType("int"));
        Assert.IsType<NumberPropertyConverter>(registry.GetConverterByDataType("number"));
        Assert.IsType<BoolPropertyConverter>(registry.GetConverterByDataType("boolean"));
        Assert.IsType<DatePropertyConverter>(registry.GetConverterByDataType("date"));
        Assert.IsType<UuidPropertyConverter>(registry.GetConverterByDataType("uuid"));
        Assert.IsType<BlobPropertyConverter>(registry.GetConverterByDataType("blob"));
        Assert.IsType<BlobHashPropertyConverter>(registry.GetConverterByDataType("blobHash"));
        Assert.IsType<GeoPropertyConverter>(registry.GetConverterByDataType("geoCoordinates"));
        Assert.IsType<PhonePropertyConverter>(registry.GetConverterByDataType("phoneNumber"));
    }

    /// <summary>
    /// Tests that registry get converter by data type handles array types
    /// </summary>
    [Fact]
    public void Registry_GetConverterByDataType_HandlesArrayTypes()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<TextPropertyConverter>(registry.GetConverterByDataType("text[]"));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterByDataType("int[]"));
    }

    /// <summary>
    /// Tests that registry get converter for type returns correct converter
    /// </summary>
    [Fact]
    public void Registry_GetConverterForType_ReturnsCorrectConverter()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<TextPropertyConverter>(registry.GetConverterForType(typeof(string)));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterForType(typeof(int)));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterForType(typeof(long)));
        Assert.IsType<NumberPropertyConverter>(registry.GetConverterForType(typeof(double)));
        Assert.IsType<BoolPropertyConverter>(registry.GetConverterForType(typeof(bool)));
        Assert.IsType<DatePropertyConverter>(registry.GetConverterForType(typeof(DateTime)));
        Assert.IsType<UuidPropertyConverter>(registry.GetConverterForType(typeof(Guid)));
        Assert.IsType<GeoPropertyConverter>(registry.GetConverterForType(typeof(GeoCoordinate)));
        Assert.IsType<PhonePropertyConverter>(registry.GetConverterForType(typeof(PhoneNumber)));
    }

    /// <summary>
    /// Tests that registry get converter for type handles nullable types
    /// </summary>
    [Fact]
    public void Registry_GetConverterForType_HandlesNullableTypes()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<IntPropertyConverter>(registry.GetConverterForType(typeof(int?)));
        Assert.IsType<BoolPropertyConverter>(registry.GetConverterForType(typeof(bool?)));
        Assert.IsType<DatePropertyConverter>(registry.GetConverterForType(typeof(DateTime?)));
    }

    /// <summary>
    /// Tests that registry get converter for type handles array types
    /// </summary>
    [Fact]
    public void Registry_GetConverterForType_HandlesArrayTypes()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<TextPropertyConverter>(registry.GetConverterForType(typeof(string[])));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterForType(typeof(int[])));
    }

    #endregion

    #region PropertyConverterRegistry Serialization Tests

    /// <summary>
    /// The test data class
    /// </summary>
    public class TestDataClass
    {
        /// <summary>
        /// Gets or sets the value of the name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the age
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Gets or sets the value of the score
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Gets or sets the value of the is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the value of the created at
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the value of the id
        /// </summary>
        public Guid Id { get; set; }
    }

    /// <summary>
    /// Tests that registry serialize to rest serializes all properties
    /// </summary>
    [Fact]
    public void Registry_SerializeToRest_SerializesAllProperties()
    {
        var registry = PropertyConverterRegistry.Default;
        var obj = new TestDataClass
        {
            Name = "Test",
            Age = 30,
            Score = 95.5,
            IsActive = true,
            CreatedAt = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
            Id = Guid.Parse("12345678-1234-1234-1234-123456789012"),
        };

        var result = registry.SerializeToRest(obj);

        Assert.Equal("Test", result["name"]);
        Assert.Equal(30L, result["age"]);
        Assert.Equal(95.5, result["score"]);
        Assert.Equal(true, result["isActive"]);
        Assert.Equal("2024-01-15T00:00:00.0000000Z", result["createdAt"]);
        Assert.Equal("12345678-1234-1234-1234-123456789012", result["id"]);
    }

    /// <summary>
    /// Tests that registry build concrete type from properties deserializes all properties
    /// </summary>
    [Fact]
    public void Registry_BuildConcreteTypeFromProperties_DeserializesAllProperties()
    {
        var registry = PropertyConverterRegistry.Default;
        var dict = new Dictionary<string, object?>
        {
            ["name"] = "Test",
            ["age"] = 30.0,
            ["score"] = 95.5,
            ["isActive"] = true,
            ["createdAt"] = "2024-01-15T00:00:00Z",
            ["id"] = "12345678-1234-1234-1234-123456789012",
        };

        var result = registry.BuildConcreteTypeFromProperties<TestDataClass>(dict);

        Assert.NotNull(result);
        Assert.Equal("Test", result.Name);
        Assert.Equal(30, result.Age);
        Assert.Equal(95.5, result.Score);
        Assert.True(result.IsActive);
        Assert.Equal(2024, result.CreatedAt.Year);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789012"), result.Id);
    }

    #endregion

    #region PropertyBag Tests

    /// <summary>
    /// Tests that property bag get string returns string value
    /// </summary>
    [Fact]
    public void PropertyBag_GetString_ReturnsStringValue()
    {
        var bag = new PropertyBag { ["name"] = "Test" };
        Assert.Equal("Test", bag.GetString("name"));
    }

    /// <summary>
    /// Tests that property bag get int converts from double
    /// </summary>
    [Fact]
    public void PropertyBag_GetInt_ConvertsFromDouble()
    {
        var bag = new PropertyBag { ["count"] = 42.0 };
        Assert.Equal(42, bag.GetInt("count"));
    }

    /// <summary>
    /// Tests that property bag get bool returns bool value
    /// </summary>
    [Fact]
    public void PropertyBag_GetBool_ReturnsBoolValue()
    {
        var bag = new PropertyBag { ["active"] = true };
        Assert.True(bag.GetBool("active"));
    }

    /// <summary>
    /// Tests that property bag get date time parses string
    /// </summary>
    [Fact]
    public void PropertyBag_GetDateTime_ParsesString()
    {
        var bag = new PropertyBag { ["date"] = "2024-01-15T10:30:00Z" };
        var result = bag.GetDateTime("date");
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
    }

    /// <summary>
    /// Tests that property bag get guid parses string
    /// </summary>
    [Fact]
    public void PropertyBag_GetGuid_ParsesString()
    {
        var bag = new PropertyBag { ["id"] = "12345678-1234-1234-1234-123456789012" };
        var result = bag.GetGuid("id");
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789012"), result.Value);
    }

    /// <summary>
    /// Tests that property bag get geo parses dictionary
    /// </summary>
    [Fact]
    public void PropertyBag_GetGeo_ParsesDictionary()
    {
        var bag = new PropertyBag
        {
            ["location"] = new Dictionary<string, object?>
            {
                ["latitude"] = 52.52,
                ["longitude"] = 13.405,
            },
        };
        var result = bag.GetGeo("location");
        Assert.NotNull(result);
        Assert.Equal(52.52f, result.Latitude, 2);
    }

    /// <summary>
    /// Tests that property bag case insensitive
    /// </summary>
    [Fact]
    public void PropertyBag_CaseInsensitive()
    {
        var bag = new PropertyBag { ["Name"] = "Test" };
        Assert.Equal("Test", bag.GetString("name"));
        Assert.Equal("Test", bag.GetString("NAME"));
    }

    /// <summary>
    /// Tests that property bag get missing returns null
    /// </summary>
    [Fact]
    public void PropertyBag_GetMissing_ReturnsNull()
    {
        var bag = new PropertyBag();
        Assert.Null(bag.GetString("missing"));
        Assert.Null(bag.GetInt("missing"));
    }

    #endregion
}
