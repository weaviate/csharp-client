namespace Weaviate.Client.Rest.Dto;

internal partial class NestedProperty
{
    public Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.ToList() ?? new List<string>(),
            Description = Description,
            IndexFilterable = IndexFilterable,
            IndexSearchable = IndexSearchable,
            IndexRangeFilters = IndexRangeFilters,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
        };
    }
}

internal partial class Property {
    public  Models.Property ToModel()
    {
        return new Models.Property
        {
            Name = Name ?? string.Empty,
            DataType = DataType?.ToList() ?? new List<string>(),
            Description = Description,
            IndexFilterable = IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
            IndexInverted = IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
            IndexRangeFilters = IndexRangeFilters,
            IndexSearchable = IndexSearchable,
            PropertyTokenization = (Models.PropertyTokenization?)Tokenization,
            NestedProperties = NestedProperties?.Select(np => np.ToModel()).ToArray(),
        };
    }
}

internal partial class GeoCoordinates
{
    public Models.GeoCoordinate ToModel()
    {
        return new Models.GeoCoordinate(Latitude!.Value, Longitude!.Value);
    }
}

internal partial class PhoneNumber
{
    public Models.PhoneNumber ToModel()
    {
        return new Models.PhoneNumber(Input!)
        {
            DefaultCountry = DefaultCountry,
            CountryCode = CountryCode is null ? null :Convert.ToUInt64(CountryCode),
            National = National is null ? null : Convert.ToUInt64(National),
            InternationalFormatted = InternationalFormatted,
            NationalFormatted = NationalFormatted,
            Valid = Valid,
        };
    }
}

