using System.Reflection;
using ApprovalTests;
using ApprovalTests.Reporters;
using PublicApiGenerator;
using Xunit;

namespace Weaviate.Client.Tests
{
    [UseReporter(typeof(DiffReporter))]
    public class PublicApiApprovalTests
    {
        [Fact]
        public void PublicApi_Should_Not_Change_Unexpectedly()
        {
            // Replace 'WeaviateClient' with a public type from your main library assembly
            var assembly = typeof(Weaviate.Client.WeaviateClient).Assembly;
            var publicApi = assembly.GeneratePublicApi();
            Approvals.Verify(publicApi);
        }
    }
}
