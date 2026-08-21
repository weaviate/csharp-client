namespace Weaviate.Client.Tests.Integration;

using Weaviate.Client.Models;

/// <summary>
/// The vectorizer module tests class. Covers vectorizer configurations end to end, i.e. that the
/// module name and settings the client emits are ones a real server accepts.
/// </summary>
/// <seealso cref="IntegrationTests"/>
public class TestVectorizers : IntegrationTests
{
    /// <summary>
    /// Tests that the Google Gemini multimodal factory produces a collection the server accepts.
    /// </summary>
    [Fact]
    public async Task Test_Multi2VecGoogleGemini_Creates_Collection()
    {
        RequireModule("multi2vec-google");
        // The module is on every lane, but its Gemini path (apiEndpoint) only lands in 1.34.20;
        // older builds still demand the Vertex projectId/location this factory omits.
        RequireVersion("1.34.20");

        var collection = await CollectionFactory(
            name: "TestMulti2VecGoogleGemini",
            properties: [Property.Text("text"), Property.Blob("image")],
            vectorConfig: Configure.Vector(
                "default",
                v =>
                    v.Multi2VecGoogleGemini(
                        imageFields: ["image"],
                        textFields: ["text"],
                        dimensions: 512
                    )
            )
        );

        var config = await collection.Config.Get(
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(config);
        var vectorizer = config.VectorConfig["default"].Vectorizer;
        var google = Assert.IsType<Vectorizer.Multi2VecGoogle>(vectorizer);

        Assert.Equal("multi2vec-palm", google.Identifier);
        Assert.Equal("generativelanguage.googleapis.com", google.ApiEndpoint);
        Assert.Equal(512, google.Dimensions);
        // The factory has no vectorizeClassName: nothing is sent, nothing stored or defaulted back.
        Assert.Null(google.VectorizeCollectionName);
        Assert.NotNull(google.ImageFields);
        Assert.Equal(["image"], google.ImageFields);
        Assert.NotNull(google.TextFields);
        Assert.Equal(["text"], google.TextFields);
        // Vertex-only settings: the Gemini API has neither, and the server must not echo them.
        Assert.Null(google.ProjectId);
        Assert.Null(google.Location);
    }

    /// <summary>
    /// Tests that the Vertex AI factory creates a collection with project id and location intact.
    /// </summary>
    [Fact]
    public async Task Test_Multi2VecGoogle_Vertex_Creates_Collection()
    {
        RequireModule("multi2vec-google");

        var collection = await CollectionFactory(
            name: "TestMulti2VecGoogleVertex",
            properties: [Property.Text("text"), Property.Blob("image")],
            vectorConfig: Configure.Vector(
                "default",
                v =>
                    v.Multi2VecGoogle(
                        projectId: "my-project",
                        location: "us-central1",
                        imageFields: ["image"],
                        textFields: ["text"]
                    )
            )
        );

        var config = await collection.Config.Get(
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.NotNull(config);
        var vectorizer = config.VectorConfig["default"].Vectorizer;
        var google = Assert.IsType<Vectorizer.Multi2VecGoogle>(vectorizer);

        Assert.Equal("multi2vec-palm", google.Identifier);
        Assert.Equal("my-project", google.ProjectId);
        Assert.Equal("us-central1", google.Location);
        Assert.Null(google.ApiEndpoint);
    }
}
