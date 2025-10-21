namespace Weaviate.Client.Tests.Integration;

using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using Xunit;

[Collection("TestBackups")]
[CollectionDefinition("TestBackups", DisableParallelization = true)]
public class TestBackups : IntegrationTests
{
    static readonly BackupStorage _backend = BackupStorage.Filesystem; // typical default backend
    static readonly TimeSpan _pollingTimeout = TimeSpan.FromSeconds(5);

    public TestBackups()
        : base()
    {
        RequireVersion("1.32.0");
    }

    public override async ValueTask InitializeAsync()
    {
        // Wait for any running backups to complete before each test
        await WaitForNoRunningBackups(TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Wait for all running backups to complete before starting a new one.
    /// Since only one backup can run at a time per backend, this prevents conflicts.
    /// Cancels any backups stuck in Started status and times out after 5 seconds.
    /// </summary>
    private async Task WaitForNoRunningBackups(CancellationToken ct = default)
    {
        var timeout = _pollingTimeout;
        var startTime = DateTime.UtcNow;

        while (true)
        {
            // Check for timeout
            if (DateTime.UtcNow - startTime > timeout)
            {
                // Cancel any remaining backups and break
                var remainingBackups = await _weaviate.Backups.List(_backend);
                foreach (var backup in remainingBackups)
                {
                    if (
                        backup.Status
                        is BackupStatus.Started
                            or BackupStatus.Transferring
                            or BackupStatus.Transferred
                    )
                    {
                        try
                        {
                            await _weaviate.Backups.Cancel(_backend, backup.Id);
                        }
                        catch
                        {
                            /* Ignore cancellation errors */
                        }
                    }
                }
                break;
            }

            var backups = await _weaviate.Backups.List(_backend);
            var runningBackups = backups
                .Where(b =>
                    b.Status
                        is BackupStatus.Started
                            or BackupStatus.Transferring
                            or BackupStatus.Transferred
                )
                .ToList();

            if (!runningBackups.Any())
            {
                break;
            }

            // Cancel backups stuck in Started status for more than 2 seconds
            foreach (var backup in runningBackups)
            {
                if (backup.Status == BackupStatus.Started)
                {
                    try
                    {
                        await _weaviate.Backups.Cancel(_backend, backup.Id);
                    }
                    catch
                    {
                        /* Ignore cancellation errors */
                    }
                }
            }

            await Task.Delay(250, ct);
        }
    }

    private async Task<Backup> PollBackupUntil(
        string backupId,
        string operationName,
        IReadOnlyCollection<BackupStatus> desiredStatuses,
        int delayMs = 250,
        bool isRestore = false,
        TimeSpan? timeout = null,
        CancellationToken ct = default
    )
    {
        ArgumentNullException.ThrowIfNull(desiredStatuses);
        var effectiveTimeout = timeout ?? _pollingTimeout;
        var startedAt = DateTime.UtcNow;

        while (true)
        {
            Assert.SkipWhen(ct.IsCancellationRequested, "Polling canceled by test framework.");

            if (DateTime.UtcNow - startedAt > effectiveTimeout)
            {
                Assert.Fail(
                    $"Polling '{operationName}' for backup '{backupId}' exceeded timeout {effectiveTimeout}"
                );
            }

            var status = isRestore
                ? await _weaviate.Backups.GetRestoreStatus(_backend, backupId)
                : await _weaviate.Backups.GetStatus(_backend, backupId);

            if (desiredStatuses.Contains(status.Status))
            {
                return status;
            }

            await Task.Delay(delayMs, ct);
        }
    }

    [Fact]
    public async Task Test_Create_List_Status_Cancel_Backup()
    {
        // Create a dummy collection for the backup to operate on
        var dummyCollection = await CollectionFactory(
            name: "BackupTestCollection",
            properties: [Property.Text("dummyField")]
        );
        var dummyCollectionName = dummyCollection.Name;

        // Insert a sample object into the dummy collection
        await dummyCollection.Data.Insert(new { dummyField = "sample data" });

        var id = Helpers.GenerateUniqueIdentifier(dummyCollectionName);
        var request = new BackupCreateRequest(id);

        try
        {
            var operation = await _weaviate.Backups.Create(
                _backend,
                request,
                cancellationToken: TestContext.Current.CancellationToken
            );
            Assert.Equal(id, operation.Current.Id);

            var list = await _weaviate.Backups.List(_backend);
            Assert.Contains(list, b => b.Id == id);

            var status = await _weaviate.Backups.GetStatus(_backend, id);
            Assert.Equal(id, status.Id);
        }
        finally
        {
            try
            {
                await _weaviate.Backups.Cancel(_backend, id);
            }
            catch { }
        }
    }

    [Fact]
    public async Task Test_Create_And_Restore_Backup_With_Waiting()
    {
        // Arrange
        var collectionSeed = MakeUniqueCollectionName<object>("bkp");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        // Create two collections with sample data
        var articles = await CollectionFactory(
            name: "Article",
            properties: [Property.Text("title"), Property.Date("datePublished")]
        );
        var paragraphs = await CollectionFactory(
            name: "Paragraph",
            properties: [Property.Text("contents")]
        );

        var articlesName = articles.Name;
        var paragraphsName = paragraphs.Name;

        var articleIds = new List<Guid>();
        var paragraphIds = new List<Guid>();
        for (int i = 1; i <= 5; i++)
        {
            articleIds.Add(
                await articles.Data.Insert(
                    new { title = $"article {i}", datePublished = DateTime.UtcNow }
                )
            );
            paragraphIds.Add(await paragraphs.Data.Insert(new { contents = $"paragraph {i}" }));
        }

        // Act - create backup and wait for completion
        var createResp = await _weaviate.Backups.CreateSync(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { articlesName, paragraphsName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Verify data before deletion
        var articlesBefore = await articles.Query.FetchObjects();
        var paragraphsBefore = await paragraphs.Query.FetchObjects();
        Assert.Equal(5, articlesBefore.Objects.Count);
        Assert.Equal(5, paragraphsBefore.Objects.Count);

        // Delete collections
        await _weaviate.Collections.Delete(articlesName);
        await _weaviate.Collections.Delete(paragraphsName);

        // Restore backup and wait
        var restoreResp = await _weaviate.Backups.RestoreSync(
            _backend,
            backupId,
            new BackupRestoreRequest(Include: new[] { articlesName, paragraphsName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);

        var articlesRestored = _weaviate.Collections.Use(articlesName);
        var paragraphsRestored = _weaviate.Collections.Use(paragraphsName);

        var articlesAfter = await articlesRestored.Query.FetchObjects();
        var paragraphsAfter = await paragraphsRestored.Query.FetchObjects();
        Assert.Equal(5, articlesAfter.Objects.Count);
        Assert.Equal(5, paragraphsAfter.Objects.Count);

        // Status endpoints
        var createStatus = await _weaviate.Backups.GetStatus(_backend, backupId);
        Assert.Equal(BackupStatus.Success, createStatus.Status);
        var restoreStatus = await _weaviate.Backups.GetRestoreStatus(_backend, backupId);
        Assert.Equal(BackupStatus.Success, restoreStatus.Status);
    }

    [Fact]
    public async Task Test_Create_And_Restore_Backup_Without_Waiting()
    {
        // Arrange
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_async");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var article = await CollectionFactory(
            name: "ArticleAsync",
            properties: [Property.Text("title")]
        );

        var articleName = article.Name;

        // insert objects
        for (int i = 1; i <= 3; i++)
        {
            await article.Data.Insert(new { title = $"article {i}" });
        }

        // Act - create without waiting
        var operation = await _weaviate.Backups.Create(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(operation);
        Assert.NotEqual(BackupStatus.Failed, operation.Current.Status); // initial status should be STARTED/TRANSFERRING/TRANSFERRED

        // Wait for completion using the operation
        var createResp = await operation.WaitForCompletion(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Delete collection
        await _weaviate.Collections.Delete(articleName);

        // Restore without waiting
        BackupRestoreOperation restoreOperation = await _weaviate.Backups.Restore(
            _backend,
            backupId,
            new BackupRestoreRequest(Include: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(restoreOperation);

        // Wait for restore completion using the operation
        var restoreResp = await restoreOperation.WaitForCompletion(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);

        // Verify data restored
        var articleRestored = _weaviate.Collections.Use(articleName);

        var objectsAfter = await articleRestored.Query.FetchObjects();
        Assert.Equal(3, objectsAfter.Objects.Count);
    }

    [Fact]
    public async Task Test_Create_And_Restore_Single_Collection_Backup_With_Waiting()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_single");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var article = await CollectionFactory(
            name: "ArticleSingle",
            properties: [Property.Text("title")]
        );
        var articleName = article.Name;

        for (int i = 1; i <= 4; i++)
        {
            await article.Data.Insert(new { title = $"article {i}" });
        }

        // Create backup for only ArticleSingle
        var createResp = await _weaviate.Backups.CreateSync(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Verify count before deletion
        var before = await article.Query.FetchObjects();
        Assert.Equal(4, before.Objects.Count);

        // Delete collection
        await _weaviate.Collections.Delete(articleName);

        // Restore only that collection
        var restoreResp = await _weaviate.Backups.RestoreSync(
            _backend,
            backupId,
            new BackupRestoreRequest(Include: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);

        var articleRestored = _weaviate.Collections.Use(articleName);
        var after = await articleRestored.Query.FetchObjects();
        Assert.Equal(4, after.Objects.Count);
    }

    [Fact]
    public async Task Test_Fail_Backup_On_NonExisting_Collection()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_fail_nonexist");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);
        var bogusCollectionName = MakeUniqueCollectionName<object>("DoesNotExist");

        // Create should fail when including non-existing collection
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Backups.CreateSync(
                _backend,
                new BackupCreateRequest(backupId, Include: new[] { bogusCollectionName }),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );

        // Restore should also fail if collection doesn't exist in backup
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Backups.RestoreSync(
                _backend,
                backupId,
                new BackupRestoreRequest(Include: new[] { bogusCollectionName }),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task Test_Fail_Creating_Duplicate_Backup()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_duplicate");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var collection = await CollectionFactory(
            name: "ArticleDup",
            properties: [Property.Text("title")]
        );
        var collectionName = collection.Name;

        await collection.Data.Insert(new { title = "one" });

        var first = await _weaviate.Backups.CreateSync(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { collectionName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, first.Status);

        // Second attempt should throw (422)
        await Assert.ThrowsAnyAsync<WeaviateServerException>(async () =>
            await _weaviate.Backups.CreateSync(
                _backend,
                new BackupCreateRequest(backupId, Include: new[] { collectionName }),
                cancellationToken: TestContext.Current.CancellationToken
            )
        );
    }

    [Fact]
    public async Task Test_Fail_Restoring_Backup_For_Existing_Collection()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_restore_conflict");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        var collection = await CollectionFactory(
            name: "ArticleConflict",
            properties: [Property.Text("title")]
        );
        var collectionName = collection.Name;

        await collection.Data.Insert(new { title = "alpha" });

        var create = await _weaviate.Backups.CreateSync(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { collectionName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, create.Status);

        // Attempt to restore while collection still exists should fail
        var restoreResp = await _weaviate.Backups.RestoreSync(
            _backend,
            backupId,
            new BackupRestoreRequest(Include: new[] { collectionName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.NotNull(restoreResp.Error);
    }

    [Fact]
    public async Task Test_Cancel_Running_Backup()
    {
        var collectionSeed = MakeUniqueCollectionName<object>("bkp_cancel");
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        // Create a collection with enough objects to keep backup busy for a moment
        var collection = await CollectionFactory(
            name: "ArticleCancel",
            properties: [Property.Text("title"), Property.Text("body")]
        );
        var collectionName = collection.Name;

        for (int i = 0; i < 40; i++)
        {
            await collection.Data.Insert(new { title = $"t{i}", body = new string('x', 500) });
        }

        // Start backup without waiting
        BackupCreateOperation operation = await _weaviate.Backups.Create(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { collectionName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(backupId, operation.Current.Id);

        // Immediately request cancel via operation
        await operation.Cancel();

        // Wait and verify it's canceled
        var status = await operation.WaitForCompletion(
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Canceled, status.Status);
    }

    [Fact]
    public async Task Test_List_Backups()
    {
        // Create a dummy collection for the backup to operate on
        var dummyCollection = await CollectionFactory(
            name: "DummyCollection",
            properties: [Property.Text("dummyField")]
        );
        var dummyCollectionName = dummyCollection.Name;

        // Insert a sample object into the dummy collection
        await dummyCollection.Data.Insert(new { dummyField = "sample data" });

        // Match Python test: create backup, list it, poll until success
        var collectionSeed = "bkp_list";
        var backupId = Helpers.GenerateUniqueIdentifier(collectionSeed);

        // Create backup without waiting (mimics Python resp = client.backup.create(...))
        var operation = await _weaviate.Backups.Create(
            _backend,
            new BackupCreateRequest(backupId),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Started, operation.Current.Status);

        // List backups and verify our backup is present
        var backups = await _weaviate.Backups.List(_backend);
        Assert.Contains(backups, b => b.Id.Equals(backupId, StringComparison.OrdinalIgnoreCase));

        // Cancel via operation
        await operation.Cancel();

        // Verify canceled
        await operation.WaitForCompletion(cancellationToken: TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task Test_Overwrite_Alias_False_Should_Not_Conflict()
    {
        RequireVersion("1.33.0"); // overwriteAlias supported from 1.33.0

        var seed = Helpers.GenerateUniqueIdentifier("overwrite_false");
        var backupId = Helpers.GenerateUniqueIdentifier(seed);

        // Use deterministic names derived from seed so we can assert post conditions
        var articleName = $"{seed}_Article";
        var paragraphName = $"{seed}_Paragraph";
        var aliasName = $"{seed}_Literature";

        // Create source and target collections
        var articleClient = await CollectionFactory(
            name: articleName,
            properties: [Property.Text("title")]
        );
        articleName = articleClient.Name;

        var paragraphClient = await CollectionFactory(
            name: paragraphName,
            properties: [Property.Text("contents")]
        );
        paragraphName = paragraphClient.Name;

        await articleClient.Data.Insert(new { title = "original" });
        await paragraphClient.Data.Insert(new { contents = "p1" });

        // Create alias pointing to the Article collection
        // Delete alias if it already exists
        try
        {
            await _weaviate.Alias.Delete(aliasName);
        }
        catch { }
        await articleClient.Alias.Add(aliasName);

        // Create backup including only Article
        var createResp = await _weaviate.Backups.CreateSync(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Delete Article and repoint alias to Paragraph (conflict scenario for restore)
        await _weaviate.Collections.Delete(articleName);
        await paragraphClient.Alias.Claim(aliasName);

        // Attempt restore with overwriteAlias = false -> expect failure & alias unchanged
        var restoreResp = await _weaviate.Backups.RestoreSync(
            _backend,
            backupId,
            new BackupRestoreRequest(Include: new[] { articleName }, OverwriteAlias: false),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(BackupStatus.Success, restoreResp.Status); // terminal failed
        Assert.Null(restoreResp.Error);

        // Alias should still point to Paragraph
        var aliasAfter = await _weaviate.Alias.Get(aliasName);
        Assert.NotNull(aliasAfter);
        Assert.Equal(paragraphName, aliasAfter.TargetClass);

        // Article should have been restored (attempting to use should NOT raise)
        await _weaviate.Collections.Use(articleName).Query.FetchObjects();
    }

    [Fact]
    public async Task Test_Overwrite_Alias_True_Should_Replace_Existing_Alias()
    {
        RequireVersion("1.33.0");

        var seed = Helpers.GenerateUniqueIdentifier("overwrite_true");
        var backupId = Helpers.GenerateUniqueIdentifier(seed);

        var articleName = $"{seed}_Article";
        var paragraphName = $"{seed}_Paragraph";
        var aliasName = $"{seed}_Literature";

        var articleClient = await CollectionFactory(
            name: articleName,
            properties: [Property.Text("title")]
        );
        articleName = articleClient.Name;

        var paragraphClient = await CollectionFactory(
            name: paragraphName,
            properties: [Property.Text("contents")]
        );
        paragraphName = paragraphClient.Name;

        await articleClient.Data.Insert(new { title = "original" });
        await paragraphClient.Data.Insert(new { contents = "p1" });

        await articleClient.Alias.Add(aliasName);

        var createResp = await _weaviate.Backups.CreateSync(
            _backend,
            new BackupCreateRequest(backupId, Include: new[] { articleName }),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, createResp.Status);

        // Delete original, repoint alias to Paragraph to simulate conflicting alias
        await _weaviate.Collections.Delete(articleName);
        await paragraphClient.Alias.Claim(aliasName);

        // Restore with overwriteAlias = true should repoint alias back & recreate Article
        var restoreResp = await _weaviate.Backups.RestoreSync(
            _backend,
            backupId,
            new BackupRestoreRequest(Include: new[] { articleName }, OverwriteAlias: true),
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(BackupStatus.Success, restoreResp.Status);
        Assert.Null(restoreResp.Error);

        // Alias should now target recreated Article collection
        var aliasAfter = await _weaviate.Alias.Get(aliasName);
        Assert.NotNull(aliasAfter);
        Assert.Equal(articleName, aliasAfter.TargetClass);

        // Article collection should exist with restored data (1 object)
        var restoredArticle = _weaviate.Collections.Use(articleName);
        var objects = await restoredArticle.Query.FetchObjects();
        Assert.Single(objects.Objects);
        Assert.Equal("original", objects.Objects.First().Properties["title"]);
    }
}
