namespace Weaviate.Client.Models;

/// <summary>
/// The models to dto extensions class
/// </summary>
internal static class ModelsToDtoExtensions
{
    /// <summary>
    /// Returns the dto using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The rest dto geo coordinates</returns>
    internal static Rest.Dto.GeoCoordinates ToDto(this GeoCoordinate model)
    {
        return new Rest.Dto.GeoCoordinates
        {
            Latitude = model.Latitude,
            Longitude = model.Longitude,
        };
    }

    /// <summary>
    /// Returns the dto using the specified model
    /// </summary>
    /// <param name="model">The model</param>
    /// <returns>The rest dto phone number</returns>
    internal static Rest.Dto.PhoneNumber ToDto(this PhoneNumber model)
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

    /// <summary>
    /// Returns the nested property dto using the specified property
    /// </summary>
    /// <param name="property">The property</param>
    /// <returns>The rest dto nested property</returns>
    internal static Rest.Dto.NestedProperty ToNestedPropertyDto(this Property property)
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
            TextAnalyzer = property.TextAnalyzer.ToDto(),
        };
    }

    /// <summary>
    /// Returns the dto using the specified reference
    /// </summary>
    /// <param name="reference">The reference</param>
    /// <returns>The rest dto property</returns>
    internal static Rest.Dto.Property ToDto(this Reference reference)
    {
        IReferenceBase refProp = reference;

        return new Rest.Dto.Property
        {
            Name = refProp.Name,
            DataType = refProp.TargetCollections,
            Description = refProp.Description,
        };
    }

    /// <summary>
    /// Returns the dto using the specified property
    /// </summary>
    /// <param name="property">The property</param>
    /// <param name="vectorizers">The vectorizers</param>
    /// <returns>The rest dto property</returns>
    internal static Rest.Dto.Property ToDto(
        this Property property,
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
            TextAnalyzer = property.TextAnalyzer.ToDto(),
        };
    }
}
