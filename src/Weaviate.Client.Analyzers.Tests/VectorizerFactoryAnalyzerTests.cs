using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Weaviate.Client.Analyzers.VectorizerFactoryAnalyzer>;

namespace Weaviate.Client.Analyzers.Tests;

public class VectorizerFactoryAnalyzerTests
{
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
            };
    }
}";

        var expected = VerifyCS
            .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
            .WithLocation(0)
            .WithArguments("TestVectorizer", "BaseURL");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

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
            string? baseURL = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model,
                BaseURL = baseURL
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

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
        public VectorizerConfig TestVectorizer(
            WeightedFields imageFields,
            WeightedFields textFields,
            WeightedFields videoFields
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields,
                {|#0:Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields)|}
            };
    }
}";

        var expected = VerifyCS
            .Diagnostic(VectorizerFactoryAnalyzer.MissingWeightFieldDiagnosticId)
            .WithLocation(0)
            .WithArguments("TestVectorizer", "videoFields");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

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
            WeightedFields videoFields
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields,
                Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields, videoFields)
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

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
            WeightedFields videoFields
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                ImageFields = imageFields,
                TextFields = textFields,
                VideoFields = videoFields,
                Weights = VectorizerWeights.FromWeightedFields(imageFields, textFields, videoFields: videoFields)
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

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
            string? model = null
        ) =>
            new Models.Vectorizer.TestVectorizer
            {
                Model = model
            };
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

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
            };
    }
}";

        var expected = VerifyCS
            .Diagnostic(VectorizerFactoryAnalyzer.MissingPropertyDiagnosticId)
            .WithLocation(0)
            .WithArguments("TestVectorizer", "BaseURL");

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }
}
