using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Weaviate.Client.Analyzers.VectorizerFactoryAnalyzer>;

namespace Weaviate.Client.Analyzers.Tests;

/// <summary>
/// The vectorizer factory analyzer tests class
/// </summary>
public class VectorizerFactoryAnalyzerTests
{
    /// <summary>
    /// The vectorizer config base source
    /// </summary>
    private const string VectorizerConfigBaseSource =
        @"
namespace Weaviate.Client.Models
{
    public abstract record VectorizerConfig { }

    public record WeightedFields
    {
        public double[]? Weights { get; set; }
    }
}

namespace Weaviate.Client.Models
{
    internal static class Vectorizer
    {
        internal record VectorizerWeights
        {
            public static VectorizerWeights FromWeightedFields(
                WeightedFields? imageFields = null,
                WeightedFields? textFields = null,
                WeightedFields? videoFields = null
            ) => new();
        }

        public record TestVectorizer : VectorizerConfig
        {
            public string? Model { get; set; }
            public string? BaseURL { get; set; }
            public WeightedFields? ImageFields { get; set; }
            public WeightedFields? TextFields { get; set; }
            public WeightedFields? VideoFields { get; set; }
            internal VectorizerWeights? Weights { get; set; }
        }
    }
}";

    /// <summary>
    /// Tests that missing property initialization reports diagnostic
    /// </summary>
    [Fact]
    public async Task MissingPropertyInitialization_ReportsDiagnostic()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;

    public class VectorizerFactory
    {
        public VectorizerConfig {|#0:TestVectorizer|}(
            string? model = null,
            string? baseURL = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model
                // Missing: BaseURL = baseURL
                // Also missing: ImageFields, TextFields, VideoFields (no params)
            };
    }
}";

        var expected = new[]
        {
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "BaseURL"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "ImageFields"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "TextFields"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "VideoFields"),
        };

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that all properties initialized no diagnostic
    /// </summary>
    [Fact]
    public async Task AllPropertiesInitialized_NoDiagnostic()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;

    public class VectorizerFactory
    {
        public VectorizerConfig TestVectorizer(
            string? model = null,
            string? baseURL = null,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            WeightedFields? videoFields = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model,
                BaseURL = baseURL,
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Tests that missing weighted field in weights calculation reports diagnostic
    /// </summary>
    [Fact]
    public async Task MissingWeightedFieldInWeightsCalculation_ReportsDiagnostic()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;
    using static Weaviate.Client.Models.Vectorizer;

    public class VectorizerFactory
    {
        public VectorizerConfig {|#0:TestVectorizer|}(
            WeightedFields imageFields,
            WeightedFields textFields,
            WeightedFields videoFields
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields,
                {|#1:Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields)|}
            };
    }
}";

        var expected = new[]
        {
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingWeightFieldDiagnosticId)
                .WithLocation(1)
                .WithArguments("TestVectorizer", "videoFields"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "Model"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "BaseURL"),
        };

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    /// <summary>
    /// Tests that all weighted fields in weights calculation no diagnostic
    /// </summary>
    [Fact]
    public async Task AllWeightedFieldsInWeightsCalculation_NoDiagnostic()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;
    using static Weaviate.Client.Models.Vectorizer;

    public class VectorizerFactory
    {
        public VectorizerConfig TestVectorizer(
            WeightedFields imageFields,
            WeightedFields textFields,
            WeightedFields videoFields,
            string? model = null,
            string? baseURL = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model,
                BaseURL = baseURL,
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields,
                Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields, videoFields)
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Tests that named arguments in weights calculation no diagnostic
    /// </summary>
    [Fact]
    public async Task NamedArgumentsInWeightsCalculation_NoDiagnostic()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;
    using static Weaviate.Client.Models.Vectorizer;

    public class VectorizerFactory
    {
        public VectorizerConfig TestVectorizer(
            WeightedFields imageFields,
            WeightedFields textFields,
            WeightedFields videoFields,
            string? model = null,
            string? baseURL = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model,
                BaseURL = baseURL,
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields,
                Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields, videoFields: videoFields)
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Tests that no weighted fields parameters no diagnostic
    /// </summary>
    [Fact]
    public async Task NoWeightedFieldsParameters_NoDiagnostic()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;

    public class VectorizerFactory
    {
        public VectorizerConfig TestVectorizer(
            string? model = null,
            string? baseURL = null,
            WeightedFields? imageFields = null,
            WeightedFields? textFields = null,
            WeightedFields? videoFields = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model,
                BaseURL = baseURL,
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    /// <summary>
    /// Tests that vectorizer factory multi class also analyzed
    /// </summary>
    [Fact]
    public async Task VectorizerFactoryMultiClass_AlsoAnalyzed()
    {
        var testCode =
            VectorizerConfigBaseSource
            + @"
namespace Weaviate.Client
{
    using Weaviate.Client.Models;

    public class VectorizerFactoryMulti
    {
        public VectorizerConfig {|#0:TestVectorizer|}(
            string? model = null,
            string? baseURL = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model
                // Missing: BaseURL = baseURL
                // Also missing: ImageFields, TextFields, VideoFields (no params)
            };
    }
}";

        var expected = new[]
        {
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "BaseURL"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "ImageFields"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "TextFields"),
            VerifyCS
                .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
                .WithLocation(0)
                .WithArguments("TestVectorizer", "VideoFields"),
        };

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }
}
