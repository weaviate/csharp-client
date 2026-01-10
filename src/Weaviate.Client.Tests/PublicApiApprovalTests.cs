using ApprovalTests;
using ApprovalTests.Reporters;
using PublicApiGenerator;

namespace Weaviate.Client.Tests
{
    /// <summary>
    /// The public api approval tests class
    /// </summary>
    [UseReporter(typeof(DiffReporter))]
    public class PublicApiApprovalTests
    {
        /// <summary>
        /// Tests that public api should not change unexpectedly
        /// </summary>
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
