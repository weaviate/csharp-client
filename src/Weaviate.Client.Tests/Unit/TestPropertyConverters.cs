using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;
using Weaviate.Client.Serialization;
using Weaviate.Client.Serialization.Converters;

namespace Weaviate.Client.Tests.Unit;

public class TestPropertyConverters
{
    #region TextPropertyConverter Tests

    [Fact]
    public void TextConverter_ToRest_ReturnsString()
    {
        var converter = new TextPropertyConverter();
        Assert.Equal("hello", converter.ToRest("hello"));
        Assert.Null(converter.ToRest(null));
    }

    [Fact]
    public void TextConverter_ToGrpc_ReturnsStringValue()
    {
        var converter = new TextPropertyConverter();
        var result = converter.ToGrpc("hello");
        Assert.Equal(Value.KindOneofCase.StringValue, result.KindCase);
        Assert.Equal("hello", result.StringValue);
    }

    [Fact]
    public void TextConverter_FromRest_ReturnsString()
    {
        var converter = new TextPropertyConverter();
        Assert.Equal("hello", converter.FromRest("hello", typeof(string)));
    }

    [Fact]
    public void TextConverter_FromGrpc_ReturnsString()
    {
        var converter = new TextPropertyConverter();
        var grpcValue = Value.ForString("hello");
        Assert.Equal("hello", converter.FromGrpc(grpcValue, typeof(string)));
    }

    [Fact]
    public void TextConverter_Array_RoundTrip()
    {
        var converter = new TextPropertyConverter();
        var input = new object?[] { "a", "b", "c" };

        var rest = converter.ToRestArray(input);
        Assert.IsType<object?[]>(rest);

        var grpc = converter.ToGrpcArray(input);
        Assert.Equal(3, grpc.Values.Count);
    }

    #endregion

    #region IntPropertyConverter Tests

    [Fact]
    public void IntConverter_ToRest_HandlesVariousIntTypes()
    {
        var converter = new IntPropertyConverter();
        Assert.Equal(42, converter.ToRest(42));
        Assert.Equal(42L, converter.ToRest(42L));
        Assert.Equal(42, converter.ToRest((short)42));
        Assert.Equal(42, converter.ToRest((byte)42));
    }

    [Fact]
    public void IntConverter_ToGrpc_ReturnsNumberValue()
    {
        var converter = new IntPropertyConverter();
        var result = converter.ToGrpc(42);
        Assert.Equal(Value.KindOneofCase.NumberValue, result.KindCase);
        Assert.Equal(42, result.NumberValue);
    }

    [Fact]
    public void IntConverter_FromRest_ReturnsLong()
    {
        var converter = new IntPropertyConverter();
        var result = converter.FromRest(42.0, typeof(int));
        Assert.IsType<long>(result);
        Assert.Equal(42L, result);
    }

    [Fact]
    public void IntConverter_FromGrpc_ReturnsLong()
    {
        var converter = new IntPropertyConverter();
        var grpcValue = Value.ForNumber(42);
        var result = converter.FromGrpc(grpcValue, typeof(int));
        Assert.IsType<long>(result);
        Assert.Equal(42L, result);
    }

    #endregion

    #region NumberPropertyConverter Tests

    [Fact]
    public void NumberConverter_ToRest_HandlesVariousNumberTypes()
    {
        var converter = new NumberPropertyConverter();
        Assert.Equal(3.14, converter.ToRest(3.14));
        Assert.Equal(3.14, (double)converter.ToRest(3.14f)!, 2);
        Assert.Equal(3.14, converter.ToRest(3.14m));
    }

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

    [Fact]
    public void BoolConverter_ToRest_ReturnsBool()
    {
        var converter = new BoolPropertyConverter();
        Assert.Equal(true, converter.ToRest(true));
        Assert.Equal(false, converter.ToRest(false));
    }

    [Fact]
    public void BoolConverter_ToGrpc_ReturnsBoolValue()
    {
        var converter = new BoolPropertyConverter();
        var result = converter.ToGrpc(true);
        Assert.Equal(Value.KindOneofCase.BoolValue, result.KindCase);
        Assert.True(result.BoolValue);
    }

    [Fact]
    public void BoolConverter_FromGrpc_ReturnsBool()
    {
        var converter = new BoolPropertyConverter();
        Assert.Equal(true, converter.FromGrpc(Value.ForBool(true), typeof(bool)));
        Assert.Equal(false, converter.FromGrpc(Value.ForBool(false), typeof(bool)));
    }

    #endregion

    #region DatePropertyConverter Tests

    [Fact]
    public void DateConverter_ToRest_ReturnsIso8601String()
    {
        var converter = new DatePropertyConverter();
        var date = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var result = converter.ToRest(date);
        Assert.Equal("2024-01-15T10:30:00Z", result);
    }

    [Fact]
    public void DateConverter_FromRest_ParsesIso8601()
    {
        var converter = new DatePropertyConverter();
        var result = converter.FromRest("2024-01-15T10:30:00Z", typeof(DateTime));
        Assert.IsType<DateTime>(result);
        var dt = (DateTime)result!;
        Assert.Equal(2024, dt.Year);
        Assert.Equal(1, dt.Month);
        Assert.Equal(15, dt.Day);
        Assert.Equal(DateTimeKind.Utc, dt.Kind);
    }

    [Fact]
    public void DateConverter_FromRest_ToDateTimeOffset()
    {
        var converter = new DatePropertyConverter();
        var result = converter.FromRest("2024-01-15T10:30:00Z", typeof(DateTimeOffset));
        Assert.IsType<DateTimeOffset>(result);
    }

    #endregion

    #region UuidPropertyConverter Tests

    [Fact]
    public void UuidConverter_ToRest_ReturnsGuidString()
    {
        var converter = new UuidPropertyConverter();
        var guid = Guid.Parse("12345678-1234-1234-1234-123456789012");
        var result = converter.ToRest(guid);
        Assert.Equal("12345678-1234-1234-1234-123456789012", result);
    }

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

    [Fact]
    public void BlobConverter_ToRest_ReturnsBase64()
    {
        var converter = new BlobPropertyConverter();
        var bytes = new byte[] { 1, 2, 3, 4 };
        var result = converter.ToRest(bytes);
        Assert.Equal(Convert.ToBase64String(bytes), result);
    }

    [Fact]
    public void BlobConverter_FromRest_DecodesBase64()
    {
        var converter = new BlobPropertyConverter();
        var base64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4 });
        var result = converter.FromRest(base64, typeof(byte[]));
        Assert.IsType<byte[]>(result);
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, result);
    }

    [Fact]
    public void BlobConverter_DoesNotSupportArray()
    {
        var converter = new BlobPropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    #endregion

    #region GeoPropertyConverter Tests

    [Fact]
    public void GeoConverter_ToRest_ReturnsDictionary()
    {
        var converter = new GeoPropertyConverter();
        var geo = new GeoCoordinate(52.52f, 13.405f);
        var result = converter.ToRest(geo);

        Assert.IsType<Dictionary<string, object>>(result);
        var dict = (Dictionary<string, object>)result!;
        Assert.Equal(52.52f, dict["latitude"]);
        Assert.Equal(13.405f, dict["longitude"]);
    }

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

    [Fact]
    public void GeoConverter_DoesNotSupportArray()
    {
        var converter = new GeoPropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    #endregion

    #region PhonePropertyConverter Tests

    [Fact]
    public void PhoneConverter_ToRest_ReturnsDictionary()
    {
        var converter = new PhonePropertyConverter();
        var phone = new PhoneNumber("+49 123 456789") { DefaultCountry = "DE" };
        var result = converter.ToRest(phone);

        Assert.IsType<Dictionary<string, object?>>(result);
        var dict = (Dictionary<string, object?>)result!;
        Assert.Equal("+49 123 456789", dict["input"]);
        Assert.Equal("DE", dict["defaultCountry"]);
    }

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

    [Fact]
    public void PhoneConverter_DoesNotSupportArray()
    {
        var converter = new PhonePropertyConverter();
        Assert.False(converter.SupportsArray);
    }

    #endregion

    #region PropertyConverterRegistry Tests

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
        Assert.IsType<GeoPropertyConverter>(registry.GetConverterByDataType("geoCoordinates"));
        Assert.IsType<PhonePropertyConverter>(registry.GetConverterByDataType("phoneNumber"));
    }

    [Fact]
    public void Registry_GetConverterByDataType_HandlesArrayTypes()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<TextPropertyConverter>(registry.GetConverterByDataType("text[]"));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterByDataType("int[]"));
    }

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

    [Fact]
    public void Registry_GetConverterForType_HandlesNullableTypes()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<IntPropertyConverter>(registry.GetConverterForType(typeof(int?)));
        Assert.IsType<BoolPropertyConverter>(registry.GetConverterForType(typeof(bool?)));
        Assert.IsType<DatePropertyConverter>(registry.GetConverterForType(typeof(DateTime?)));
    }

    [Fact]
    public void Registry_GetConverterForType_HandlesArrayTypes()
    {
        var registry = PropertyConverterRegistry.Default;

        Assert.IsType<TextPropertyConverter>(registry.GetConverterForType(typeof(string[])));
        Assert.IsType<IntPropertyConverter>(registry.GetConverterForType(typeof(int[])));
    }

    #endregion

    #region PropertyConverterRegistry Serialization Tests

    public class TestDataClass
    {
        public string? Name { get; set; }
        public int Age { get; set; }
        public double Score { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid Id { get; set; }
    }

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
        Assert.Equal(30, result["age"]);
        Assert.Equal(95.5, result["score"]);
        Assert.Equal(true, result["isActive"]);
        Assert.Equal("2024-01-15T00:00:00Z", result["createdAt"]);
        Assert.Equal("12345678-1234-1234-1234-123456789012", result["id"]);
    }

    [Fact]
    public void Registry_DeserializeFromRest_DeserializesAllProperties()
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

        var result = registry.DeserializeFromRest<TestDataClass>(dict);

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

    [Fact]
    public void PropertyBag_GetString_ReturnsStringValue()
    {
        var bag = new PropertyBag { ["name"] = "Test" };
        Assert.Equal("Test", bag.GetString("name"));
    }

    [Fact]
    public void PropertyBag_GetInt_ConvertsFromDouble()
    {
        var bag = new PropertyBag { ["count"] = 42.0 };
        Assert.Equal(42, bag.GetInt("count"));
    }

    [Fact]
    public void PropertyBag_GetBool_ReturnsBoolValue()
    {
        var bag = new PropertyBag { ["active"] = true };
        Assert.True(bag.GetBool("active"));
    }

    [Fact]
    public void PropertyBag_GetDateTime_ParsesString()
    {
        var bag = new PropertyBag { ["date"] = "2024-01-15T10:30:00Z" };
        var result = bag.GetDateTime("date");
        Assert.NotNull(result);
        Assert.Equal(2024, result.Value.Year);
    }

    [Fact]
    public void PropertyBag_GetGuid_ParsesString()
    {
        var bag = new PropertyBag { ["id"] = "12345678-1234-1234-1234-123456789012" };
        var result = bag.GetGuid("id");
        Assert.NotNull(result);
        Assert.Equal(Guid.Parse("12345678-1234-1234-1234-123456789012"), result.Value);
    }

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

    [Fact]
    public void PropertyBag_CaseInsensitive()
    {
        var bag = new PropertyBag { ["Name"] = "Test" };
        Assert.Equal("Test", bag.GetString("name"));
        Assert.Equal("Test", bag.GetString("NAME"));
    }

    [Fact]
    public void PropertyBag_GetMissing_ReturnsNull()
    {
        var bag = new PropertyBag();
        Assert.Null(bag.GetString("missing"));
        Assert.Null(bag.GetInt("missing"));
    }

    #endregion
}
