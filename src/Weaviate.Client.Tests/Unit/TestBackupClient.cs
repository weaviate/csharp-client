using System.Net;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for BackupClient, focusing on HTTP request routing and cancel semantics.
/// </summary>
public class TestBackupClient
{
    /// <summary>
    /// CancelRestore() must issue a DELETE to /v1/backups/{backend}/{id}/restore.
    /// </summary>
    [Fact]
    public async Task CancelRestore_SendsDeleteToRestorePath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: req => new HttpResponseMessage(HttpStatusCode.NoContent)
        );

        await client.Backup.CancelRestore(
            new FilesystemBackend("/backups"),
            "my-backup",
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/backups/filesystem/my-backup/restore");
    }

    /// <summary>
    /// CancelRestore() with an object-storage backend must include bucket and path query params.
    /// </summary>
    [Fact]
    public async Task CancelRestore_WithObjectStorageBackend_IncludesBucketAndPath()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: req => new HttpResponseMessage(HttpStatusCode.NoContent)
        );

        await client.Backup.CancelRestore(
            ObjectStorageBackend.S3(bucket: "my-bucket", path: "/backups"),
            "my-backup",
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/backups/s3/my-backup/restore");

        var uri = handler.LastRequest!.RequestUri!.Query;
        Assert.Contains("bucket=my-bucket", uri);
        Assert.Contains("path=", uri);
    }

    /// <summary>
    /// When a BackupRestoreOperation is cancelled, it must use the restore-specific DELETE endpoint,
    /// not the backup-create cancel endpoint (/v1/backups/{backend}/{id}).
    /// </summary>
    [Fact]
    public async Task Restore_CancelDelegate_CallsRestoreEndpointNotCreateEndpoint()
    {
        var requests = new List<HttpRequestMessage>();

        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            handlerWithToken: (req, ct) =>
            {
                requests.Add(req);
                if (req.Method == HttpMethod.Post)
                {
                    // Restore initiation response
                    var json =
                        """{"id":"my-backup","backend":"filesystem","status":"STARTED","path":"/backups"}""";
                    return Task.FromResult(
                        new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(
                                json,
                                System.Text.Encoding.UTF8,
                                "application/json"
                            ),
                        }
                    );
                }
                if (req.Method == HttpMethod.Delete)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
                }
                // GET: return CANCELED status so operation terminates
                var statusJson =
                    """{"id":"my-backup","backend":"filesystem","status":"CANCELED","path":"/backups"}""";
                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            statusJson,
                            System.Text.Encoding.UTF8,
                            "application/json"
                        ),
                    }
                );
            }
        );

        var operation = await client.Backup.Restore(
            new BackupRestoreRequest("my-backup", new FilesystemBackend("/backups")),
            TestContext.Current.CancellationToken
        );

        await operation.Cancel(TestContext.Current.CancellationToken);

        var deleteRequest = requests.SingleOrDefault(r => r.Method == HttpMethod.Delete);
        Assert.NotNull(deleteRequest);
        Assert.Contains("/restore", deleteRequest!.RequestUri!.PathAndQuery);
    }
}
