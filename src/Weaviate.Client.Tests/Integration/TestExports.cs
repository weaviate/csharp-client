namespace Weaviate.Client.Tests.Integration;

using Weaviate.Client.Models;

/// <summary>
/// Integration tests for ExportClient.
/// Requires Weaviate 1.37.0+ with export support enabled.
/// </summary>
[Trait("Category", "Slow")]
[Collection("TestExports")]
[CollectionDefinition("TestExports", DisableParallelization = true)]
public class TestExports : IntegrationTests
{
    static readonly BackupBackend _backend = new FilesystemBackend();

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        RequireVersion("1.37.0");
    }

    [Fact]
    public async Task CreateSync_CompletesSuccessfully()
    {
        var ct = TestContext.Current.CancellationToken;
        var collection = await CollectionFactory("ExportTest1");

        var export = await _weaviate.Export.CreateSync(
            new ExportCreateRequest(
                $"export-sync-{Guid.NewGuid():N}",
                _backend,
                IncludeCollections: [collection.Name]
            ),
            timeout: TimeSpan.FromMinutes(2),
            cancellationToken: ct
        );

        Assert.Equal(ExportStatus.Success, export.Status);
        Assert.NotNull(export.CompletedAt);
    }

    [Fact]
    public async Task Create_ThenWaitForCompletion()
    {
        var ct = TestContext.Current.CancellationToken;
        var collection = await CollectionFactory("ExportTest2");

        await using var operation = await _weaviate.Export.Create(
            new ExportCreateRequest(
                $"export-async-{Guid.NewGuid():N}",
                _backend,
                IncludeCollections: [collection.Name]
            ),
            ct
        );

        Assert.NotNull(operation.Current);

        var result = await operation.WaitForCompletion(TimeSpan.FromMinutes(2), ct);

        Assert.Equal(ExportStatus.Success, result.Status);
        Assert.True(operation.IsCompleted);
        Assert.True(operation.IsSuccessful);
    }

    [Fact]
    public async Task GetStatus_ReturnsExportInfo()
    {
        var ct = TestContext.Current.CancellationToken;
        var collection = await CollectionFactory("ExportTest3");
        var exportId = $"export-status-{Guid.NewGuid():N}";

        await _weaviate.Export.CreateSync(
            new ExportCreateRequest(exportId, _backend, IncludeCollections: [collection.Name]),
            timeout: TimeSpan.FromMinutes(2),
            cancellationToken: ct
        );

        var status = await _weaviate.Export.GetStatus(_backend, exportId, ct);

        Assert.Equal(exportId, status.Id);
        Assert.Equal(ExportStatus.Success, status.Status);
    }

    [Fact]
    public async Task Cancel_StopsRunningExport()
    {
        var ct = TestContext.Current.CancellationToken;
        var collection = await CollectionFactory("ExportTest4");

        await using var operation = await _weaviate.Export.Create(
            new ExportCreateRequest(
                $"export-cancel-{Guid.NewGuid():N}",
                _backend,
                IncludeCollections: [collection.Name]
            ),
            ct
        );

        // Cancel immediately — may already have completed for small collections.
        // Server responds with 409 Conflict when the export has already finished,
        // or 404 Not Found if the record is no longer available.
        try
        {
            await _weaviate.Export.Cancel(_backend, operation.Current.Id, ct);
        }
        catch (WeaviateConflictException)
        {
            // Export already finished — can't cancel a terminal export.
        }
        catch (WeaviateNotFoundException)
        {
            // Export record no longer available.
        }

        try
        {
            var status = await _weaviate.Export.GetStatus(_backend, operation.Current.Id, ct);

            Assert.True(
                status.Status is ExportStatus.Canceled or ExportStatus.Success,
                $"Expected Canceled or Success but got {status.Status}"
            );
        }
        catch (WeaviateNotFoundException)
        {
            // Treat vanished export as successfully canceled.
        }
    }
}
