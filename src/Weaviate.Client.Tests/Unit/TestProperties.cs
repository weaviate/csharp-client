using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public partial class PropertyTests
{
    public static TheoryData<string> PropertyCasesKeys => [.. PropertyCases.Keys];

    private static Dictionary<string, (PropertyFactory, PropertyFactory)> PropertyCases = new()
    {
        ["Text"] = (Property.Text, Property<string>.New),
        ["TextArray"] = (Property.TextArray, Property<string[]>.New),
        ["Int"] = (Property.Int, Property<int>.New),
        ["IntArray"] = (Property.IntArray, Property<int[]>.New),
        ["Bool"] = (Property.Bool, Property<bool>.New),
        ["BoolArray"] = (Property.BoolArray, Property<bool[]>.New),
        ["Number"] = (Property.Number, Property<double>.New),
        ["NumberArray"] = (Property.NumberArray, Property<double[]>.New),
        ["Date"] = (Property.Date, Property<DateTime>.New),
        ["DateArray"] = (Property.DateArray, Property<DateTime[]>.New),
        ["Uuid"] = (Property.Uuid, Property<Guid>.New),
        ["UuidArray"] = (Property.UuidArray, Property<Guid[]>.New),
        ["Geo"] = (Property.GeoCoordinate, Property<GeoCoordinate>.New),
        ["Phone"] = (Property.PhoneNumber, Property<PhoneNumber>.New),
        // TODO Add support for the properties below
        // ["Blob"] = (Property.Blob, Property<byte[]>.New),
        // ["Object"] = (Property.Object, Property<object>.New),
        // ["ObjectArray"] = (Property.ObjectArray, Property<object[]>.New),
    };

    [Theory]
    [MemberData(nameof(PropertyCasesKeys))]
    public void Property_DataType_Helpers(string key)
    {
        var (f1, f2) = PropertyCases[key];
        var (p1, p2) = (f1(key), f2(key));

        Assert.Equivalent(p1, p2);
    }

    [Fact]
    public void Equals_Should_Return_True_For_Identical_Properties()
    {
        // Arrange
        var property1 = new Property
        {
            Name = "name",
            DataType = { "text" },
            Description = "description",
            IndexFilterable = true,
            IndexRangeFilters = false,
            IndexSearchable = true,
            PropertyTokenization = PropertyTokenization.Whitespace,
        };

        var property2 = new Property
        {
            Name = "name",
            DataType = { "text" },
            Description = "description",
            IndexFilterable = true,
            IndexRangeFilters = false,
            IndexSearchable = true,
            PropertyTokenization = PropertyTokenization.Whitespace,
        };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_Names_Differ()
    {
        // Arrange
        var property1 = new Property { Name = "name1" };
        var property2 = new Property { Name = "name2" };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_DataTypes_Differ()
    {
        // Arrange
        var property1 = new Property { Name = "prop", DataType = { "text" } };
        var property2 = new Property { Name = "prop", DataType = { "int" } };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_Descriptions_Differ()
    {
        // Arrange
        var property1 = new Property { Name = "prop", Description = "desc1" };
        var property2 = new Property { Name = "prop", Description = "desc2" };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_IndexFilterable_Differs()
    {
        // Arrange
        var property1 = new Property { Name = "prop", IndexFilterable = true };
        var property2 = new Property { Name = "prop", IndexFilterable = false };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_IndexRangeFilters_Differs()
    {
        // Arrange
        var property1 = new Property { Name = "prop", IndexRangeFilters = true };
        var property2 = new Property { Name = "prop", IndexRangeFilters = false };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_IndexSearchable_Differs()
    {
        // Arrange
        var property1 = new Property { Name = "prop", IndexSearchable = true };
        var property2 = new Property { Name = "prop", IndexSearchable = false };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_Should_Return_False_When_PropertyTokenization_Differs()
    {
        // Arrange
        var property1 = new Property
        {
            Name = "prop",
            PropertyTokenization = PropertyTokenization.Word,
        };
        var property2 = new Property
        {
            Name = "prop",
            PropertyTokenization = PropertyTokenization.Lowercase,
        };

        // Act
        var result = property1.Equals(property2);

        // Assert
        Assert.False(result);
    }
}
