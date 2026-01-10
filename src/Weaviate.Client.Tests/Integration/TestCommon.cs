namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The test common class
/// </summary>
/// <seealso cref="IntegrationTests"/>
[Collection("TestCommon")]
public class TestCommon : IntegrationTests
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCommon"/> class
    /// </summary>
    public TestCommon()
        : base() { }

    /// <summary>
    /// Tests that test version is in range
    /// </summary>
    [Fact]
    public void TestVersionIsInRange()
    {
        Assert.True(VersionIsInRange(System.Version.Parse("1.2.3"), "1.0.0"));
        Assert.True(VersionIsInRange(System.Version.Parse("1.2.3"), "1.0.0", "2.0.0"));
        Assert.False(VersionIsInRange(System.Version.Parse("1.2.3"), "1.2.4"));
        Assert.False(VersionIsInRange(System.Version.Parse("1.2.3"), "1.0.0", "1.2.2"));
    }
}
