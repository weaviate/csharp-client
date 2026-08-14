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
    /// GetStatus should populate Size from the response.
    /// </summary>
    [Fact]
    public async Task GetStatus_SizeIsPopulatedFromResponse()
    {
        var json = """
            {
                "id": "my-backup",
                "status": "SUCCESS",
                "path": "/backups",
                "backend": "filesystem",
                "size": 1.5
            }
            """;

        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            }
        );

        var backup = await client.Backup.GetStatus(
            new FilesystemBackend("/backups"),
            "my-backup",
            TestContext.Current.CancellationToken
        );

        Assert.Equal(1.5, backup.Size);
    }

    /// <summary>
    /// GetStatus should return null Size when the response omits the size field.
    /// </summary>
    [Fact]
    public async Task GetStatus_SizeIsNull_WhenNotInResponse()
    {
        var json = """
            {
                "id": "my-backup",
                "status": "STARTED",
                "backend": "filesystem",
                "path": "/backups"
            }
            """;

        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            }
        );

        var backup = await client.Backup.GetStatus(
            new FilesystemBackend("/backups"),
            "my-backup",
            TestContext.Current.CancellationToken
        );

        Assert.Null(backup.Size);
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

    /// <summary>
    /// GetStatus should correctly deserialize the new CANCELLING and FINALIZING status values.
    /// </summary>
    [Theory]
    [InlineData("CANCELLING", BackupStatus.Cancelling)]
    [InlineData("FINALIZING", BackupStatus.Finalizing)]
    public async Task GetStatus_DeserializesCancellingAndFinalizingStatuses(
        string statusString,
        BackupStatus expected
    )
    {
        var json = $$"""
            {
                "id": "my-backup",
                "status": "{{statusString}}",
                "path": "/backups",
                "backend": "filesystem"
            }
            """;

        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            }
        );

        var backup = await client.Backup.GetStatus(
            new FilesystemBackend("/backups"),
            "my-backup",
            TestContext.Current.CancellationToken
        );

        Assert.Equal(expected, backup.Status);
    }

    /// <summary>
    /// A create response body, reused by the incremental-backup cases below.
    /// </summary>
    private const string CreateResponseJson = """
        {
            "id": "my-backup",
            "backend": "filesystem",
            "status": "STARTED",
            "path": "/backups"
        }
        """;

    /// <summary>
    /// Create() must put IncrementalBaseBackupId on the wire under the spec's
    /// <c>incremental_base_backup_id</c> key. Without it the client silently drops the
    /// caller's request and takes a full backup instead of an incremental one.
    /// </summary>
    [Fact]
    public async Task Create_SendsIncrementalBaseBackupId()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: req =>
                req.Method == HttpMethod.Post
                    ? new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            CreateResponseJson,
                            System.Text.Encoding.UTF8,
                            "application/json"
                        ),
                    }
                    : null!,
            serverVersion: "1.37.0"
        );

        await client.Backup.Create(
            new BackupCreateRequest(
                "my-backup",
                new FilesystemBackend("/backups"),
                IncrementalBaseBackupId: "base-backup"
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        var body = await handler.LastRequest!.Content!.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        Assert.Contains("\"incremental_base_backup_id\":\"base-backup\"", body);
    }

    /// <summary>
    /// A create request that asks for no base backup must not send the key at all, so a plain
    /// backup is unchanged.
    /// </summary>
    [Fact]
    public async Task Create_OmitsIncrementalBaseBackupId_WhenNotRequested()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: req =>
                req.Method == HttpMethod.Post
                    ? new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            CreateResponseJson,
                            System.Text.Encoding.UTF8,
                            "application/json"
                        ),
                    }
                    : null!,
            serverVersion: "1.37.0"
        );

        await client.Backup.Create(
            new BackupCreateRequest("my-backup", new FilesystemBackend("/backups")),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        var body = await handler.LastRequest!.Content!.ReadAsStringAsync(
            TestContext.Current.CancellationToken
        );
        Assert.DoesNotContain("incremental_base_backup_id", body);
    }

    /// <summary>
    /// Incremental backups arrived in Weaviate 1.37.0, so asking for one against an older
    /// server must fail in the client rather than silently producing a full backup.
    /// </summary>
    [Fact]
    public async Task Create_Throws_WhenIncrementalRequestedOnOlderServer()
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: req =>
                req.Method == HttpMethod.Post
                    ? new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            CreateResponseJson,
                            System.Text.Encoding.UTF8,
                            "application/json"
                        ),
                    }
                    : null!,
            serverVersion: "1.36.0"
        );

        var exception = await Assert.ThrowsAsync<WeaviateVersionMismatchException>(async () =>
            await client.Backup.Create(
                new BackupCreateRequest(
                    "my-backup",
                    new FilesystemBackend("/backups"),
                    IncrementalBaseBackupId: "base-backup"
                ),
                TestContext.Current.CancellationToken
            )
        );

        Assert.Equal(new Version(1, 37, 0), exception.RequiredVersion);
        Assert.Equal(new Version(1, 36, 0), exception.ActualVersion);
    }

    /// <summary>
    /// The version gate is on the field, not the operation: a plain backup must still work on a
    /// server older than 1.37.0.
    /// </summary>
    [Fact]
    public async Task Create_PlainBackup_Succeeds_OnOlderServer()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: req =>
                req.Method == HttpMethod.Post
                    ? new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(
                            CreateResponseJson,
                            System.Text.Encoding.UTF8,
                            "application/json"
                        ),
                    }
                    : null!,
            serverVersion: "1.36.0"
        );

        await client.Backup.Create(
            new BackupCreateRequest("my-backup", new FilesystemBackend("/backups")),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
    }
}
