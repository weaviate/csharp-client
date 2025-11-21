namespace Weaviate.Client.Tests.Integration;

[Collection("TestCommon")]
public class TestCommon : IntegrationTests
{
    public TestCommon()
        : base() { }

    [Fact]
    public void TestVersionIsInRange()
    {
        Assert.True(VersionIsInRange(System.Version.Parse("1.2.3"), "1.0.0"));
        Assert.True(VersionIsInRange(System.Version.Parse("1.2.3"), "1.0.0", "2.0.0"));
        Assert.False(VersionIsInRange(System.Version.Parse("1.2.3"), "1.2.4"));
        Assert.False(VersionIsInRange(System.Version.Parse("1.2.3"), "1.0.0", "1.2.2"));
    }
}
