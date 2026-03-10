using System.Text.Json;
using Weaviate.Client.Models;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Tests for reranker configuration classes
/// </summary>
public class RerankerConfigTests
{
    /// <summary>
    /// Reranker.Cohere should expose a BaseURL property that can be set
    /// </summary>
    [Fact]
    public void Cohere_Reranker_Has_BaseURL_Property()
    {
        var cohere = new Reranker.Cohere { BaseURL = "https://custom.cohere.ai" };

        Assert.Equal("https://custom.cohere.ai", cohere.BaseURL);
    }

    /// <summary>
    /// Reranker.Cohere BaseURL should default to null when not specified
    /// </summary>
    [Fact]
    public void Cohere_Reranker_BaseURL_Defaults_To_Null()
    {
        var cohere = new Reranker.Cohere { Model = "rerank-english-v2.0" };

        Assert.Null(cohere.BaseURL);
    }

    /// <summary>
    /// RerankerConfigFactory.Cohere should accept and set baseURL
    /// </summary>
    [Fact]
    public void RerankerConfigFactory_Cohere_Accepts_BaseURL()
    {
        var factory = new RerankerConfigFactory();
        var config = (Reranker.Cohere)factory.Cohere(baseURL: "https://custom.cohere.ai");

        Assert.Equal("https://custom.cohere.ai", config.BaseURL);
    }

    /// <summary>
    /// RerankerConfigFactory.Cohere should accept both baseURL and model
    /// </summary>
    [Fact]
    public void RerankerConfigFactory_Cohere_Accepts_BaseURL_And_Model()
    {
        var factory = new RerankerConfigFactory();
        var config = (Reranker.Cohere)
            factory.Cohere(
                baseURL: "https://custom.cohere.ai",
                model: Reranker.Cohere.Models.RerankEnglishV2
            );

        Assert.Equal("https://custom.cohere.ai", config.BaseURL);
        Assert.Equal(Reranker.Cohere.Models.RerankEnglishV2, config.Model);
    }

    /// <summary>
    /// Reranker.Cohere BaseURL should be serialized to JSON when set
    /// </summary>
    [Fact]
    public void Cohere_Reranker_BaseURL_Is_Serialized_To_JSON()
    {
        var cohere = new Reranker.Cohere { BaseURL = "https://custom.cohere.ai" };
        var json = JsonSerializer.Serialize(cohere);

        Assert.Contains("baseURL", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("https://custom.cohere.ai", json);
    }
}
