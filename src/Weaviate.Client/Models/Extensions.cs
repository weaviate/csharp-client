namespace Weaviate.Client.Models;

internal static class ModelsToDtoExtensions
{
    internal static Rest.Dto.GeoCoordinates ToDto(this Models.GeoCoordinate model)
    {
        return new Rest.Dto.GeoCoordinates
        {
            Latitude = model.Latitude,
            Longitude = model.Longitude,
        };
    }

    internal static Rest.Dto.PhoneNumber ToDto(this Models.PhoneNumber model)
    {
        return new Rest.Dto.PhoneNumber
        {
            Input = model.Input,
            DefaultCountry = model.DefaultCountry,
            CountryCode = model.CountryCode,
            National = model.National,
            InternationalFormatted = model.InternationalFormatted,
            NationalFormatted = model.NationalFormatted,
            Valid = model.Valid,
        };
    }

    internal static Rest.Dto.NestedProperty ToNestedPropertyDto(this Models.Property property)
    {
        return new Rest.Dto.NestedProperty
        {
            Name = property.Name,
            DataType = [property.DataType.ToEnumMemberString()],
            Description = property.Description,
            IndexFilterable = property.IndexFilterable,
            IndexSearchable = property.IndexSearchable,
            IndexRangeFilters = property.IndexRangeFilters,
            Tokenization = (Rest.Dto.NestedPropertyTokenization?)property.PropertyTokenization,
            NestedProperties = property
                .NestedProperties?.Select(np => np.ToNestedPropertyDto())
                .ToList(),
        };
    }

    internal static Rest.Dto.Property ToDto(this Models.Reference reference)
    {
        IReferenceBase refProp = reference;

        return new Rest.Dto.Property
        {
            Name = refProp.Name,
            DataType = refProp.TargetCollections,
            Description = refProp.Description,
        };
    }

    internal static Rest.Dto.Property ToDto(
        this Models.Property property,
        IEnumerable<string?>? vectorizers = null
    )
    {
        // Build moduleConfig dictionary if vectorizers are provided
        Dictionary<string, object>? moduleConfig = null;
        if (vectorizers != null && vectorizers.Any())
        {
            moduleConfig = new Dictionary<string, object>();
            foreach (var vectorizer in vectorizers)
            {
                if (vectorizer != null)
                {
                    moduleConfig[vectorizer] = new Dictionary<string, object>
                    {
                        ["skip"] = property.SkipVectorization,
                        ["vectorizePropertyName"] = property.VectorizePropertyName,
                    };
                }
            }
        }

        return new Rest.Dto.Property
        {
            Name = property.Name,
            DataType = [property.DataType.ToEnumMemberString()],
            Description = property.Description,
            IndexFilterable = property.IndexFilterable,
#pragma warning disable CS0612 // Type or member is obsolete
            IndexInverted = property.IndexInverted,
#pragma warning restore CS0612 // Type or member is obsolete
            IndexRangeFilters = property.IndexRangeFilters,
            IndexSearchable = property.IndexSearchable,
            Tokenization = (Rest.Dto.PropertyTokenization?)property.PropertyTokenization,
            NestedProperties = property
                .NestedProperties?.Select(np => np.ToNestedPropertyDto())
                .ToList(),
            ModuleConfig = moduleConfig,
        };
    }
}
