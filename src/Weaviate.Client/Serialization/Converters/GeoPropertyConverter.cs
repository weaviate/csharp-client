using Google.Protobuf.WellKnownTypes;
using Weaviate.Client.Models;

namespace Weaviate.Client.Serialization.Converters;

/// <summary>
/// Converter for Weaviate "geoCoordinates" data type (no array variant).
/// </summary>
internal class GeoPropertyConverter : PropertyConverterBase
{
    public override string DataType => "geoCoordinates";
    public override bool SupportsArray => false;

    public override IReadOnlyList<System.Type> SupportedTypes => [typeof(GeoCoordinate)];

    public override object? ToRest(object? value)
    {
        if (value is not GeoCoordinate geo)
            return null;

        return geo.ToDto();
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

        // If already a GeoCoordinate model, return it
        if (value is GeoCoordinate geo)
            return geo;

        // Handle REST DTO type
        if (value is Rest.Dto.GeoCoordinates dtoGeo)
        {
            return dtoGeo.ToModel();
        }

        // Handle dictionary (both nullable and non-nullable)
        // Try both capitalized and lowercase keys for compatibility
        if (value is IDictionary<string, object?> dictNullable)
        {
            var lat = Convert.ToSingle(
                dictNullable.TryGetValue("latitude", out var latVal) ? latVal
                : dictNullable.TryGetValue("Latitude", out latVal) ? latVal
                : 0
            );
            var lon = Convert.ToSingle(
                dictNullable.TryGetValue("longitude", out var lonVal) ? lonVal
                : dictNullable.TryGetValue("Longitude", out lonVal) ? lonVal
                : 0
            );
            return new GeoCoordinate(lat, lon);
        }

        if (value is IDictionary<string, object> dict2)
        {
            var lat = Convert.ToSingle(
                dict2.TryGetValue("latitude", out var latVal) ? latVal
                : dict2.TryGetValue("Latitude", out latVal) ? latVal
                : 0
            );
            var lon = Convert.ToSingle(
                dict2.TryGetValue("longitude", out var lonVal) ? lonVal
                : dict2.TryGetValue("Longitude", out lonVal) ? lonVal
                : 0
            );
            return new GeoCoordinate(lat, lon);
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
