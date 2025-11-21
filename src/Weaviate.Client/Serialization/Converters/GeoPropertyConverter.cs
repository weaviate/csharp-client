using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "geoCoordinates" data type (no array variant).
/// </summary>
public class GeoPropertyConverter : PropertyConverterBase
{
    public override string DataType => "geoCoordinates";
    public override bool SupportsArray => false;

    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(GeoCoordinate)];

    public override object? ToRest(object? value)
    {
        if (value is not GeoCoordinate geo)
            return null;

        return new Dictionary<string, object>
        {
            ["latitude"] = geo.Latitude,
            ["longitude"] = geo.Longitude,
        };
    }

    public override Value ToGrpc(object? value)
    {
        if (value is not GeoCoordinate geo)
            return Value.ForNull();

        var geoStruct = new Struct();
        geoStruct.Fields["latitude"] = Value.ForNumber(geo.Latitude);
        geoStruct.Fields["longitude"] = Value.ForNumber(geo.Longitude);

        return Value.ForStruct(geoStruct);
    }

    public override object? FromRest(object? value, System.Type targetType)
    {
        if (value is null)
            return null;

        if (value is IDictionary<string, object> dict)
        {
            var lat = Convert.ToSingle(dict.TryGetValue("latitude", out var latVal) ? latVal : 0);
            var lon = Convert.ToSingle(dict.TryGetValue("longitude", out var lonVal) ? lonVal : 0);
            return new GeoCoordinate(lat, lon);
        }

        // Handle REST DTO type
        if (value is Rest.Dto.GeoCoordinates dtoGeo)
        {
            return new GeoCoordinate((float)dtoGeo.Latitude, (float)dtoGeo.Longitude);
        }

        return null;
    }

    public override object? FromGrpc(Value value, System.Type targetType)
    {
        if (value.KindCase == Value.KindOneofCase.NullValue)
            return null;

        if (value.KindCase == Value.KindOneofCase.StructValue)
        {
            var s = value.StructValue;
            var lat = s.Fields.TryGetValue("latitude", out var latVal)
                ? (float)latVal.NumberValue
                : 0f;
            var lon = s.Fields.TryGetValue("longitude", out var lonVal)
                ? (float)lonVal.NumberValue
                : 0f;
            return new GeoCoordinate(lat, lon);
        }

        return null;
    }
}
