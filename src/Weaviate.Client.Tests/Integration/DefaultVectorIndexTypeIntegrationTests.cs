using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;

namespace Weaviate.Client.Tests.Integration;

/// <summary>
/// End-to-end coverage for default vector index behavior across server versions.
/// Assertions rely on the connected server version reported by the test client.
/// </summary>
[Collection("DefaultVectorIndexTypeIntegrationTests")]
public sealed class DefaultVectorIndexTypeIntegrationTests : IntegrationTests
{
    private const string DefaultVectorIndexEnvVar = "DEFAULT_VECTOR_INDEX";
    private const string DefaultVectorIndexFallback = "hfresh";
    private static readonly Version ServerDefaultCutoff = Version.Parse(
        ServerVersions.DefaultVectorIndexTypeServerSide
    );

    private string ExpectedDefaultIndexType()
    {
        Assert.NotNull(_weaviate.WeaviateVersion);

        var defaultVectorIndex = Environment.GetEnvironmentVariable(DefaultVectorIndexEnvVar);
        if (string.IsNullOrWhiteSpace(defaultVectorIndex))
        {
            defaultVectorIndex = DefaultVectorIndexFallback;
        }

        return _weaviate.WeaviateVersion! >= ServerDefaultCutoff
            ? defaultVectorIndex
            : VectorIndex.HNSW.TypeValue;
    }

    /// <summary>
    /// Scenario A on the named-vector path: user omits <c>VectorIndexType</c>
    /// when configuring a named vector.
    /// <list type="bullet">
    ///   <item>1.37.5+: server applies <c>DEFAULT_VECTOR_INDEX</c> from the
    ///   host env var (default <see cref="DefaultVectorIndexFallback"/>).</item>
    ///   <item>Older: client injects <c>"hnsw"</c> and server stores it.</item>
    /// </list>
    /// </summary>
    [Fact]
    public async Task NamedVector_NoIndexType_GetsServerOrInjectedDefault()
    {
        var create = new CollectionCreateParams
        {
            Name = MakeUniqueCollectionName(
                nameof(NamedVector_NoIndexType_GetsServerOrInjectedDefault)
            ),
            Properties = [Property.Text("title")],
            VectorConfig = Configure.Vector("default", v => v.SelfProvided(), index: null),
        };

        var collection = await CollectionFactory<object>(create);
        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(config.VectorConfig);
        Assert.True(config.VectorConfig.ContainsKey("default"));
        Assert.Equal(ExpectedDefaultIndexType(), config.VectorConfig["default"].VectorIndexType);
    }

    /// <summary>
    /// Scenario B on the named-vector path: user explicitly configures
    /// <c>VectorIndex.Flat</c>. Must round-trip as <c>"flat"</c> on every
    /// server version — the inject helper must never overwrite an explicit
    /// value.
    /// </summary>
    [Fact]
    public async Task NamedVector_ExplicitFlat_IsPreserved()
    {
        var create = new CollectionCreateParams
        {
            Name = MakeUniqueCollectionName(nameof(NamedVector_ExplicitFlat_IsPreserved)),
            Properties = [Property.Text("title")],
            VectorConfig = Configure.Vector(
                "default",
                v => v.SelfProvided(),
                index: new VectorIndex.Flat()
            ),
        };

        var collection = await CollectionFactory<object>(create);
        var config = await collection.Config.Get(TestContext.Current.CancellationToken);
        Assert.NotNull(config.VectorConfig);
        Assert.True(config.VectorConfig.ContainsKey("default"));
        Assert.Equal(VectorIndex.Flat.TypeValue, config.VectorConfig["default"].VectorIndexType);
    }
}
