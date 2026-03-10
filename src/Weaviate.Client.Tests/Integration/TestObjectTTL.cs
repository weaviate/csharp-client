using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// The object TTL tests class
/// </summary>
/// <seealso cref="IntegrationTests"/>
public partial class ObjectTTL : IntegrationTests
{
    /// <summary>
    /// Initializes this instance
    /// </summary>
    /// <returns>The value task</returns>
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        RequireVersion("1.35.0");
    }

    [Fact]
    public async Task Test_ObjectTTL_Creation()
    {
        var collection = await CollectionFactory(
            objectTTLConfig: ObjectTTLConfig.ByCreationTime(
                TimeSpan.FromDays(30),
                filterExpiredObjects: true
            ),
            invertedIndexConfig: new InvertedIndexConfig { IndexTimestamps = true }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(config.ObjectTTLConfig);
        Assert.Equal("_creationTimeUnix", config.ObjectTTLConfig.DeleteOn);
        Assert.Equal(30 * 24 * 3600, config.ObjectTTLConfig.DefaultTTL);
        Assert.True(config.ObjectTTLConfig.FilterExpiredObjects);
    }

    [Fact]
    public async Task Test_ObjectTTL_UpdateTime()
    {
        var collection = await CollectionFactory(
            objectTTLConfig: ObjectTTLConfig.ByUpdateTime(
                TimeSpan.FromDays(30),
                filterExpiredObjects: true
            ),
            invertedIndexConfig: new InvertedIndexConfig { IndexTimestamps = true }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(config.ObjectTTLConfig);
        Assert.Equal("_lastUpdateTimeUnix", config.ObjectTTLConfig.DeleteOn);
        Assert.True(config.ObjectTTLConfig.FilterExpiredObjects);
        Assert.Equal(30 * 24 * 3600, config.ObjectTTLConfig.DefaultTTL);
    }

    [Fact]
    public async Task Test_ObjectTTL_CustomProperty()
    {
        var collection = await CollectionFactory(
            properties: new[]
            {
                new Property { Name = "customDate", DataType = DataType.Date },
            },
            objectTTLConfig: ObjectTTLConfig.ByDateProperty(
                "customDate",
                -1,
                filterExpiredObjects: false
            ),
            invertedIndexConfig: new InvertedIndexConfig { IndexTimestamps = true }
        );

        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(config.ObjectTTLConfig);
        Assert.Equal("customDate", config.ObjectTTLConfig.DeleteOn);
        Assert.Equal(-1, config.ObjectTTLConfig.DefaultTTL);
        Assert.False(config.ObjectTTLConfig.FilterExpiredObjects);
    }

    [Fact]
    public async Task Test_ObjectTTL_Update()
    {
        var collection = await CollectionFactory<object>(
            properties: new[]
            {
                new Property { Name = "customDate", DataType = DataType.Date },
                new Property { Name = "customDate2", DataType = DataType.Date },
            },
            invertedIndexConfig: new InvertedIndexConfig { IndexTimestamps = true }
        );

        var conf = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(conf.ObjectTTLConfig);
        Assert.False(conf.ObjectTTLConfig.Enabled);

        // Update to customDate
        await collection.Config.Update(
            c => c.ObjectTTLConfig.ByDateProperty("customDate", 3600, filterExpiredObjects: true),
            cancellationToken: TestContext.Current.CancellationToken
        );
        conf = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(conf.ObjectTTLConfig);
        Assert.Equal("customDate", conf.ObjectTTLConfig.DeleteOn);
        Assert.Equal(3600, conf.ObjectTTLConfig.DefaultTTL);
        Assert.True(conf.ObjectTTLConfig.FilterExpiredObjects);

        // Update to update time
        await collection.Config.Update(
            c => c.ObjectTTLConfig.ByUpdateTime(3600, filterExpiredObjects: false),
            cancellationToken: TestContext.Current.CancellationToken
        );
        conf = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(conf.ObjectTTLConfig);
        Assert.Equal("_lastUpdateTimeUnix", conf.ObjectTTLConfig.DeleteOn);
        Assert.Equal(3600, conf.ObjectTTLConfig.DefaultTTL);
        Assert.False(conf.ObjectTTLConfig.FilterExpiredObjects);

        // Update to creation time
        await collection.Config.Update(
            c => c.ObjectTTLConfig.ByCreationTime(600),
            cancellationToken: TestContext.Current.CancellationToken
        );
        conf = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(conf.ObjectTTLConfig);
        Assert.Equal("_creationTimeUnix", conf.ObjectTTLConfig.DeleteOn);
        Assert.Equal(600, conf.ObjectTTLConfig.DefaultTTL);
        Assert.False(conf.ObjectTTLConfig.FilterExpiredObjects);

        // Disable TTL
        await collection.Config.Update(
            c => c.ObjectTTLConfig.Disable(),
            cancellationToken: TestContext.Current.CancellationToken
        );
        conf = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(conf.ObjectTTLConfig);
        Assert.False(conf.ObjectTTLConfig.Enabled);
    }
}
