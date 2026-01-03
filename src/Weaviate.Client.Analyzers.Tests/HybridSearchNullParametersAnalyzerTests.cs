using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<Weaviate.Client.Analyzers.HybridSearchNullParametersAnalyzer>;

namespace Weaviate.Client.Analyzers.Tests;

public class HybridSearchNullParametersAnalyzerTests
{
    private const string ClientStubs =
        @"
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Weaviate.Client.Models
{
    public class HybridVectorInput { }
    public class HybridFusion { }
    public class BM25Operator { }
    public class Filter { }
    public class Rerank { }
    public class AutoArray<T> { }
    public class QueryReference { }
    public class MetadataQuery { }
    public class VectorQuery { }
    public class WeaviateResult { }
    public class GroupByResult { }
    public class GroupByRequest { }
    public class AggregateResult { }
    public class AggregateGroupByResult { }
    public class GenerativeWeaviateResult { }
    public class GenerativeGroupByResult { }
    public class SinglePrompt { }
    public class GroupedTask { }
    public class GenerativeProvider { }
    public class Aggregate
    {
        public class GroupBy { }
        public class Metric { }
    }
}

namespace Weaviate.Client
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Weaviate.Client.Models;

    public class QueryClient
    {
        public async Task<WeaviateResult> Hybrid(
            string? query = null,
            HybridVectorInput? vectors = null,
            float? alpha = null,
            string[]? queryProperties = null,
            HybridFusion? fusionType = null,
            float? maxVectorDistance = null,
            uint? limit = null,
            uint? offset = null,
            BM25Operator? bm25Operator = null,
            uint? autoLimit = null,
            Filter? filters = null,
            Rerank? rerank = null,
            AutoArray<string>? returnProperties = null,
            IList<QueryReference>? returnReferences = null,
            MetadataQuery? returnMetadata = null,
            VectorQuery? includeVectors = null,
            CancellationToken cancellationToken = default
        )
        {
            if (query is null && vectors is null)
            {
                throw new ArgumentException(""At least one of 'query' or 'vectors' must be provided for hybrid search."");
            }
            return await Task.FromResult(new WeaviateResult());
        }

        public async Task<GroupByResult> Hybrid(
            GroupByRequest groupBy,
            string? query = null,
            HybridVectorInput? vectors = null,
            float? alpha = null,
            string[]? queryProperties = null,
            HybridFusion? fusionType = null,
            float? maxVectorDistance = null,
            uint? limit = null,
            uint? offset = null,
            BM25Operator? bm25Operator = null,
            uint? autoLimit = null,
            Filter? filters = null,
            Rerank? rerank = null,
            AutoArray<string>? returnProperties = null,
            IList<QueryReference>? returnReferences = null,
            MetadataQuery? returnMetadata = null,
            VectorQuery? includeVectors = null,
            CancellationToken cancellationToken = default
        )
        {
            if (query is null && vectors is null)
            {
                throw new ArgumentException(""At least one of 'query' or 'vectors' must be provided for hybrid search."");
            }
            return await Task.FromResult(new GroupByResult());
        }
    }

    public class GenerateClient
    {
        public async Task<GenerativeWeaviateResult> Hybrid(
            string? query = null,
            HybridVectorInput? vectors = null,
            float? alpha = null,
            string[]? queryProperties = null,
            HybridFusion? fusionType = null,
            float? maxVectorDistance = null,
            uint? limit = null,
            uint? offset = null,
            BM25Operator? bm25Operator = null,
            uint? autoLimit = null,
            Filter? filters = null,
            Rerank? rerank = null,
            SinglePrompt? singlePrompt = null,
            GroupedTask? groupedTask = null,
            GenerativeProvider? provider = null,
            AutoArray<string>? returnProperties = null,
            IList<QueryReference>? returnReferences = null,
            MetadataQuery? returnMetadata = null,
            VectorQuery? includeVectors = null,
            CancellationToken cancellationToken = default
        )
        {
            if (query is null && vectors is null)
            {
                throw new ArgumentException(""At least one of 'query' or 'vectors' must be provided for hybrid search."");
            }
            return await Task.FromResult(new GenerativeWeaviateResult());
        }
    }

    public class AggregateClient
    {
        public async Task<AggregateResult> Hybrid(
            string? query = null,
            HybridVectorInput? vectors = null,
            float alpha = 0.7f,
            string[]? queryProperties = null,
            uint? objectLimit = null,
            BM25Operator? bm25Operator = null,
            Filter? filters = null,
            float? maxVectorDistance = null,
            bool totalCount = true,
            IEnumerable<Aggregate.Metric>? returnMetrics = null,
            CancellationToken cancellationToken = default
        )
        {
            if (query is null && vectors is null)
            {
                throw new ArgumentException(""At least one of 'query' or 'vectors' must be provided for hybrid search."");
            }
            return await Task.FromResult(new AggregateResult());
        }
    }
}

namespace Weaviate.Client.Typed
{
    using System.Threading;
    using System.Threading.Tasks;
    using Weaviate.Client.Models;

    public class WeaviateObject<T> { }

    public class TypedQueryClient<T>
    {
        public async Task<WeaviateResult> Hybrid(
            string? query = null,
            HybridVectorInput? vectors = null,
            CancellationToken cancellationToken = default
        )
        {
            if (query is null && vectors is null)
            {
                throw new System.ArgumentException(""At least one of 'query' or 'vectors' must be provided for hybrid search."");
            }
            return await Task.FromResult(new WeaviateResult());
        }
    }

    public class TypedGenerateClient<T>
    {
        public async Task<GenerativeWeaviateResult> Hybrid(
            string? query = null,
            HybridVectorInput? vectors = null,
            CancellationToken cancellationToken = default
        )
        {
            if (query is null && vectors is null)
            {
                throw new System.ArgumentException(""At least one of 'query' or 'vectors' must be provided for hybrid search."");
            }
            return await Task.FromResult(new GenerativeWeaviateResult());
        }
    }
}
";

    [Fact]
    public async Task ExplicitNullArguments_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.QueryClient();
        await client.{|#0:Hybrid|}((string)null, (Weaviate.Client.Models.HybridVectorInput)null);
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NoArguments_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.QueryClient();
        await client.{|#0:Hybrid|}();
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task QueryOnlyProvided_NoDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.QueryClient();
        await client.Hybrid(""search query"");
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    [Fact]
    public async Task VectorsOnlyProvided_NoDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.QueryClient();
        var vectors = new Weaviate.Client.Models.HybridVectorInput();
        await client.Hybrid(null, vectors);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    [Fact]
    public async Task BothQueryAndVectorsProvided_NoDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.QueryClient();
        var vectors = new Weaviate.Client.Models.HybridVectorInput();
        await client.Hybrid(""search query"", vectors);
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }

    [Fact]
    public async Task NamedArgumentsWithBothNull_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.QueryClient();
        await client.{|#0:Hybrid|}(query: null, vectors: null);
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task GenerateClient_BothNull_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.GenerateClient();
        await client.{|#0:Hybrid|}();
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task AggregateClient_BothNull_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.AggregateClient();
        await client.{|#0:Hybrid|}();
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task TypedQueryClient_BothNull_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class MyType { }

class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.Typed.TypedQueryClient<MyType>();
        await client.{|#0:Hybrid|}();
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task TypedGenerateClient_BothNull_ReportsDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class MyType { }

class TestClass
{
    async Task TestMethod()
    {
        var client = new Weaviate.Client.Typed.TypedGenerateClient<MyType>();
        await client.{|#0:Hybrid|}();
    }
}";

        var expected = VerifyCS
            .Diagnostic(HybridSearchNullParametersAnalyzer.DiagnosticId)
            .WithLocation(0);

        await VerifyCS.VerifyAnalyzerAsync(testCode, expected);
    }

    [Fact]
    public async Task NonHybridMethod_NoDiagnostic()
    {
        var testCode =
            ClientStubs
            + @"
class TestClass
{
    void SomeMethod(string? query = null, object? vectors = null)
    {
    }

    void TestMethod()
    {
        SomeMethod();
    }
}";

        await VerifyCS.VerifyAnalyzerAsync(testCode);
    }
}
