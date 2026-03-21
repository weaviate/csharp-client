using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

public class TestDataReference
{
    private static readonly Guid _from = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _to1 = new("22222222-2222-2222-2222-222222222222");
    private static readonly Guid _to2 = new("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Beacon_ReturnsCorrectFormat()
    {
        var ref_ = new DataReference(_from, "hasAuthor", _to1) { FromCollection = "Articles" };
        Assert.Equal($"weaviate://localhost/Articles/{_from}/hasAuthor", ref_.Beacon);
    }

    [Fact]
    public void Beacon_DoesNotIncludeToCollection()
    {
        var ref_ = new DataReference(_from, "hasAuthor", _to1)
        {
            FromCollection = "Articles",
            ToCollection = "Authors",
        };
        Assert.Equal($"weaviate://localhost/Articles/{_from}/hasAuthor", ref_.Beacon);
    }

    [Fact]
    public void Beacon_NullWhenFromCollectionNotSet()
    {
        var ref_ = new DataReference(_from, "hasAuthor", _to1);
        Assert.Null(ref_.Beacon);
    }

    [Fact]
    public void ParamsConstructor_SetsMultipleTargets()
    {
        var ref_ = new DataReference(_from, "hasAuthor", _to1, _to2);
        Assert.Equal(2, ref_.To.Count());
        Assert.Null(ref_.ToCollection);
    }

    [Fact]
    public void FromCollection_CanBeSetViaInit()
    {
        var ref_ = new DataReference(_from, "hasAuthor", _to1) { FromCollection = "Articles" };
        Assert.Equal("Articles", ref_.FromCollection);
    }
}
