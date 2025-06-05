namespace Weaviate.Client.Models;

public static class DataType
{
    public static string Date { get; } = "date";
    public static string GeoCoordinate { get; } = "geo";
    public static string Int { get; } = "int";
    public static string Bool { get; } = "boolean";
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

    internal static Property Bool(string name)
    {
        return new Property { Name = name, DataType = { Models.DataType.Bool } };
    }

    public static ReferenceProperty Reference(string name, string targetCollection)
    {
        return new ReferenceProperty { Name = name, TargetCollection = targetCollection };
    }

    private static Property For(Type t, string name)
    {
        Func<string, Property>? f = Type.GetTypeCode(t) switch
        {
            TypeCode.String => Text,
            TypeCode.Int16 => Int,
            TypeCode.UInt16 => Int,
            TypeCode.Int32 => Int,
            TypeCode.UInt32 => Int,
            TypeCode.Int64 => Int,
            TypeCode.UInt64 => Int,
            TypeCode.DateTime => Date,
            TypeCode.Boolean => Bool,
            TypeCode.Char => Text,
            TypeCode.SByte => null,
            TypeCode.Byte => null,
            TypeCode.Single => Number,
            TypeCode.Double => Number,
            TypeCode.Decimal => Number,
            TypeCode.Empty => null,
            TypeCode.Object => null,
            TypeCode.DBNull => null,
            _ => null,
        };

        return f!(name);
    }

    public static Property For<TField>(string name)
    {
        return For(typeof(TField), name);
    }

    // Extract collection properties from type specified by TData.
    public static IList<Property> FromCollection<TData>()
    {
        return [.. typeof(TData).GetProperties().Select(x => For(x.PropertyType, x.Name))];
    }
}
