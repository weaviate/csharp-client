namespace Weaviate.Client.Models;

public static class DataType
{
    public static string Date { get; } = "date";
    public static string GeoCoordinate { get; } = "geo";
    public static string Int { get; } = "int";
    public static string List { get; } = "list";
    public static string Number { get; } = "number";
    public static string Object { get; } = "object";
    public static string PhoneNumber { get; } = "phone";
    public static string Text { get; } = "text";
    public static string Uuid { get; } = "uuid";

    public static string Reference(string property) => property.Capitalize();
}

public class ReferenceProperty
{
    public required string Name { get; set; }
    public required string TargetCollection { get; set; }

    public static implicit operator Property(ReferenceProperty p)
    {
        return new Property
        {
            Name = p.Name,
            DataType = { DataType.Reference(p.TargetCollection) },
        };
    }
}

public class Property
{
    public required string Name { get; set; }
    public IList<string> DataType { get; set; } = new List<string>();
    public string? Description { get; set; }
    public bool? IndexFilterable { get; set; }
    public bool? IndexInverted { get; set; }
    public bool? IndexRangeFilters { get; set; }
    public bool? IndexSearchable { get; set; }

    public static Property Text(string name)
    {
        return new Property { Name = name, DataType = { Models.DataType.Text } };
    }

    public static Property Int(string name)
    {
        return new Property { Name = name, DataType = { Models.DataType.Int } };
    }

    public static Property Date(string name)
    {
        return new Property { Name = name, DataType = { Models.DataType.Date } };
    }

    public static Property Number(string name)
    {
        return new Property { Name = name, DataType = { Models.DataType.Number } };
    }

    public static ReferenceProperty Reference(string name, string targetCollection)
    {
        return new ReferenceProperty { Name = name, TargetCollection = targetCollection };
    }

    // Extract collection properties from type specified by TData.
    public static IList<Property> FromType<TData>()
    {
        return
        [
            .. typeof(TData)
                .GetProperties()
                .Select(x =>
                    Type.GetTypeCode(x.PropertyType) switch
                    {
                        TypeCode.String => Property.Text(x.Name),
                        TypeCode.Int16 => Property.Int(x.Name),
                        TypeCode.UInt16 => Property.Int(x.Name),
                        TypeCode.Int32 => Property.Int(x.Name),
                        TypeCode.UInt32 => Property.Int(x.Name),
                        TypeCode.Int64 => Property.Int(x.Name),
                        TypeCode.UInt64 => Property.Int(x.Name),
                        TypeCode.DateTime => Property.Date(x.Name),
                        TypeCode.Boolean => null,
                        TypeCode.Char => null,
                        TypeCode.SByte => null,
                        TypeCode.Byte => null,
                        TypeCode.Single => null,
                        TypeCode.Double => null,
                        TypeCode.Decimal => null,
                        TypeCode.Empty => null,
                        TypeCode.Object => null,
                        TypeCode.DBNull => null,
                        _ => null,
                    }
                )
                .Where(p => p is not null)
                .Select(p => p!),
        ];
    }
}
