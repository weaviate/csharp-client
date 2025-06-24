using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

public partial class UnitTests
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
    public void Properties(string test)
    {
        var (f1, f2) = PropertyCases[test];
        var (p1, p2) = (f1(test), f2(test));

        Assert.Equivalent(p1, p2);
    }
}
