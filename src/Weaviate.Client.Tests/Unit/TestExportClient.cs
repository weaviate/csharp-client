using System.Net;
using Weaviate.Client.Models;
using Weaviate.Client.Tests.Unit.Mocks;

namespace Weaviate.Client.Tests.Unit;

/// <summary>
/// Unit tests for ExportClient, focusing on HTTP request routing and DTO-to-model mapping.
/// </summary>
public class TestExportClient
{
    /// <summary>
    /// Create must issue a POST to /v1/export/filesystem with the correct JSON body.
    /// </summary>
    [Fact]
    public async Task Create_SendsPostToExportEndpoint()
    {
        var json = """
            {
                "id": "my-export",
                "backend": "filesystem",
                "status": "STARTED",
                "classes": ["Article"]
            }
            """;

        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            },
            serverVersion: "1.37.0"
        );

        await using var operation = await client.Export.Create(
            new ExportCreateRequest("my-export", new FilesystemBackend(), ExportFileFormat.Parquet),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Post)
            .ShouldHavePath("/v1/export/filesystem");
    }

    /// <summary>
    /// GetStatus must issue a GET to /v1/export/filesystem/{id}.
    /// </summary>
    [Fact]
    public async Task GetStatus_SendsGetToStatusEndpoint()
    {
        var json = """
            {
                "id": "my-export",
                "backend": "filesystem",
                "status": "SUCCESS",
                "startedAt": "2026-01-01T00:00:00Z",
                "completedAt": "2026-01-01T00:01:00Z",
                "tookInMs": 60000
            }
            """;

        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            },
            serverVersion: "1.37.0"
        );

        var export = await client.Export.GetStatus(
            new FilesystemBackend(),
            "my-export",
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Get)
            .ShouldHavePath("/v1/export/filesystem/my-export");

        Assert.Equal("my-export", export.Id);
        Assert.Equal(ExportStatus.Success, export.Status);
        Assert.Equal(60000, export.TookInMs);
    }

    /// <summary>
    /// Cancel must issue a DELETE to /v1/export/filesystem/{id} and return true on 204.
    /// </summary>
    [Fact]
    public async Task Cancel_SendsDeleteToExportEndpoint()
    {
        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.NoContent),
            serverVersion: "1.37.0"
        );

        var canceled = await client.Export.Cancel(
            new FilesystemBackend(),
            "my-export",
            TestContext.Current.CancellationToken
        );

        Assert.True(canceled);
        Assert.NotNull(handler.LastRequest);
        handler
            .LastRequest!.ShouldHaveMethod(HttpMethod.Delete)
            .ShouldHavePath("/v1/export/filesystem/my-export");
    }

    /// <summary>
    /// Cancel returns false (no throw) when the server responds 409 Conflict —
    /// the export reached a terminal state and cannot be canceled.
    /// </summary>
    [Fact]
    public async Task Cancel_OnConflict_ReturnsFalseWithoutThrowing()
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.Conflict),
            serverVersion: "1.37.0"
        );

        var canceled = await client.Export.Cancel(
            new FilesystemBackend(),
            "my-export",
            TestContext.Current.CancellationToken
        );

        Assert.False(canceled);
    }

    /// <summary>
    /// Cancel still throws on 404 Not Found — the export id is unknown.
    /// </summary>
    [Fact]
    public async Task Cancel_OnNotFound_Throws()
    {
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.NotFound),
            serverVersion: "1.37.0"
        );

        await Assert.ThrowsAsync<WeaviateNotFoundException>(async () =>
            await client.Export.Cancel(
                new FilesystemBackend(),
                "unknown-export",
                TestContext.Current.CancellationToken
            )
        );
    }

    /// <summary>
    /// Status parsing should correctly map all known status strings.
    /// </summary>
    [Theory]
    [InlineData("STARTED", ExportStatus.Started)]
    [InlineData("TRANSFERRING", ExportStatus.Transferring)]
    [InlineData("SUCCESS", ExportStatus.Success)]
    [InlineData("FAILED", ExportStatus.Failed)]
    [InlineData("CANCELED", ExportStatus.Canceled)]
    public async Task GetStatus_ParsesStatusCorrectly(string statusString, ExportStatus expected)
    {
        var json = $$"""
            {
                "id": "my-export",
                "backend": "filesystem",
                "status": "{{statusString}}"
            }
            """;

        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            },
            serverVersion: "1.37.0"
        );

        var export = await client.Export.GetStatus(
            new FilesystemBackend(),
            "my-export",
            TestContext.Current.CancellationToken
        );

        Assert.Equal(expected, export.Status);
    }

    /// <summary>
    /// GetStatus should correctly deserialize shard progress from the response.
    /// </summary>
    [Fact]
    public async Task GetStatus_DeserializesShardStatus()
    {
        var json = """
            {
                "id": "my-export",
                "backend": "filesystem",
                "status": "TRANSFERRING",
                "shardStatus": {
                    "Article": {
                        "shard0": {
                            "status": "TRANSFERRING",
                            "objectsExported": 42
                        },
                        "shard1": {
                            "status": "SUCCESS",
                            "objectsExported": 100
                        }
                    }
                }
            }
            """;

        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            },
            serverVersion: "1.37.0"
        );

        var export = await client.Export.GetStatus(
            new FilesystemBackend(),
            "my-export",
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(export.ShardStatus);
        Assert.True(export.ShardStatus!.ContainsKey("Article"));
        var articleShards = export.ShardStatus["Article"];
        Assert.Equal(2, articleShards.Count);
        Assert.Equal(42, articleShards["shard0"].ObjectsExported);
        Assert.Equal(100, articleShards["shard1"].ObjectsExported);
    }

    /// <summary>
    /// Create with include/exclude collections should send the correct JSON body.
    /// </summary>
    [Fact]
    public async Task Create_WithIncludeCollections_SendsCorrectBody()
    {
        var json = """
            {
                "id": "my-export",
                "backend": "filesystem",
                "status": "STARTED",
                "classes": ["Article"]
            }
            """;

        string? requestBody = null;
        var (client, _) = MockWeaviateClient.CreateWithMockHandler(
            handlerWithToken: async (req, ct) =>
            {
                if (req.Method == HttpMethod.Post)
                {
                    requestBody = await req.Content!.ReadAsStringAsync(ct);
                }
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        json,
                        System.Text.Encoding.UTF8,
                        "application/json"
                    ),
                };
            },
            serverVersion: "1.37.0"
        );

        await using var operation = await client.Export.Create(
            new ExportCreateRequest(
                "my-export",
                new FilesystemBackend(),
                ExportFileFormat.Parquet,
                IncludeCollections: ["Article", "Author"]
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(requestBody);
        Assert.Contains("\"include\":[\"Article\",\"Author\"]", requestBody);
    }

    /// <summary>
    /// Create with S3 backend uses correct endpoint path.
    /// </summary>
    [Fact]
    public async Task Create_WithS3Backend_UsesCorrectEndpoint()
    {
        var json = """
            {
                "id": "my-export",
                "backend": "s3",
                "status": "STARTED"
            }
            """;

        var (client, handler) = MockWeaviateClient.CreateWithMockHandler(
            syncHandler: _ => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
            },
            serverVersion: "1.37.0"
        );

        await using var operation = await client.Export.Create(
            new ExportCreateRequest(
                "my-export",
                ObjectStorageBackend.S3(),
                ExportFileFormat.Parquet
            ),
            TestContext.Current.CancellationToken
        );

        Assert.NotNull(handler.LastRequest);
        handler.LastRequest!.ShouldHaveMethod(HttpMethod.Post).ShouldHavePath("/v1/export/s3");
    }
}
