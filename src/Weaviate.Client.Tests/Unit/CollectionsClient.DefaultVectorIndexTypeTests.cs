using System.Text.Json;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Common;
using Weaviate.Client.Tests.Unit.Mocks;
using Dto = Weaviate.Client.Rest.Dto;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests that mirror weaviate-python-client#2042: starting with Weaviate 1.37.5 the
/// server applies its own default for <c>vectorIndexType</c> when the client omits it.
/// The C# client should leave the field empty on 1.37.5+, but keep injecting
/// <c>"hnsw"</c> on older servers (or when the version is unknown).
/// </summary>
public class CollectionsClientDefaultVectorIndexTypeTests
{
    private const string LegacyServerVersion = "1.37.4";
    private const string DefaultIndexServerVersion =
        ServerVersions.DefaultVectorIndexTypeServerSide;

    /// <summary>
    /// Captures the JSON body of the POST to /v1/schema for a Create call.
    /// </summary>
    private static async Task<JsonElement> CaptureCreateBody(
        string? serverVersion,
        CollectionCreateParams config
    )
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            serverVersion: serverVersion
        );
        handler.AddJsonResponse(new Dto.Class { Class1 = config.Name }, "/v1/schema");

        await client.Collections.Create(config, TestContext.Current.CancellationToken);

        Assert.NotNull(handler.LastRequest);
        var body = await handler.LastRequest!.GetBodyAsString();
        return JsonDocument.Parse(body).RootElement;
    }

    /// <summary>
    /// Reads the named vector's <c>vectorIndexType</c>, or returns <c>null</c> if the
    /// property is absent or explicitly serialized as JSON null.
    /// </summary>
    private static string? GetNamedVectorIndexType(JsonElement root, string vectorName)
    {
        if (!root.TryGetProperty("vectorConfig", out var vectorConfig))
            return null;
        if (!vectorConfig.TryGetProperty(vectorName, out var entry))
            return null;
        if (!entry.TryGetProperty("vectorIndexType", out var vit))
            return null;
        return vit.ValueKind == JsonValueKind.Null ? null : vit.GetString();
    }

    /// <summary>
    /// Builds a single named-vector config. When <paramref name="indexConfig"/> is null
    /// the resulting <c>VectorConfig.VectorIndexType</c> is also null.
    /// </summary>
    private static CollectionCreateParams MakeCollection(
        string name,
        VectorIndexConfig? indexConfig
    )
    {
        return new CollectionCreateParams
        {
            Name = name,
            Properties = [Property.Text("title")],
            VectorConfig = new VectorConfig("default", new Vectorizer.SelfProvided(), indexConfig),
        };
    }

    [Fact]
    public async Task UserSet_Hnsw_IsEmittedOnLegacyServer()
    {
        var root = await CaptureCreateBody(
            LegacyServerVersion,
            MakeCollection("Hnsw_Legacy", new VectorIndex.HNSW())
        );

        Assert.Equal("hnsw", GetNamedVectorIndexType(root, "default"));
    }

    [Fact]
    public async Task UserSet_Hnsw_IsEmittedOnNewServer()
    {
        var root = await CaptureCreateBody(
            DefaultIndexServerVersion,
            MakeCollection("Hnsw_New", new VectorIndex.HNSW())
        );

        Assert.Equal("hnsw", GetNamedVectorIndexType(root, "default"));
    }

    [Fact]
    public async Task UserSet_Flat_IsEmittedOnLegacyServer()
    {
        var root = await CaptureCreateBody(
            LegacyServerVersion,
            MakeCollection("Flat_Legacy", new VectorIndex.Flat())
        );

        Assert.Equal("flat", GetNamedVectorIndexType(root, "default"));
    }

    [Fact]
    public async Task UserSet_Flat_IsEmittedOnNewServer()
    {
        var root = await CaptureCreateBody(
            DefaultIndexServerVersion,
            MakeCollection("Flat_New", new VectorIndex.Flat())
        );

        Assert.Equal("flat", GetNamedVectorIndexType(root, "default"));
    }

    [Fact]
    public async Task UnsetIndex_OnLegacyServer_InjectsHnsw()
    {
        var root = await CaptureCreateBody(
            LegacyServerVersion,
            MakeCollection("Unset_Legacy", indexConfig: null)
        );

        Assert.Equal("hnsw", GetNamedVectorIndexType(root, "default"));
    }

    [Fact]
    public async Task UnsetIndex_OnNewServer_IsOmitted()
    {
        var root = await CaptureCreateBody(
            DefaultIndexServerVersion,
            MakeCollection("Unset_New", indexConfig: null)
        );

        // On 1.37.5+ the client must leave vectorIndexType empty so the server applies
        // its DEFAULT_VECTOR_INDEX. The DTO property is nullable; the serializer may
        // omit it or write JSON null — either is acceptable as long as it is not "hnsw".
        var value = GetNamedVectorIndexType(root, "default");
        Assert.Null(value);
    }

    [Fact]
    public async Task UnknownServerVersion_InjectsHnsw()
    {
        // serverVersion null in MockHelpers means no meta is pre-populated, so the
        // client defaults InjectLegacyVectorIndexDefault to true (safe fallback).
        var root = await CaptureCreateBody(
            serverVersion: null,
            MakeCollection("Unknown_Server", indexConfig: null)
        );

        Assert.Equal("hnsw", GetNamedVectorIndexType(root, "default"));
    }

    [Fact]
    public void InjectLegacyDefaultVectorIndexType_FillsTopLevelEmptyType()
    {
        var dto = new Dto.Class { Class1 = "Top", VectorIndexType = null };

        CollectionsClient.InjectLegacyDefaultVectorIndexType(dto);

        Assert.Equal("hnsw", dto.VectorIndexType);
    }

    [Fact]
    public void InjectLegacyDefaultVectorIndexType_PreservesTopLevelExplicitType()
    {
        var dto = new Dto.Class { Class1 = "Top", VectorIndexType = "flat" };

        CollectionsClient.InjectLegacyDefaultVectorIndexType(dto);

        Assert.Equal("flat", dto.VectorIndexType);
    }

    [Fact]
    public void InjectLegacyDefaultVectorIndexType_FillsNamedEmptyTypes()
    {
        var dto = new Dto.Class
        {
            Class1 = "Named",
            VectorConfig = new Dictionary<string, Dto.VectorConfig>
            {
                ["a"] = new Dto.VectorConfig { VectorIndexType = null },
                ["b"] = new Dto.VectorConfig { VectorIndexType = "" },
                ["c"] = new Dto.VectorConfig { VectorIndexType = "flat" },
            },
        };

        CollectionsClient.InjectLegacyDefaultVectorIndexType(dto);

        Assert.Equal("hnsw", dto.VectorConfig!["a"].VectorIndexType);
        Assert.Equal("hnsw", dto.VectorConfig["b"].VectorIndexType);
        Assert.Equal("flat", dto.VectorConfig["c"].VectorIndexType);
    }

    [Fact]
    public void InjectLegacyDefaultVectorIndexType_HandlesNullVectorConfigDictionary()
    {
        var dto = new Dto.Class { Class1 = "NoVectorConfig", VectorConfig = null };

        // Should not throw on a null dictionary.
        CollectionsClient.InjectLegacyDefaultVectorIndexType(dto);

        Assert.Equal("hnsw", dto.VectorIndexType);
    }

    [Fact]
    public void InjectLegacyDefaultVectorIndexType_SkipsTopLevelWhenNamedVectorsPresent()
    {
        // The server rejects a class that has both a class-level VectorIndexType
        // AND named vectors (VectorConfig). The inject helper must not create
        // that invalid combination.
        var dto = new Dto.Class
        {
            Class1 = "Mixed",
            VectorIndexType = null,
            VectorConfig = new Dictionary<string, Dto.VectorConfig>
            {
                ["main"] = new Dto.VectorConfig { VectorIndexType = null },
            },
        };

        CollectionsClient.InjectLegacyDefaultVectorIndexType(dto);

        Assert.Null(dto.VectorIndexType);
        Assert.Equal("hnsw", dto.VectorConfig!["main"].VectorIndexType);
    }
}
