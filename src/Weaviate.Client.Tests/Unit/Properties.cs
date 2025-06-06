using Weaviate.Client.Models;

namespace Weaviate.Client.Tests;

public partial class UnitTests
{
    public static IEnumerable<object[]> CaseKeys => Cases.Keys.Select(k => new object[] { k });

    private static Dictionary<string, (Func<string, Property>, Func<string, Property>)> Cases =
        new()
        {
            ["Text"] = (Property.Text, Property.For<string>),
            ["TextArray"] = (Property.TextArray, Property.For<string[]>),
            ["Int"] = (Property.Int, Property.For<int>),
            ["IntArray"] = (Property.IntArray, Property.For<int[]>),
            ["Bool"] = (Property.Bool, Property.For<bool>),
            ["BoolArray"] = (Property.BoolArray, Property.For<bool[]>),
            ["Number"] = (Property.Number, Property.For<double>),
            ["NumberArray"] = (Property.NumberArray, Property.For<double[]>),
            ["Date"] = (Property.Date, Property.For<DateTime>),
            ["DateArray"] = (Property.DateArray, Property.For<DateTime[]>),
            ["Uuid"] = (Property.Uuid, Property.For<Guid>),
            ["UuidArray"] = (Property.UuidArray, Property.For<Guid[]>),
            ["Geo"] = (Property.GeoCoordinate, Property.For<GeoCoordinate>),
            ["Phone"] = (Property.PhoneNumber, Property.For<PhoneNumber>),
            // TODO Add support for the properties below
            // ["Blob"] = (Property.Blob, Property.For<byte[]>),
            // ["Object"] = (Property.Object, Property.For<object>),
            // ["ObjectArray"] = (Property.ObjectArray, Property.For<object[]>),
        };

    [Theory]
    [MemberData(nameof(CaseKeys))]
    public void Properties(string test)
    {
        var (f1, f2) = Cases[test];
        var (p1, p2) = (f1(test), f2(test));

        Assert.Equivalent(p1, p2);
    }
}
